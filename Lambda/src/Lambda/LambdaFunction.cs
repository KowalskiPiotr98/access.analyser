using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

using Amazon.Lambda.Core;
using Lambda.Models;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda
{
    public class LambdaFunction
    {
        private static readonly Regex logEntryRegex = new Regex(
            @"(?<ip>[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}) - - " +
            @"\[(?<requestTime>[^\]]+)\] " +
            @"""(?<query>[^""]*)"" " + //"" matches " in verbatim strings
            @"(?<statusCode>\d{3}) \d+ ""[^""]*"" " +
            @"""(?<userAgent>[^""]*)"" ""[^""]*""", RegexOptions.Compiled);
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public string FunctionHandler(string input, ILambdaContext context)
        {
            return input;
        }

        public static LogEntry ProcessLogLine(string logLine)
        {
            LogEntry l = new LogEntry();
            var match = logEntryRegex.Match(logLine);
            if (!match.Success) return null;
            
            l.ClientIp = match.Groups["ip"].Value;
            l.RequestTime = ProcessDate(match.Groups["requestTime"].Value);
            l.RawEntry = logLine;
            (l.Method, l.Resource) = ProcessQuery(match.Groups["query"].Value);
            l.ResponseCode = int.Parse(match.Groups["statusCode"].Value,
                System.Globalization.CultureInfo.CurrentCulture);
            l.UserAgent = match.Groups["userAgent"].Value;
            if (l.UserAgent == "-") l.UserAgent = null;
            return l;
        }

        private static (LogEntry.RequestType method, string resource) ProcessQuery(string request)
        {
            var splitRequest = request.Split(' ');
            if (splitRequest.Length != 3 || !IsRequestMethod(splitRequest[0]))
                return (LogEntry.RequestType.Other, request);
            else
                return (AsRequestType(splitRequest[0]), splitRequest[1]);
        }
        private static bool IsRequestMethod(string s)
        {
            return Enum.IsDefined(typeof(LogEntry.RequestType), s) && s != "Other";
        }
        private static LogEntry.RequestType AsRequestType(string s)
        {
            return (LogEntry.RequestType)Enum.Parse(typeof(LogEntry.RequestType), s);
        }

        private static DateTime ProcessDate(string date)
        {
            var dto = DateTimeOffset.ParseExact(date, "dd/MMMM/yyyy:HH:mm:ss K",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal);
            return dto.UtcDateTime;
        }

    }
}
