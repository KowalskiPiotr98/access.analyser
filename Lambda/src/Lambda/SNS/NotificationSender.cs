using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace Lambda.SNS
{
    public static class NotificationSender
    {
        public static bool IsNotificationNecessary(string objectsKey)
        {
            Contract.Requires(objectsKey != null);
            var trimmedKey = objectsKey.Substring(0, objectsKey.Length - 4);
            trimmedKey = trimmedKey.Substring(trimmedKey.LastIndexOf('-')+1);
            int logNumToday = Int32.Parse(trimmedKey,CultureInfo.InvariantCulture);
            var logNumLimitStr = Environment.GetEnvironmentVariable("LOGLIMIT");
            var logNumLimit = logNumLimitStr is null ? 10 :
                Int32.Parse(logNumLimitStr, CultureInfo.InvariantCulture);
            Console.WriteLine($"Request number {logNumToday}, Notification Threshold is {logNumLimit}");
            return logNumToday == logNumLimit;

        }
        public static async Task TryPublishNotification(string objectsKey)
        {
            string topicArn = Environment.GetEnvironmentVariable("TOPICARN");
            using var client = new AmazonSimpleNotificationServiceClient(Amazon.RegionEndpoint.USEast1);
            var request = new PublishRequest
            {
                TopicArn = topicArn,
                Message = "User sent too many logs today, objectKey" +
                " ({userId}-{TodaysDate}-{#of log sent today}.log)=" + objectsKey,
                Subject = "Access.analyser - too many logs"
            };
            try
            {
                await client.PublishAsync(request).ConfigureAwait(false);
                Console.WriteLine("Successfully published notification");
            }
            catch(AmazonClientException e)
            {
                Console.WriteLine("failed to send message" + e);
            }
            catch (AmazonSimpleNotificationServiceException e)
            {
                Console.WriteLine("failed to send message" + e);
            }
        }
    }
}
