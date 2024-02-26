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
        Assert.AreEqual(p.entryList.Count, 2);
        Assert.AreEqual(p.filteredList.Count, 2);
        Assert.AreEqual(p.maxPage, 1);
        Assert.AreEqual(p.currentPage, 1);
        Assert.AreEqual(p.filterBy, null);
        Assert.AreEqual(p.filterString, null);
    }

    [Test]
    public void PagniationTestFilter()
    {
        List<Pizza> pizzaList = new List<Pizza>();
        pizzaList.Add(p1);
        pizzaList.Add(p2);
        pizzaList.Add(p3);
        Pagination<Pizza> p = new Pagination<Pizza>(pizzaList, "size", "medium", pageCount);
        Assert.AreEqual(p.entryList.Count, 3);
        Assert.AreEqual(p.filteredList.Count, 2);
        Assert.IsTrue(p.entryList.Contains(p2));
        Assert.IsFalse(p.filteredList.Contains(p2));
        Assert.AreEqual(p.maxPage, 1);
        Assert.AreEqual(p.currentPage, 1);
        Assert.AreEqual(p.filterBy, "size");
        Assert.AreEqual(p.filterString, "medium");
    }

    [Test]
    public void PagniationTestAddition()
    {
        List<Pizza> pizzaList = new List<Pizza>();
        pizzaList.Add(p1);
        pizzaList.Add(p2);
        pizzaList.Add(p3);
        Pagination<Pizza> p = new Pagination<Pizza>(pizzaList, null, null, pageCount);
        Assert.AreEqual(p.entryList.Count, 3);
        p.addNewEntry(p4);
        Assert.IsTrue(p.entryList.Contains(p4));
        Assert.AreEqual(p.entryList.Count, 4);
        Assert.AreEqual(p.maxPage, 2);
    }

    [Test]
    public void PagniationTestDeletion()
    {
        List<Pizza> pizzaList = new List<Pizza>();
        pizzaList.Add(p1);
        pizzaList.Add(p2);
        pizzaList.Add(p3);
        Pagination<Pizza> p = new Pagination<Pizza>(pizzaList, null, null, pageCount);
        Assert.AreEqual(p.entryList.Count, 3);
        p.removeEntry(p3);
        Assert.IsFalse(p.entryList.Contains(p3));
        Assert.AreEqual(p.entryList.Count, 2);
        Assert.AreEqual(p.maxPage, 1);
    }

    [Test]
    public void PagniationTestNextAndPrevPage()
    {
        List<Pizza> pizzaList = new List<Pizza>();
        pizzaList.Add(p1);
        pizzaList.Add(p2);
        pizzaList.Add(p3);
        Pagination<Pizza> p = new Pagination<Pizza>(pizzaList, null, null, pageCount);
        Assert.AreEqual(p.currentPage, 1);
        p.nextPage();
        Assert.AreEqual(p.currentPage, 2);
        p.nextPage();
        Assert.AreEqual(p.currentPage, 2);
        p.prevPage();
        Assert.AreEqual(p.currentPage, 1);
        p.prevPage();
        Assert.AreEqual(p.currentPage, 1);
    }

    [Test]
    public void PagniationTestFirstAndLastPage()
    {
        List<Pizza> pizzaList = new List<Pizza>();
        pizzaList.Add(p1);
        pizzaList.Add(p2);
        pizzaList.Add(p3);
        Pagination<Pizza> p = new Pagination<Pizza>(pizzaList, null, null, pageCount);
        Assert.AreEqual(p.currentPage, 1);
        p.lastPage();
        Assert.AreEqual(p.currentPage, 2);
        p.firstPage();
        Assert.AreEqual(p.currentPage, 1);
    }
}
