using Microsoft.AspNetCore.SignalR;

namespace RTCServer.Hubs;

/// <summary>
/// A Hub endpoint class for real-time location sharing between users.
/// Author: Waseef Nayeem
/// Date: 2024-02-01
/// </summary>
public class LocationHub : Hub
{
    /// <summary>
    /// An RPC function that shares the invoking client's location with other connected clients.
    /// </summary>
    /// <param name="location">Location object containing latitude, longitude and user email</param>
    /// <returns></returns>
    public async Task SendLocation(Location location)
    {
        Console.WriteLine($"Received: {location.Latitude}, {location.Longitude}");
        await Clients.Others.SendAsync("ReceiveMessage", location);
    }
}

/// <summary>
/// A data helper class that wraps and serializes the required parameters for location sharing.
/// </summary>
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