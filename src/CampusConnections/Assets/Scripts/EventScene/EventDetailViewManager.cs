using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventDetailViewManager : MonoBehaviour
{
    [Header("Firebase")]
    public FirebaseAuth auth;
    public FirebaseUser currentUser;
    public DatabaseReference databaseReference;

    [Header("DetailView")]
    [SerializeField] GameObject ViewPanel;
    [SerializeField] GameObject EditPanel;
    private Event target;
    private bool pinned;
    private List<string> myEventNames = new List<string>();
    [SerializeField] GameObject PinIcon;
    [SerializeField] GameObject UnpinIcon;

    [Header("ViewPanel")]
    [SerializeField] TMP_Text viewName;
    [SerializeField] TMP_Text viewDescription;
    [SerializeField] TMP_Text viewOrganizer;
    [SerializeField] TMP_Text viewLocation;
    [SerializeField] TMP_Text viewTime;
    [SerializeField] TMP_Text viewDuration;
    [SerializeField] Toggle viewIsPublic;

    [Header("EditPanel")]
    [SerializeField] TMP_InputField editName;
    [SerializeField] TMP_InputField editDesciprtion;
    [SerializeField] TMP_InputField editOrganizer;
    [SerializeField] TMP_InputField editLocation;
    [SerializeField] TMP_InputField editTime;
    [SerializeField] TMP_InputField editDuration;
    [SerializeField] Toggle editIsPublic;

    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        currentUser = auth.CurrentUser;
        StartCoroutine(GetPinnedLectures((List<string> data) =>
        {
            myEventNames = data;
        }));
    }
    IEnumerator GetPinnedLectures(Action<List<string>> onCallBack)
    {
        string emailWithoutDot = Utilities.removeDot(currentUser.Email);                
        var userData = databaseReference.Child("users/" + emailWithoutDot + "/events").GetValueAsync();
        yield return new WaitUntil(predicate: () => userData.IsCompleted);
        if(userData != null)
        {
            List<string> pinnedLectures = new List<string>();
            DataSnapshot snapshot = userData.Result;
            foreach (var x in snapshot.Children)
            {
                pinnedLectures.Add(x.Key.ToString());
            }
            onCallBack.Invoke(pinnedLectures);
        }
    }
    void OnEnable()
    {
        target = EventManager.currentEvent;
        pinned = myEventNames.Contains(target.name);
        PinIcon.SetActive(!pinned);
        UnpinIcon.SetActive(pinned);
        UpdateView();
    }

    void UpdateView()
    {
        viewName.text = target.name;
        viewDescription.text = target.description;
        viewOrganizer.text = target.organizer;
        viewLocation.text = target.location;
        viewTime.text = DateTimeOffset.FromUnixTimeSeconds(target.time).ToLocalTime().ToString("MM/dd/yyyy HH:mm");
        viewDuration.text = target.duration.ToString();
        viewIsPublic.isOn = target.isPublic;
        editName.text = target.name;
        editDesciprtion.text = target.description;
        editOrganizer.text = target.organizer;
        editLocation.text = target.location;
        editTime.text = DateTimeOffset.FromUnixTimeSeconds(target.time).ToLocalTime().ToString("MM/dd/yyyy HH:mm");
        editDuration.text = target.duration.ToString();
        editIsPublic.isOn = target.isPublic;
    }

    public void SaveChanges()
    {
        target.name = editName.text;
        target.description = editDesciprtion.text;
        target.organizer = editOrganizer.text;
        target.location = editLocation.text;
        DateTimeOffset dto = new DateTimeOffset(DateTime.Parse(editTime.text).ToUniversalTime());
        target.time = dto.ToUnixTimeSeconds();
        target.duration = int.Parse(editDuration.text);
        target.isPublic = editIsPublic.isOn;
        UpdateView();
        string targetJson = JsonUtility.ToJson(target);
        string prefix = target.isPublic ? "events/public/" : "events/private/";
        databaseReference.Child(prefix + target.name).SetRawJsonValueAsync(targetJson);
    }

    public void Pin()
    {
        myEventNames.Add(target.name);
        PinIcon.SetActive(false);
        UnpinIcon.SetActive(true);
        string emailWithoutDot = Utilities.removeDot(currentUser.Email);
        databaseReference.Child("users/" + emailWithoutDot + "/events/" + target.name).SetValueAsync("True");
    }

    public void Unpin()
    {
        myEventNames.Remove(target.name);
        PinIcon.SetActive(true);
        UnpinIcon.SetActive(false);
        string emailWithoutDot = Utilities.removeDot(currentUser.Email);
        databaseReference.Child("users/" + emailWithoutDot + "/events/" + target.name).SetValueAsync(null);
    }

    public void DetailViewClose()
    {
        ViewPanel.SetActive(true);
        EditPanel.SetActive(false);
    }
}
