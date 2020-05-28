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
        /// Time when log was uploaded
        /// </summary>
        [Required]
        [DataType (DataType.DateTime)]
        public DateTime UploadDate { get; set; }
        /// <summary>
        /// S3 bucket object key
        /// </summary>
        [Required]
        public string S3ObjectKey { get; set; }

        public List<LogEntry> LogEntries { get; set; }
    }
}
