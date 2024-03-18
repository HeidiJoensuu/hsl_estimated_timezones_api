using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace api.Controllers
{
    [ApiController]
    [Route("/ws")]
    public class WebSocketController : ControllerBase
    {
        [HttpGet]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await Echo(webSocket);
                Console.WriteLine("sisällä");
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                HttpContext.Response.WriteAsync("Expected a WebSocket request");
                Console.WriteLine("Huono yhteys");
            }
        }
        private static async Task Echo(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            Console.WriteLine("New message received : " + Encoding.UTF8.GetString(buffer, 0, receiveResult.Count));
            Console.WriteLine("keke:" + receiveResult);
            var jei = NewCoordinates(Encoding.UTF8.GetString(buffer, 0, receiveResult.Count));
            while (!receiveResult.CloseStatus.HasValue)
            {
                byte[] bytes = Encoding.Default.GetBytes(jei);
                jei = Encoding.UTF8.GetString(bytes);
                await webSocket.SendAsync(
                    bytes,
                    //new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                    receiveResult.MessageType,
                    receiveResult.EndOfMessage,
                    CancellationToken.None
                );

                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None
                );
                Console.WriteLine("New message received : " + Encoding.UTF8.GetString(buffer, 0, receiveResult.Count));
            }

            await webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);
        }

        private static string NewCoordinates(string oldCoordinates)
        {
            var currentCoordinate = JsonSerializer.Deserialize<Dictionary<string, double>>(oldCoordinates);
            double r = 0.0017;
            double pI = Math.PI;
            double rotaitingAngle = 2 * pI / 12;
            double newAngle = 2 * pI / 6;
            Console.WriteLine(currentCoordinate["lat"] + " " + currentCoordinate["lng"] +" "+ rotaitingAngle +" "+ newAngle);
            Dictionary<string, double> newCoordinate = new Dictionary<string, double>
            {
                { "lat", currentCoordinate["lat"] + r * Math.Cos(rotaitingAngle) },
                { "lng", currentCoordinate["lng"] + r * Math.Sin(rotaitingAngle) }
            };

            var returnvalue = JsonSerializer.Serialize(newCoordinate);
            Console.WriteLine("palautettava value: " +returnvalue);
            return returnvalue;
        }
    }
}
