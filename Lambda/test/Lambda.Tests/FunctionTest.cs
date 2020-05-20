using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

using Lambda;
using Lambda.Models;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.ConstrainedExecution;
using System.Collections;

namespace Lambda.Tests
{
    public class FunctionTest
    {
        //[Fact]
        //public void TestToUpperFunction()
        //{

        //    // Invoke the lambda function and confirm the string was upper cased.
        //    var function = new LambdaFunction();
        //    var context = new TestLambdaContext();
        //    var upperCase = function.FunctionHandler("hello world", context);

        //    Assert.Equal("HELLO WORLD", upperCase);
        //}
        [Theory]
        [InlineData(
            "208.80.194.27 - - [15/May/2020:03:34:59 +0200] \"GET /bins/hoho.mpsl HTTP/1.0\"" +
            " 444 0 \" - \" \"Mozilla / 5.0(Windows NT 5.1; U; en) Opera 8.01\" \"-\"",
            "208.80.194.27"
            )]
        [InlineData(
            @"184.154.47.2 - - [18/May/2020:06:20:17 +0200] " +
            @"""\x16\x03\x01\x00\x9A\x01\x00\x00\x96\x03\x03q - V\xE1\x0F\x17\xA7\xF6\xAB\x83\x92\x5C\" +
            @"xFARZc\x16(\x1C\xDE\xCA)\xB6)\xEB1\xEE\xEB\x08\x17\x04\xAD\x00\x00\x1A\xC0/\xC0+\xC0\x11\" +
            @"xC0\x07\xC0\x13\xC0\x09\xC0\x14\xC0"" 400 173 ""-"" ""-"" ""-""",
            "184.154.47.2"
            )]
        public void TestProcessLogLineMatchIP(string logLine, string expectedIP)
        {
            var logEntry = LambdaFunction.ProcessLogLine(logLine);
            Assert.Equal(expectedIP,logEntry.ClientIp);
        }
        [Theory]
        [InlineData(
            "208.80.194.27 - - [15/May/2020:03:34:59 +0200] \"GET /bins/hoho.mpsl HTTP/1.0\"" +
            " 444 0 \" - \" \"Mozilla / 5.0(Windows NT 5.1; U; en) Opera 8.01\" \"-\"",
            444
            )]
        [InlineData(
            @"184.154.47.2 - - [18/May/2020:06:20:17 +0200] " +
            @"""\x16\x03\x01\x00\x9A\x01\x00\x00\x96\x03\x03q - V\xE1\x0F\x17\xA7\xF6\xAB\x83\x92\x5C\" +
            @"xFARZc\x16(\x1C\xDE\xCA)\xB6)\xEB1\xEE\xEB\x08\x17\x04\xAD\x00\x00\x1A\xC0/\xC0+\xC0\x11\" +
            @"xC0\x07\xC0\x13\xC0\x09\xC0\x14\xC0"" 400 173 ""-"" ""-"" ""-""",
            400
            )]

        public void TestProcessLogLineMatchResponseCode(string logLine, int expectedResponseCode)
        {
            var logEntry = LambdaFunction.ProcessLogLine(logLine);
            Assert.Equal(expectedResponseCode, logEntry.ResponseCode);
        }
        [Theory]
        [InlineData(
            "208.80.194.27 - - [15/May/2020:03:34:59 +0200] \"GET /bins/hoho.mpsl HTTP/1.0\"" +
            " 444 0 \" - \" \"Mozilla / 5.0(Windows NT 5.1; U; en) Opera 8.01\" \"-\""
            )]
        [InlineData(
            @"184.154.47.2 - - [18/May/2020:06:20:17 +0200] " +
            @"""\x16\x03\x01\x00\x9A\x01\x00\x00\x96\x03\x03q - V\xE1\x0F\x17\xA7\xF6\xAB\x83\x92\x5C\" +
            @"xFARZc\x16(\x1C\xDE\xCA)\xB6)\xEB1\xEE\xEB\x08\x17\x04\xAD\x00\x00\x1A\xC0/\xC0+\xC0\x11\" +
            @"xC0\x07\xC0\x13\xC0\x09\xC0\x14\xC0"" 400 173 ""-"" ""-"" ""-"""
            )]

        public void TestProcessLogLineMatchRawEntry(string logLine)
        {
            var logEntry = LambdaFunction.ProcessLogLine(logLine);
            Assert.Equal(logLine, logEntry.RawEntry);
        }
        [Theory]
        [InlineData(
            "208.80.194.27 - - [15/May/2020:03:34:59 +0200] \"GET /bins/hoho.mpsl HTTP/1.0\"" +
            " 444 0 \" - \" \"Mozilla / 5.0(Windows NT 5.1; U; en) Opera 8.01\" \"-\"",
            "Mozilla / 5.0(Windows NT 5.1; U; en) Opera 8.01"
            )]
        [InlineData(
            @"184.154.47.2 - - [18/May/2020:06:20:17 +0200] " +
            @"""\x16\x03\x01\x00\x9A\x01\x00\x00\x96\x03\x03q - V\xE1\x0F\x17\xA7\xF6\xAB\x83\x92\x5C\" +
            @"xFARZc\x16(\x1C\xDE\xCA)\xB6)\xEB1\xEE\xEB\x08\x17\x04\xAD\x00\x00\x1A\xC0/\xC0+\xC0\x11\" +
            @"xC0\x07\xC0\x13\xC0\x09\xC0\x14\xC0"" 400 173 ""-"" ""-"" ""-""",
            null
            )]

        public void TestProcessLogLineMatchUserAgent(string logLine, string expectedUserAgent)
        {
            var logEntry = LambdaFunction.ProcessLogLine(logLine);
            Assert.Equal(expectedUserAgent, logEntry.UserAgent);
        }
        [Theory]
        [InlineData(
            "208.80.194.27 - - [15/May/2020:03:34:59 +0200] \"GET /bins/hoho.mpsl HTTP/1.0\"" +
            " 444 0 \" - \" \"Mozilla / 5.0(Windows NT 5.1; U; en) Opera 8.01\" \"-\"",
            LogEntry.RequestType.GET
            )]
        [InlineData(
            @"184.154.47.2 - - [18/May/2020:06:20:17 +0200] " +
            @"""\x16\x03\x01\x00\x9A\x01\x00\x00\x96\x03\x03q - V\xE1\x0F\x17\xA7\xF6\xAB\x83\x92\x5C\" +
            @"xFARZc\x16(\x1C\xDE\xCA)\xB6)\xEB1\xEE\xEB\x08\x17\x04\xAD\x00\x00\x1A\xC0/\xC0+\xC0\x11\" +
            @"xC0\x07\xC0\x13\xC0\x09\xC0\x14\xC0"" 400 173 ""-"" ""-"" ""-""",
            LogEntry.RequestType.Other
            )]

        public void TestProcessLogLineMatchMethod(string logLine, LogEntry.RequestType expectedMethod)
        {
            var logEntry = LambdaFunction.ProcessLogLine(logLine);
            Assert.Equal(expectedMethod, logEntry.Method);
        }
        [Theory]
        [InlineData(
            "208.80.194.27 - - [15/May/2020:03:34:59 +0200] \"GET /bins/hoho.mpsl HTTP/1.0\"" +
            " 444 0 \" - \" \"Mozilla / 5.0(Windows NT 5.1; U; en) Opera 8.01\" \"-\"",
            "/bins/hoho.mpsl"
            )]
        [InlineData(
            @"184.154.47.2 - - [18/May/2020:06:20:17 +0200] " +
            @"""\x16\x03\x01\x00\x9A\x01\x00\x00\x96\x03\x03q - V\xE1\x0F\x17\xA7\xF6\xAB\x83\x92\x5C\" +
            @"xFARZc\x16(\x1C\xDE\xCA)\xB6)\xEB1\xEE\xEB\x08\x17\x04\xAD\x00\x00\x1A\xC0/\xC0+\xC0\x11\" +
            @"xC0\x07\xC0\x13\xC0\x09\xC0\x14\xC0"" 400 173 ""-"" ""-"" ""-""",
            @"\x16\x03\x01\x00\x9A\x01\x00\x00\x96\x03\x03q - V\xE1\x0F\x17\xA7\xF6\xAB\x83\x92\x5C\" +
            @"xFARZc\x16(\x1C\xDE\xCA)\xB6)\xEB1\xEE\xEB\x08\x17\x04\xAD\x00\x00\x1A\xC0/\xC0+\xC0\x11\" +
            @"xC0\x07\xC0\x13\xC0\x09\xC0\x14\xC0"
            )]

        public void TestProcessLogLineMatchResource(string logLine, string expectedResource)
        {
            var logEntry = LambdaFunction.ProcessLogLine(logLine);
            Assert.Equal(expectedResource, logEntry.Resource);
        }

        private class TestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[]{
                    "208.80.194.27 - - [15/May/2020:03:34:59 +0200]" +
                    " \"GET /bins/hoho.mpsl HTTP/1.0\"" +
                    " 444 0 \" - \" \"Mozilla / 5.0(Windows NT 5.1; U; en) Opera 8.01\" \"-\"",
                    new DateTime(year: 2020, month: 5, day: 15, hour: 1, minute: 34, second: 59,
                            DateTimeKind.Utc) 
                };
                yield return new object[]
                {
                    @"184.154.47.2 - - [18/May/2020:06:20:17 +0200] " +
                    @"""\x16\x03\x01\x00\x9A\x01\x00\x00\x96\x03\x03q - V\xE1\x0F\x17\xA7\xF6\xAB\x83\x92\x5C\" +
                    @"xFARZc\x16(\x1C\xDE\xCA)\xB6)\xEB1\xEE\xEB\x08\x17\x04\xAD\x00\x00\x1A\xC0/\xC0+\xC0\x11\" +
                    @"xC0\x07\xC0\x13\xC0\x09\xC0\x14\xC0"" 400 173 ""-"" ""-"" ""-""",
                    new DateTime(year: 2020, month: 5, day: 18, hour: 4, minute: 20, second: 17,
                    DateTimeKind.Utc)
                };
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory]
        [ClassData(typeof(TestData))]
        public void TestProcessLogLineMatchRequestTime(string logLine, DateTime expectedRequestTime)
        {
            var logEntry = LambdaFunction.ProcessLogLine(logLine);
            Assert.Equal(expectedRequestTime, logEntry.RequestTime);
        }
    }
}
