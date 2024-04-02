using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Firebase.Database;
using UnityEngine;
using Database;
using UnityEngine.Serialization;

public class LectureCarousel : MonoBehaviour
{
    [FormerlySerializedAs("_carouselView")] [SerializeField] private LectureCarouselView carouselView;
    private List<Lecture> _allLectures;
    [FormerlySerializedAs("_RoomNumUpperRange")] [SerializeField] private string roomNumUpperRange;
    [FormerlySerializedAs("_RoomNumLowerRange")] [SerializeField] private string roomNumLowerRange;

    private void Start()
    {
        Setup();
    }

    private void Setup()
    {
        _allLectures = new List<Lecture>();
        StartCoroutine(GetLectures());
    }

    private void Cleanup()
    {
        carouselView.Cleanup();
    }

    private IEnumerator GetLectures()
    {
        var lectureData = DatabaseConnector.Instance.Root.Child("lectures/").GetValueAsync();
        yield return new WaitUntil(() => lectureData.IsCompleted);
        if (lectureData != null)
        {
            var snapshot = lectureData.Result;
            foreach (var lecture in snapshot.Children) _allLectures.Add(Utilities.FormalizeDBLectureData(lecture));
            var items = new List<LectureCarouselData>();
            var filteredLectures = FilterLecturesByRoom(_allLectures, roomNumLowerRange, roomNumUpperRange);
            for (var i = 0; i < filteredLectures.Count; i++)
            {
                var spriteResourceKey = $"tex_demo_banner_{i % 3:D2}";
                var text = filteredLectures[i];
                var item = new LectureCarouselData(spriteResourceKey, filteredLectures[i],
                    () => Debug.Log($"Clicked: {text}"));
                items.Add(item);
            }

            carouselView.Setup(items.ToArray());
        }
    }

    private static List<Lecture> FilterLecturesByRoom(List<Lecture> allLectures, string lowerBound, string upperBound)
    {
        // E.g. room JHE 103 - JHE 124
        // Find the 3 digit room number, check if the part before room number(e.g. JHE A vs JHE ) is the same and compare room number as a integer
        var regex = new Regex(@"\d+");
        var roomNumU = regex.Match(upperBound).Value;
        var roomNumL = regex.Match(lowerBound).Value;
        // Assume upper bound and lower bound have the same prefix
        var prefix = upperBound.Split(roomNumU)[0];
        var filteredLectures = new List<Lecture>();
        foreach (var l in allLectures)
        {
            var targetRoomNum = regex.Match(l.Location).Value;
            if (string.IsNullOrWhiteSpace(targetRoomNum)) continue;
            var targetPrefix = l.Location.Split(targetRoomNum)[0];
            if ((int.Parse(targetRoomNum) >= int.Parse(roomNumL)) & (int.Parse(targetRoomNum) <= int.Parse(roomNumU)) &
                (prefix == targetPrefix)) filteredLectures.Add(l);
        }

        return filteredLectures;
    }
}