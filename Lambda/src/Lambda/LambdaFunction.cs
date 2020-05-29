
using Amazon;
using Amazon.Lambda.Core;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using Lambda.S3;
using Lambda.Models;
using Lambda.Parsing;
using Lambda.Database;
using System.Linq;

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
        [SuppressMessage("Style", "IDE0060:Remove unused parameter",
            Justification = "Necesarry for lambda to work")]
        public void FunctionHandler(S3NotificationEvent input, ILambdaContext context)
        {
            HandlerAsync(input).Wait();
        }
        private async Task HandlerAsync(S3NotificationEvent input)
        {
            var data = input?.GetFileData();
            var retriever = new S3ObjectRetriever(data.Value.objectsKey,
                RegionEndpoint.GetBySystemName(data.Value.region));
            using var connection = new DbConnection();
            await connection.GetConnection().OpenAsync().ConfigureAwait(false);
            var logSaver = new LogSaver(connection.GetConnection());
            string logId = await logSaver.GetAssociatedLogIdAsync(data.Value.objectsKey).ConfigureAwait(false);
            if (!String.IsNullOrEmpty(logId))
            {
                var entries = retriever.GetObjectDataAsync().Select(
                    logLine => EntryParser.ProcessLogLine(logLine));
                await logSaver.SaveLogsAsync(logId, entries).ConfigureAwait(false);
            }
            else
            {
                throw new Exception($"Log with given object name " +
                    $"({data.Value.objectsKey}) has no associated record in database");
            }
        }




    }
}
