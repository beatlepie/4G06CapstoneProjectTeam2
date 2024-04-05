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
        Assert.AreEqual(x.Email, guestEmail);
        Assert.AreEqual(x.NickName, "");
        Assert.AreEqual(x.PhotoUri, null);
        Assert.AreEqual(x.Program, "");
        Assert.AreEqual(x.Level, 0);
    }

    [Test]
    public void UserTestGetterAndSetter()
    {
        User x = new User(guestEmail);
        x.NickName = nickName;
        x.PhotoUri = photoUri;
        x.Program = program;
        x.Level = level;
        x.Friends = friends;
        x.Lectures = lectures;
        x.Events = events;
        x.FriendInvitation = friendInvitation;
        Assert.AreEqual(x.Email, guestEmail);
        Assert.AreEqual(x.NickName, nickName);
        Assert.AreEqual(x.PhotoUri, photoUri);
        Assert.AreEqual(x.Program, program);
        Assert.AreEqual(x.Level, level);
        Assert.AreEqual(x.Friends, friends);
        Assert.AreEqual(x.FriendInvitation, friendInvitation);
        Assert.AreEqual(x.Lectures, lectures);
        Assert.AreEqual(x.Events, events);
    }

    [Test]
    public void UserTestPermission()
    {
        User guest = new User(guestEmail);
        User student = new User(studentEmail);
        Assert.AreEqual(guest.Perms, 0);
        Assert.AreEqual(student.Perms, 1);
    }
}
