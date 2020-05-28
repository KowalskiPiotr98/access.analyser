using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Linq;

namespace Lambda.S3
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only",
        Justification = "Needed for deserialization")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible",
        Justification = "Needed for deserialization")]
    public class S3NotificationEvent
    {
        public (string bucketName, string objectsKey, string region) GetFileData()
        {
            var record = Records.First();
            return (record.S3.ObjectsBucket.Name, record.S3.ObjectData.Key, record.AWSRegion);
        }


        [JsonPropertyName("Records")]
        public IEnumerable<Record> Records { get; set; }

        public class Record
        {
            [JsonPropertyName("s3")]
            public S3Data S3 { get; set; }
            [JsonPropertyName("awsRegion")]
            public string AWSRegion { get; set; }
            public class S3Data
            {
                [JsonPropertyName("bucket")]
                public Bucket ObjectsBucket { get; set; }
                [JsonPropertyName("object")]
                public StoredObject ObjectData { get; set; }
                public class Bucket
                {
                    [JsonPropertyName("name")]
                    public string Name { get; set; }
                }
                public class StoredObject
                {
                    [JsonPropertyName("key")]
                    public string Key { get; set; }
                }
            }
        }
    }
}
