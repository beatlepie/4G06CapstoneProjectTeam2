using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class UserTest
{
    string guestEmail = "test@example.com";
    string studentEmail = "test@mcmaster.ca";
    string nickName = "TestUser";
    Uri photoUri = new Uri("http://example.com/photo.jpg");
    string program = "Computer Science";
    int level = 1;
    List<string> friends = new List<string> { "Friend1", "Friend2" };
    List<string> lectures = new List<string> { "Lecture1", "Lecture2" };
    List<string> events = new List<string> { "Event1", "Event2" };
    List<string> friendInvitation = new List<string> { "Invitation1", "Invitation2" };

    [Test]
    public void UserTestDefault()
    {
        User x = new User(guestEmail);
        Assert.AreEqual(x.email, guestEmail);
        Assert.AreEqual(x.nickName, "");
        Assert.AreEqual(x.photoUri, null);
        Assert.AreEqual(x.program, "");
        Assert.AreEqual(x.level, 0);
    }

    [Test]
    public void UserTestGetterAndSetter()
    {
        User x = new User(guestEmail);
        x.nickName = nickName;
        x.photoUri = photoUri;
        x.program = program;
        x.level = level;
        x.friends = friends;
        x.lectures = lectures;
        x.events = events;
        x.friendInvitation = friendInvitation;
        Assert.AreEqual(x.email, guestEmail);
        Assert.AreEqual(x.nickName, nickName);
        Assert.AreEqual(x.photoUri, photoUri);
        Assert.AreEqual(x.program, program);
        Assert.AreEqual(x.level, level);
        Assert.AreEqual(x.friends, friends);
        Assert.AreEqual(x.friendInvitation, friendInvitation);
        Assert.AreEqual(x.lectures, lectures);
        Assert.AreEqual(x.events, events);
    }

    [Test]
    public void UserTestPermission()
    {
        User guest = new User(guestEmail);
        User student = new User(studentEmail);
        Assert.AreEqual(guest.perms, 0);
        Assert.AreEqual(student.perms, 1);
    }
}
