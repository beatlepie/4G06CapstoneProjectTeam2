using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PaginationTest
{
    class Pizza
    {
        public string size;
        public string toppings;
        public Pizza(string s, string t)
        {
            size = s;
            toppings = t;
        }
    }
    Pizza p1 = new Pizza("medium", "cheese");
    Pizza p2 = new Pizza("large", "veggie");
    Pizza p3 = new Pizza("medium", "pepperoni");
    Pizza p4 = new Pizza("small", "garlic shrimp");
    int pageCount = 2;

    [Test]
    public void PagniationTestDefault()
    {
        List<Pizza> pizzaList = new List<Pizza>();
        pizzaList.Add(p1);
        pizzaList.Add(p2);
        Pagination<Pizza> p = new Pagination<Pizza>(pizzaList, null, null, pageCount);
        Assert.AreEqual(p.EntryList.Count, 2);
        Assert.AreEqual(p.FilteredList.Count, 2);
        Assert.AreEqual(p.MaxPage, 1);
        Assert.AreEqual(p.CurrentPage, 1);
        Assert.AreEqual(p.FilterBy, null);
        Assert.AreEqual(p.FilterString, null);
    }

    [Test]
    public void PagniationTestFilter()
    {
        List<Pizza> pizzaList = new List<Pizza>();
        pizzaList.Add(p1);
        pizzaList.Add(p2);
        pizzaList.Add(p3);
        Pagination<Pizza> p = new Pagination<Pizza>(pizzaList, "size", "medium", pageCount);
        Assert.AreEqual(p.EntryList.Count, 3);
        Assert.AreEqual(p.FilteredList.Count, 2);
        Assert.IsTrue(p.EntryList.Contains(p2));
        Assert.IsFalse(p.FilteredList.Contains(p2));
        Assert.AreEqual(p.MaxPage, 1);
        Assert.AreEqual(p.CurrentPage, 1);
        Assert.AreEqual(p.FilterBy, "size");
        Assert.AreEqual(p.FilterString, "medium");
    }

    [Test]
    public void PagniationTestAddition()
    {
        List<Pizza> pizzaList = new List<Pizza>();
        pizzaList.Add(p1);
        pizzaList.Add(p2);
        pizzaList.Add(p3);
        Pagination<Pizza> p = new Pagination<Pizza>(pizzaList, null, null, pageCount);
        Assert.AreEqual(p.EntryList.Count, 3);
        p.AddNewEntry(p4);
        Assert.IsTrue(p.EntryList.Contains(p4));
        Assert.AreEqual(p.EntryList.Count, 4);
        Assert.AreEqual(p.MaxPage, 2);
    }

    [Test]
    public void PagniationTestDeletion()
    {
        List<Pizza> pizzaList = new List<Pizza>();
        pizzaList.Add(p1);
        pizzaList.Add(p2);
        pizzaList.Add(p3);
        Pagination<Pizza> p = new Pagination<Pizza>(pizzaList, null, null, pageCount);
        Assert.AreEqual(p.EntryList.Count, 3);
        p.RemoveEntry(p3);
        Assert.IsFalse(p.EntryList.Contains(p3));
        Assert.AreEqual(p.EntryList.Count, 2);
        Assert.AreEqual(p.MaxPage, 1);
    }

    [Test]
    public void PagniationTestNextAndPrevPage()
    {
        List<Pizza> pizzaList = new List<Pizza>();
        pizzaList.Add(p1);
        pizzaList.Add(p2);
        pizzaList.Add(p3);
        Pagination<Pizza> p = new Pagination<Pizza>(pizzaList, null, null, pageCount);
        Assert.AreEqual(p.CurrentPage, 1);
        p.NextPage();
        Assert.AreEqual(p.CurrentPage, 2);
        p.NextPage();
        Assert.AreEqual(p.CurrentPage, 2);
        p.PrevPage();
        Assert.AreEqual(p.CurrentPage, 1);
        p.PrevPage();
        Assert.AreEqual(p.CurrentPage, 1);
    }

    [Test]
    public void PagniationTestFirstAndLastPage()
    {
        List<Pizza> pizzaList = new List<Pizza>();
        pizzaList.Add(p1);
        pizzaList.Add(p2);
        pizzaList.Add(p3);
        Pagination<Pizza> p = new Pagination<Pizza>(pizzaList, null, null, pageCount);
        Assert.AreEqual(p.CurrentPage, 1);
        p.LastPage();
        Assert.AreEqual(p.CurrentPage, 2);
        p.FirstPage();
        Assert.AreEqual(p.CurrentPage, 1);
    }
}
