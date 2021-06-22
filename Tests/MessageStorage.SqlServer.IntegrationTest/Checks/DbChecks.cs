using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;

namespace MessageStorage.SqlServer.IntegrationTest.Checks
{
    public class DbChecks
    {
        private readonly string _connectionString;
        private readonly string _schema;

        public DbChecks(string connectionString, string schema)
        {
            _connectionString = connectionString;
            _schema = schema;
        }

        public async Task<bool> CheckMessageIsExistAsync(Guid id)
        {
            bool result = await CheckIsExistAsync("messages", id);
            return result;
        }

        public async Task<bool> CheckJobIsExistAsync(Guid id)
        {
            bool result = await CheckIsExistAsync("jobs", id);
            return result;
        }

        private async Task<bool> CheckIsExistAsync(string tableName, Guid id)
        {
            await using var connection = new SqlConnection(_connectionString);
            string countScript = $"select count(*) from {_schema}.{tableName} where id = @id";
            var count = await connection.ExecuteScalarAsync<int>(countScript, new {id = id});
            return count > 0;
        }
    }
}