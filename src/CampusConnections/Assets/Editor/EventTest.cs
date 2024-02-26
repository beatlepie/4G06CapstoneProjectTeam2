using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EventTest
{
    string name = "Unit Test";
    string description = "Implement some unit test for basic data structures.";
    string organizer = "CampusConnections";
    string location = "Online";
    long time = 1708922625;
    int duration = 60;
    bool isPublic = true;

    [Test]
    public void EventTestDefault()
    {
        Event e = new Event(name);
        Assert.AreEqual(e.name, name);
        Assert.AreEqual(e.description, Event.placeholder);
        Assert.AreEqual(e.organizer, Event.placeholder);
        Assert.AreEqual(e.location, Event.placeholder);
        Assert.AreEqual(e.duration, Event.defaultDuration);
        Assert.AreEqual(e.time, Event.defaultTime);
        Assert.AreEqual(e.isPublic, false);
    }

    [Test]
    public void EventTestGetterAndSetter()
    {
        Event e = new Event(name);
        e.description = description;
        e.organizer = organizer;
        e.location = location;
        e.time = time;
        e.duration = duration;
        e.isPublic = isPublic;
        Assert.AreEqual(e.name, name);
        Assert.AreEqual(e.description, description);
        Assert.AreEqual(e.organizer, organizer);
        Assert.AreEqual(e.location, location);
        Assert.AreEqual(e.duration, duration);
        Assert.AreEqual(e.time, time);
        Assert.AreEqual(e.isPublic, isPublic);
    }
}
