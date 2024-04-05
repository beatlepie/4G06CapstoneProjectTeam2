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
        Assert.AreEqual(e.Name, name);
        Assert.AreEqual(e.Description, Event.Placeholder);
        Assert.AreEqual(e.Organizer, Event.Placeholder);
        Assert.AreEqual(e.Location, Event.Placeholder);
        Assert.AreEqual(e.Duration, Event.DefaultDuration);
        Assert.AreEqual(e.Time, Event.DefaultTime);
        Assert.AreEqual(e.IsPublic, false);
    }

    [Test]
    public void EventTestGetterAndSetter()
    {
        Event e = new Event(name, time, duration, organizer, description, location, isPublic);
        Assert.AreEqual(e.Name, name);
        Assert.AreEqual(e.Description, description);
        Assert.AreEqual(e.Organizer, organizer);
        Assert.AreEqual(e.Location, location);
        Assert.AreEqual(e.Duration, duration);
        Assert.AreEqual(e.Time, time);
        Assert.AreEqual(e.IsPublic, isPublic);
    }
}
