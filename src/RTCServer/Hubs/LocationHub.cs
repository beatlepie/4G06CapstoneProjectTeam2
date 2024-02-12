using Microsoft.AspNetCore.SignalR;

namespace RTCServer.Hubs
{
    public class LocationHub : Hub
    {
        public async Task SendLocation(Location location)
        {
            Console.WriteLine($"Received: {location.Latitude}, {location.Longitude}");
            await Clients.Others.SendAsync("ReceiveMessage", location);
        }
    }

    [System.Serializable]
    public class Location
    {
        private String email = "";
        private double lat = 0;
        private double lng = 0;

        public double Latitude
        {
            get => lat;
            set => lat = value;
        }

        public double Longitude
        {
            get => lng;
            set => lng = value;
        }

        public String Email
        {
            get => email;
            set => email = value;
        }
    }
}
