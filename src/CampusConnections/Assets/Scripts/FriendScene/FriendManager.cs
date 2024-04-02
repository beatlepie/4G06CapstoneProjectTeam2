using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Database;
using Auth;
using Firebase.Database;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class FriendManager : MonoBehaviour
{
    [Header("Chat")] public List<User> friends;
    [SerializeField] private Transform friendEntryContainer;
    [SerializeField] private Transform friendEntryTemplate;
    private List<Transform> friendEntryTransformList;
    private GameObject deleteTarget;
    public const int friendEntryHeight = 300;

    [Header("Invitation")] public List<User> requesters;
    [SerializeField] private Transform requestEntryContainer;
    [SerializeField] private Transform requestEntryTemplate;
    private List<Transform> requestEntryTransformList;

    public const int requestEntryHeight = 150;

    [Header("Comfirmation")] [SerializeField]
    private GameObject Confirmation;

    public TMP_Text confirmationText;

    [Header("Notification")] [SerializeField]
    private GameObject Notification;

    public TMP_Text notificationText;

    private void Awake()
    {
        friendEntryTransformList = new List<Transform>();
        requestEntryTransformList = new List<Transform>();
        createFriendList();
        createRequestList();
    }

    private IEnumerator GetFriends(Action<List<User>> onCallBack)
    {
        var emailWithoutDot = Utilities.removeDot(AuthConnector.Instance.CurrentUser.Email);
        var userData = DatabaseConnector.Instance.Root.Child("users/" + emailWithoutDot + "/friends").GetValueAsync();
        yield return new WaitUntil(() => userData.IsCompleted);
        if (userData != null)
        {
            var friends = new List<User>();
            var snapshot = userData.Result;
            foreach (var x in snapshot.Children)
            {
                var email = Utilities.addDot(x.Key.ToString());
                var friendData = DatabaseConnector.Instance.Root.Child("users/" + x.Key.ToString()).GetValueAsync();
                yield return new WaitUntil(() => friendData.IsCompleted);
                var friendSnapshot = friendData.Result;
                if (friendData != null)
                {
                    var friend = Utilities.FormalizeDBUserData(friendSnapshot);
                    friends.Add(friend);
                }
            }

            onCallBack.Invoke(friends);
        }
    }

    private IEnumerator GetInvitations(Action<List<User>> onCallBack)
    {
        var emailWithoutDot = Utilities.removeDot(AuthConnector.Instance.CurrentUser.Email);
        var userData = DatabaseConnector.Instance.Root.Child("users/" + emailWithoutDot + "/invitations")
            .GetValueAsync();
        yield return new WaitUntil(() => userData.IsCompleted);
        if (userData != null)
        {
            var requesters = new List<User>();
            var snapshot = userData.Result;
            foreach (var x in snapshot.Children)
            {
                var email = Utilities.addDot(x.Key.ToString());
                var requesterData = DatabaseConnector.Instance.Root.Child("users/" + x.Key.ToString()).GetValueAsync();
                yield return new WaitUntil(() => requesterData.IsCompleted);
                var requesterSnapshot = requesterData.Result;
                if (requesterData != null)
                {
                    var requester = Utilities.FormalizeDBUserData(requesterSnapshot);
                    requesters.Add(requester);
                }
            }

            onCallBack.Invoke(requesters);
        }
    }

    private void refreshFriendList()
    {
        // Update transforms of all friend templates once friends changes (accept/ignore)
        for (var i = 0; i < friends.Count; i++)
        {
            var entryRectTransform = friendEntryTransformList[i].GetComponent<RectTransform>();
            entryRectTransform.anchoredPosition = new Vector2(0, -friendEntryHeight * i);
        }

        if (friendEntryHeight * friends.Count > 1460)
            // If the friend list is short, the default container height is viewport height (1460)
            friendEntryContainer.GetComponent<RectTransform>().sizeDelta =
                new Vector2(800, friendEntryHeight * friends.Count);
    }

    private void createFriendList()
    {
        StartCoroutine(GetFriends((List<User> data) =>
        {
            friends = data;
            foreach (var friend in friends)
            {
                var entryTransform = Instantiate(friendEntryTemplate, friendEntryContainer);
                var entryRectTransform = entryTransform.GetComponent<RectTransform>();
                entryRectTransform.anchoredPosition =
                    new Vector2(0, -friendEntryHeight * friendEntryTransformList.Count);
                entryTransform.gameObject.SetActive(true);
                entryTransform.Find("Email").GetComponent<TMP_Text>().text = friend.email;
                entryTransform.Find("Name").GetComponent<TMP_Text>().text = friend.nickName;
                friendEntryTransformList.Add(entryTransform);
            }

            if (friendEntryHeight * friends.Count > 1460)
                // If the friend list is short, the default container height is viewport height (1460)
                friendEntryContainer.GetComponent<RectTransform>().sizeDelta =
                    new Vector2(800, friendEntryHeight * friends.Count);
        }));
    }

    public void onFriendDeleteClick()
    {
        var template = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        var targetName = template.transform.Find("Name").GetComponent<TMP_Text>().text;
        confirmationText.text = "Are you sure you want to delete this friend, <color=#0000FF>" + targetName +
                                "</color> from the list?";
        deleteTarget = template;
    }

    public void OnFriendDeleteConfirm()
    {
        var targetEmail = deleteTarget.transform.Find("Email").GetComponent<TMP_Text>().text;
        var userEmailWithoutDot = Utilities.removeDot(AuthConnector.Instance.CurrentUser.Email);
        var targetEmailWithoutDot = Utilities.removeDot(targetEmail);
        // Remove friend with that email from friend and tranform list
        friends.RemoveAll(friend => friend.email == targetEmail);
        Destroy(deleteTarget);
        friendEntryTransformList.Remove(deleteTarget.transform);
        refreshFriendList();
        // Remove target user from current user list in database
        DatabaseConnector.Instance.Root.Child("users/" + userEmailWithoutDot + "/friends/" + targetEmailWithoutDot)
            .SetValueAsync(null);
        // Remove target user from current user list in database
        DatabaseConnector.Instance.Root.Child("users/" + targetEmailWithoutDot + "/friends/" + userEmailWithoutDot)
            .SetValueAsync(null);
    }

    public void OnFriendViewclick()
    {
        var template = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        var targetEmail = template.transform.Find("Email").GetComponent<TMP_Text>().text;
        SettingsManager.queryEmail = targetEmail;
        SettingsManager.currentUser = false;
        SettingsManager.state = 0;
        SceneManager.LoadScene("SettingsScene");
    }

    public void OnFriendChatClick()
    {
        var template = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        var targetEmail = template.transform.Find("Email").GetComponent<TMP_Text>().text;

        foreach (var friend in friends.Where(friend => friend.email == targetEmail))
        {
            PlayerPrefs.SetString("LatestChatFriend", friend.nickName);
            PlayerPrefs.Save();
        }

        SceneManager.LoadScene("ChatScene");
    }

    private void refreshRequestList()
    {
        // Update transforms of all request templates once requests changes (accept/ignore)
        for (var i = 0; i < requesters.Count; i++)
        {
            var entryRectTransform = requestEntryTransformList[i].GetComponent<RectTransform>();
            entryRectTransform.anchoredPosition = new Vector2(0, -requestEntryHeight * i);
        }

        if (requestEntryHeight * requesters.Count > 1460)
            // If the request list is short, the default container height is viewport height (1460)
            requestEntryContainer.GetComponent<RectTransform>().sizeDelta =
                new Vector2(800, requestEntryHeight * requesters.Count);
    }

    private void createRequestList()
    {
        StartCoroutine(GetInvitations((List<User> data) =>
        {
            requesters = data;
            foreach (var requester in requesters)
            {
                var entryTransform = Instantiate(requestEntryTemplate, requestEntryContainer);
                var entryRectTransform = entryTransform.GetComponent<RectTransform>();
                entryRectTransform.anchoredPosition =
                    new Vector2(0, -requestEntryHeight * requestEntryTransformList.Count);
                entryTransform.gameObject.SetActive(true);
                entryTransform.Find("Email").GetComponent<TMP_Text>().text = requester.email;
                requestEntryTransformList.Add(entryTransform);
            }

            if (requestEntryHeight * requesters.Count > 1460)
                // If the request list is short, the default container height is viewport height (1460)
                requestEntryContainer.GetComponent<RectTransform>().sizeDelta =
                    new Vector2(800, requestEntryHeight * requesters.Count);
        }));
    }

    public void AddFriendHelpMsg()
    {
        StartCoroutine(CheckUserByEmail());
        Notification.SetActive(true);
    }

    private IEnumerator CheckUserByEmail()
    {
        var email = GameObject.Find("InputEmail").GetComponent<TMP_InputField>().text;
        if (AuthConnector.Instance.CurrentUser.Email == email)
        {
            // Cannot use user email
            notificationText.text = "<color=#f44336>You cannot add yourself as friend";
            yield break;
        }

        var senderEmailWithoutDot = Utilities.removeDot(AuthConnector.Instance.CurrentUser.Email);
        var receiverEmailWithoutDot = Utilities.removeDot(email);
        var userData = DatabaseConnector.Instance.Root.Child("users/" + receiverEmailWithoutDot).GetValueAsync();
        var invitationData = DatabaseConnector.Instance.Root
            .Child("users/" + receiverEmailWithoutDot + "/invitations/" + senderEmailWithoutDot).GetValueAsync();
        yield return new WaitUntil(() => invitationData.IsCompleted && userData.IsCompleted);
        if (invitationData != null && userData != null)
        {
            var invitationSnapshot = invitationData.Result;
            if (!userData.Result.Exists)
            {
                // Invalid email
                notificationText.text = "<color=#f44336>Invalid email, user does not exist.";
            }
            else if (invitationData.Result.Exists)
            {
                // Duplicated reques
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

    public void OnRequestAcceptclick()
    {
        var template = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        var targetEmail = template.transform.Find("Email").GetComponent<TMP_Text>().text;
        var userEmailWithoutDot = Utilities.removeDot(AuthConnector.Instance.CurrentUser.Email);
        var requesterEmailWithoutDot = Utilities.removeDot(targetEmail);
        // Remove invitation from requests and tranform list
        var targetUser = requesters.Find(requester => requester.email == targetEmail);
        requesters.Remove(targetUser);
        Destroy(template);
        requestEntryTransformList.Remove(template.transform);
        refreshRequestList();
        // Remove invitation from database
        DatabaseConnector.Instance.Root
            .Child("users/" + userEmailWithoutDot + "/invitations/" + requesterEmailWithoutDot).SetValueAsync(null);
        // Add friends
        DatabaseConnector.Instance.Root.Child("users/" + userEmailWithoutDot + "/friends/" + requesterEmailWithoutDot)
            .SetValueAsync(true);
        DatabaseConnector.Instance.Root.Child("users/" + requesterEmailWithoutDot + "/friends/" + userEmailWithoutDot)
            .SetValueAsync(true);
        // Add new User to friend and transform list so that friend page can get updated
        friends.Add(targetUser);
        var entryTransform = Instantiate(friendEntryTemplate, friendEntryContainer);
        var entryRectTransform = entryTransform.GetComponent<RectTransform>();
        entryRectTransform.anchoredPosition = new Vector2(0, -friendEntryHeight * friendEntryTransformList.Count);
        entryTransform.gameObject.SetActive(true);
        entryTransform.Find("Email").GetComponent<TMP_Text>().text = targetUser.email;
        friendEntryTransformList.Add(entryTransform);
        refreshFriendList();
    }

    public void OnRequestIgnoreclick()
    {
        var template = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        var targetEmail = template.transform.Find("Email").GetComponent<TMP_Text>().text;
        var userEmailWithoutDot = Utilities.removeDot(AuthConnector.Instance.CurrentUser.Email);
        var requesterEmailWithoutDot = Utilities.removeDot(targetEmail);
        // Remove invitation from requests and tranform list
        requesters.RemoveAll(requester => requester.email == targetEmail);
        Destroy(template);
        requestEntryTransformList.Remove(template.transform);
        refreshRequestList();
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