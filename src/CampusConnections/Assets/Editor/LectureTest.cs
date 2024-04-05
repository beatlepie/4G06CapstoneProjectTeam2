using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class LectureTest
{
    string code = "SFWRENG 4G06";
    string name = "Capstone Project";
    string instructor = "Dr. Smith";
    string location = "ITB 102";
    string time = "Tue, Wed, Fri 12:30 - 13:20";

    [Test]
    public void LectureTestDefault()
    {
        Lecture l = new Lecture(code);
        Assert.AreEqual(l.Code, code);
        Assert.AreEqual(l.Name, Lecture.Placeholder);
        Assert.AreEqual(l.Instructor, Lecture.Placeholder);
        Assert.AreEqual(l.Location, Lecture.Placeholder);
        Assert.AreEqual(l.Time, Lecture.Placeholder);
    }

    [Test]
    public void LectureTestGetterAndSetter()
    {
        Lecture l = new Lecture(code, instructor, location, name, time);
        Assert.AreEqual(l.Code, code);
        Assert.AreEqual(l.Name, name);
        Assert.AreEqual(l.Instructor, instructor);
        Assert.AreEqual(l.Location, location);
        Assert.AreEqual(l.Time, time);
    }
}
