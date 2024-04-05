using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;

/// <summary>
/// Client-side connector for the location sharing system.
/// Establishes a connection with the remote server endpoint and provides wrapper function for remote procedure calls.
/// Author: Waseef Nayeem
/// Date: 2024-03-15
/// </summary>
public class LocationHubConnector
{
    public Action<RemoteUserLocation> OnLocationReceived;
    private HubConnection _connection;
    public HubConnection Connection => _connection;

    /// <summary>
    /// Opens a connection to the remote SignalR server hub.
    /// </summary>
    /// <param name="url">URL of the server hub endpoint</param>
    /// <param name="handler">Name of the client side RPC function to register.</param>
    /// <typeparam name="T">Generic return type parameter for the client RPC function.</typeparam>
    public async Task InitAsync<T>(string url, string handler) where T : RemoteUserLocation
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(url)
            .WithAutomaticReconnect()
            .Build();

        // Register the client side RPC function that will be called by the server
        _connection.On<T>(handler, (location) => { OnLocationReceived?.Invoke(location); });

        await StartConnectionAsync();
    }

    /// <summary>
    /// Sends a location packet to the server.
    /// </summary>
    /// <param name="location">Location packet to be send</param>
    /// <typeparam name="T">The type of the sent packet object.</typeparam>
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

    /// <summary>
    /// Starts the connection with the remote server.
    /// </summary>
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