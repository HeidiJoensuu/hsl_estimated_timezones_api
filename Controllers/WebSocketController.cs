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

                if (!jsonString.Equals("Connection established") && jsonString != null)
                {
                    RequestType request = JsonSerializer.Deserialize<RequestType>(jsonString);
                    if (request != null)
                    {
                        Coordinates firstCoords = request.coordinates;

                        firstCoords.lat = Math.Round(firstCoords.lat, 6);
                        firstCoords.lng = Math.Round(firstCoords.lng, 6);

                        coordinates.Add(firstCoords);
                        double rotaitingAngle = 2 * Math.PI / 12;
                        await CoordinatesLoop(firstCoords, firstCoords, rotaitingAngle, coordinates, webSocket);
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
}
            await webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);
        }


        private async Task CoordinatesLoop(Coordinates firstCoords, Coordinates currentCoordinates, double rotaitingAngle, List<Coordinates> coordinates, WebSocket webSocket)
        {
            
            
            List<Coordinates> NewCoordinates = WebSocketController.NewCoordinates(currentCoordinates, rotaitingAngle);
            foreach (var item in NewCoordinates)
            {
                if (!coordinates.Exists(c => c.lng == item.lng && c.lat == item.lat))
                {
                    try
                    {
                        coordinates.Add(item);

                        GraphQLResponse<ResponseType>? response =  await _service.Get(firstCoords, item);

                        if (response != null)
                        {
                            if (response.Data.Plan.Itineraries.Count > 0)
                            {
                                ReturnResponse responsse = new ReturnResponse(Guid.NewGuid(), item, response.Data.Plan.Itineraries);
                                string stringAnswer = JsonSerializer.Serialize(responsse).ToLower();
                                var encodedResponse = Encoding.UTF8.GetBytes(stringAnswer);
                                await webSocket.SendAsync(
                                    new ArraySegment<byte>(encodedResponse, 0, stringAnswer.Length),
                                    WebSocketMessageType.Text,
                                    true,
                                    CancellationToken.None);

                                Coordinates newValue = item;

                                rotaitingAngle = rotaitingAngle + 2 * Math.PI / 6;
                                await CoordinatesLoop(firstCoords, newValue, rotaitingAngle, coordinates, webSocket);
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