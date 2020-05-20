using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lambda.Models
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
        public DateTime UploadDate { get; set; }

        public List<LogEntry> LogEntries { get; set; }
    }
}
