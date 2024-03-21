using UnityEngine;

[System.Serializable]
public class RemoteUserLocation
{
    private string email = "";
    private float lat;
    private float lng;

    public float Latitude
    {
        get => lat;
        set => lat = value;
    }

    public float Longitude
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