using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class LocationHubService : MonoBehaviour
{
    [SerializeField] private string endpointUrl = "";
    [SerializeField] private string endpointClientMethodName = "";

    private readonly LocationHubConnector _connector = new();
    public UnityEvent<RemoteUserLocation> NewLocationReceived { get; } = new();

    private async void Start()
    {
        await InitAsync();
    }

    private async Task InitAsync()
    {
        await _connector.InitAsync<RemoteUserLocation>(endpointUrl, endpointClientMethodName);
        _connector.OnLocationReceived += Receive;
    }

    public async Task SendAsync(RemoteUserLocation loc)
    {
        //Debug.Log("Trying to send Location");
        await _connector.SendLocationAsync(loc);
        //Debug.Log("Location Task returned");
    }

    private void Receive(RemoteUserLocation loc)
    {
        //Debug.Log($"Received location {loc.Latitude}, {loc.Longitude} from {loc.Email}");
        NewLocationReceived.Invoke(loc);
    }
}