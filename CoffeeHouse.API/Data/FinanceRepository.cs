using MySqlConnector;
using CoffeeHouse.API.Models;

namespace CoffeeHouse.API.Data
{
    public class FinanceRepository
    {
        private readonly string _connectionString;

        public FinanceRepository(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new Exception("Không tìm thấy DefaultConnection");
        }

        // Lấy danh sách giao dịch
        public async Task<List<Finance>> GetAllAsync()
        {
            var result = new List<Finance>();

            await using var connection =
                new MySqlConnection(_connectionString);

            await connection.OpenAsync();

            const string sql = @"
                SELECT
                    finance_id,
                    type,
                    description,
                    amount,
                    finance_date,
                    status,
                    note
                FROM finance
                ORDER BY finance_date DESC;
            ";

            await using var command =
                new MySqlCommand(sql, connection);

            await using var reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new Finance
                {
                    FinanceId = reader.GetInt64("finance_id"),

                    Type = reader.GetString("type"),

                    Description =
                        reader.GetString("description"),

                    Amount =
                        reader.GetDecimal("amount"),

                    FinanceDate =
                        reader.GetDateTime("finance_date"),

                    Status =
                        reader.GetString("status"),

                    Note =
                        reader.IsDBNull(
                            reader.GetOrdinal("note"))
                        ? ""
                        : reader.GetString("note")
                });
            }

            return result;
        }


        // Thêm giao dịch
        public async Task<long> CreateAsync(Finance finance)
        {
            await using var connection =
                new MySqlConnection(_connectionString);

            await connection.OpenAsync();

            const string sql = @"
                INSERT INTO finance
                (
                    type,
                    description,
                    amount,
                    finance_date,
                    status,
                    note
                )
                VALUES
                (
                    @type,
                    @description,
                    @amount,
                    @finance_date,
                    @status,
                    @note
                );

                SELECT LAST_INSERT_ID();
            ";

            await using var command =
                new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@type",
                finance.Type);

            command.Parameters.AddWithValue(
                "@description",
                finance.Description);

            command.Parameters.AddWithValue(
                "@amount",
                finance.Amount);

            command.Parameters.AddWithValue(
                "@finance_date",
                finance.FinanceDate);

            command.Parameters.AddWithValue(
                "@status",
                finance.Status);

            command.Parameters.AddWithValue(
                "@note",
                finance.Note);

            var id =
                await command.ExecuteScalarAsync();

            return Convert.ToInt64(id);
        }


        // Xóa giao dịch
        public async Task<bool> DeleteAsync(long id)
        {
            await using var connection =
                new MySqlConnection(_connectionString);

            await connection.OpenAsync();

            const string sql = @"
                DELETE FROM finance
                WHERE finance_id = @id;
            ";

            await using var command =
                new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@id",
                id);

            int rows =
                await command.ExecuteNonQueryAsync();

            return rows > 0;
        }
    }
}