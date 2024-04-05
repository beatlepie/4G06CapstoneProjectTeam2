using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

/// <summary>
/// Generic class that provides pagination functionality for large requests made to the DB.
/// Author: Zihao Du
/// Date: 2024-02-25
/// </summary>
/// <typeparam name="T"></typeparam>
public class Pagination<T>
{
    public readonly List<T> EntryList;
    public List<T> FilteredList;
    public int MaxPage;
    public int CurrentPage;
    private readonly int _pageCount;
    public string FilterBy;
    public string FilterString;

    public Pagination(List<T> entryList, string filterBy, string filterString, int pageCount = 10)
    {
        this.EntryList = new List<T>(entryList);
        FilteredList = new List<T>(entryList);
        this.FilterBy = filterBy;
        this.FilterString = filterString;
        this._pageCount = pageCount;
        FilterEntries();
        CurrentPage = 1;
    }

    /// <summary>
    /// Applies a filter to the list of entries.
    /// </summary>
    /// <returns>Filtered list of entries.</returns>
    public List<T> FilterEntries()
    {
        if ((FilterBy != null) & (FilterString != null))
        {
            FilteredList.Clear();
            foreach (var x in EntryList)
            {
                var json = JObject.Parse(JsonUtility.ToJson(x));
                var target = (string)json[FilterBy];
                if (target != null && target.ToLower().Contains(FilterString.ToLower())) FilteredList.Add(x);
            }
        }
        else
        {
            FilteredList = new List<T>(EntryList);
        }

        UpdateMaxPage();
        FirstPage();
        return FilteredList;
    }

    public void AddNewEntry(T x)
    {
        EntryList.Add(x);
        FilterEntries();
        UpdateMaxPage();
        FirstPage();
    }

    public void RemoveEntry(T x)
    {
        EntryList.Remove(x);
        FilterEntries();
        UpdateMaxPage();
        FirstPage();
    }

    private void UpdateMaxPage()
    {
        if (FilteredList.Count == 0)
            MaxPage = 1;
        else
            MaxPage = FilteredList.Count % _pageCount == 0
                ? FilteredList.Count / _pageCount
                : FilteredList.Count / _pageCount + 1;
    }

    public int NextPage()
    {
        if (currentPage == maxPage)
        {
            return currentPage;
        }
        currentPage = currentPage + 1;
        return currentPage;
    }

    public int PrevPage()
    {
        if (currentPage <= 1)
        {
            return currentPage;
        }
        currentPage = currentPage - 1;
        return currentPage;
    }

    public int LastPage()
    {
        CurrentPage = MaxPage;
        return CurrentPage;
    }

    public int FirstPage()
    {
        CurrentPage = 1;
        return CurrentPage;
    }
}