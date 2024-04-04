using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Controller class that provides the client-side location sharing service.
/// Author: Waseef Nayeem
/// Date: 2024-03-15
/// </summary>
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

    /// <summary>
    /// Starts the connection with the server and registers the LocationReceived listener.
    /// </summary>
    private async Task InitAsync()
    {
        await _connector.InitAsync<RemoteUserLocation>(endpointUrl, endpointClientMethodName);
        _connector.OnLocationReceived += Receive;
    }

    /// <summary>
    /// Sends a location packet to the server.
    /// </summary>
    /// <param name="loc">Location packet to be sent.</param>
    public async Task SendAsync(RemoteUserLocation loc)
    {
        await _connector.SendLocationAsync(loc);
    }

    /// <summary>
    /// Handles receiving location packets from the server.
    /// </summary>
    /// <param name="loc">Location packet that was received.</param>
    private void Receive(RemoteUserLocation loc)
    {
        NewLocationReceived.Invoke(loc);
    }
}