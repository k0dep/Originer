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
        public int id { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string projectUrl { get; set; }

        [DataMember]
        public string urlForManifest { get; set; }
        
        [DataMember]
        public string displayName { get; set; }

        [DataMember]
        public string description { get; set; }
        
        [DataMember]
        public string version { get; set; }

        [DataMember]
        public string[] dependencies { get; set; }

        [DataMember]
        public string category { get; set; }

        [DataMember]
        public string unity { get; set; }

        [DataMember]
        public string[] versions { get; set; }
    }
}