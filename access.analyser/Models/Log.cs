using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        /// URL to log in S3 bucket
        /// </summary>
        [Required]
        public Uri LogS3Url { get; set; }
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
            using var client = new AmazonS3Client (RegionEndpoint.USEast1);
            var delObject = new DeleteObjectRequest ()
            {
                BucketName = bucketName,
                Key = S3ObjectKey
            };
            _ = await client.DeleteObjectAsync (delObject);
        }
    }
}
