using Firebase.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class InputFieldHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject rtcObject;
    private SignalRService _rtcService;
    
    private TMP_InputField _input;
    
    public UnityEvent<Message> MessageSent { get; } = new();

    private void Start()
    {
        _rtcService = rtcObject.GetComponent<SignalRService>();

        _input = GetComponent<TMP_InputField>();
        _input.onEndEdit.AddListener(OnEndEditListener);
    }

    private async void OnEndEditListener(string value)
    {
        value = value.Trim();
        if (value == "") return;
        
        var chatMessage = $"{value}\n";

        if (!Input.GetButton("Submit")) return;

        var msg = new Message { Content = chatMessage };
        await _rtcService.SendAsync(msg);
        
        MessageSent.Invoke(msg);

        _input.text = "";
        _input.ActivateInputField();
    }

    public async void ButtonListener()
    {
        var value = _input.text;

        value = value.Trim();
        if (value == "") return;
        
        var chatMessage = $"{value}\n";

        var msg = new Message { Content = chatMessage };
        await _rtcService.SendAsync(msg);
        
        MessageSent.Invoke(msg);
        
        _input.text = "";
        _input.ActivateInputField();
    }
}