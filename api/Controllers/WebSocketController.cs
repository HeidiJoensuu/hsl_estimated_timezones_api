using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;

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
            while (!receiveResult.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(
                    new ArraySegment<byte>(buffer, 0, receiveResult.Count),
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
    }
}
