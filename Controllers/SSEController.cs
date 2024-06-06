using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealTimeWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TableDependency.SqlClient;
using System.Text.Json;
using RealTimeWebApp.Services;
using Microsoft.Extensions.Logging;

namespace RealTimeWebApp.Controllers
{
    [ApiController]
    public class SSEController : ControllerBase
    {
        private SqlTableDependency<ExampleDataModel> SQLdependency;
        private String connectionString;

        readonly private ILogger<SSEController> logger;

        public SSEController(ILogger<SSEController> _logger, SourceDataService source) {
            logger = _logger;            
            connectionString = source.connectionString;
        }

        [HttpGet]
        [Route("/sse")]
        public async Task SeeConnection(CancellationToken cancToken) {
            Response.Headers.Add("Content-Type", "text/event-stream");

            using (SQLdependency = new SqlTableDependency<ExampleDataModel>(connectionString, "DataTable"))
            {
                logger.LogInformation("Dependency has been established, sending message in order to update table");
                await Response.WriteAsync($"data: update table request" + "\n\n");
                Response.Body.Flush();

                String updateMessage ;
                SQLdependency.OnChanged += (sender, e) =>
                {
                    updateMessage = JsonSerializer.Serialize(e.Entity);

                    Response.WriteAsync($"data: {updateMessage}" + "\n\n");
                    Response.Body.FlushAsync();
                };
                try//catch SQLdependency error
                {
                    SQLdependency.Start();

                    while (true)
                    {
                        if (cancToken.IsCancellationRequested)
                            break;
                    };

                    SQLdependency.Stop();
                }
                catch (Exception err)
                {
                    logger.LogError(err.Message);
                }
                finally
                {
                    if (SQLdependency.Status == TableDependency.SqlClient.Base.Enums.TableDependencyStatus.Started || SQLdependency.Status == TableDependency.SqlClient.Base.Enums.TableDependencyStatus.WaitingForNotification)
                        SQLdependency.Stop();
                }
            }
        }
    }
}
