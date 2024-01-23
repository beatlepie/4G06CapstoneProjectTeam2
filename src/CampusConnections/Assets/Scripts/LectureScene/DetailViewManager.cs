using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using UnityEngine;

public class DetailViewManager : MonoBehaviour
{
    [Header("Firebase")]
    public FirebaseAuth auth;
    public FirebaseUser currentUser;
    public DatabaseReference databaseReference;

    [Header("DetailView")]
    [SerializeField] GameObject ViewPanel;
    [SerializeField] GameObject EditPanel;
    private Lecture target;
    private Boolean pinned;
    private List<string> myLectureCodes = new List<string>();
    [SerializeField] GameObject PinIcon;
    [SerializeField] GameObject UnpinIcon;
    [SerializeField] TMP_Text viewCode;
    [SerializeField] TMP_Text viewName;
    [SerializeField] TMP_Text viewInstructor;
    [SerializeField] TMP_Text viewLocation;
    [SerializeField] TMP_Text viewTimes;
    [SerializeField] TMP_InputField editCode;
    [SerializeField] TMP_InputField editName;
    [SerializeField] TMP_InputField editInstructor;
    [SerializeField] TMP_InputField editLocation;
    [SerializeField] TMP_InputField editTimes;

    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        currentUser = auth.CurrentUser;
        StartCoroutine(GetPinnedLectures((List<string> data) =>
        {
            myLectureCodes = data;
        }));
    }
    IEnumerator GetPinnedLectures(Action<List<string>> onCallBack)
    {
        string emailWithoutDot = Utilities.removeDot(currentUser.Email);                
        var userData = databaseReference.Child("users/" + emailWithoutDot + "/lectures").GetValueAsync();
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
        target = LectureManager.currentLecture;
        pinned = myLectureCodes.Contains(target.code);
        PinIcon.SetActive(!pinned);
        UnpinIcon.SetActive(pinned);
        UpdateView();
    }

    void UpdateView()
    {
        viewCode.text = target.code;
        viewName.text = target.name;
        viewInstructor.text = target.instructor;
        viewLocation.text = target.location;
        viewTimes.text = target.time;
        editCode.text = target.code;
        editName.text = target.name;
        editInstructor.text = target.instructor;
        editLocation.text = target.location;
        editTimes.text = target.time;
    }

    public void SaveChanges()
    {
        target.code = editCode.text;
        target.name = editName.text;
        target.instructor = editInstructor.text;
        target.location = editLocation.text;
        target.time = editTimes.text;
        UpdateView();
        string targetJson = JsonUtility.ToJson(target);
        databaseReference.Child("lectures/" + target.code).SetRawJsonValueAsync(targetJson);
    }

    public void Pin()
    {
        myLectureCodes.Add(target.code);
        PinIcon.SetActive(false);
        UnpinIcon.SetActive(true);
        string emailWithoutDot = Utilities.removeDot(currentUser.Email);
        databaseReference.Child("users/" + emailWithoutDot + "/lectures/" + target.code).SetValueAsync("True");
    }

    public void Unpin()
    {
        myLectureCodes.Remove(target.code);
        PinIcon.SetActive(true);
        UnpinIcon.SetActive(false);
        string emailWithoutDot = Utilities.removeDot(currentUser.Email);
        databaseReference.Child("users/" + emailWithoutDot + "/lectures/" + target.code).SetValueAsync(null);
    }

    public void DetailViewClose()
    {
        ViewPanel.SetActive(true);
        EditPanel.SetActive(false);
    }
}
