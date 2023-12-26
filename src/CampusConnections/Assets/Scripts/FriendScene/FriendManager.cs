using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class FriendManager : MonoBehaviour
{
    [Header("Firebase")]
    public FirebaseAuth auth;
    public FirebaseUser currentUser;
    public DatabaseReference databaseReference;

    [Header("Chat")]
    public List<User> friends;
    [SerializeField] Transform friendEntryContainer;
    [SerializeField] Transform friendEntryTemplate;
    private List<Transform> friendEntryTransformList;


    [Header("Invitation")]
    public List<string> requests;
    [SerializeField] Transform requestEntryContainer;
    [SerializeField] Transform requestEntryTemplate;
    private List<Transform> requestEntryTransformList;
    private int invitationCount = 0;
    [SerializeField] GameObject badge;

    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        currentUser = auth.CurrentUser;
        friendEntryTransformList = new List<Transform>();
        requestEntryTransformList = new List<Transform>();
        createChatList();
        createRequestList();
    }

    void Update()
    {
        if (invitationCount == 0)
        {
            badge.SetActive(false);
        }
        else if (invitationCount < 100)
        {
            badge.SetActive(true);
            badge.GetComponentInChildren<TMP_Text>().text = invitationCount.ToString();
        }
        else
        {
            badge.SetActive(true);
            badge.GetComponentInChildren<TMP_Text>().text = "99+";
        }
    }

    IEnumerator GetFriends(Action<List<User>> onCallBack)
    {
        string emailWithoutDot = Utilities.removeDot(currentUser.Email);                
        var userData = databaseReference.Child("users/" + emailWithoutDot + "/friends").GetValueAsync();
        yield return new WaitUntil(predicate: () => userData.IsCompleted);
        if(userData != null)
        {
            List<User> friends = new List<User>();
            DataSnapshot snapshot = userData.Result;
            foreach (var x in snapshot.Children)
            {
                string email = Utilities.addDot(x.Key.ToString());
                var friendData = databaseReference.Child("users/" + x.Key.ToString()).GetValueAsync();
                yield return new WaitUntil(predicate: () => friendData.IsCompleted);
                DataSnapshot friendSnapshot = friendData.Result;
                if(friendData != null)
                {
                    User friend = Utilities.FormalizeDBUserData(friendSnapshot);
                    friends.Add(friend);
                }
            }
            onCallBack.Invoke(friends);
        }
    }

    IEnumerator GetInvitations(Action<List<string>> onCallBack)
    {
        string emailWithoutDot = Utilities.removeDot(currentUser.Email);                
        var userData = databaseReference.Child("users/" + emailWithoutDot + "/invitations").GetValueAsync();
        yield return new WaitUntil(predicate: () => userData.IsCompleted);
        if(userData != null)
        {
            List<string> requests = new List<string>();
            DataSnapshot snapshot = userData.Result;
            foreach (var x in snapshot.Children)
            {
                string email = Utilities.addDot(x.Key.ToString());
                requests.Add(email);
            }
            onCallBack.Invoke(requests);
        }
    }

    private void createChatList()
    {
        StartCoroutine(GetFriends((List<User> data) =>
        {
            friends = data;
            foreach (User friend in friends)
            {
                Transform entryTransform = Instantiate(friendEntryTemplate, friendEntryContainer);
                RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
                entryRectTransform.anchoredPosition = new Vector2(0, -300 * friendEntryTransformList.Count);
                entryTransform.gameObject.SetActive(true);
                entryTransform.Find("Email").GetComponent<TMP_Text>().text = friend.email;
                entryTransform.Find("Name").GetComponent<TMP_Text>().text = friend.nickName;
                friendEntryTransformList.Add(entryTransform);  
            } 
            if(300 * friends.Count > 1460){
                // If the friend list is short, the default container height is viewport height (1460)
                friendEntryContainer.GetComponent<RectTransform>().sizeDelta = new Vector2 (800, 300*friends.Count);
            }
        }));
    }

    private void orderRequestList()
    {
        for (int i = 0; i < requests.Count; i++)
        {
            RectTransform entryRectTransform = requestEntryTransformList[i].GetComponent<RectTransform>();
            entryRectTransform.anchoredPosition = new Vector2(0, -150 * i);    
        }
    }

    private void createRequestList()
    {
        StartCoroutine(GetInvitations((List<string> data) =>
        {
            requests = data;
            foreach (string requesterEmail in requests)
            {
                Transform entryTransform = Instantiate(requestEntryTemplate, requestEntryContainer);
                RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
                entryRectTransform.anchoredPosition = new Vector2(0, -150 * requestEntryTransformList.Count);
                entryTransform.gameObject.SetActive(true);
                entryTransform.Find("Email").GetComponent<TMP_Text>().text = requesterEmail;
                requestEntryTransformList.Add(entryTransform);  
            }
            invitationCount = requests.Count;
            if(150 * requests.Count > 1460){
                // If the request list is short, the default container height is viewport height (1460)
                requestEntryContainer.GetComponent<RectTransform>().sizeDelta = new Vector2 (800, 150*requests.Count);
            }
        }));
        
    }

    public void EditHelpMsg()
    {
        StartCoroutine(CheckUserByEmail((string msg) =>
        {
            GameObject.Find("Help_Message").GetComponent<TMP_Text>().text = msg;
        }));
    }

    IEnumerator CheckUserByEmail(Action<string> onCallBack)
    {
        string email = GameObject.Find("InputEmail").GetComponent<TMP_InputField>().text;
        if(currentUser.Email == email) {
            // Cannot use user email
            onCallBack.Invoke("<color=#f44336>You cannot add yourself as friend");
            yield break;
        }
        string senderEmailWithoutDot = Utilities.removeDot(currentUser.Email);
        string receiverEmailWithoutDot = Utilities.removeDot(email);
        var userData = databaseReference.Child("users/" + receiverEmailWithoutDot).GetValueAsync();
        var invitationData = databaseReference.Child("users/" + receiverEmailWithoutDot + "/invitations/" + senderEmailWithoutDot).GetValueAsync();
        yield return new WaitUntil(predicate: () => invitationData.IsCompleted && userData.IsCompleted);
        if(invitationData != null && userData != null)
        {
            DataSnapshot invitationSnapshot = invitationData.Result;
            string helpMsg = "";
            if(!userData.Result.Exists)
            {
                // Invalid email
                helpMsg = "<color=#f44336>Invalid email, user does not exist";
            }
            else if (invitationData.Result.Exists) 
            {
                // Duplicated reques
                helpMsg = "<color=#ff9800>Friend request has been sent, please wait";
            }
            else
            {
                helpMsg = "<color=#4caf50>Success! Request is sent";
                databaseReference.Child("users/" + receiverEmailWithoutDot + "/invitations/" + senderEmailWithoutDot).SetValueAsync(true);
                onCallBack.Invoke(helpMsg);
            }
        }
    }

    public void OnRequestAcceptclick()
    {
        GameObject template = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        string targetEmail = template.transform.Find("Email").GetComponent<TMP_Text>().text;
        string userEmailWithoutDot = Utilities.removeDot(currentUser.Email);
        string requesterEmailWithoutDot = Utilities.removeDot(targetEmail);
       // Remove invitation from requests and tranform list
        requests.Remove(targetEmail);
        invitationCount--;
        Destroy(template);
        requestEntryTransformList.Remove(template.transform);
        orderRequestList();
        // Remove invitation from database
        databaseReference.Child("users/" + userEmailWithoutDot + "/invitations/" + requesterEmailWithoutDot).SetValueAsync(null);
        // Add friends
        databaseReference.Child("users/" + userEmailWithoutDot + "/friends/" + requesterEmailWithoutDot).SetValueAsync(true);
        databaseReference.Child("users/" + requesterEmailWithoutDot + "/friends/" + userEmailWithoutDot).SetValueAsync(true);
        // TODO: Either delete current friends list and request again from db or add friend to current friends list to visualize change on chat page
    }

    public void OnRequestIgnoreclick()
    {
        GameObject template = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        string targetEmail = template.transform.Find("Email").GetComponent<TMP_Text>().text;
        string userEmailWithoutDot = Utilities.removeDot(currentUser.Email);
        string requesterEmailWithoutDot = Utilities.removeDot(targetEmail);
        // Remove invitation from requests and tranform list
        requests.Remove(targetEmail);
        invitationCount--;
        Destroy(template);
        requestEntryTransformList.Remove(template.transform);
        orderRequestList();
        // Remove invitation from database
        databaseReference.Child("users/" + userEmailWithoutDot + "/invitations/" + requesterEmailWithoutDot).SetValueAsync(null);     
    }
}
