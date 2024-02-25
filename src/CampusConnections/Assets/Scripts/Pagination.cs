using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pagination<T>
{
    private List<T> entryList;
    private List<T> filteredList;
    private int maxPage;
    private int currentPage;
    public int pageCount;
    private string filterBy;
    private string filterString;

    public Pagination(List<T> entryList, string filterBy, string filterString, int pageCount = 10)
    {
        this.entryList = entryList;
        this.filterBy = filterBy;
        this.filterString = filterString;
        this.pageCount = pageCount;
        filterEntries();
        UpdateMaxPage();
        this.currentPage = 1;
    }

    public List<T> filterEntries()
    {
        if (filterBy != null & filterString != null)
        {
            filteredList.Clear();
            foreach (T x in entryList) {
                string target = (string)x.GetType().GetProperty(filterBy).GetValue(x, null);
                if (target.Contains(filterString))
                {
                    filteredList.Add(x);
                }
            }
        }
        else
        {
            filteredList = entryList;
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
