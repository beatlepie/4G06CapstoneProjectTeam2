using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;

public class LocationHubConnector
{
    public Action<RemoteUserLocation> OnLocationReceived;
    private HubConnection _connection;
    public HubConnection Connection => _connection;

    public async Task InitAsync<T>(string url, string handler) where T : RemoteUserLocation
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(url)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<T>(handler, (location) => { OnLocationReceived?.Invoke(location); });

        await StartConnectionAsync();
    }

    public async Task SendLocationAsync<T>(T location) where T : RemoteUserLocation
    {
        try
        {
            await _connection.InvokeAsync("SendLocation", location);
        }
        catch (Exception e)
        {
            Debug.LogError($"[LocationHubConnector] Error in SendLocationAsync: {e.Message}");
        }
    }

    private async Task StartConnectionAsync()
    {
        try
        {
            Debug.Log("[LocationHubConnector] Awaiting _connection.StartAsync()");
            await _connection.StartAsync();
            Debug.Log("[LocationHubConnector] _connection.StartAsync() returned");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in StartConnectionAsync: {e.Message}");
        }
    }
}