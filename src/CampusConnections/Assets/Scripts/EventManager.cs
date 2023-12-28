using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using Firebase;
using Firebase.Database;
using TMPro;

public class EventManager : MonoBehaviour
{
    private DatabaseReference databaseReference;
    public TMP_Text eventList;
    [SerializeField] TMP_InputField eventName;
    [SerializeField] TMP_InputField eventDate;
    [SerializeField] TMP_InputField eventLocation;
    [SerializeField] TMP_InputField eventOrganizer;
    void Start()
    {
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        GetEventData();
    }

    public void WriteNewEvent()
    {
        Event newEvent = new Event(eventName.text, eventDate.text, eventLocation.text, eventOrganizer.text);
        string eventJson = JsonUtility.ToJson(newEvent);
        databaseReference.Child("events").Child(newEvent.name).SetRawJsonValueAsync(eventJson);
    }

    IEnumerator GetEvents(Action<string> onCallBack)
    {
        var eventData = databaseReference.Child("events").OrderByChild("date").GetValueAsync();
        yield return new WaitUntil(predicate: () => eventData.IsCompleted);
        if(eventData != null)
        {
            string result = "";
            DataSnapshot snapshot = eventData.Result;
            foreach (var x in snapshot.Children)
            {
                foreach (var i in x.Children)
                {
                    result += i.Value + " ";
                }
                result += "\n";
            }
            onCallBack.Invoke(result);
        }
    }

    public void GetEventData()
    {
        StartCoroutine(GetEvents((string data) =>
        {
            eventList.text = data;
        }));
    }

    public void ExitDataPage()
    {
        SceneManager.LoadScene("LoginScene");
    }
}
