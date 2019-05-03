using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace Originer
{
    public class OriginerExtension
    {
        private const float PHASES = 2f;
        private const string PROGRESS_TITLE = "Originer | Resolve packages";
        
        [InitializeOnLoadMethod]
        public static void Init()
        {
            EditorApplication.projectChanged += EditorApplicationOnProjectChanged;
        }

        [MenuItem("Edit/Originer/Fix missing packages")]
        private static void EditorApplicationOnProjectChanged()
        {
            try
            {
                var installedPackages = CollectInstalledPackages();
                var missingPackages = MissingPackages(installedPackages);
                if (missingPackages.Count == 0)
                {
                    return;
                }
                
                EditorUtility.DisplayProgressBar(PROGRESS_TITLE, "Find github missing packages", 0f / PHASES);
                
                var foundOnGithub = GetMisingManifestDeps(missingPackages);
                if (foundOnGithub.Count == 0)
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }
                
                EditorUtility.DisplayProgressBar(PROGRESS_TITLE, "Prepare for changes", 1f / PHASES);

                var manifest = File.ReadAllText("./Packages/manifest.json").JsonSerialize<ManifestOrPackageInfo>();
                foreach (var missingPackage in foundOnGithub)
                {
                    manifest.dependencies.Add(missingPackage.Key, missingPackage.Value);
                }

                if (manifest.scopedRegistries == null)
                {
                    manifest.scopedRegistries = new ManifestOrPackageInfo.ScopedRegistryEntry[0];
                }

                LogSummary(foundOnGithub, missingPackages);
                var canMakeChanges = AskCanMakeChanges(foundOnGithub, missingPackages);
                
                EditorUtility.DisplayProgressBar(PROGRESS_TITLE, "Make changes", 2f / PHASES);
                
                if (canMakeChanges)
                {
                    File.WriteAllText("./Packages/manifest.json", manifest.JsonDeserialize());
                    AssetDatabase.Refresh();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static bool AskCanMakeChanges(Dictionary<string, string> foundOnGithub, Dictionary<string, Version> missingPackages)
        {
            var resultInfo = "Will added packages:\n\n";
            foreach (var ghEntry in foundOnGithub)
            {
                resultInfo += "+ " + ghEntry.Key + "\n";
            }
            
            var notFounds = missingPackages.ToList().Where(p => !foundOnGithub.ContainsKey(p.Key)).ToList();
            if (notFounds.Count > 0)
            {
                resultInfo += "\nNot found at Github:\n\n";
            }
            foreach (var notFound in notFounds)
            {
                resultInfo += "- " + notFound.Key + "\n";
            }

            resultInfo += "\nAdd founded packages into manifest.json?";

            var canMakeChanges = EditorUtility.DisplayDialog("Found missing packages in Github", resultInfo,
                "Add packages!", "Skip");
            return canMakeChanges;
        }

        private static string LogSummary(Dictionary<string, string> foundOnGithub, Dictionary<string, Version> missingPackages)
        {
            var resultInfo = "<color=blue>[Originer]</color> Found missing packages from github.\n";
            foreach (var ghEntry in foundOnGithub)
            {
                resultInfo += "<color=green> + found package `" + ghEntry.Key + "` with url: " + ghEntry.Value +
                              "</color>\n";
            }

            var notFounds = missingPackages.ToList().Where(p => !foundOnGithub.ContainsKey(p.Key)).ToList();
            foreach (var notFound in notFounds)
            {
                resultInfo += "<color=red> ! not found package `" + notFound.Key + "` version: " + notFound.Value +
                              "</color>\n";
            }

            if (notFounds.Any())
            {
                Debug.LogError(resultInfo);
            }
            else
            {
                Debug.Log(resultInfo);
            }

            return resultInfo;
        }

        private static string[] CollectInstalledPackages()
        {
            return Directory.GetDirectories("./Library/PackageCache")
                .Select(Path.GetFileName)
                .Union(Directory.GetDirectories("./Packages").Select(Path.GetFileName).ToArray())
                .ToArray();
        }
        
        private static Dictionary<string, Version> MissingPackages(string[] installedPackages)
        {
            var missings = new Dictionary<string, Version>();
            foreach (var package in installedPackages)
            {
                var dependencies = GetPackageDependencies(package);
                foreach (var dependency in dependencies)
                {
                    var depName = dependency.Key;
                    if (installedPackages.Any(s =>
                            s.Substring(0, s.Contains("@") ? s.IndexOf("@") : s.Length).ToLower() ==
                            depName.ToLower()) ||
                        depName.StartsWith("com.unity."))
                    {
                        continue;
                    }

                    if (!Version.TryParse(dependency.Value, out var version))
                    {
                        Debug.LogWarning(
                            "<color=blue>[Originer]</color> Cant parse version from package dependency.\npackage: " +
                            package + "\ndependency: " + depName + "\ndependency version: " + dependency.Value);
                        version = new Version(0, 0, 0);
                    }
                    missings[dependency.Key] = version;
                }
            }

            return missings;
        }

        private static Dictionary<string, string> GetPackageDependencies(string package)
        {
            var packagesDir = package.Contains('@') ? "./Library/PackageCache/" : "./Packages/";
            
            var manifest = File.ReadAllText(packagesDir + package + "/package.json")
                .JsonSerialize<ManifestOrPackageInfo>();

            if(manifest.dependencies == null)
            {
                return new Dictionary<string, string>();
            }

            return manifest.dependencies;
        }
        
        private static Dictionary<string, string> GetMisingManifestDeps(Dictionary<string, Version> missingPackages)
        {
            var matchGithub = missingPackages
                .Select((p, i) =>
                    new KeyValuePair<string, string>(p.Key,
                        RequestGithubRepositiory(p.Key, p.Value, i, missingPackages.Count)))
                .Where(p => !string.IsNullOrEmpty(p.Value))
                .ToDictionary(p => p.Key, p => p.Value);

            return matchGithub;
        }

        private static string RequestGithubRepositiory(string packageName, Version version, int itemIndex, int maxCount)
        {
            var url = "https://api.github.com/search/repositories?q=" + packageName +
                      "+fork:true+topic:upm-package";

            EditorUtility.DisplayProgressBar(PROGRESS_TITLE, "Find package: " + packageName + "#" + version,
                (itemIndex / (float) maxCount) / PHASES);
            
            var response = Request(url);

            GithubResponse data = null;
            try
            {
                data = response.JsonSerialize<GithubResponse>();
            }
            catch
            {
                Debug.LogError("Cant parse github json search response\nurl: " + url + "data:\n" + response);
                return null;
            }

            if (data == null)
            {
                Debug.LogError("Unknown serialization error. Code: 1");
                return null;
            }

            var relevantRepo = data.items.FirstOrDefault(r => r.name.ToLower() == packageName.ToLower());
            if (relevantRepo == null)
            {
                Debug.LogWarning("Repository with name `" + packageName + "` not found! Skip.");
                return null;
            }
            
            if (!IsTagExist(relevantRepo, version))
            {
                Debug.LogWarning("Not found concrete version for repository\nversion: " + version
                                 + "\nrepository url: " + relevantRepo.clone_url);
                return relevantRepo.clone_url;
            }

            return relevantRepo.clone_url + "#" + version;
        }

        private static bool IsTagExist(GithubResponse.RepositoryInfoItem repo, Version version)
        {
            var url = "https://api.github.com/repos/" + repo.owner.login + "/" + repo.name + "/tags";

            var response = Request(url);

            if (string.IsNullOrEmpty(response))
            {
                return false;
            }
            
            var tags = response.JsonSerialize<GithubRepositoryTag[]>();
            if (tags == null)
            {
                return false;
            }

            return tags.Any(t => t.name == version.ToString());
        }

        private static string Request(string url, bool logHttpError = false)
        {
            UnityWebRequest request = null;
            for (int retry = 0; retry < 3; retry++)
            {
                request = UnityWebRequest.Get(url);
                request.SetRequestHeader("Accept", "application/vnd.github.mercy-preview+json");

                var operation = request.SendWebRequest();

                for (int i = 0; i < 5000 && !operation.isDone; i++)
                {
                    Thread.Sleep(10);
                }

                if (!request.isDone)
                {
                    continue;
                }

                if (request.isHttpError || request.isNetworkError)
                {
                    if (logHttpError)
                    {
                        Debug.LogError("Request error!\nurl: " + url + "\nerror: " + request.error);
                    }
                    return null;
                }

                return request.downloadHandler.text;
            }
            
            Debug.LogError("Request timeout!\nurl: " + url); 
            return null;
        }
    }
}
