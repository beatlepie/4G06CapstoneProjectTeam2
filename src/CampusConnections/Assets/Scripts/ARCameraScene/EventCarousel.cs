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

    private void Start()
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

    private IEnumerator GetLectures()
    {
        var publicEventData = DatabaseConnector.Instance.Root.Child("events/public").GetValueAsync();
        yield return new WaitUntil(() => publicEventData.IsCompleted);
        if (publicEventData != null)
        {
            var snapshot = publicEventData.Result;
            foreach (var e in snapshot.Children) allEvents.Add(Utilities.FormalizeDBEventData(e));
        }

        var privateEventData = DatabaseConnector.Instance.Root.Child("events/private").GetValueAsync();
        yield return new WaitUntil(() => privateEventData.IsCompleted);
        if (privateEventData != null)
        {
            var snapshot = privateEventData.Result;
            foreach (var e in snapshot.Children) allEvents.Add(Utilities.FormalizeDBEventData(e));
        }

        var items = new List<EventCarouselData>();
        var filteredEvents = FilterLecturesbyRoom(allEvents, _RoomNumLowerRange, _RoomNumUpperRange);
        for (var i = 0; i < filteredEvents.Count; i++)
        {
            var spriteResourceKey = $"tex_demo_banner_{(i + 2) % 3:D2}";
            var text = filteredEvents[i];
            var item = new EventCarouselData(spriteResourceKey, filteredEvents[i], () => Debug.Log($"Clicked: {text}"));
            items.Add(item);
        }

        _carouselView.Setup(items.ToArray());
    }

    public static List<Event> FilterLecturesbyRoom(List<Event> allEvents, string LowerBound, string UpperBound)
    {
        // E.g. room JHE 103 - JHE 124
        // Find the 3 digit room number, check if the part before room number(e.g. JHE A vs JHE ) is the same and compare room number as a integer
        var regex = new Regex(@"\d+");
        var roomNumU = regex.Match(UpperBound).Value;
        var roomNumL = regex.Match(LowerBound).Value;
        // Assume upperbound and lowerbound have the same prefix
        var prefix = UpperBound.Split(roomNumU)[0];
        var filteredEvents = new List<Event>();
        foreach (var e in allEvents)
        {
            var targetRoomNum = regex.Match(e.location).Value;
            if (string.IsNullOrWhiteSpace(targetRoomNum)) continue;
            var targetPrefix = e.location.Split(targetRoomNum)[0];
            if ((int.Parse(targetRoomNum) >= int.Parse(roomNumL)) & (int.Parse(targetRoomNum) <= int.Parse(roomNumU)) &
                (prefix == targetPrefix)) filteredEvents.Add(e);
        }

        return filteredEvents;
    }
}