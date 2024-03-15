using UnityEngine;

[System.Serializable]
public class RemoteUserLocation
{
    private string email = "";
    private double lat;
    private double lng;

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

    public string Email
    {
        get => email;
        set => email = value;
    }
}