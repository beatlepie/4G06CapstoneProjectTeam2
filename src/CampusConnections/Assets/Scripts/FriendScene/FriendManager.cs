using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    public const int friendEntryHeight = 300;

    [Header("Invitation")]
    public List<User> requesters;
    [SerializeField] Transform requestEntryContainer;
    [SerializeField] Transform requestEntryTemplate;
    private List<Transform> requestEntryTransformList;
    private int invitationCount = 0;

    public const int requestEntryHeight = 150;
    [SerializeField] GameObject badge;

    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        currentUser = auth.CurrentUser;
        friendEntryTransformList = new List<Transform>();
        requestEntryTransformList = new List<Transform>();
        createFriendList();
        createRequestList();
    }

    void Update()
    {
        // if (invitationCount == 0)
        // {
        //     badge.SetActive(false);
        // }
        // else if (invitationCount < 100)
        // {
        //     badge.SetActive(true);
        //     badge.GetComponentInChildren<TMP_Text>().text = invitationCount.ToString();
        // }
        // else
        // {
        //     badge.SetActive(true);
        //     badge.GetComponentInChildren<TMP_Text>().text = "99+";
        // }
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

    IEnumerator GetInvitations(Action<List<User>> onCallBack)
    {
        string emailWithoutDot = Utilities.removeDot(currentUser.Email);                
        var userData = databaseReference.Child("users/" + emailWithoutDot + "/invitations").GetValueAsync();
        yield return new WaitUntil(predicate: () => userData.IsCompleted);
        if(userData != null)
        {
            List<User> requesters = new List<User>();
            DataSnapshot snapshot = userData.Result;
            foreach (var x in snapshot.Children)
            {
                string email = Utilities.addDot(x.Key.ToString());
                var requesterData = databaseReference.Child("users/" + x.Key.ToString()).GetValueAsync();
                yield return new WaitUntil(predicate: () => requesterData.IsCompleted);
                DataSnapshot requesterSnapshot = requesterData.Result;
                if(requesterData != null)
                {
                    User requester = Utilities.FormalizeDBUserData(requesterSnapshot);
                    requesters.Add(requester);
                }
            }
            onCallBack.Invoke(requesters);
        }
    }

    private void refreshFriendList()
    {
        // Update transforms of all friend templates once friends changes (accept/ignore)
        for (int i = 0; i < friends.Count; i++)
        {
            RectTransform entryRectTransform = friendEntryTransformList[i].GetComponent<RectTransform>();
            entryRectTransform.anchoredPosition = new Vector2(0, -friendEntryHeight * i);    
        }
        if(friendEntryHeight * friends.Count > 1460){
            // If the friend list is short, the default container height is viewport height (1460)
            friendEntryContainer.GetComponent<RectTransform>().sizeDelta = new Vector2 (800, friendEntryHeight*friends.Count);
        }
    }

    private void createFriendList()
    {
        StartCoroutine(GetFriends((List<User> data) =>
        {
            friends = data;
            foreach (User friend in friends)
            {
                Transform entryTransform = Instantiate(friendEntryTemplate, friendEntryContainer);
                RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
                entryRectTransform.anchoredPosition = new Vector2(0, -friendEntryHeight * friendEntryTransformList.Count);
                entryTransform.gameObject.SetActive(true);
                entryTransform.Find("Email").GetComponent<TMP_Text>().text = friend.email;
                entryTransform.Find("Name").GetComponent<TMP_Text>().text = friend.nickName;
                friendEntryTransformList.Add(entryTransform);  
            } 
            if(friendEntryHeight * friends.Count > 1460){
                // If the friend list is short, the default container height is viewport height (1460)
                friendEntryContainer.GetComponent<RectTransform>().sizeDelta = new Vector2 (800, friendEntryHeight*friends.Count);
            }
        }));
    }

    public void OnFriendDeleteclick()
    {
        GameObject template = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        string targetEmail = template.transform.Find("Email").GetComponent<TMP_Text>().text;
        string userEmailWithoutDot = Utilities.removeDot(currentUser.Email);
        string targetEmailWithoutDot = Utilities.removeDot(targetEmail);
       // Remove friend with that email from friend and tranform list
        friends.RemoveAll(friend => friend.email == targetEmail);
        Destroy(template);
        friendEntryTransformList.Remove(template.transform);
        refreshFriendList();
        // Remove target user from current user list in database
        databaseReference.Child("users/" + userEmailWithoutDot + "/friends/" + targetEmailWithoutDot).SetValueAsync(null);
        // Remove target user from current user list in database
        databaseReference.Child("users/" + targetEmailWithoutDot + "/friends/" + userEmailWithoutDot).SetValueAsync(null);
    }

    public void OnFriendViewclick()
    {
        GameObject template = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        string targetEmail = template.transform.Find("Email").GetComponent<TMP_Text>().text;
        SettingsManager.queryEmail = targetEmail;
        SettingsManager.currentUser = false;
        SceneManager.LoadScene("SettingsScene");
    }

    public void OnFriendNameClick()
    {
        SceneManager.LoadScene("ChatScene");
    }

    private void refreshRequestList()
    {
        // Update transforms of all request templates once requests changes (accept/ignore)
        for (int i = 0; i < requesters.Count; i++)
        {
            RectTransform entryRectTransform = requestEntryTransformList[i].GetComponent<RectTransform>();
            entryRectTransform.anchoredPosition = new Vector2(0, -requestEntryHeight * i);    
        }
        if(requestEntryHeight * requesters.Count > 1460){
            // If the request list is short, the default container height is viewport height (1460)
            requestEntryContainer.GetComponent<RectTransform>().sizeDelta = new Vector2 (800, requestEntryHeight*requesters.Count);
        }
    }

    private void createRequestList()
    {
        StartCoroutine(GetInvitations((List<User> data) =>
        {
            requesters = data;
            foreach (User requester in requesters)
            {
                Transform entryTransform = Instantiate(requestEntryTemplate, requestEntryContainer);
                RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
                entryRectTransform.anchoredPosition = new Vector2(0, -requestEntryHeight * requestEntryTransformList.Count);
                entryTransform.gameObject.SetActive(true);
                entryTransform.Find("Email").GetComponent<TMP_Text>().text = requester.email;
                requestEntryTransformList.Add(entryTransform);  
            }
            invitationCount = requesters.Count;
            if(requestEntryHeight * requesters.Count > 1460){
                // If the request list is short, the default container height is viewport height (1460)
                requestEntryContainer.GetComponent<RectTransform>().sizeDelta = new Vector2 (800, requestEntryHeight*requesters.Count);
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
        User targetUser = requesters.Find(requester => requester.email == targetEmail);
        requesters.Remove(targetUser);
        invitationCount--;
        Destroy(template);
        requestEntryTransformList.Remove(template.transform);
        refreshRequestList();
        // Remove invitation from database
        databaseReference.Child("users/" + userEmailWithoutDot + "/invitations/" + requesterEmailWithoutDot).SetValueAsync(null);
        // Add friends
        databaseReference.Child("users/" + userEmailWithoutDot + "/friends/" + requesterEmailWithoutDot).SetValueAsync(true);
        databaseReference.Child("users/" + requesterEmailWithoutDot + "/friends/" + userEmailWithoutDot).SetValueAsync(true);
        // Add new User to friend and transform list so that friend page can get updated
        friends.Add(targetUser);
        Transform entryTransform = Instantiate(friendEntryTemplate, friendEntryContainer);
        RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
        entryRectTransform.anchoredPosition = new Vector2(0, -friendEntryHeight * friendEntryTransformList.Count);
        entryTransform.gameObject.SetActive(true);
        entryTransform.Find("Email").GetComponent<TMP_Text>().text = targetUser.email;
        friendEntryTransformList.Add(entryTransform); 
        refreshFriendList();
    }

    public void OnRequestIgnoreclick()
    {
        GameObject template = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        string targetEmail = template.transform.Find("Email").GetComponent<TMP_Text>().text;
        string userEmailWithoutDot = Utilities.removeDot(currentUser.Email);
        string requesterEmailWithoutDot = Utilities.removeDot(targetEmail);
        // Remove invitation from requests and tranform list
        requesters.RemoveAll(requester => requester.email == targetEmail);
        invitationCount--;
        Destroy(template);
        requestEntryTransformList.Remove(template.transform);
        refreshRequestList();
        // Remove invitation from database
        databaseReference.Child("users/" + userEmailWithoutDot + "/invitations/" + requesterEmailWithoutDot).SetValueAsync(null);     
    }

    public void SwitchToFriends()
    {
        SceneManager.LoadScene("FriendScene");
    }

    public void SwitchToRequests()
    {
        SceneManager.LoadScene("FriendRequestScene");
    }

    public void GoBack()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
