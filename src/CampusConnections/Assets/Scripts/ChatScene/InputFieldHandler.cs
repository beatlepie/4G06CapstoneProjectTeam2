using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This class controls the chat input field and sends messages.
/// Author: Waseef Nayeem
/// Date: 2024-02-09
/// </summary>
public class InputFieldHandler : MonoBehaviour
{
    [SerializeField] private GameObject rtcObject;
    private SignalRService _rtcService;

    private TMP_InputField _input;

    public UnityEvent<Message> MessageSent { get; } = new();

    private void Start()
    {
        _rtcService = rtcObject.GetComponent<SignalRService>();

        _input = GetComponent<TMP_InputField>();
        _input.onEndEdit.AddListener(OnEndEditListener);
    }

    /// <summary>
    /// Listens for Enter key presses and sends the contents of the chat input field when triggered.
    /// </summary>
    /// <param name="value">Contents of the chat InputField.</param>
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

    /// <summary>
    /// Listens for Send button clicks and sends the contents of the chat input field when triggered.
    /// </summary>
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