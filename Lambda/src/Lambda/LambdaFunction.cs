
using Amazon;
using Amazon.Lambda.Core;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda
{
    public class LambdaFunction
    {
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        public async Task<string> FunctionHandler(S3NotificationEvent input, ILambdaContext context)
        {
            var data = input?.GetFileData();
            var retriever = new S3ObjectRetriever(data.Value.bucketName, data.Value.objectsKey,
                RegionEndpoint.GetBySystemName(data.Value.region));
            StringBuilder result = new StringBuilder();
            await foreach (string line in retriever.GetObjectDataAsync())
            {
                var logEntry = EntryParser.ProcessLogLine(line);
                result.Append($"log entry: {logEntry.ClientIp}, {logEntry.Resource}, {logEntry.ResponseCode}\n");
            }
            return result.ToString();
            
        }

        

    }
}
