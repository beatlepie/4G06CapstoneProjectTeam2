using System;
using System.Collections.Generic;

public class User
{
    public string email;
    public string nickName;
    public Uri photoUri;
    public string program;
    public int level;
    public List<string> friends;
    public List<string> lectures;
    public List<string> events;
    public List<string> friendInvitation;

    public User(string email)
    {
        this.email = email;
        this.friends = new List<string>();
        this.events = new List<string>();
        this.lectures = new List<string>();
        this.friendInvitation = new List<string>();
    }
}