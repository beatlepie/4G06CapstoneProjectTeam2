using TMPro;
using UnityEngine;

public class InputFieldHandler : MonoBehaviour
{
    public GameObject messageObject;
    public GameObject rtcObject;

    private TMP_Text _messages;
    private TMP_InputField _input;

    private SignalRService _rtcService;

    void Start()
    {
        _rtcService = rtcObject.GetComponent<SignalRService>();
        _messages = messageObject.GetComponent<TMP_Text>();
        _rtcService.NewMessageReceived.AddListener(OnReceivedListener);
        
        _input = GetComponent<TMP_InputField>();
        _input.onEndEdit.AddListener(OnEndEditListener);
    }

    private async void OnEndEditListener(string value)
    {
        value = value.Trim();
        var chatMessage = $"<Local User>: {value}\n";
        
        if (!Input.GetButton("Submit")) return;
        
        var msg = new Message { Content = value };
        await _rtcService.SendAsync(msg);
                
        _messages.text += chatMessage;
        _input.text = "";
        _input.ActivateInputField();
    }

    private void OnReceivedListener(Message msg)
    {
        var value = msg.Content.Trim();
        var chatMessage = $"<Remote User>: {value}\n";
                
        _messages.text += chatMessage;
    }
}
