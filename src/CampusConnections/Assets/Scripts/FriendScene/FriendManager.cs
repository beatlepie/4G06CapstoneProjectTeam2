using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using UnityEngine;

public class FriendManager : MonoBehaviour
{
    [Header("Firebase")]
    public FirebaseAuth auth;
    public FirebaseUser currentUser;
    public DatabaseReference databaseReference;

    [Header("Chat")]
    public List<string> friends;
    [SerializeField] Transform entryContainer;
    [SerializeField] Transform entryTemplate;
    private List<Transform> FriendEntryTransformList;


    [Header("Invitation")]
    public List<string> requests;

    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        currentUser = auth.CurrentUser;
        FriendEntryTransformList = new List<Transform>();
        StartCoroutine(GetFriends((List<string> data) =>
        {
            friends = data;
            createChatList(friends);
            if(300 * friends.Count > 1460){
                // If the friend list is short, the default container height is viewport height (1460)
                entryContainer.GetComponent<RectTransform>().sizeDelta = new Vector2 (800, 300*friends.Count);
            }
        }));
        StartCoroutine(GetInvitations((List<string> data) =>
        {
            requests = data;
        }));
    }

    IEnumerator GetFriends(Action<List<string>> onCallBack)
    {
        int indexOfDot = currentUser.Email.LastIndexOf('.');
        string emailWithoutDot = currentUser.Email.Substring(0, indexOfDot) + "_" + currentUser.Email.Substring(indexOfDot + 1);                
        var userData = databaseReference.Child("users/" + emailWithoutDot + "/friends").GetValueAsync();
        // var userData = databaseReference.Child("users/test2@test_com/friends").GetValueAsync();
        yield return new WaitUntil(predicate: () => userData.IsCompleted);
        if(userData != null)
        {
            List<string> friends = new List<string>();
            DataSnapshot snapshot = userData.Result;
            foreach (var x in snapshot.Children)
            {
                friends.Add(x.Value.ToString());
            }
            onCallBack.Invoke(friends);
        }
    }

    IEnumerator GetInvitations(Action<List<string>> onCallBack)
    {
        int indexOfDot = currentUser.Email.LastIndexOf('.');
        string emailWithoutDot = currentUser.Email.Substring(0, indexOfDot) + "_" + currentUser.Email.Substring(indexOfDot + 1);                
        var userData = databaseReference.Child("users/" + emailWithoutDot + "/invitations").GetValueAsync();
        // var userData = databaseReference.Child("users/test2@test_com/invitations").GetValueAsync();
        yield return new WaitUntil(predicate: () => userData.IsCompleted);
        if(userData != null)
        {
            List<string> requests = new List<string>();
            DataSnapshot snapshot = userData.Result;
            foreach (var x in snapshot.Children)
            {
                requests.Add(x.Value.ToString());
            }
            onCallBack.Invoke(requests);
        }
    }

    private void createChatList(List<string> friends)
    {
        foreach (string friendEmail in friends)
        {
            Transform entryTransform = Instantiate(entryTemplate, entryContainer);
            RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
            entryRectTransform.anchoredPosition = new Vector2(0, -300 * FriendEntryTransformList.Count);
            entryTransform.gameObject.SetActive(true);
            entryTransform.Find("Email").GetComponent<TMP_Text>().text = friendEmail;
            entryTransform.Find("Name").GetComponent<TMP_Text>().text = "Alice";
            FriendEntryTransformList.Add(entryTransform);  
        }
        
    }

    public void onClickFriend()
    {
        Debug.Log("Enter Chat");
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
        string email = GameObject.Find("Email").GetComponent<TMP_InputField>().text;
        if(currentUser.Email == email) {
            // Cannot use user email
            onCallBack.Invoke("<color=#f44336>You cannot add yourself as friend");
            yield break;
        }
        int indexOfDot = email.LastIndexOf('.');
        string emailWithoutDot = indexOfDot == -1 ? email : email.Substring(0, indexOfDot) + "_" + email.Substring(indexOfDot + 1);
        var userData = databaseReference.Child("users/" + emailWithoutDot).GetValueAsync();
        var invitationData = databaseReference.Child("users/" + emailWithoutDot + "/invitations").GetValueAsync();
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
            else
            {
                foreach (var x in invitationSnapshot.Children)
                {
                    // Duplicated request
                    if (currentUser.Email == x.Value.ToString())
                    {
                        helpMsg = "<color=#ff9800>Friend request has been sent, please wait";
                        onCallBack.Invoke(helpMsg);
                        yield break;
                    }
                }
                helpMsg = "<color=#4caf50>Success! Request is sent";
                int index = (int)invitationSnapshot.ChildrenCount + 1;
                databaseReference.Child("users/" + emailWithoutDot + "/invitations/" + index).SetValueAsync(currentUser.Email);
                onCallBack.Invoke(helpMsg);
            }
        }
    }
}
