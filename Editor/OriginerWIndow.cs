using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Originer.Editor
{
    public class OriginerWindow : EditorWindow
    {
        private OriginerModel model = new OriginerModel();

        private VisualTreeAsset listItemTemplate;
        private VisualTreeAsset packageTemplate;

        [MenuItem("Window/Originer/Packages")]
        static void Init()
        {
            var wnd = GetWindow<OriginerWindow>();
            wnd.titleContent = new GUIContent("Originer");
        }

        void OnEnable()
        {

            var root = rootVisualElement;
            var containerElement = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/Originer/Editor/Originer.uxml").CloneTree();
            listItemTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/Originer/Editor/PackageListItem.uxml");
            packageTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/Originer/Editor/PackageInfo.uxml");

            root.Add(containerElement);
            containerElement.StretchToParentSize();
            root.AddToClassList("container");

            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/Originer/Editor/Originer.uss"));

            root.Q<ToolbarPopupSearchField>().RegisterValueChangedCallback(e => {
                model.searchRequest.name = e.newValue;
            });

            root.Q<Button>("searchButton").RegisterCallback<MouseUpEvent>(e => {
                model.Search(result =>
                {
                    if(result)
                    {
                        return;
                    }

                    RefreshPackages();
                });
            });

            model.Search(result =>
            {
                if(result)
                {
                    return; // TODO: create retry button
                }

                SubscribePager(root.Q("pagerTop"));

                RefreshPackages();
            });
        }

        void RefreshPackages()
        {
            RefreshPager(model.searchResult, rootVisualElement.Q("pagerTop"));

            var packageList = rootVisualElement.Q("packageList");
            packageList.Clear();

            var itemElements = model.searchResult.items.Select(CreatePackageListItem);
            foreach (var item in itemElements)
            {
                packageList.Add(item);
            }
        }

        private VisualElement CreatePackageListItem(PackageInfoDto package)
        {
            var element = listItemTemplate.CloneTree();
            element.Q<Label>("itemFullName").text = package.displayName;
            element.RegisterCallback<MouseUpEvent>(e => OnSelectPackage(element, package));
            return element;
        }

        private void RefreshPager(OriginerPackageSearchModelDto model, VisualElement pager)
        {
            pager.Q<Label>("currentPage").text = (model.page + 1).ToString();
            pager.Q<Label>("maxPage").text = ((model.count / model.itemsPerPage ) + 1).ToString();
        }

        private void SubscribePager(VisualElement pager)
        {
            pager.Q("prevPage").RegisterCallback<MouseUpEvent>(e => OnPagerPageChange(-1));
            pager.Q("nextPage").RegisterCallback<MouseUpEvent>(e => OnPagerPageChange(1));
        }

        private VisualElement CreatePackageInfo(PackageInfoDto package)
        {
            var element = packageTemplate.CloneTree();

            element.Q<Label>("package-displayName").text = package.displayName;
            element.Q<Label>("package-name").text = package.name;
            element.Q<Label>("package-lastVersion").text = package.version;
            element.Q<TextElement>("package-desc").text = package.description;
            element.Q<TextElement>("package-url").text = package.projectUrl;

            element.Q<Button>("installMain").RegisterCallback<MouseUpEvent>(e => Install(package));

            return element;
        }


        private void OnPagerPageChange(int v)
        {
            var maxPage = (int)Math.Ceiling((double)model.searchResult.count / model.searchResult.itemsPerPage);

            model.searchRequest.page += v;
            model.searchRequest.page = Math.Min(maxPage, Math.Max(0, model.searchRequest.page));

            model.Search(b => {
                if(b)
                {
                    return;
                }

                RefreshPackages();
            });
        }

        private void OnSelectPackage(TemplateContainer element, PackageInfoDto package)
        {
            rootVisualElement.Q("packageInfo").Clear();
            rootVisualElement.Q("packageInfo").Add(CreatePackageInfo(package));
        }

        private void Install(PackageInfoDto package)
        {
            OriginerExtension.Install(package.name, package.urlForManifest);
        }
    }
}