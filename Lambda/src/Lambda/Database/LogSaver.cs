using Lambda.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Lambda.Database
{
    public class LogSaver
    {
        readonly NpgsqlConnection connection;
        public LogSaver(NpgsqlConnection connection)
        {
            this.connection = connection;
        }
        public async Task<string> GetAssociatedLogIdAsync(string objectKey)
        {
            using var command = new NpgsqlCommand("SELECT \"Id\" FROM \"Logs\"" +
                " WHERE \"S3ObjectKey\" = @objectKey;", connection);
            command.Parameters.AddWithValue("@objectKey", objectKey);
            Console.WriteLine(command.CommandText);
            using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            if (!await reader.ReadAsync().ConfigureAwait(false)) return null;
            return reader.GetString(0);

        }
        public async Task SaveLogsAsync(string logId, IAsyncEnumerable<LogEntry> entries)
        {
            Contract.Requires(entries != null);
            var transaction = await connection.BeginTransactionAsync().ConfigureAwait(false);
            using var command = new NpgsqlCommand("INSERT INTO \"LogEntries\"" +
                "(\"Id\", \"LogId\", \"ClientIp\", \"RequestTime\", \"Method\"," +
                " \"Resource\", \"ResponseCode\", \"UserAgent\", \"RawEntry\")" +
                " VALUES (@Id, @LogId, @ClientIp, @RequestTime," +
                " @Method, @Resource, @ResponseCode, @UserAgent, @RawEntry);", connection);
            command.Parameters.Add("@Id", NpgsqlTypes.NpgsqlDbType.Text);
            command.Parameters.Add("@LogId", NpgsqlTypes.NpgsqlDbType.Text);
            command.Parameters.Add("@ClientIp", NpgsqlTypes.NpgsqlDbType.Text);
            command.Parameters.Add("@RequestTime", NpgsqlTypes.NpgsqlDbType.Timestamp);
            command.Parameters.Add("@Method", NpgsqlTypes.NpgsqlDbType.Integer);
            command.Parameters.Add("@Resource", NpgsqlTypes.NpgsqlDbType.Text);
            command.Parameters.Add("@ResponseCode", NpgsqlTypes.NpgsqlDbType.Integer);
            command.Parameters.Add("@UserAgent", NpgsqlTypes.NpgsqlDbType.Text);
            command.Parameters.Add("@RawEntry", NpgsqlTypes.NpgsqlDbType.Text);
            Console.WriteLine("Start of saving entries");
            //TODO foreach doesn't start
            await foreach (var entry in entries)
            {
                Console.WriteLine($"entry {entry.ClientIp}");
                command.Parameters["@Id"].NpgsqlValue = Guid.NewGuid();
                command.Parameters["@LogId"].NpgsqlValue = logId;
                command.Parameters["@ClientIp"].NpgsqlValue = entry.ClientIp;
                command.Parameters["@RequestTime"].NpgsqlValue = entry.RequestTime;
                command.Parameters["@Method"].NpgsqlValue = entry.Method;
                command.Parameters["@Resource"].NpgsqlValue = entry.Resource;
                command.Parameters["@ResponseCode"].NpgsqlValue = entry.ResponseCode;
                command.Parameters["@UserAgent"].NpgsqlValue
                    = entry.UserAgent == "-" ? "NULL" : $"'{entry.UserAgent}'";
                command.Parameters["@RawEntry"].NpgsqlValue = entry.RawEntry;
                await command.PrepareAsync().ConfigureAwait(false);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                Console.WriteLine(command.CommandText);
            }
            await transaction.CommitAsync().ConfigureAwait(false);
        }
    }
}
