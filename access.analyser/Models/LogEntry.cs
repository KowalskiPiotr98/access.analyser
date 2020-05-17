using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace access.analyser.Models
{
    public class LogEntry
    {
        public enum RequestType
        {
            Other,
            GET,
            HEAD,
            POST,
            PUT,
            DELETE,
            CONNECT,
            OPTIONS,
            TRACE,
            PATCH
        }

        [Key]
        [DatabaseGenerated (DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        /// <summary>
        /// Reference to log file from which this entry was read
        /// </summary>
        [Required]
        public string LogId { get; set; }
        public Log Log { get; set; }
        /// <summary>
        /// IP from which the request was sent
        /// </summary>
        [Required]
        [Display (Name = "Client IP")]
        public string ClientIp { get; set; }
        /// <summary>
        /// Date and time of the request
        /// </summary>
        [Required]
        [Display (Name = "Request date")]
        [DataType (DataType.DateTime)]
        public DateTime RequestTime { get; set; }
        /// <summary>
        /// HTTP request method
        /// </summary>
        [Required]
        [Display (Name = "HTTP method")]
        public RequestType Method { get; set; }
        /// <summary>
        /// Resource requested, together with query string
        /// </summary>
        /// <remarks>
        /// Might sometimes be empty.
        /// Default field to put client's request in, when it's not a correct HTTP request.
        /// </remarks>
        [Required]
        public string Resource { get; set; }
        /// <summary>
        /// Response code returned by the server
        /// </summary>
        [Required]
        [Display (Name = "Response code")]
        public int ResponseCode { get; set; }
        /// <summary>
        /// User Agent string provided in the request
        /// </summary>
        [Display (Name = "User agent")]
        public string UserAgent { get; set; }
        /// <summary>
        /// Raw line from the log
        /// </summary>
        [Required]
        [Display (Name = "Raw log entry")]
        public string RawEntry { get; set; }
    }
}
