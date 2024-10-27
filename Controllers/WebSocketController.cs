using api1.Models;
using api1.Service;
using GraphQL;
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
        private readonly IGraphQLService _service;

        public WebSocketController(IGraphQLService service)
        {
            _service = service;
        }

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
        private async Task Echo(WebSocket webSocket)
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
                    await CoordinatesLoop(firstCoords, jsonString, coordinates, webSocket, count);
                    //jsonString = WebSocketController.NewCoordinates(jsonString);
 
                } 
                else
                {
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


        private async Task CoordinatesLoop(Coordinates firstCoords, string currentCoordinates, List<Coordinates> coordinates, WebSocket webSocket, int count)
        {
            double rotaitingAngle = 2 * Math.PI / 12;
            var startingCoordinates = JsonSerializer.Deserialize<Coordinates>(currentCoordinates);
            
            List<Coordinates> NewCoordinates = WebSocketController.NewCoordinates(startingCoordinates, rotaitingAngle);
            foreach (var item in NewCoordinates)
            {
                if (!coordinates.Exists(c => c.lng == item.lng && c.lat == item.lat))
                {
                    try
                    {
                        coordinates.Add(item);
                        Point newPoint = new Point(Guid.NewGuid(), item);
                        var encoded = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(newPoint));

                        GraphQLResponse<ResponseType>? response =  await _service.Get(firstCoords, item);

                        if (response != null)
                        {
                            if (response.Data.Plan.Itineraries.Count > 0)
                            {
                                TimeSpan time = TimeSpan.FromMilliseconds(response.Data.Plan.Itineraries[0].EndTime);
                                DateTime result = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                                result = result.Add(time);

                                TimeSpan time2 = TimeSpan.FromMilliseconds(response.Data.Plan.Itineraries[0].StartTime);
                                DateTime result2 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                                result2 = result.Add(time);

                                var encodedResponse = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
                                await webSocket.SendAsync(
                                    new ArraySegment<byte>(encoded, 0, JsonSerializer.Serialize(newPoint).Length),
                                    //new ArraySegment<byte>(encodedResponse, 0, JsonSerializer.Serialize(response).Length),
                                    WebSocketMessageType.Text,
                                    true,
                                    CancellationToken.None);

                                string newValue = JsonSerializer.Serialize(item);
                                
                                await CoordinatesLoop(firstCoords, newValue, coordinates, webSocket, count);
                            }
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

        private static List<Coordinates> NewCoordinates(Coordinates oldCoordinates, double rotaitingAngle)
        {
            // currentCoordinate = JsonSerializer.Deserialize<Coordinates>(oldCoordinates);
            double r = 0.00433;

            List<Coordinates> coordinates = new List<Coordinates>();
            for (var i = 1; i <= 6; i++)
            {
                Coordinates NewCoordinate = new Coordinates(
                    Math.Round(oldCoordinates.lat + r * Math.Cos(rotaitingAngle), 6),
                    Math.Round(oldCoordinates.lng + r * Math.Sin(rotaitingAngle), 6)
                );
                coordinates.Add(NewCoordinate);
                rotaitingAngle = rotaitingAngle + 2 * Math.PI / 6;
            }

            return coordinates;
        }

        private static void FetchTimeToTravel(string coordinates)
        {

        }
    }
}