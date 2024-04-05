using Microsoft.AspNetCore.SignalR;

namespace RTCServer.Hubs;

/// <summary>
/// A Hub endpoint class for chatting.
/// Author: Waseef Nayeem
/// Date: 2024-02-01
/// </summary>
public class ChatHub : Hub
{
    /// <summary>
    /// An RPC function that shares the invoking client's message with other connected clients.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task SendMessage(Message message)
    {
        Console.WriteLine($"Received: {message.Content}");
        await Clients.Others.SendAsync("ReceiveMessage", message);
    }
}

/// <summary>
/// A data helper class that wrap and serialize the message parameter.
/// </summary>
[Serializable]
public class Message
{
    private string _content = "";

    public string Content
    {
        get => _content;
        set => _content = value;
    }
}