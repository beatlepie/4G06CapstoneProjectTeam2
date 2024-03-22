using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Firebase.Database;
using UnityEngine;
using Database;

public class EventCarousel : MonoBehaviour
{
    [SerializeField] private EventCarouselView _carouselView;
    public List<Event> allEvents;
    [SerializeField] private string _RoomNumUpperRange;
    [SerializeField] private string _RoomNumLowerRange;

    void Start()
    {
        Setup();
    }

    private void Setup()
    {
        allEvents = new List<Event>();
        StartCoroutine(GetLectures()); 
    }

    private void Cleanup()
    {
        _carouselView.Cleanup();
    }

    IEnumerator GetLectures()
    {                
        var publicEventData = DatabaseConnector.Instance.Root.Child("events/public").GetValueAsync();
        yield return new WaitUntil(predicate: () => publicEventData.IsCompleted);
        if(publicEventData != null)
        {
            DataSnapshot snapshot = publicEventData.Result;
            foreach (var e in snapshot.Children)
            {
               allEvents.Add(Utilities.FormalizeDBEventData(e));
            }
        }
        var privateEventData = DatabaseConnector.Instance.Root.Child("events/private").GetValueAsync();
        yield return new WaitUntil(predicate: () => privateEventData.IsCompleted);
        if(privateEventData != null)
        {
            DataSnapshot snapshot = privateEventData.Result;
            foreach (var e in snapshot.Children)
            {
               allEvents.Add(Utilities.FormalizeDBEventData(e));
            }
        }
            List<EventCarouselData> items = new List<EventCarouselData>();
            List<Event> filteredLectures = FilterLecturesbyRoom(allEvents, _RoomNumLowerRange, _RoomNumUpperRange);
            for(int i = 0; i < filteredLectures.Count; i++)
            {
                var spriteResourceKey = $"tex_demo_banner_{(i%3):D2}";
                var text = filteredLectures[i];
                EventCarouselData item =  new EventCarouselData(spriteResourceKey, filteredLectures[i], () => Debug.Log($"Clicked: {text}"));
                items.Add(item);        
            }
            _carouselView.Setup(items.ToArray());
    }

    public static List<Event> FilterLecturesbyRoom(List<Event> allEvents, string LowerBound, string UpperBound)
    {
        // E.g. room JHE 103 - JHE 124
        // Find the 3 digit room number, check if the part before room number(e.g. JHE A vs JHE ) is the same and compare room number as a integer
        Regex regex = new Regex(@"\d+");
        string roomNumU = regex.Match(UpperBound).Value;
        string roomNumL = regex.Match(LowerBound).Value;
        // Assume upperbound and lowerbound have the same prefix
        string prefix = UpperBound.Split(roomNumU)[0];
        List<Event> filteredEvents = new List<Event>();
        foreach (Event e in allEvents)
        {
            string targetRoomNum = regex.Match(e.location).Value;
            if(string.IsNullOrWhiteSpace(targetRoomNum))
            {
                continue;
            }
            string targetPrefix = e.location.Split(targetRoomNum)[0];
            if (int.Parse(targetRoomNum) >= int.Parse(roomNumL) & int.Parse(targetRoomNum) <= int.Parse(roomNumU) & prefix == targetPrefix)
            {
                filteredEvents.Add(e);
            }
        }
        return filteredEvents;
    }
}

