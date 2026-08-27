using MySqlConnector;

namespace CoffeeHouse.API.Data
{
    public class MySqlConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public MySqlConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public MySqlConnection CreateConnection()
        {
            var connectionString =
                _configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new Exception(
                    "Không tìm thấy chuỗi kết nối MySQL: DefaultConnection"
                );
            }

            return new MySqlConnection(connectionString);
        }
    }
}