using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Collections.Generic;
using MCT.extra;

namespace MCT.functions
{
    public static class DagenFunctions
    {
        [FunctionName("GetDagen")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "days")] HttpRequest req,
            ILogger log)
        {
            string connectionString = Environment.GetEnvironmentVariable("ConnectionString");
            List<string> dagen = new List<string>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "SELECT DISTINCT DagVanDeWeek FROM Bezoekers";
                    SqlDataReader reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var dag = reader["DagVanDeWeek"].ToString();
                        dagen.Add(dag);
                    }
                }
            }

            return new OkObjectResult(dagen);
        }
        [FunctionName("Getvisitors")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "visitors/Day")] HttpRequest req,
            string Day,
            ILogger log)
        {
            try
            {
                string connectionString = Environment.GetEnvironmentVariable("ConnectionsString");
                List<Visit> visits = new List<Visit>();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "SELECT Tijdstip, AantalBezoekers FROM Bezoekers WHERE DagVanDeWaak = @dag";
                        command.Parameters.AddWithValue("@dag", Day);

                        SqlDataReader reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            var visit = new Visit();
                            visit.Tijdstip = Convert.ToInt32(reader["TijdstipDag"]);
                            visit.AantalBezoekers = Convert.ToInt32(reader["AantalBezoekers"]);
                            visit.DagVanDeWeek = Day;
                            visits.Add(visit);
                        }
                    }
                }


                return new OkObjectResult(visits);
            }
            catch (System.Exception ex)
            {
                log.LogError(ex.Message);
                return new StatusCodeResult(500);
            }
        }
    }
}
