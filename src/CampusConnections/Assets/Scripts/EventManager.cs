using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using TMPro;

public class EventManager : MonoBehaviour
{
    private DatabaseReference databaseReference;
    public TMP_Text eventList;
    public GameObject scrollView;
    public RectTransform eventsTextRectTransform;
    public UnityEngine.UI.Text errorMessage;
    private Vector2 originalPosition;
    [SerializeField] TMP_InputField eventName;
    [SerializeField] TMP_InputField eventDate;
    [SerializeField] TMP_InputField eventLocation;
    [SerializeField] TMP_InputField eventOrganizer;
    void Start()
    {
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        originalPosition = eventsTextRectTransform.anchoredPosition;
        databaseReference.Child("events").ChildAdded += HandleChildAdded;
        GetEventData();
    }

    void Update()
    {
        if (scrollView.activeSelf)
        {
            eventsTextRectTransform.anchoredPosition = new Vector2(originalPosition.x, originalPosition.y + 300);
        }
        else
        {
            eventsTextRectTransform.anchoredPosition = originalPosition;
        }
    }

    void HandleChildAdded(object sender, ChildChangedEventArgs args)
{
    if (args.DatabaseError != null)
    {
        Debug.LogError(args.DatabaseError.Message);
        return;
    }
    StartCoroutine(GetEvents((result) => {
        eventList.text = result;
    }));
}

    public void WriteNewEvent()
{
    DateTime parsedDate;
    if (!DateTime.TryParseExact(eventDate.text, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out parsedDate))
    {
        errorMessage.text = "Invalid date format. Please use DD/MM/YYYY.";
        return;
    }
    
    string formattedDate = parsedDate.ToString("yyyy/MM/dd");
    Event newEvent = new Event(eventName.text, formattedDate, eventLocation.text, eventOrganizer.text);
    string eventJson = JsonUtility.ToJson(newEvent);
    databaseReference.Child("events").Child(newEvent.name).SetRawJsonValueAsync(eventJson);

    errorMessage.text = "Event Added!";
    errorMessage.color = Color.green;
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
            string date = "";
            string location = "";
            string name = "";
            string organizer = "";

            foreach (var i in x.Children)
            {
                switch (i.Key)
                {
                    case "date":
                        DateTime parsedDate;
                        if (DateTime.TryParseExact(i.Value.ToString(), "yyyy/MM/dd", null, System.Globalization.DateTimeStyles.None, out parsedDate))
                        {
                            date = "Date: " + parsedDate.ToString("dd/MM/yyyy");
                        }
                        break;
                    case "location":
                        location = "Location: " + i.Value.ToString();
                        break;
                    case "name":
                        name = "Name: " + i.Value.ToString();
                        break;
                    case "organizer":
                        organizer = "Organizer: " + i.Value.ToString();
                        break;
                }
            }

            result += $"{name}\n{date}\n{location}\n{organizer}\n\n";
        }
        onCallBack.Invoke(result);
    }
}

    public ScrollRect scrollRect;
    public void GetEventData()
    {
        StartCoroutine(GetEvents((string data) =>
        {
            eventList.text = data;
            scrollRect.normalizedPosition = new Vector2(0, 1);
        }));
    }

    public void ExitDataPage()
    {
        SceneManager.LoadScene("LoginScene");
    }

    void OnDestroy()
{
    databaseReference.Child("events").ChildAdded -= HandleChildAdded;
}
}
