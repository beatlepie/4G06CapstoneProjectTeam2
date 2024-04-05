using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Database;
using Auth;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

/// <summary>
/// Controls the behaviour and interactions in the FriendScene including listing and viewing friends,
/// accepting, creating and sending friend requests.
/// Author: Zihao Du
/// Date: 2023-12-11
/// </summary>
public class FriendManager : MonoBehaviour
{
    [Header("Chat")] private List<User> _friends;
    [SerializeField] private Transform friendEntryContainer;
    [SerializeField] private Transform friendEntryTemplate;
    private List<Transform> _friendEntryTransformList;
    private GameObject _deleteTarget;
    private const int FriendEntryHeight = 300;

    [Header("Invitation")] private List<User> _requesters;
    [SerializeField] private Transform requestEntryContainer;
    [SerializeField] private Transform requestEntryTemplate;
    private List<Transform> _requestEntryTransformList;

    private const int RequestEntryHeight = 150;

    [FormerlySerializedAs("Confirmation")] [Header("Confirmation")] [SerializeField]
    private GameObject confirmation;

    public TMP_Text confirmationText;

    [FormerlySerializedAs("Notification")] [Header("Notification")] [SerializeField]
    private GameObject notification;

    public TMP_Text notificationText;

    private void Awake()
    {
        _friendEntryTransformList = new List<Transform>();
        _requestEntryTransformList = new List<Transform>();
        CreateFriendList();
        CreateRequestList();
    }

    /// <summary>
    /// Coroutine function that asynchronously queries the database for the current user's friend list.
    /// Must be provided with a handler for processing the received data.
    /// </summary>
    /// <param name="onCallBack">Handler for processing the received friend data.</param>
    private IEnumerator GetFriends(Action<List<User>> onCallBack)
    {
        var emailWithoutDot = Utilities.RemoveDot(AuthConnector.Instance.CurrentUser.Email);
        var userData = DatabaseConnector.Instance.Root.Child("users/" + emailWithoutDot + "/friends").GetValueAsync();
        yield return new WaitUntil(() => userData.IsCompleted);
        if (userData != null)
        {
            var friends = new List<User>();
            var snapshot = userData.Result;
            foreach (var x in snapshot.Children)
            {
                var friendData = DatabaseConnector.Instance.Root.Child("users/" + x.Key).GetValueAsync();
                yield return new WaitUntil(() => friendData.IsCompleted);
                var friendSnapshot = friendData.Result;
                var friend = Utilities.FormalizeDBUserData(friendSnapshot);
                friends.Add(friend);
            }

            onCallBack.Invoke(friends);
        }
    }

    /// <summary>
    /// Coroutine function that asynchronously queries the database for the current user's friend requests.
    /// Must be provided with a handler for processing the received data.
    /// </summary>
    /// <param name="onCallBack">Handler for processing the received friend request data.</param>
    private IEnumerator GetInvitations(Action<List<User>> onCallBack)
    {
        var emailWithoutDot = Utilities.RemoveDot(AuthConnector.Instance.CurrentUser.Email);
        var userData = DatabaseConnector.Instance.Root.Child("users/" + emailWithoutDot + "/invitations")
            .GetValueAsync();
        yield return new WaitUntil(() => userData.IsCompleted);
        if (userData != null)
        {
            var requesters = new List<User>();
            var snapshot = userData.Result;
            foreach (var x in snapshot.Children)
            {
                var requesterData = DatabaseConnector.Instance.Root.Child("users/" + x.Key).GetValueAsync();
                yield return new WaitUntil(() => requesterData.IsCompleted);
                var requesterSnapshot = requesterData.Result;
                var requester = Utilities.FormalizeDBUserData(requesterSnapshot);
                requesters.Add(requester);
            }

            onCallBack.Invoke(requesters);
        }
    }

    /// <summary>
    /// Helper function for visually updating the friend list when it changes.
    /// </summary>
    private void RefreshFriendList()
    {
        // Update transforms of all friend templates once friends changes (accept/ignore)
        for (var i = 0; i < _friends.Count; i++)
        {
            var entryRectTransform = _friendEntryTransformList[i].GetComponent<RectTransform>();
            entryRectTransform.anchoredPosition = new Vector2(0, -FriendEntryHeight * i);
        }

        if (FriendEntryHeight * _friends.Count > 1460)
            // If the friend list is short, the default container height is viewport height (1460)
            friendEntryContainer.GetComponent<RectTransform>().sizeDelta =
                new Vector2(800, FriendEntryHeight * _friends.Count);
    }

    /// <summary>
    /// Handles the data obtained from GetFriends() and constructs the friend list from the received data.
    /// </summary>
    private void CreateFriendList()
    {
        StartCoroutine(GetFriends(data =>
        {
            _friends = data;
            foreach (var friend in _friends)
            {
                var entryTransform = Instantiate(friendEntryTemplate, friendEntryContainer);
                var entryRectTransform = entryTransform.GetComponent<RectTransform>();
                entryRectTransform.anchoredPosition =
                    new Vector2(0, -FriendEntryHeight * _friendEntryTransformList.Count);
                entryTransform.gameObject.SetActive(true);
                entryTransform.Find("Email").GetComponent<TMP_Text>().text = friend.Email;
                entryTransform.Find("Name").GetComponent<TMP_Text>().text = friend.NickName;
                _friendEntryTransformList.Add(entryTransform);
            }

            if (FriendEntryHeight * _friends.Count > 1460)
                // If the friend list is short, the default container height is viewport height (1460)
                friendEntryContainer.GetComponent<RectTransform>().sizeDelta =
                    new Vector2(800, FriendEntryHeight * _friends.Count);
        }));
    }
    
    public void OnFriendDeleteClick()
    {
        var template = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        var targetName = template.transform.Find("Name").GetComponent<TMP_Text>().text;
        confirmationText.text = "Are you sure you want to delete this friend, <color=#0000FF>" + targetName +
                                "</color> from the list?";
        _deleteTarget = template;
    }
    
    public void OnFriendDeleteConfirm()
    {
        var targetEmail = _deleteTarget.transform.Find("Email").GetComponent<TMP_Text>().text;
        var userEmailWithoutDot = Utilities.RemoveDot(AuthConnector.Instance.CurrentUser.Email);
        var targetEmailWithoutDot = Utilities.RemoveDot(targetEmail);
        // Remove friend with that email from friend and transform list
        _friends.RemoveAll(friend => friend.Email == targetEmail);
        Destroy(_deleteTarget);
        _friendEntryTransformList.Remove(_deleteTarget.transform);
        RefreshFriendList();
        // Remove target user from current user list in database
        DatabaseConnector.Instance.Root.Child("users/" + userEmailWithoutDot + "/friends/" + targetEmailWithoutDot)
            .SetValueAsync(null);
        // Remove target user from current user list in database
        DatabaseConnector.Instance.Root.Child("users/" + targetEmailWithoutDot + "/friends/" + userEmailWithoutDot)
            .SetValueAsync(null);
    }
    
    public void OnFriendViewClick()
    {
        var template = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        var targetEmail = template.transform.Find("Email").GetComponent<TMP_Text>().text;
        SettingsManager.QueryEmail = targetEmail;
        SettingsManager.CurrentUser = false;
        SettingsManager.State = 0;
        SceneManager.LoadScene("SettingsScene");
    }
    
    public void OnFriendChatClick()
    {
        var template = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        var targetEmail = template.transform.Find("Email").GetComponent<TMP_Text>().text;

        foreach (var friend in _friends.Where(friend => friend.Email == targetEmail))
        {
            PlayerPrefs.SetString("LatestChatFriend", friend.NickName);
            PlayerPrefs.Save();
        }

        SceneManager.LoadScene("ChatScene");
    }

    /// <summary>
    /// Helper function for visually updating the friend request list when updated.
    /// </summary>
    private void RefreshRequestList()
    {
        // Update transforms of all request templates once requests changes (accept/ignore)
        for (var i = 0; i < _requesters.Count; i++)
        {
            var entryRectTransform = _requestEntryTransformList[i].GetComponent<RectTransform>();
            entryRectTransform.anchoredPosition = new Vector2(0, -RequestEntryHeight * i);
        }

        if (RequestEntryHeight * _requesters.Count > 1460)
            // If the request list is short, the default container height is viewport height (1460)
            requestEntryContainer.GetComponent<RectTransform>().sizeDelta =
                new Vector2(800, RequestEntryHeight * _requesters.Count);
    }

    /// <summary>
    /// Handles the data obtained from GetInvitations() and constructs the request list from the received data.
    /// </summary>
    private void CreateRequestList()
    {
        StartCoroutine(GetInvitations(data =>
        {
            _requesters = data;
            foreach (var requester in _requesters)
            {
                var entryTransform = Instantiate(requestEntryTemplate, requestEntryContainer);
                var entryRectTransform = entryTransform.GetComponent<RectTransform>();
                entryRectTransform.anchoredPosition =
                    new Vector2(0, -RequestEntryHeight * _requestEntryTransformList.Count);
                entryTransform.gameObject.SetActive(true);
                entryTransform.Find("Email").GetComponent<TMP_Text>().text = requester.Email;
                _requestEntryTransformList.Add(entryTransform);
            }

            if (RequestEntryHeight * _requesters.Count > 1460)
                // If the request list is short, the default container height is viewport height (1460)
                requestEntryContainer.GetComponent<RectTransform>().sizeDelta =
                    new Vector2(800, RequestEntryHeight * _requesters.Count);
        }));
    }
    
    public void AddFriendHelpMsg()
    {
        StartCoroutine(CheckUserByEmail());
        notification.SetActive(true);
    }
    
    /// <summary>
    /// Updates client notification messages by monitoring the state of asynchronous requests made to the database.
    /// </summary>
    private IEnumerator CheckUserByEmail()
    {
        var email = GameObject.Find("InputEmail").GetComponent<TMP_InputField>().text;
        if (AuthConnector.Instance.CurrentUser.Email == email)
        {
            // Cannot use user email
            notificationText.text = "<color=#f44336>You cannot add yourself as friend";
            yield break;
        }

        var senderEmailWithoutDot = Utilities.RemoveDot(AuthConnector.Instance.CurrentUser.Email);
        var receiverEmailWithoutDot = Utilities.RemoveDot(email);
        var userData = DatabaseConnector.Instance.Root.Child("users/" + receiverEmailWithoutDot).GetValueAsync();
        var invitationData = DatabaseConnector.Instance.Root
            .Child("users/" + receiverEmailWithoutDot + "/invitations/" + senderEmailWithoutDot).GetValueAsync();
        yield return new WaitUntil(() => invitationData.IsCompleted && userData.IsCompleted);
        if (invitationData != null && userData != null)
        {
            if (!userData.Result.Exists)
            {
                // Invalid email
                notificationText.text = "<color=#f44336>Invalid email, user does not exist.";
            }
            else if (invitationData.Result.Exists)
            {
                // Duplicated request
                notificationText.text = "<color=#ff9800>Friend request has been sent, please wait.";
            }
            else
            {
                notificationText.text = "<color=#4caf50>Success! Request is sent";
                DatabaseConnector.Instance.Root
                    .Child("users/" + receiverEmailWithoutDot + "/invitations/" + senderEmailWithoutDot)
                    .SetValueAsync(true);
            }
        }
    }
    
    public void OnRequestAcceptClick()
    {
        var template = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        var targetEmail = template.transform.Find("Email").GetComponent<TMP_Text>().text;
        var userEmailWithoutDot = Utilities.RemoveDot(AuthConnector.Instance.CurrentUser.Email);
        var requesterEmailWithoutDot = Utilities.RemoveDot(targetEmail);
        // Remove invitation from requests and transform list
        var targetUser = _requesters.Find(requester => requester.Email == targetEmail);
        _requesters.Remove(targetUser);
        Destroy(template);
        _requestEntryTransformList.Remove(template.transform);
        RefreshRequestList();
        // Remove invitation from database
        DatabaseConnector.Instance.Root
            .Child("users/" + userEmailWithoutDot + "/invitations/" + requesterEmailWithoutDot).SetValueAsync(null);
        // Add friends
        DatabaseConnector.Instance.Root.Child("users/" + userEmailWithoutDot + "/friends/" + requesterEmailWithoutDot)
            .SetValueAsync(true);
        DatabaseConnector.Instance.Root.Child("users/" + requesterEmailWithoutDot + "/friends/" + userEmailWithoutDot)
            .SetValueAsync(true);
        // Add new User to friend and transform list so that friend page can get updated
        _friends.Add(targetUser);
        var entryTransform = Instantiate(friendEntryTemplate, friendEntryContainer);
        var entryRectTransform = entryTransform.GetComponent<RectTransform>();
        entryRectTransform.anchoredPosition = new Vector2(0, -FriendEntryHeight * _friendEntryTransformList.Count);
        entryTransform.gameObject.SetActive(true);
        entryTransform.Find("Email").GetComponent<TMP_Text>().text = targetUser.Email;
        _friendEntryTransformList.Add(entryTransform);
        RefreshFriendList();
    }

    public void OnRequestIgnoreClick()
    {
        var template = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        var targetEmail = template.transform.Find("Email").GetComponent<TMP_Text>().text;
        var userEmailWithoutDot = Utilities.RemoveDot(AuthConnector.Instance.CurrentUser.Email);
        var requesterEmailWithoutDot = Utilities.RemoveDot(targetEmail);
        // Remove invitation from requests and transform list
        _requesters.RemoveAll(requester => requester.Email == targetEmail);
        Destroy(template);
        _requestEntryTransformList.Remove(template.transform);
        RefreshRequestList();
        // Remove invitation from database
        DatabaseConnector.Instance.Root
            .Child("users/" + userEmailWithoutDot + "/invitations/" + requesterEmailWithoutDot).SetValueAsync(null);
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