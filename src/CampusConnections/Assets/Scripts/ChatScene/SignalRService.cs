using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

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

    private async Task InitAsync()
    {
        await _connector.InitAsync<Message>(endpointUrl, endpointClientMethodName);
        _connector.OnMessageReceived += Receive;
    }

    public async Task SendAsync(Message msg)
    {
        await _connector.SendMessageAsync(msg);
    }

    private void Receive(Message msg)
    {
        Debug.Log("Received message " + msg.Content);
        NewMessageReceived.Invoke(msg);
    }
}