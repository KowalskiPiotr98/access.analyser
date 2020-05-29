using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Lambda.S3
{
    class S3ObjectRetriever
    {
        private readonly string accessPoint = Environment.GetEnvironmentVariable("S3ACCESSPOINT");
        private readonly string keyName;
        private static IAmazonS3 client;

        public S3ObjectRetriever(string keyName, RegionEndpoint bucketRegion)
        {
            this.keyName = keyName;
            client = new AmazonS3Client(bucketRegion);
        }

        public async IAsyncEnumerable<string> GetObjectDataAsync()
        {
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = accessPoint,
                Key = keyName
            };
            using GetObjectResponse response = await client.GetObjectAsync(request).ConfigureAwait(false);
            using Stream responseStream = response.ResponseStream;
            using StreamReader reader = new StreamReader(responseStream);
            while (!reader.EndOfStream)
                yield return reader.ReadLine();
        }
    }
}
