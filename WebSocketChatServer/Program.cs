using System.Net;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

class WebSocketChatServer
{
    private static Dictionary<InitialMessageModel, WebSocket> _clients = new();

    public static async Task Main()
    {
        var httpListener = new HttpListener();
        httpListener.Prefixes.Add("http://localhost:5000/");
        httpListener.Start();
        Console.WriteLine("WebSocket server started at ws://localhost:5000");

        while (true)
        {
            HttpListenerContext context = await httpListener.GetContextAsync();

            if (context.Request.IsWebSocketRequest)
            {
                HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
                WebSocket webSocket = wsContext.WebSocket;

                Console.WriteLine("Client connected!");

                var buffer = new byte[1024];

                var initialMessage = await ParseInitialMessage(webSocket);
                if (initialMessage is null)
                {
                    Console.WriteLine("Error: Received empty or invalid message.");
                    continue;
                }

                try
                {
                    _clients.Add(initialMessage, webSocket);
                }
                catch(ArgumentException)
                {
                    _clients.Remove(initialMessage);
                    _clients.Add(initialMessage, webSocket);
                }

                Console.WriteLine($"Listed client {initialMessage.UserName} in chat {initialMessage.ChatId}");

                await BroadcastMessageAsync(initialMessage);

                _ = Task.Run(() => HandleClientAsync(webSocket));
            }
            else
            {
                context.Response.StatusCode = 400;
                context.Response.Close();
            }
        }
    }

    private static async Task HandleClientAsync(WebSocket webSocket)
    {
        var buffer = new byte[10240];

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                // If the client wants to close the WebSocket
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    var initialMessage = await ParseInitialMessage(webSocket);
                    if (initialMessage is null)
                    {
                        Console.WriteLine("Error: Received empty or invalid message.");
                        continue;
                    }
                    Console.WriteLine($"Client {initialMessage.UserName} disconnected from chat (ID: {initialMessage.ChatId}).");

                    lock(new object())
                    {
                        _clients.Remove(initialMessage);
                    }
                    
                    break;
                }
                
                string clientMessageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                MessageModel messageModel = null!;
                try
                {
                    messageModel = JsonConvert.DeserializeObject<MessageModel>(clientMessageJson)!;
                    if (messageModel is null)
                    {
                        Console.WriteLine("Error: Received empty or invalid message.");
                        continue;
                    }

                    Console.WriteLine($"User (ID: {messageModel.UserName}) - {messageModel.Message} - (at {messageModel.TimeSend})");

                    // Broadcast the message to all connected clients
                    await BroadcastMessageAsync(messageModel);
                }
                catch (Exception jsonEx)
                {
                    Console.WriteLine("Error parsing JSON message: " + jsonEx.Message);
                }
            }
        }
        catch (WebSocketException ex)
        {
            Console.WriteLine("WebSocket error: " + ex.Message);
        }
        finally
        {
            Console.WriteLine("Error handling message");
        }
    }

    private static async Task BroadcastMessageAsync(MessageModel message)
    {
        foreach (var client in _clients)
        {
            if (client.Key.UserName != message.UserName && client.Value.State == WebSocketState.Open && client.Key.ChatId == message.ChatId)
            {
                byte[] responseBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                await client.Value.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }

    private static async Task BroadcastMessageAsync(InitialMessageModel message)
    {
        foreach (var client in _clients)
        {
            if (client.Key.UserName != message.UserName && client.Value.State == WebSocketState.Open && client.Key.ChatId == message.ChatId)
            {
                byte[] responseBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                await client.Value.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }

    private static async Task<InitialMessageModel> ParseInitialMessage(WebSocket webSocket)
    {
        var buffer = new byte[1024];

        var initailMessage = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        return JsonConvert.DeserializeObject<InitialMessageModel>(
            Encoding.UTF8.GetString(buffer, 0, initailMessage.Count))!;
    }
}
