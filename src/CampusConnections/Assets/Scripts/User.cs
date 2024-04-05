using System;
using System.Collections.Generic;

/// <summary>
/// Data record class for User objects. Provides a local data type for User objects retrieved from the DB.
/// Author: Zihao Du
/// Date: 2023-12-11
/// </summary>
public class User
{
    public string Email;
    public string NickName;
    public Uri PhotoUri;
    public string Program;
    public int Level;
    public readonly int Perms;
    public List<string> Friends;
    public List<string> Lectures;
    public List<string> Events;
    public List<string> FriendInvitation;

    public User(string email)
    {
        this.Email = email;
        NickName = "";
        Program = "";
        Level = 0;
        PhotoUri = null;
        Friends = new List<string>();
        Events = new List<string>();
        Lectures = new List<string>();
        FriendInvitation = new List<string>();
        // Sets the permissions of the user, where it is by default 1 for mcmaster email and 0 for all else
        Perms = email.EndsWith("mcmaster.ca") ? 1 : 0;
    }
}