using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Controller class that provides the client-side chatting service.
/// Author: Waseef Nayeem
/// Date: 2024-02-09
/// </summary>
public class SignalRService : MonoBehaviour
{
    [SerializeField] private string endpointUrl = "";
    [SerializeField] private string endpointClientMethodName = "";

    private readonly SignalRConnector _connector = new();
    public UnityEvent<Message> NewMessageReceived { get; } = new();

    private async void Start()
    {
        await InitAsync();
    }

    /// <summary>
    /// Starts the connection with the server and registers MessageReceived listener.
    /// </summary>
    private async Task InitAsync()
    {
        await _connector.InitAsync<Message>(endpointUrl, endpointClientMethodName);
        _connector.OnMessageReceived += Receive;
    }

    /// <summary>
    /// Sends a message to the server.
    /// </summary>
    /// <param name="msg">Message to be sent.</param>
    public async Task SendAsync(Message msg)
    {
        await _connector.SendMessageAsync(msg);
    }

    /// <summary>
    /// Handles receiving messages from the server.
    /// </summary>
    /// <param name="msg">Message that is received.</param>
    private void Receive(Message msg)
    {
        NewMessageReceived.Invoke(msg);
    }
}