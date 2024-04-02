using Microsoft.AspNetCore.SignalR;

namespace RTCServer.Hubs;

public class LocationHub : Hub
{
    public async Task SendLocation(Location location)
    {
        Console.WriteLine($"Received: {location.Latitude}, {location.Longitude}");
        await Clients.Others.SendAsync("ReceiveMessage", location);
    }
}

[Serializable]
public class Location
{
    private string _email = "";
    private float _lat;
    private float _lng;

    public float Latitude
    {
        get => _lat;
        set => _lat = value;
    }

    public float Longitude
    {
        get => _lng;
        set => _lng = value;
    }

    public string Email
    {
        get => _email;
        set => _email = value;
    }
}