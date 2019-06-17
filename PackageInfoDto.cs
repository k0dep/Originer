using System;
using System.Runtime.Serialization;

namespace Originer
{
    [Serializable]
    [DataContract]
    [KnownType(typeof(string[]))]
    public class PackageInfoDto
    {
        [DataMember]
        public int id;

        [DataMember]
        public string name;

        [DataMember]
        public string projectUrl;

        [DataMember]
        public string urlForManifest;
        
        [DataMember]
        public string displayName;

        [DataMember]
        public string description;
        
        [DataMember]
        public string version;

        [DataMember]
        public string[] dependencies;

        [DataMember]
        public string category;

        [DataMember]
        public string unity;

        [DataMember]
        public string[] versions;
    }
}