using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using RealTimeWebApp.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using TableDependency.SqlClient;
using RealTimeWebApp.Models;
using System.Threading;
using System.Text.Json;
using System.Text;

namespace RealTimeWebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WSController : ControllerBase
    {
        private SqlTableDependency<ExampleDataModel> SQLdependency;
        private String connectionString;

        private readonly ILogger<WSController> logger;
        
        private readonly List<WebSocket> clients;

        public WSController(ILogger<WSController> _logger, SourceDataService source){
            logger = _logger;
            connectionString = source.connectionString;
            clients = new();
        }

        [HttpGet]
        [Route("/ws")]
        public async Task Get()
        {   
            //перевірка чи є wss:// у рядку запита
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await WSServer(webSocket);
            }
            else
            {
                logger.LogError("Not a Websocket request");
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private async Task WSServer(WebSocket webSocket)
        {
            clients.Add(webSocket);
            var buffer = new byte[1024 * 2];

            using (SQLdependency = new SqlTableDependency<ExampleDataModel>(connectionString, "DataTable"))
            {
                logger.LogInformation("Dependency has been established, sending message in order to update table");
                await webSocket.SendAsync(buffer[..1], WebSocketMessageType.Text, true, default);
                
                SQLdependency.OnChanged += (sender, e) => {
                    var updateMessage = JsonSerializer.Serialize(e.Entity);
                    var answerArray = new ArraySegment<byte>(Encoding.UTF8.GetBytes(updateMessage));

                    foreach (var item in clients)
                        item.SendAsync(answerArray, WebSocketMessageType.Text, true, default);
                    
                };
                try//catch SQLdependency error
                {
                    SQLdependency.Start();

                    while (webSocket.State == WebSocketState.Open)
                    {
                        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                            break;
                        }
                    }

                    SQLdependency.Stop();
                }
                catch (Exception err)
                {
                    logger.LogError(err.Message);
                }
                finally {
                    if (SQLdependency.Status == TableDependency.SqlClient.Base.Enums.TableDependencyStatus.Started || SQLdependency.Status == TableDependency.SqlClient.Base.Enums.TableDependencyStatus.WaitingForNotification)
                        SQLdependency.Stop();
                    if(webSocket.State == WebSocketState.Open)
                        await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "close due SQLdependency error", CancellationToken.None);
                }
            }
        }
    }
}
