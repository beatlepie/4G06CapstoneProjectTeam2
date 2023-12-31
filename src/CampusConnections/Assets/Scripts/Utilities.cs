using System;
using Firebase.Database;

public class Utilities
{
    public static string removeDot(string email)
    {   
        // Replace last . in email with _ since firebase index cannot contain dot
        int indexOfDot = email.LastIndexOf('.');
        string emailWithOutDot = indexOfDot == -1 ? email : email.Substring(0, indexOfDot) + "_" + email.Substring(indexOfDot + 1);
        return emailWithOutDot;
    }
    public static string addDot(string emailWithoutDot)
    {   
        // Change back email from removeDot method
        int index = emailWithoutDot.LastIndexOf('_');
        string email= index == -1 ? emailWithoutDot : emailWithoutDot.Substring(0, index) + "." + emailWithoutDot.Substring(index + 1);
        return email;
    }

    public static User FormalizeDBUserData(DataSnapshot user)
    {
        User result = new User("InvalidUser");
        foreach (var x in user.Children)
        {
            switch(x.Key.ToString())
            {
                case "email":
                result.email = x.Value.ToString();
                break;
                case "nickName":
                result.nickName = x.Value.ToString();
                break;
                case "photoUri":
                result.photoUri = (Uri)x.Value;
                break;
                case "level":
                result.level = int.Parse(x.Value.ToString());
                break;
                case "program":
                result.program = x.Value.ToString();
                break;
                case "friends":
                foreach (var friend in x.Children)
                {
                    result.friends.Add(addDot(friend.Key.ToString()));
                }
                break;
                case "invitations":
                foreach (var invitation in x.Children)
                {
                    result.friendInvitation.Add(addDot(invitation.Key.ToString()));
                }
                break;
                case "events":
                foreach (var e in x.Children)
                {
                    result.friends.Add(e.Key.ToString());
                }
                break;
                case "lectures":
                foreach (var lecture in x.Children)
                {
                    result.friendInvitation.Add(lecture.Key.ToString());
                }
                break;
            }
        }
        return result;
    }
}
