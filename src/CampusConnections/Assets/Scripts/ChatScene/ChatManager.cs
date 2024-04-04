using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manager class that handles interactions in the ChatScene.
/// Interactions include sending and receiving messages as well as navigating back to the previous scene.
/// Author: Waseef Nayeem
/// Date: 2024-02-09
/// </summary>
public class ChatManager : MonoBehaviour
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
        // Get the name of the user we will be chatting with
        var latestName = PlayerPrefs.GetString("LatestChatFriend");
        _title.text = $"Chat with {latestName}";

        _rtcService.NewMessageReceived.AddListener(OnReceivedListener);
        _inputFieldHandler.MessageSent.AddListener(OnSentListener);
    }

    private void Update()
    {
        // Pop received messages from the queue and display them as chat bubbles.
        while (_remoteMessageQueue.Count > 0)
        {
            var msg = _remoteMessageQueue.Dequeue();
            CreateChatBubble(remoteMsgPrefab, msg);
        }
    }

    /// <summary>
    /// Handler function that loads the previous scene.
    /// </summary>
    public void OnBackButtonClick()
    {
        SceneManager.LoadScene("FriendScene");
    }
    
    /// <summary>
    /// Handler function that is called when a message is received.
    /// </summary>
    /// <remarks>
    /// Incoming messages must be added to queue.
    /// This is because the UI cannot be updated from inside an event handler.
    /// </remarks>
    /// <param name="msg">Message object received from remote user.</param>
    private void OnReceivedListener(Message msg)
    {
        var value = msg.Content.Trim();
        _remoteMessageQueue.Enqueue(value);
    }

    /// <summary>
    /// Handler function that is called when a message is sent.
    /// Create a chat bubble locally with the sent message contents.
    /// </summary>
    /// <param name="msg">Message object sent by local user.</param>
    private void OnSentListener(Message msg)
    {
        var value = msg.Content.Trim();
        CreateChatBubble(localMsgPrefab, value);
    }

    /// <summary>
    /// Helper function that creates ChatBubble object instances.
    /// </summary>
    /// <param name="prefab">The object prefab to be instantiated.</param>
    /// <param name="msg">The message that will be displayed</param>
    private void CreateChatBubble(GameObject prefab, string msg)
    {
        var chatBubble = Instantiate(prefab, msgPanelObj.transform);
        var controller = chatBubble.GetComponent<ChatBubbleController>();
        controller.SetMessage(msg);
        controller.SetTimestamp(DateTime.Now);
        msgObjList.Add(chatBubble);
    }
}