using Microsoft.AspNetCore.SignalR;

namespace RTCServer.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(Message message)
        {
            Console.WriteLine($"Received: {message.Content}");
            await Clients.Others.SendAsync("ReceiveMessage", message);
        }

        //public override async Task OnConnectedAsync()
        //{
        //    Console.WriteLine($"{Context.ConnectionId} has joined.");
        //    var message = new Message { Content = $"{Context.ConnectionId} has joined." };
        //    await Clients.Others.SendAsync("ReceiveMessage", message);
        //}
    }

    [System.Serializable]
    public class Message
    {
        private string content = "";
        public string Content
        {
            get => content;
            set => content = value;
        }
    }
}
