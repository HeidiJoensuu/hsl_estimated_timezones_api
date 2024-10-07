using api1.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace api1.Controllers
{
    [ApiController]
    [Route("/ws")]
    public class WebSocketController : ControllerBase
    {
        Guid guid = Guid.NewGuid();

        [HttpGet]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                Console.WriteLine("sisällä get");
                await Echo(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                //HttpContext.Response.WriteAsync("Expected a WebSocket request");
                Console.WriteLine("Huono yhteys");
            }
        }
        private static async Task Echo(WebSocket webSocket)
        {
            
            ASCIIEncoding ascii = new ASCIIEncoding();
            var buffer = new byte[1024 * 4];
            
            var receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
            
            //Loop
            while (!receiveResult.CloseStatus.HasValue)
            {
                List<Coordinates> coordinates = new List<Coordinates>();
                var webSocketMessage = new ArraySegment<byte>(buffer, 0, receiveResult.Count);
                string jsonString = ascii.GetString(webSocketMessage);

                if (!jsonString.Equals("Connection established")) {
                    
                    
                    Coordinates firstCoords = JsonSerializer.Deserialize<Coordinates>(jsonString);

                    firstCoords.lat = Math.Round(firstCoords.lat, 6);
                    firstCoords.lng = Math.Round(firstCoords.lng, 6);
                    coordinates.Add(firstCoords);

                    int count = 0;
                    await CoordinatesLoop(jsonString, coordinates, webSocket, count);
                    //jsonString = WebSocketController.NewCoordinates(jsonString);
 
                } 
                else
                {
                    Console.WriteLine("Palautetaan sama");
                    await webSocket.SendAsync(
                        new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                        receiveResult.MessageType,
                        receiveResult.EndOfMessage,
                        CancellationToken.None);
                }
                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);
        }


        private static async Task CoordinatesLoop(string currentCoordinates, List<Coordinates> coordinates, WebSocket webSocket, int count)
        {
            List<Coordinates> NewCoordinates = WebSocketController.NewCoordinates(currentCoordinates);
            foreach (var item in NewCoordinates)
            {
                if (!coordinates.Exists(c => c.lng == item.lng && c.lat == item.lat))
                {
                    try
                    {
                        coordinates.Add(item);
                        Point newPoint = new Point(Guid.NewGuid(), item);
                        var encoded = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(newPoint));
                        //Console.WriteLine(item);
                        await webSocket.SendAsync(
                            new ArraySegment<byte>(encoded, 0, JsonSerializer.Serialize(newPoint).Length),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                        count += 1;
                        if (count < 2)
                        {
                            string newValue = JsonSerializer.Serialize(item);
                            await CoordinatesLoop(newValue, coordinates, webSocket, count);
                        }
                    }
                    catch (Exception error)
                    {
                        Console.WriteLine(error);
                        break;
                    }
                }
            }
        }

        private static List<Coordinates> NewCoordinates(string oldCoordinates)
        {
            var currentCoordinate = JsonSerializer.Deserialize<Coordinates>(oldCoordinates);
            double r = 0.00294;
            double pI = Math.PI;
            double rotaitingAngle = 2 * pI / 12;
            double newAngle = 2 * pI / 6;

            List<Coordinates> coordinates = new List<Coordinates>();

            //Console.WriteLine("angle:" + newAngle+ "rotating: " + rotaitingAngle + "coords: " + currentCoordinate);
            for (var i = 1; i <= 6; i++)
            {
                Coordinates NewCoordinate = new Coordinates(
                    Math.Round(currentCoordinate.lat + r * Math.Cos(rotaitingAngle), 6),
                    Math.Round(currentCoordinate.lng + r * Math.Sin(rotaitingAngle), 6)
                );
                coordinates.Add(NewCoordinate);
                rotaitingAngle = rotaitingAngle+ 2 * pI / 6;
            }

            return coordinates;
        }
    }
}