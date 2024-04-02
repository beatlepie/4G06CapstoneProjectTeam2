using Microsoft.AspNetCore.SignalR;

namespace RTCServer.Hubs;

public class ChatHub : Hub
{
    public async Task SendMessage(Message message)
    {
        Console.WriteLine($"Received: {message.Content}");
        await Clients.Others.SendAsync("ReceiveMessage", message);
    }
}

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