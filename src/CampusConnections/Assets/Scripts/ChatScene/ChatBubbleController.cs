using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controls the behaviour of chat bubble game objects.
/// Chat bubbles display messages sent by users in the chat scene.
/// Authors: Waseef Nayeem, Zihao Du
/// Date: 2024=03-12
/// </summary>
public class ChatBubbleController : MonoBehaviour
{
    [SerializeField] private GameObject msgObj;
    private TMP_Text _message;

    [SerializeField] private GameObject timeObj;
    private TMP_Text _timestamp;

    // For linking to events and lectures
    [SerializeField] private Button msgBubble;
    [SerializeField] public string linkColor;
    private string _targetType;
    private string _targetID;

    private void Awake()
    {
        _message = msgObj.GetComponent<TMP_Text>();
        _timestamp = timeObj.GetComponent<TMP_Text>();
    }

    public void SetTimestamp(DateTime dt)
    {
        _timestamp.SetText(dt.ToString("hh:mm tt"));
    }

    public void SetMessage(string msg)
    {
        var pattern = Utilities.GetActivityPattern(msg);
        if (pattern[0] != "null")
        {
            _targetType = pattern[0];
            _targetID = pattern[1];
            var polishedMsg = Utilities.PolishChatMessage(msg, linkColor);
            _message.SetText(polishedMsg);
        }
        else
        {
            _message.SetText(msg);
        }
    }

    /// <summary>
    /// Handler function for clicking on message links.
    /// Messages can have embedded links to events or lectures.
    /// </summary>
    public void ClickOnMessage()
    {
        if (_targetType == "lecture")
        {
            SceneManager.LoadScene("LectureScene");
            LectureManager.DefaultSearchOption = "code";
            LectureManager.DefaultSearchString = _targetID;
        }
        else if (_targetType == "event")
        {
            SceneManager.LoadScene("EventScene");
            EventManager.DefaultSearchOption = "name";
            EventManager.DefaultSearchString = _targetID;
        }
    }
}