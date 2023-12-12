using System.Collections.Generic;

public class User
{
    public string email;
    public string nickName;
    public string program;
    public int level;
    public List<string> friends;
    public List<string> lectures;
    public List<string> events;
    public List<string> friendInvitation;

    public User(string email, string nickName)
    {
        this.email = email;
        this.nickName = nickName;
        this.program = "";
        this.level = 0;
        this.friends = new List<string>() {"test@test.com", "test@test.ca"};
        this.events = new List<string>();
        this.lectures = new List<string>();
        this.friendInvitation = new List<string>();
    }
}