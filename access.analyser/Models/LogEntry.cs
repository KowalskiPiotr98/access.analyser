using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace access.analyser.Models
{
    public class LogEntry
    {
        public enum RequestType
        {
            other,
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
        public string ClientIp { get; set; }
        /// <summary>
        /// Date and time of the request
        /// </summary>
        [Required]
        [DataType (DataType.DateTime)]
        public DateTime RequestTime { get; set; }
        /// <summary>
        /// HTTP request method
        /// </summary>
        [Required]
        public RequestType Method { get; set; }
        /// <summary>
        /// Resource requested, together with query string
        /// </summary>
        /// <remarks>
        /// Might sometimes be empty.
        /// Default field to put client's request in, when it's not a correct HTTP request.
        /// </remarks>
        public string Resource { get; set; }
        /// <summary>
        /// Response code returned by the server
        /// </summary>
        [Required]
        public int ResponseCode { get; set; }
        /// <summary>
        /// User Agent string provided in the request
        /// </summary>
        public string UserAgent { get; set; }
        /// <summary>
        /// Raw line from the log
        /// </summary>
        [Required]
        public string RawEntry { get; set; }
    }
}
