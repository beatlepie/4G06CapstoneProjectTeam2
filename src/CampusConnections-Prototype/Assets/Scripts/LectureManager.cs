using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using Firebase;
using Firebase.Database;
using TMPro;

public class LectureManager : MonoBehaviour
{   
    private Transform entryContainer;
    private Transform entryTemplate;
    private List<LectureEntry> lectureEntryList;
    private List<Transform> lectureEntryTransformList;

    private DatabaseReference databaseReference;
    public TMP_Text lecList;

    public TMP_Text pgNum;

    private void Awake()
    {
        UnityEngine.Debug.Log("lecture manager script running");
        //db setup
        Firebase.AppOptions options = new Firebase.AppOptions();
        options.ApiKey = "AIzaSyADnG2YOg9G7q9pddYlTthUj9G16G8dlOE";
        options.AppId = "1:89992135088:android:0f8c9a92bf587c521e5675";
        options.ProjectId = "campusconnections";
        options.DatabaseUrl = new System.Uri("https://campusconnections-default-rtdb.firebaseio.com");
        options.StorageBucket = "campusconnections.appspot.com";
        FirebaseApp.Create(options);
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;



        //after db stuff

        pgNum.text = "0";
        entryContainer = transform.Find("lectureEntryContainer");
        entryTemplate = entryContainer.Find("lectureEntryTemplate");
        entryTemplate.gameObject.SetActive(false);
        lectureEntryTransformList = new List<Transform>();


        GetLectureData();

    }

    private void CreateLectureEntryTransform(LectureEntry lectureEntry, Transform container, List<Transform> transformList){
        float templateHeight = 90f;
        Transform entryTransform = Instantiate(entryTemplate, container);
        RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
        entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * transformList.Count);
        entryTransform.gameObject.SetActive(true);

        int ind = transformList.Count + 1; //count for each entry starting at 1

        entryTransform.Find("nameText").GetComponent<TMP_Text>().text = lectureEntry.name;
        entryTransform.Find("codeText").GetComponent<TMP_Text>().text = lectureEntry.code;
        entryTransform.Find("instrucText").GetComponent<TMP_Text>().text = lectureEntry.instructor;
        entryTransform.Find("locText").GetComponent<TMP_Text>().text = lectureEntry.location;
        entryTransform.Find("timeText").GetComponent<TMP_Text>().text = lectureEntry.time;

        entryTransform.Find("entryBG").gameObject.SetActive(ind % 2 == 1);  //alternate bg

        transformList.Add(entryTransform);
    }

    public void clearing()
    {           
        foreach (Transform entryTransform in lectureEntryTransformList)
        {
            Destroy(entryTransform.gameObject);
        }

        lectureEntryTransformList.Clear();  //even after destroying size isnt 0 so we have to clear

    }

    IEnumerator GetLectures(Action<string> onCallBack)
    {
        var lecInfo = new List<string>();
        lectureEntryList = new List<LectureEntry>();




        var lectureData = databaseReference.Child("lectures").OrderByKey().StartAt("-").LimitToFirst(6).GetValueAsync();
        yield return new WaitUntil(predicate: () => lectureData.IsCompleted);
        if (lectureData != null)
        {
            string result = "";
            DataSnapshot snapshot = lectureData.Result;
            foreach (var x in snapshot.Children)
            {
                foreach (var i in x.Children)
                {
                    result += i.Value + " ";
                    lecInfo.Add(i.Value.ToString());
                    //UnityEngine.Debug.Log(lecInfo);
                }
                result += "\n";
                // make a lectureEntry object with constructor  (lecInfo[0],lecInfo[1]...)
                LectureEntry newEntry = new LectureEntry();
                newEntry.code = lecInfo[0];
                newEntry.instructor = lecInfo[1];
                newEntry.location = lecInfo[2];
                newEntry.name = lecInfo[3];
                newEntry.time = lecInfo[4];

                //UnityEngine.Debug.Log(newEntry.code);
                
                lectureEntryList.Add(newEntry);
                //empty lecInfo for next iteration
                lecInfo.Clear();

                //CreateLectureEntryTransform(newEntry, entryContainer, lectureEntryTransformList);


            }

            foreach (LectureEntry lectureEntry in lectureEntryList)
            {
                CreateLectureEntryTransform(lectureEntry, entryContainer, lectureEntryTransformList);
            }

            onCallBack.Invoke(result);
        }
    }

    public void GetLectureData()
    {
        StartCoroutine(GetLectures((string data) =>
        {
            //lecList.text = data;
        }));
    }

    private class LectureEntry
    {
        public string code;
        public string instructor;
        public string location;
        public string name;
        public string time;
    }

}


