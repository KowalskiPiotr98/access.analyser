using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Data.SqlClient;
using Amazon.S3.Transfer;
using System.Threading.Tasks;
using System.IO;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Npgsql;
using System.Data;

namespace Lambda.Database
{
    //https://www.1strategy.com/blog/2019/02/06/connecting-to-an-rds-database-with-lambda/
    public sealed class DbConnection : IDisposable
    {
        string server = Environment.GetEnvironmentVariable("DB_ENDPOINT");
        string database = Environment.GetEnvironmentVariable("DATABASE");
        string username = Environment.GetEnvironmentVariable("USER");
        string pwd = Environment.GetEnvironmentVariable("PASSWORD");
        string connectionString;
        NpgsqlConnection connection;

        public DbConnection()
        {
            connectionString = $"Host={server};Username={username};Password={pwd};Database={database};";
            connection = new NpgsqlConnection(connectionString);
        }

        public void Dispose()
        {
            connection.Dispose();
        }

        public NpgsqlConnection GetConnection()
        {
            return connection;
        }
        private static async Task<string> DecodeEnvVar(string envVarName)
        {
            // Retrieve env var text
            var encryptedBase64Text = Environment.GetEnvironmentVariable(envVarName);
            // Convert base64-encoded text to bytes
            var encryptedBytes = Convert.FromBase64String(encryptedBase64Text);
            // Construct client
            using var client = new AmazonKeyManagementServiceClient(Amazon.RegionEndpoint.USEast1);
            // Construct request
            var decryptRequest = new DecryptRequest
            {
                CiphertextBlob = new MemoryStream(encryptedBytes),
            };
            // Call KMS to decrypt data
            var response = await client.DecryptAsync(decryptRequest).ConfigureAwait(false);
            using var plaintextStream = response.Plaintext;
            // Get decrypted bytes
            var plaintextBytes = plaintextStream.ToArray();
            // Convert decrypted bytes to ASCII text
            var plaintext = Encoding.UTF8.GetString(plaintextBytes);
            return plaintext;
        }
    }
}
