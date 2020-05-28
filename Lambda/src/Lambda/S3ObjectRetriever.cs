﻿using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Lambda
{
    class S3ObjectRetriever
    {
        private readonly string bucketName;
        private readonly string keyName;
        private static IAmazonS3 client;

        public S3ObjectRetriever(string bucketName, string keyName, RegionEndpoint bucketRegion)
        {
            this.bucketName = bucketName;
            this.keyName = keyName;
            client = new AmazonS3Client(bucketRegion);
        }

        public async IAsyncEnumerable<string> GetObjectDataAsync()
        {
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = keyName
            };
            using GetObjectResponse response = await client.GetObjectAsync(request).ConfigureAwait(true);
            using Stream responseStream = response.ResponseStream;
            using StreamReader reader = new StreamReader(responseStream);
            while(!reader.EndOfStream)
                yield return reader.ReadLine();
        }
    }
}
