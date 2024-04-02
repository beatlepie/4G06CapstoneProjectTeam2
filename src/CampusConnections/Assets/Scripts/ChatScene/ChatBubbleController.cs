using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChatBubbleController : MonoBehaviour
{
    [SerializeField] private GameObject msgObj;
    private TMP_Text _message;

    [SerializeField] private GameObject timeObj;
    private TMP_Text _timestamp;

    // For link to event and lecture
    [SerializeField] private Button msgBubble;
    [SerializeField] public string linkColor;
    private string targetType;
    private string targetID;

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
            targetType = pattern[0];
            targetID = pattern[1];
            var polishedMsg = Utilities.PolishChatMessage(msg, linkColor);
            _message.SetText(polishedMsg);
        }
        else
        {
            _message.SetText(msg);
        }
    }

    public void clickOnMessage()
    {
        if (targetType == "lecture")
        {
            SceneManager.LoadScene("LectureScene");
            LectureManager.defaultSearchOption = "code";
            LectureManager.defaultSearchString = targetID;
        }
        else if (targetType == "event")
        {
            SceneManager.LoadScene("EventScene");
            EventManager.defaultSearchOption = "name";
            EventManager.defaultSearchString = targetID;
        }
    }
}