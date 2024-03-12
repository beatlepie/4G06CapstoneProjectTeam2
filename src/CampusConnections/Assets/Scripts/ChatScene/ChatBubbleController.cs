using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class ChatBubbleController : MonoBehaviour
{
    [SerializeField]
    private GameObject msgObj;
    private TMP_Text _message;
    
    [SerializeField]
    private GameObject timeObj;
    private TMP_Text _timestamp;
    
    void Awake()
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
        _message.SetText(msg);
    }
}
