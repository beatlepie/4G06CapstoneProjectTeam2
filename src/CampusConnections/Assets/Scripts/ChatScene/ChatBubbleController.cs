using System.Collections;
using System.Collections.Generic;
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

    public void SetTimestamp()
    {
        Debug.Log("WIP");
    }

    public void SetMessage(string msg)
    {
        _message.SetText(msg);
    }
}
