using access.analyser.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

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

        public static async Task<List<LogEntry>> FilterEntries (IQueryable<LogEntry> list, bool isAdmin, string userId, DateTime? dateFrom, DateTime? dateTo, LogEntry.RequestType? type, string ip, string resource, string response, string agent)
        {
            if (!isAdmin)
            {
                list = from l in list where l.Log.UserId == userId select l;
            }
            if (dateFrom.HasValue)
            {
                list = from l in list where l.RequestTime >= dateFrom.Value select l;
            }
            if (dateTo.HasValue)
            {
                list = from l in list where l.RequestTime <= dateTo.Value select l;
            }
            if (type.HasValue)
            {
                list = from l in list where l.Method == type.Value select l;
            }
            if (ip != null)
            {
                ip = ip.Trim ();
                var ips = ip.Split (" ");
                list = from l in list where ips.Contains (l.ClientIp) select l;
            }
            if (resource != null)
            {
                resource = resource.Trim ();
                var resources = resource.Split (" ");
                list = from l in list where resources.Contains (l.Resource) select l;
            }
            if (response != null)
            {
                response = response.Trim ();
                var responses = response.Split (" ");
                list = from l in list where responses.Contains (l.ResponseCode.ToString ()) select l;
            }
            if (agent != null)
            {
                agent = agent.Trim ();
                var agents = new List<string> ();
                int index = agent.IndexOf ('\"');
                var listTempAsync = list.ToListAsync ();
                while (index >= 0)
                {
                    agent = agent.Remove (0, index + 1);
                    var endIndex = agent.IndexOf ('\"');
                    agents.Add (agent.Substring (0, endIndex));
                    agent = agent.Remove (0, endIndex + 1);
                    index = agent.IndexOf ('\"');
                }
                var listTemp = from l in await listTempAsync where agents.Any (a => l.UserAgent.Contains (a)) select l;
                list = listTemp.AsQueryable ();
            }
            return list.ToList ();
        }
    }
}
