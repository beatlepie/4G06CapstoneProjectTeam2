using Firebase.Auth;
using TMPro;
using UnityEngine;

public class InputFieldHandler : MonoBehaviour
{
    public GameObject messageObject;
    public GameObject rtcObject;

    private TMP_Text _messages;
    private TMP_InputField _input;

    private SignalRService _rtcService;

    private string _username;

    private string messageLog = "";

    private void Start()
    {
        _rtcService = rtcObject.GetComponent<SignalRService>();
        _messages = messageObject.GetComponent<TMP_Text>();
        _rtcService.NewMessageReceived.AddListener(OnReceivedListener);

        _input = GetComponent<TMP_InputField>();
        _input.onEndEdit.AddListener(OnEndEditListener);

        _username = FirebaseAuth.DefaultInstance.CurrentUser.DisplayName;
    }

    private void Update()
    {
        _messages.text = messageLog;
    }

    private async void OnEndEditListener(string value)
    {
        value = value.Trim();
        var chatMessage = $"{_username}: {value}\n";

        if (!Input.GetButton("Submit")) return;

        var msg = new Message { Content = chatMessage };
        await _rtcService.SendAsync(msg);

        messageLog += chatMessage;
        _input.text = "";
        _input.ActivateInputField();
    }

    private void OnReceivedListener(Message msg)
    {
        Debug.Log("OnReceivedListener fired.");

        var value = msg.Content.Trim();
        var chatMessage = $"{value}\n";

        messageLog += chatMessage;
        //_messages.SetAllDirty();
        //_messages.ForceMeshUpdate(true, true);
        Debug.Log("OnReceivedListener ended.");
    }

    public async void ButtonListener()
    {
        var value = _input.text;

        value = value.Trim();
        var chatMessage = $"{_username}: {value}\n";

        var msg = new Message { Content = chatMessage };
        await _rtcService.SendAsync(msg);

        messageLog += chatMessage;
        _input.text = "";
        _input.ActivateInputField();
    }
}