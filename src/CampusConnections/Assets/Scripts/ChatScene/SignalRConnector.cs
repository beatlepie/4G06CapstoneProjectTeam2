using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;

/// <summary>
/// Client-side connector for the chatting system.
/// Establishes a connection with the remote server endpoint and provides wrapper function for remote procedure calls.
/// Author: Waseef Nayeem
/// Date: 2024-02-09
/// </summary>
public class SignalRConnector
{
    public Action<Message> OnMessageReceived;
    private HubConnection _connection;
    public HubConnection Connection => _connection;

    /// <summary>
    /// Opens a connection to the remote SignalR server hub.
    /// </summary>
    /// <param name="url">URL of the server hub endpoint</param>
    /// <param name="handler">Name of the client side RPC function to register.</param>
    /// <typeparam name="T">Generic return type parameter for the client RPC function.</typeparam>
    public async Task InitAsync<T>(string url, string handler) where T : Message
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(url)
            .WithAutomaticReconnect()
            .Build();

        // Register the client side RPC function that will be called by the server
        _connection.On<T>(handler, (message) => { OnMessageReceived?.Invoke(message); });

        await StartConnectionAsync();
    }

    /// <summary>
    /// Sends a chat message to the server.
    /// </summary>
    /// <param name="message">Message to be send</param>
    /// <typeparam name="T">The type of the sent message object.</typeparam>
    public async Task SendMessageAsync<T>(T message) where T : Message
    {
        try
        {
            Debug.Log("Trying to send message " + message.Content);
            await _connection.InvokeAsync("SendMessage", message);
            Debug.Log("InvokeAsync() returned.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in SendMessageAsync: {e.Message}");
        }
    }

    /// <summary>
    /// Starts the connection with the remote server.
    /// </summary>
    private async Task StartConnectionAsync()
    {
        try
        {
            Debug.Log("Awaiting _connection.StartAsync()");
            await _connection.StartAsync();
            Debug.Log("_connection.StartAsync() returned");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in StartConnectionAsync: {e.Message}");
        }
    }
}