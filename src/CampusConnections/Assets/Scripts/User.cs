using System;
using System.Collections.Generic;

public class User
{
    public string email;
    public string nickName;
    public Uri photoUri;
    public string program;
    public int level;
    public int perms;
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
        // Sets the permissions of the user, where it is by default 1 for mcmaster email and 0 for all else
        if (email.EndsWith("mcmaster.ca"))
        {
            this.perms = 1;
        }
        else
        {
            this.perms = 0;
        }
    }
}