using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Firebase.Database;
using UnityEngine;
using Database;

public class LectureCarousel : MonoBehaviour
{
    [SerializeField] private LectureCarouselView _carouselView;
    public List<Lecture> allLectures;
    [SerializeField] private string _RoomNumUpperRange;
    [SerializeField] private string _RoomNumLowerRange;

    private void Start()
    {
        Setup();
    }

    private void Setup()
    {
        allLectures = new List<Lecture>();
        StartCoroutine(GetLectures());
    }

    private void Cleanup()
    {
        _carouselView.Cleanup();
    }

    private IEnumerator GetLectures()
    {
        var lectureData = DatabaseConnector.Instance.Root.Child("lectures/").GetValueAsync();
        yield return new WaitUntil(() => lectureData.IsCompleted);
        if (lectureData != null)
        {
            var snapshot = lectureData.Result;
            foreach (var lecture in snapshot.Children) allLectures.Add(Utilities.FormalizeDBLectureData(lecture));
            var items = new List<LectureCarouselData>();
            var filteredLectures = FilterLecturesbyRoom(allLectures, _RoomNumLowerRange, _RoomNumUpperRange);
            for (var i = 0; i < filteredLectures.Count; i++)
            {
                var spriteResourceKey = $"tex_demo_banner_{i % 3:D2}";
                var text = filteredLectures[i];
                var item = new LectureCarouselData(spriteResourceKey, filteredLectures[i],
                    () => Debug.Log($"Clicked: {text}"));
                items.Add(item);
            }

            _carouselView.Setup(items.ToArray());
        }
    }

    public static List<Lecture> FilterLecturesbyRoom(List<Lecture> allLectures, string LowerBound, string UpperBound)
    {
        // E.g. room JHE 103 - JHE 124
        // Find the 3 digit room number, check if the part before room number(e.g. JHE A vs JHE ) is the same and compare room number as a integer
        var regex = new Regex(@"\d+");
        var roomNumU = regex.Match(UpperBound).Value;
        var roomNumL = regex.Match(LowerBound).Value;
        // Assume upperbound and lowerbound have the same prefix
        var prefix = UpperBound.Split(roomNumU)[0];
        var filteredLectures = new List<Lecture>();
        foreach (var l in allLectures)
        {
            var targetRoomNum = regex.Match(l.location).Value;
            if (string.IsNullOrWhiteSpace(targetRoomNum)) continue;
            var targetPrefix = l.location.Split(targetRoomNum)[0];
            if ((int.Parse(targetRoomNum) >= int.Parse(roomNumL)) & (int.Parse(targetRoomNum) <= int.Parse(roomNumU)) &
                (prefix == targetPrefix)) filteredLectures.Add(l);
        }

        return filteredLectures;
    }
}