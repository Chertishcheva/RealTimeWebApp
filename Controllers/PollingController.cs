using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RealTimeWebApp.Models;
using RealTimeWebApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TableDependency.SqlClient;

namespace RealTimeWebApp.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PollingController : ControllerBase
    {
        private SqlTableDependency<ExampleDataModel> SQLdependency;
        private String connectionString;

        private readonly ILogger<PollingController> logger;

        public PollingController(ILogger<PollingController> _logger, SourceDataService source)
        {
            logger = _logger;
            connectionString = source.connectionString;
        }

        [HttpGet]
        [Route("/longpol")]
        public async Task<IActionResult> GetLongPolling(CancellationToken cancToken) {
            var timeCancellation = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken());

            ExampleDataModel updateObject = new ExampleDataModel(); 
            bool changeMade = false;

            using (SQLdependency = new SqlTableDependency<ExampleDataModel>(connectionString, "DataTable"))
            {
                logger.LogInformation("Dependency has been established");

                SQLdependency.OnChanged += (sender, e) =>
                {
                    updateObject = e.Entity;

                    changeMade = true;
                };
                try//catch SQLdependency error
                {
                    SQLdependency.Start();

                    timeCancellation.CancelAfter(TimeSpan.FromSeconds(30));
                    while (!timeCancellation.IsCancellationRequested)
                    {
                        if (cancToken.IsCancellationRequested)
                        {
                            break;
                        }
                        if (changeMade)
                        {
                            SQLdependency.Stop();
                            ((IDisposable)SQLdependency).Dispose();
                            return Ok(updateObject);
                        }
                    }
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
            return NoContent();
        }
    }
}
