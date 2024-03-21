using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Firebase.Database;
using UnityEngine;

public class LectureCarousel : MonoBehaviour
{
    [SerializeField] private LectureCarouselView _carouselView;
    public List<Lecture> allLectures;
    [SerializeField] private string _RoomNumUpperRange;
    [SerializeField] private string _RoomNumLowerRange;

    void Start()
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

    IEnumerator GetLectures()
    {                
        var lectureData = DatabaseConnector.Instance.Root.Child("lectures/").GetValueAsync();
        yield return new WaitUntil(predicate: () => lectureData.IsCompleted);
        if(lectureData != null)
        {
            DataSnapshot snapshot = lectureData.Result;
            foreach (var lecture in snapshot.Children)
            {
                allLectures.Add(Utilities.FormalizeDBLectureData(lecture));
            }
            List<LectureCarouselData> items = new List<LectureCarouselData>();
            List<Lecture> filteredLectures = FilterLecturesbyRoom(allLectures, _RoomNumLowerRange, _RoomNumUpperRange);
            for(int i = 0; i < filteredLectures.Count; i++)
            {
                var spriteResourceKey = $"tex_demo_banner_{(i%3):D2}";
                var text = filteredLectures[i];
                LectureCarouselData item =  new LectureCarouselData(spriteResourceKey, filteredLectures[i], () => Debug.Log($"Clicked: {text}"));
                items.Add(item);        
            }
            _carouselView.Setup(items.ToArray());
        }
    }

    public static List<Lecture> FilterLecturesbyRoom(List<Lecture> allLectures, string LowerBound, string UpperBound)
    {
        // E.g. room JHE 103 - JHE 124
        // Find the 3 digit room number, check if the part before room number(e.g. JHE A vs JHE ) is the same and compare room number as a integer
        Regex regex = new Regex(@"\d+");
        string roomNumU = regex.Match(UpperBound).Value;
        string roomNumL = regex.Match(LowerBound).Value;
        // Assume upperbound and lowerbound have the same prefix
        string prefix = UpperBound.Split(roomNumU)[0];
        List<Lecture> filteredLectures = new List<Lecture>();
        foreach (Lecture l in allLectures)
        {
            string targetRoomNum = regex.Match(l.location).Value;
            if(string.IsNullOrWhiteSpace(targetRoomNum))
            {
                continue;
            }
            string targetPrefix = l.location.Split(targetRoomNum)[0];
            if (int.Parse(targetRoomNum) >= int.Parse(roomNumL) & int.Parse(targetRoomNum) <= int.Parse(roomNumU) & prefix == targetPrefix)
            {
                filteredLectures.Add(l);
            }
        }
        return filteredLectures;
    }
}

