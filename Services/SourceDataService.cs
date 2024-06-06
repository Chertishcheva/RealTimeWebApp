using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RealTimeWebApp.Models;
using Microsoft.Data.SqlClient;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace RealTimeWebApp.Services
{
    public class SourceDataService
    {
        public String connectionString { get; private set; }
        private SqlConnection connection;

        private readonly ILogger<SourceDataService> logger;
        public SourceDataService(ILogger<SourceDataService> _logger, IConfiguration configuration) {
            logger = _logger;
            connectionString = configuration.GetConnectionString("SourceDB").Replace("[DataDir]", Environment.CurrentDirectory);
        }

        private void createConnection() {
            if(connection == null)
                connection = new SqlConnection(connectionString);
        }

        public List<ExampleDataModel> getAll(){
            createConnection();

            List<ExampleDataModel> existingData = new List<ExampleDataModel>();
            string SqlRequest = "SELECT * FROM DataTable";

            try
            {
                connection.Open();

                SqlCommand command = new SqlCommand(SqlRequest, connection);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read()){
                    existingData.Add(new ExampleDataModel(int.Parse(reader["id"].ToString()), reader["timeOfUpload"].ToString(), int.Parse(reader["data"].ToString())));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Some error has occurred while working with db" + e.Message);
            }
            finally {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }

            return existingData;
        }
    }
}
