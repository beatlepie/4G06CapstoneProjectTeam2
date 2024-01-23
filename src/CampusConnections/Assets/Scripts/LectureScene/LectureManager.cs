using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Firebase;
using Firebase.Database;
using TMPro;
using UnityEngine.EventSystems;


public class LectureManager : MonoBehaviour
{

    [SerializeField] TMP_Dropdown FilterDropdown;
    [SerializeField] TMP_InputField SearchString;

    private Transform entryContainer;
    private Transform entryTemplate;
    private List<Lecture> lectureEntryList;
    public static Lecture currentLecture; //The one we want to see details
    private List<Transform> lectureEntryTransformList;

    private DatabaseReference databaseReference;
    public TMP_Text lecList;

    public TMP_Text pgNum;

    public int maxPages;

    private void Awake()
    {
        UnityEngine.Debug.Log("lecture manager script running");
        //db setup
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        //after db stuff

        pgNum.text = "1";
        entryContainer = transform.Find("lectureEntryContainer");
        entryTemplate = entryContainer.Find("lectureEntryTemplate");
        entryTemplate.gameObject.SetActive(false);
        lectureEntryTransformList = new List<Transform>();


        GetLectureData();

    }

    private void CreateLectureEntryTransform(Lecture lectureEntry, Transform container, List<Transform> transformList)
    {
        float templateHeight = 130f;
        Transform entryTransform = Instantiate(entryTemplate, container);
        RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
        entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * transformList.Count);
        entryTransform.gameObject.SetActive(true);

        int ind = transformList.Count + 1; //count for each entry starting at 1

        //entryTransform.Find("nameText").GetComponent<TMP_Text>().text = lectureEntry.name;
        entryTransform.Find("codeText").GetComponent<TMP_Text>().text = lectureEntry.code;
        entryTransform.Find("instrucText").GetComponent<TMP_Text>().text = lectureEntry.instructor;
        entryTransform.Find("locText").GetComponent<TMP_Text>().text = lectureEntry.location;
        //entryTransform.Find("timeText").GetComponent<TMP_Text>().text = lectureEntry.time;

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
        lectureEntryList = new List<Lecture>();

        var entriesPerPage = 10;

        var lectureData = databaseReference.Child("lectures").OrderByKey().StartAt("-").LimitToFirst(60).GetValueAsync();



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
                Lecture newEntry = new Lecture(lecInfo[0], lecInfo[1], lecInfo[2], lecInfo[3], lecInfo[4]);
                lectureEntryList.Add(newEntry);
                //empty lecInfo for next iteration
                lecInfo.Clear();

                //CreateLectureEntryTransform(newEntry, entryContainer, lectureEntryTransformList);


            }



            maxPages = (int)(lectureEntryList.Count / entriesPerPage);
            //UnityEngine.Debug.Log(maxPages);

            for (int i = ((Int32.Parse(pgNum.text) - 1) * entriesPerPage); i < Math.Min((Int32.Parse(pgNum.text)) * entriesPerPage, lectureEntryList.Count); i++)
            {
                if (lectureEntryList[i] != null)
                {
                    //UnityEngine.Debug.Log(i);
                    Lecture lectureEntry = lectureEntryList[i];
                    CreateLectureEntryTransform(lectureEntry, entryContainer, lectureEntryTransformList);
                }
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

    public void nextPage()
    {
        if (Int32.Parse(pgNum.text) > maxPages)
        {
            return;
        }
        //clearing();
        pgNum.text = (Int32.Parse(pgNum.text) + 1).ToString();
        //GetLectureData();
    }

    public void prevPage()
    {
        if (Int32.Parse(pgNum.text) < 2)
        {
            return;
        }
        //clearing();
        pgNum.text = (Int32.Parse(pgNum.text) - 1).ToString();
        //GetLectureData();
    }

    public void lastPage()
    {

        //clearing();
        pgNum.text = (maxPages + 1).ToString();
        //GetLectureData();
    }

    public void firstPage()
    {

        //clearing();
        pgNum.text = (1).ToString();
        //GetLectureData();
    }


    /// <summary>
    ///
    /// </summary>



    public void FilterSearch()
    {
        var selection = FilterDropdown.options[FilterDropdown.value].text;
        var searchStr = SearchString.text;

        pgNum.text = "1";

        GetFilteredData(selection, searchStr);

    }

    public void GetFilteredData(string selection, string searchStr)
    {

        if (selection == "Code")
        {
            UnityEngine.Debug.Log("Code branch");
            GetLectureDataByCode();
        }

        else if (selection == "Instructor")
        {
            UnityEngine.Debug.Log("Instructor branch");
            GetLectureDataByInstructor();
        }
        else if (selection == "Location")
        {
            UnityEngine.Debug.Log("Location branch");
            GetLectureDataByLocation();
        }
        else
        {
            UnityEngine.Debug.Log("Wrong branch");
        }
        /*
        StartCoroutine(GetLectures((string data) =>
        {
            //lecList.text = data;
        }));
        */
    }


    public void GetLectureDataByCode()
    {
        StartCoroutine(GetLecturesByCode((string data) =>
        {
            //lecList.text = data;
        }));
    }



    IEnumerator GetLecturesByCode(Action<string> onCallBack)
    {
        var lecInfo = new List<string>();

        var searchStr = SearchString.text;


        lectureEntryList = new List<Lecture>();

        var entriesPerPage = 10;

        var lectureData = databaseReference.Child("lectures").OrderByKey().StartAt("-").LimitToFirst(60).GetValueAsync();
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

                Lecture newEntry = new Lecture(lecInfo[0], lecInfo[1], lecInfo[2], lecInfo[3], lecInfo[4]);
                if (newEntry.code.Contains(searchStr))
                {
                    UnityEngine.Debug.Log("match found");
                    lectureEntryList.Add(newEntry);
                }

                //empty lecInfo for next iteration
                lecInfo.Clear();

                //CreateLectureEntryTransform(newEntry, entryContainer, lectureEntryTransformList);


            }



            maxPages = (int)(lectureEntryList.Count / entriesPerPage);
            //UnityEngine.Debug.Log(maxPages);

            for (int i = ((Int32.Parse(pgNum.text) - 1) * entriesPerPage); i < Math.Min((Int32.Parse(pgNum.text)) * entriesPerPage, lectureEntryList.Count); i++)
            {
                if (lectureEntryList[i] != null)
                {
                    //UnityEngine.Debug.Log(i);
                    Lecture lectureEntry = lectureEntryList[i];
                    CreateLectureEntryTransform(lectureEntry, entryContainer, lectureEntryTransformList);
                }
            }


            onCallBack.Invoke(result);
        }


    }

    public void GetLectureDataByInstructor()
    {
        StartCoroutine(GetLecturesByInstructor((string data) =>
        {
            //lecList.text = data;
        }));
    }



    IEnumerator GetLecturesByInstructor(Action<string> onCallBack)
    {
        var lecInfo = new List<string>();

        var searchStr = SearchString.text;


        lectureEntryList = new List<Lecture>();

        var entriesPerPage = 10;

        var lectureData = databaseReference.Child("lectures").OrderByKey().StartAt("-").LimitToFirst(60).GetValueAsync();
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

                Lecture newEntry = new Lecture(lecInfo[0], lecInfo[1], lecInfo[2], lecInfo[3], lecInfo[4]);
                if (newEntry.instructor.Contains(searchStr))
                {
                    UnityEngine.Debug.Log("match found");
                    lectureEntryList.Add(newEntry);
                }

                //empty lecInfo for next iteration
                lecInfo.Clear();

                //CreateLectureEntryTransform(newEntry, entryContainer, lectureEntryTransformList);


            }



            maxPages = (int)(lectureEntryList.Count / entriesPerPage);
            //UnityEngine.Debug.Log(maxPages);

            for (int i = ((Int32.Parse(pgNum.text) - 1) * entriesPerPage); i < Math.Min((Int32.Parse(pgNum.text)) * entriesPerPage, lectureEntryList.Count); i++)
            {
                if (lectureEntryList[i] != null)
                {
                    //UnityEngine.Debug.Log(i);
                    Lecture lectureEntry = lectureEntryList[i];
                    CreateLectureEntryTransform(lectureEntry, entryContainer, lectureEntryTransformList);
                }
            }


            onCallBack.Invoke(result);
        }


    }


    public void GetLectureDataByLocation()
    {
        StartCoroutine(GetLecturesByLocation((string data) =>
        {
            //lecList.text = data;
        }));
    }



    IEnumerator GetLecturesByLocation(Action<string> onCallBack)
    {
        var lecInfo = new List<string>();

        var searchStr = SearchString.text;


        lectureEntryList = new List<Lecture>();

        var entriesPerPage = 10;

        var lectureData = databaseReference.Child("lectures").OrderByKey().StartAt("-").LimitToFirst(60).GetValueAsync();
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

                Lecture newEntry = new Lecture(lecInfo[0], lecInfo[1], lecInfo[2], lecInfo[3], lecInfo[4]);
                if (newEntry.location.Contains(searchStr))
                {
                    UnityEngine.Debug.Log("match found");
                    lectureEntryList.Add(newEntry);
                }

                //empty lecInfo for next iteration
                lecInfo.Clear();

                //CreateLectureEntryTransform(newEntry, entryContainer, lectureEntryTransformList);


            }



            maxPages = (int)(lectureEntryList.Count / entriesPerPage);
            //UnityEngine.Debug.Log(maxPages);

            for (int i = ((Int32.Parse(pgNum.text) - 1) * entriesPerPage); i < Math.Min((Int32.Parse(pgNum.text)) * entriesPerPage, lectureEntryList.Count); i++)
            {
                if (lectureEntryList[i] != null)
                {
                    //UnityEngine.Debug.Log(i);
                    Lecture lectureEntry = lectureEntryList[i];
                    CreateLectureEntryTransform(lectureEntry, entryContainer, lectureEntryTransformList);
                }
            }
            onCallBack.Invoke(result);
        }
    }

    public void OnEntryClick()
    {
        GameObject template = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        string code = template.transform.Find("codeText").GetComponent<TMP_Text>().text;
        Lecture target = lectureEntryList.Find(lecture => lecture.code == code);
        currentLecture = target;
    }
}


