using System;
using System.Runtime.Serialization;

namespace Originer
{
    [Serializable]
    [DataContract]
    public class GithubRepositoryTag
    {
        [DataMember]
        public string name { get; set; }
    }
}