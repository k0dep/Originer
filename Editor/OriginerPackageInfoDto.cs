using System.Linq;
using System;
using System.Runtime.Serialization;
using UnityEngine.Networking;
using UnityEngine;

namespace Originer.Editor
{
    public class OriginerModel
    {
        public OriginerPackageSearchModelDto searchResult = new OriginerPackageSearchModelDto();
        public OriginerPackageSearchRequrstDto searchRequest = new OriginerPackageSearchRequrstDto();
        
        public void Search(Action<bool> onComplete)
        {
            new EditorWebRequest("https://api.originer.parabox.tech/api/search",
                searchRequest.JsonDeserialize(), req =>
                {
                    OnSearchComplete(req);
                    onComplete?.Invoke(req.isHttpError | req.isNetworkError);
                });
        }

        private void OnSearchComplete(UnityWebRequest request)
        {
            try
            {
                searchResult = request
                    .downloadHandler
                    .text
                    .JsonSerialize<OriginerPackageSearchModelDto>();
            }
            catch(Exception e)
            {
                Debug.LogError("Search response serialization error");
                Debug.LogException(e);
            }
            
            var isError = request.isHttpError | request.isNetworkError;

            if(isError)
            {
                Debug.LogError($"Search request error\nerror: {request.error}");
            }
        }
    }

    [DataContract]
    [Serializable]
    public class OriginerPackageSearchRequrstDto
    {
        [DataMember]
        public int page;

        [DataMember]
        public int count = 20;

        [DataMember]
        public string name = "";
    }

    [DataContract]
    [Serializable]
    [KnownType(typeof(PackageInfoDto))]
    [KnownType(typeof(PackageInfoDto[]))]
    public class OriginerPackageSearchModelDto
    {
        [DataMember]
        public int count;

        [DataMember]
        public int itemsPerPage;

        [DataMember]
        public int page;

        [DataMember]
        public PackageInfoDto[] items;

        public OriginerPackageSearchModelDto()
        {
            count = 450;
            itemsPerPage = 15;
            page = 2;
            items = Enumerable.Range(0, 15).Select(i => new PackageInfoDto
            {
                displayName = i + Guid.NewGuid().ToString(),
                name = i + Guid.NewGuid().ToString().Substring(5)
            }).ToArray();
        }
    }
}