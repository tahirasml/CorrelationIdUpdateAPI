using CorrelationIdUpdateAPI.Cryptography;
using CorrelationIdUpdateAPI.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CorrelationIdUpdateAPI.Services
{
    public class RecordService
    {
        private readonly string _connectionString;

        public RecordService(IConfiguration configuration)
        {
            // Retrieve the encrypted connection string from the configuration
            var encryptedConnectionString = configuration.GetConnectionString("DefaultConnection");

            // Decrypt the connection string
            _connectionString = ConnectionStringSecurity.Decrypt(encryptedConnectionString);
        }

        public async Task<Record> GetRecordByCorrelationIdAsync(string correlationId)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT * FROM Records WHERE CorrelationId = @CorrelationId";
            return await connection.QueryFirstOrDefaultAsync<Record>(query, new { CorrelationId = correlationId });
        }

        public async Task<bool> UpdateRecordStatusAsync(string correlationId, string status)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "UPDATE Records SET Status = @Status WHERE CorrelationId = @CorrelationId";
            var rowsAffected = await connection.ExecuteAsync(query, new { CorrelationId = correlationId, Status = status });
            return rowsAffected > 0;
        }
    }
}
