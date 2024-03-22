using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

    public static bool containSpecialChar(string target)
    {
        string specialChar = @"\|!#$%&/()=?»«@£§€{}.-;'<>_,";
        foreach (var item in specialChar)
        {
            if (target.Contains(item))
                return true;
        }

        return false;
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
    public static Event FormalizeDBEventData(DataSnapshot e)
    {
        Event result = new Event("InvalidEvent");
        foreach (var x in e.Children)
        {
            switch(x.Key.ToString())
            {
                case "name":
                result.name = x.Value.ToString();
                break;
                case "description":
                result.description = x.Value.ToString();
                break;
                case "location":
                result.location = x.Value.ToString();
                break;
                case "duration":
                result.duration = int.Parse(x.Value.ToString());
                break;
                case "isPublic":
                result.isPublic = (bool)x.Value;
                break;
                case "organizer":
                result.organizer = x.Value.ToString();
                break;
                case "time":
                result.time = (long)x.Value;
                break;
            }
        }
        return result;
    }
    public static Lecture FormalizeDBLectureData(DataSnapshot e)
    {
        Lecture result = new Lecture("InvalidLecture");
        foreach (var x in e.Children)
        {
            switch(x.Key.ToString())
            {
                case "code":
                result.code = x.Value.ToString();
                break;
                case "name":
                result.name = x.Value.ToString();
                break;
                case "location":
                result.location = x.Value.ToString();
                break;
                case "instructor":
                result.instructor = x.Value.ToString();
                break;
                case "time":
                result.time = x.Value.ToString();
                break;
            }
        }
        return result;
    }

    public static List<string> GetActivityPattern(string message)
    {
        // Return two strings: first one is the type, second one is the ID (lecture's code, event's name)
        // Example: [event, EXPO]
        string pattern = @"\[([a-zA-Z]+)\]\(([a-zA-Z0-9 ']+)\)";
        Match match = Regex.Match(message, pattern);
        if (match.Success)
        {
            string type = match.Groups[1].Value;
            string id = match.Groups[2].Value;
            if (type == "event" | type == "lecture")
            {
                return new List<string> {type, id};
            } 
        }
        return new List<string> {"null", ""};
    }

    public static string PolishChatMessage(string message, string hex)
    {
        List<string> pattern = GetActivityPattern(message);
        if (pattern[0] == "null" & pattern[1] == "")
        {
            return message;
        }
        else
        {
            string originalString = "[" + pattern[0] + "](" + pattern[1] + ")";
            string activityInfo = "<color=" + hex + ">" + pattern[1] + "</color>";
            return message.Replace(originalString, activityInfo);
        }
    }
}
