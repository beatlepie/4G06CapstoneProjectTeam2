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
        Assert.AreEqual(l.code, code);
        Assert.AreEqual(l.name, Lecture.placeholder);
        Assert.AreEqual(l.instructor, Lecture.placeholder);
        Assert.AreEqual(l.location, Lecture.placeholder);
        Assert.AreEqual(l.time, Lecture.placeholder);
    }

    [Test]
    public void LectureTestGetterAndSetter()
    {
        Lecture l = new Lecture(code, instructor, location, name, time);
        Assert.AreEqual(l.code, code);
        Assert.AreEqual(l.name, name);
        Assert.AreEqual(l.instructor, instructor);
        Assert.AreEqual(l.location, location);
        Assert.AreEqual(l.time, time);
    }
}
