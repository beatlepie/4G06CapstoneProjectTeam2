/// <summary>
/// A data helper class that wraps and serializes user location packets.
/// Author: Waseef Nayeem
/// Date: 2024-03-15
/// </summary>
[System.Serializable]
public class RemoteUserLocation
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