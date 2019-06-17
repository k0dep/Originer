using System;
using System.Runtime.Serialization;

namespace Originer
{
    [Serializable]
    [DataContract]
    [KnownType(typeof(RepositoryInfoItem))]
    [KnownType(typeof(RepositoryInfoItem[]))]
    [KnownType(typeof(string[]))]
    public class GithubResponse
    {
        [DataMember]
        public RepositoryInfoItem[] items { get; set; }
        
        [DataContract]
        public class RepositoryInfoItem
        {
            [DataMember]
            public string name { get; set; }
        
            [DataMember]
            public string clone_url { get; set; }
            
            [DataMember]
            public RepositoryOwner owner { get; set; }
        }

        [DataContract]
        public class RepositoryOwner
        {
            [DataMember]
            public string login { get; set; }
        }
    }
}