using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Firebase.Database;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ARCameraInfo : MonoBehaviour
{
    public static ARCameraInfo Instance { get; private set; }
    public DatabaseReference databaseReference;
    public List<Lecture> allLectures;
    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        allLectures = new List<Lecture>();
        StartCoroutine(GetLectures());
    }

    IEnumerator GetLectures()
    {                
        var lectureData = databaseReference.Child("lectures/").GetValueAsync();
        yield return new WaitUntil(predicate: () => lectureData.IsCompleted);
        if(lectureData != null)
        {
            List<Lecture> lectures = new List<Lecture>();
            DataSnapshot snapshot = lectureData.Result;
            foreach (var lecture in snapshot.Children)
            {
                var lecInfo = new List<string>();
                foreach (var i in lecture.Children)
                {
                    lecInfo.Add(i.Value.ToString());
                }
                Lecture newEntry = new Lecture(lecInfo[0], lecInfo[1], lecInfo[2], lecInfo[3], lecInfo[4]);
                allLectures.Add(newEntry);
            }
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
            string targetPrefix = l.location.Split(targetRoomNum)[0];
            if (Int32.Parse(targetRoomNum) > Int32.Parse(roomNumL) & Int32.Parse(targetRoomNum) < Int32.Parse(roomNumU) & prefix == targetPrefix)
            {
                filteredLectures.Add(l);
            }
        }
        return filteredLectures;
    }

    public void OnBack()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
