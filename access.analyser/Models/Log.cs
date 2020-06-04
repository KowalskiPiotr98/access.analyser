using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace access.analyser.Models
{
    public class Log
    {
        [Key]
        [DatabaseGenerated (DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        /// <summary>
        /// Reference to user uploading the log
        /// </summary>
        [Required]
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
        /// <summary>
        /// Time when log was uploaded
        /// </summary>
        [Required]
        [DataType (DataType.DateTime)]
        [Display (Name = "Upload date")]
        public DateTime UploadDate { get; set; }
        /// <summary>
        /// S3 bucket object key
        /// </summary>
        [Required]
        public string S3ObjectKey { get; set; }

        public List<LogEntry> LogEntries { get; set; }

        private static readonly RegionEndpoint region = RegionEndpoint.USEast1;

        internal static IQueryable<Log> SelectByDate (IQueryable<Log> list, DateTime date) => from l in list where l.UploadDate.Date == date.Date select l;
        internal static IQueryable<Log> GetAuthorisedLogs (IQueryable<Log> list, string userId, bool isAdmin)
        {
            if (isAdmin)
            {
                return list;
            }
            return from l in list where l.UserId == userId select l;
        }

        internal virtual async Task DeleteS3Object (string bucketName)
        {
            using var client = new AmazonS3Client (region);
            var delObject = new DeleteObjectRequest ()
            {
                BucketName = bucketName,
                Key = S3ObjectKey
            };
            _ = await client.DeleteObjectAsync (delObject);
        }

        internal virtual async Task<FileStreamResult> GetDownloadFileStream (string bucketName)
        {
            using var client = new AmazonS3Client (region);
            var getObject = new GetObjectRequest ()
            {
                BucketName = bucketName,
                Key = S3ObjectKey
            };
            try
            {
                var response = await client.GetObjectAsync (getObject);
                return new FileStreamResult (response.ResponseStream, new Microsoft.Net.Http.Headers.MediaTypeHeaderValue ("text/plain"))
                {
                    FileDownloadName = S3ObjectKey
                };
            }
            catch (AmazonS3Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Upload a file to S3 bucket
        /// </summary>
        /// <param name="bucketName">S3 bucket name from appsettings.json</param>
        /// <param name="fileStream">File stream from IFormFile</param>
        /// <returns>True if file uploaded correctly, false otherwise</returns>
        /// <exception cref="AmazonS3Exception">Throws when file upload failes</exception>
        internal virtual async Task<bool> UploadFile (string bucketName, Stream fileStream)
        {
            using var client = new AmazonS3Client (region);
            var sendObject = new PutObjectRequest ()
            {
                BucketName = bucketName,
                Key = S3ObjectKey,
                InputStream = fileStream
            };
            var response = await client.PutObjectAsync (sendObject);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
    }
}
