using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Originer
{
    [Serializable]
    [DataContract]
    [KnownType(typeof(LockEntry))]
    [KnownType(typeof(ScopedRegistryEntry))]
    [KnownType(typeof(ScopedRegistryEntry[]))]
    [KnownType(typeof(Dictionary<string, LockEntry>))]
    public class ManifestOrPackageInfo
    {
        [DataMember]
        public Dictionary<string, string> dependencies { get; set; }

        [DataMember]
        public Dictionary<string, LockEntry> @lock { get; set; }
        
        [DataMember]
        public ScopedRegistryEntry[] scopedRegistries { get; set; }

        [DataContract]
        public class LockEntry
        {
            [DataMember]
            public string hash { get; set; }
            
            [DataMember]
            public string revision { get; set; }
        }
        
        [DataContract]
        public class ScopedRegistryEntry
        {
            [DataMember]
            public string name { get; set; }
            
            [DataMember]
            public string url { get; set; }
            
            [DataMember]
            public string[] scopes { get; set; }
        }
    }
}