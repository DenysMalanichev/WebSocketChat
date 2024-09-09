using System.Net;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

class WebSocketChatServer
{
    // Store all connected clients
    private static List<WebSocket> _clients = new();

    public static async Task Main()
    {
        HttpListener httpListener = new HttpListener();
        httpListener.Prefixes.Add("http://localhost:5000/");
        httpListener.Start();
        Console.WriteLine("WebSocket server started at ws://localhost:5000");

        while (true)
        {
            // Wait for an incoming HTTP connection request.
            HttpListenerContext context = await httpListener.GetContextAsync();

            if (context.Request.IsWebSocketRequest)
            {
                // Accept the WebSocket connection
                HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
                WebSocket webSocket = wsContext.WebSocket;

                Console.WriteLine("Client connected!");

                // Add the new client to the client list
                _clients.Add(webSocket);

                // Start handling the client in a background task
                _ = Task.Run(() => HandleClientAsync(webSocket));
            }
            else
            {
                // Respond with a 400 Bad Request for non-WebSocket requests
                context.Response.StatusCode = 400;
                context.Response.Close();
            }
        }
    }

    private static async Task HandleClientAsync(WebSocket webSocket)
    {
        byte[] buffer = new byte[10240];

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                // If the client wants to close the WebSocket
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine("Client disconnected.");
                    if(_clients.Count == 0)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    }

                    lock(new object())
                    {
                        _clients.Remove(webSocket);  // Remove client from list
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

                    Console.WriteLine($"User (ID: {messageModel.UserId}) - {messageModel.Message} - (at {messageModel.TimeSend})");

                    // Broadcast the message to all connected clients
                    await BroadcastMessageAsync(webSocket, messageModel);
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
            if (webSocket != null)
            {
                lock(new object())
                {
                    _clients.Remove(webSocket);
                }
                
                webSocket.Dispose();
            }
        }
    }

    // Broadcast a message to all connected clients except the sender
    private static async Task BroadcastMessageAsync(WebSocket sender, MessageModel message)
    {
        foreach (var client in _clients)
        {
            if (client != sender && client.State == WebSocketState.Open)
            {
                byte[] responseBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                await client.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
