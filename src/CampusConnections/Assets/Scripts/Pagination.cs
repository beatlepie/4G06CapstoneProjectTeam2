using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class Pagination<T>
{
    public List<T> entryList;
    public List<T> filteredList;
    public int maxPage;
    public int currentPage;
    public int pageCount;
    public string filterBy;
    public string filterString;

    public Pagination(List<T> entryList, string filterBy, string filterString, int pageCount = 10)
    {
        this.entryList = new List<T>(entryList);
        this.filteredList = new List<T>(entryList);
        this.filterBy = filterBy;
        this.filterString = filterString;
        this.pageCount = pageCount;
        filterEntries();
        this.currentPage = 1;
    }

    public List<T> filterEntries()
    {
        if (filterBy != null & filterString != null)
        {
            filteredList.Clear();
            foreach (T x in entryList) {
                JObject json = JObject.Parse(JsonUtility.ToJson(x));
                string target = (string)json[filterBy];
                if (target.ToLower().Contains(filterString.ToLower()))
                {
                    filteredList.Add(x);
                }
            }
        }
        else
        {
            filteredList = new List<T>(entryList);
        }
        UpdateMaxPage();
        firstPage();
        return filteredList;
    }

    public void addNewEntry(T x)
    {
        entryList.Add(x);
        filterEntries();
        UpdateMaxPage();
        firstPage();
    }

    public void removeEntry(T x)
    {
        entryList.Remove(x);
        filterEntries();
        UpdateMaxPage();
        firstPage();
    }

    private void UpdateMaxPage()
    {
        if(filteredList.Count == 0)
        {
            maxPage = 1;
        }
        else
        {
            maxPage = filteredList.Count % pageCount == 0 ? filteredList.Count / pageCount : (int)(filteredList.Count / pageCount) + 1;
        }
    }

    public int nextPage()
    {
        if (currentPage == maxPage)
        {
            return -1;
        }
        currentPage = currentPage + 1;
        return currentPage;
    }

    public int prevPage()
    {
        if (currentPage <= 1)
        {
            return -1;
        }
        currentPage = currentPage - 1;
        return currentPage;
    }

    public int lastPage()
    {
        currentPage = maxPage;
        return currentPage;
    }

    public int firstPage()
    {
        currentPage = 1;
        return currentPage;
    }
}
