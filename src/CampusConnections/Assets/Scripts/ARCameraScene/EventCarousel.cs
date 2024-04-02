using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Database;
using UnityEngine.Serialization;

public class EventCarousel : MonoBehaviour
{
    [FormerlySerializedAs("_carouselView")] [SerializeField] private EventCarouselView carouselView;
    private List<Event> _allEvents;
    [FormerlySerializedAs("_RoomNumUpperRange")] [SerializeField] private string roomNumUpperRange;
    [FormerlySerializedAs("_RoomNumLowerRange")] [SerializeField] private string roomNumLowerRange;

    private void Start()
    {
        Setup();
    }

    private void Setup()
    {
        _allEvents = new List<Event>();
        StartCoroutine(GetLectures());
    }

    private void Cleanup()
    {
        carouselView.Cleanup();
    }

    private IEnumerator GetLectures()
    {
        var publicEventData = DatabaseConnector.Instance.Root.Child("events/public").GetValueAsync();
        yield return new WaitUntil(() => publicEventData.IsCompleted);
        if (publicEventData != null)
        {
            var snapshot = publicEventData.Result;
            foreach (var e in snapshot.Children) _allEvents.Add(Utilities.FormalizeDBEventData(e));
        }

        var privateEventData = DatabaseConnector.Instance.Root.Child("events/private").GetValueAsync();
        yield return new WaitUntil(() => privateEventData.IsCompleted);
        if (privateEventData != null)
        {
            var snapshot = privateEventData.Result;
            foreach (var e in snapshot.Children) _allEvents.Add(Utilities.FormalizeDBEventData(e));
        }

        var items = new List<EventCarouselData>();
        var filteredEvents = FilterLecturesByRoom(_allEvents, roomNumLowerRange, roomNumUpperRange);
        for (var i = 0; i < filteredEvents.Count; i++)
        {
            var spriteResourceKey = $"tex_demo_banner_{(i + 2) % 3:D2}";
            var text = filteredEvents[i];
            var item = new EventCarouselData(spriteResourceKey, filteredEvents[i], () => Debug.Log($"Clicked: {text}"));
            items.Add(item);
        }

        carouselView.Setup(items.ToArray());
    }

    private static List<Event> FilterLecturesByRoom(List<Event> allEvents, string lowerBound, string upperBound)
    {
        // E.g. room JHE 103 - JHE 124
        // Find the 3 digit room number, check if the part before room number(e.g. JHE A vs JHE ) is the same and compare room number as a integer
        var regex = new Regex(@"\d+");
        var roomNumU = regex.Match(upperBound).Value;
        var roomNumL = regex.Match(lowerBound).Value;
        // Assume upperbound and lower bound have the same prefix
        var prefix = upperBound.Split(roomNumU)[0];
        var filteredEvents = new List<Event>();
        foreach (var e in allEvents)
        {
            var targetRoomNum = regex.Match(e.Location).Value;
            if (string.IsNullOrWhiteSpace(targetRoomNum)) continue;
            var targetPrefix = e.Location.Split(targetRoomNum)[0];
            if ((int.Parse(targetRoomNum) >= int.Parse(roomNumL)) & (int.Parse(targetRoomNum) <= int.Parse(roomNumU)) &
                (prefix == targetPrefix)) filteredEvents.Add(e);
        }

        return filteredEvents;
    }
}