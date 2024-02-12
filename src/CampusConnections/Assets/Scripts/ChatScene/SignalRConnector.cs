using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;

public class SignalRConnector
{
    public Action<Message> OnMessageReceived;
    private HubConnection _connection;
    public HubConnection Connection => _connection;

    public async Task InitAsync<T>(string url, string handler) where T : Message
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(url)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<T>(handler, (message) => { OnMessageReceived?.Invoke(message); });

        await StartConnectionAsync();
    }

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