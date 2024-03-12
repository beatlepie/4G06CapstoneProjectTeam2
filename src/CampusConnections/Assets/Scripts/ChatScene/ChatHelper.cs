using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChatHelper : MonoBehaviour
{
    [SerializeField] private GameObject titleObj;
    private TMP_Text _title;

    [SerializeField] private GameObject rtcObject;
    private SignalRService _rtcService;

    [SerializeField] private GameObject inputFieldObj;
    private InputFieldHandler _inputFieldHandler;

    [SerializeField] private GameObject msgPanelObj;

    [SerializeField] private GameObject remoteMsgPrefab;

    [SerializeField] private GameObject localMsgPrefab;

    public List<GameObject> msgObjList;

    private Queue<string> _remoteMessageQueue;

    private void Awake()
    {
        _title = titleObj.GetComponent<TMP_Text>();
        _rtcService = rtcObject.GetComponent<SignalRService>();
        _inputFieldHandler = inputFieldObj.GetComponent<InputFieldHandler>();

        msgObjList = new List<GameObject>();
        _remoteMessageQueue = new Queue<string>();
    }

    private void Start()
    {
        var latestName = PlayerPrefs.GetString("LatestChatFriend");
        Debug.Log(latestName);
        _title.text = $"Chat with {latestName}";

        _rtcService.NewMessageReceived.AddListener(OnReceivedListener);
        _inputFieldHandler.MessageSent.AddListener(OnSentListener);
    }

    private void Update()
    {
        while (_remoteMessageQueue.Count > 0)
        {
            var msg = _remoteMessageQueue.Dequeue();

            CreateChatBubble(remoteMsgPrefab, msg);
        }
    }

    public void OnBackButtonClick()
    {
        SceneManager.LoadScene("FriendScene");
    }

    private void OnReceivedListener(Message msg)
    {
        var value = msg.Content.Trim();
        _remoteMessageQueue.Enqueue(value);
    }

    private void OnSentListener(Message msg)
    {
        var value = msg.Content.Trim();
        CreateChatBubble(localMsgPrefab, value);
    }

    private void CreateChatBubble(GameObject prefab, string msg)
    {
        var chatBubble = Instantiate(prefab, msgPanelObj.transform);
        var controller = chatBubble.GetComponent<ChatBubbleController>();
        controller.SetMessage(msg);
        controller.SetTimestamp(DateTime.Now);
        msgObjList.Add(chatBubble);
    }
}