using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Firebase.Database;

/// <summary>
/// This class includes all utility functions
/// Author: Zihao Du
/// Date: 2024-12-20
/// </summary>
public static class Utilities
{
    /// <summary>
    /// Replace the last . in email with _ since firebase index cannot contain dot
    /// The reason why we only replace last . is because almost all of the user email will end with @mcmaster.ca
    /// </summary>
    public static string RemoveDot(string email)
    {
        var indexOfDot = email.LastIndexOf('.');
        var emailWithOutDot = indexOfDot == -1
            ? email
            : email[..indexOfDot] + "_" + email[(indexOfDot + 1)..];
        return emailWithOutDot;
    }

    /// <summary>
    /// Replace the last _ in email with . once retreived from the db
    /// </summary>
    public static string AddDot(string emailWithoutDot)
    {
        // Change back email from removeDot method
        var index = emailWithoutDot.LastIndexOf('_');
        var email = index == -1
            ? emailWithoutDot
            : emailWithoutDot[..index] + "." + emailWithoutDot[(index + 1)..];
        return email;
    }

    /// <summary>
    /// Return true if the target string contains any of the special characters: \|!#$%&/()=?»«@£§€{}.-;'<>_,
    /// </summary>
    public static bool ContainSpecialChar(string target)
    {
        const string specialChar = @"\|!#$%&/()=?»«@£§€{}.-;'<>_,";
        return specialChar.Any(target.Contains);
    }

    /// <summary>
    /// Convert a firebase database DataSnapshot containing user information to a User object
    /// </summary>
    public static User FormalizeDBUserData(DataSnapshot user)
    {
        var result = new User("InvalidUser");
        foreach (var x in user.Children)
            switch (x.Key)
            {
                case "email":
                    result.Email = x.Value.ToString();
                    break;
                case "nickName":
                    result.NickName = x.Value.ToString();
                    break;
                case "photoUri":
                    result.PhotoUri = (Uri)x.Value;
                    break;
                case "level":
                    result.Level = int.Parse(x.Value.ToString());
                    break;
                case "program":
                    result.Program = x.Value.ToString();
                    break;
                case "friends":
                    foreach (var friend in x.Children) result.Friends.Add(AddDot(friend.Key));
                    break;
                case "invitations":
                    foreach (var invitation in x.Children)
                        result.FriendInvitation.Add(AddDot(invitation.Key));
                    break;
                case "events":
                    foreach (var e in x.Children) result.Friends.Add(e.Key);
                    break;
                case "lectures":
                    foreach (var lecture in x.Children) result.FriendInvitation.Add(lecture.Key);
                    break;
            }

        return result;
    }

    /// <summary>
    /// Convert a firebase database DataSnapshot containing event information to an Event object
    /// </summary>
    public static Event FormalizeDBEventData(DataSnapshot e)
    {
        var result = new Event("InvalidEvent");
        foreach (var x in e.Children)
            switch (x.Key)
            {
                case "name":
                    result.Name = x.Value.ToString();
                    break;
                case "description":
                    result.Description = x.Value.ToString();
                    break;
                case "location":
                    result.Location = x.Value.ToString();
                    break;
                case "duration":
                    result.Duration = int.Parse(x.Value.ToString());
                    break;
                case "isPublic":
                    result.IsPublic = (bool)x.Value;
                    break;
                case "organizer":
                    result.Organizer = x.Value.ToString();
                    break;
                case "time":
                    result.Time = (long)x.Value;
                    break;
            }

        return result;
    }

    /// <summary>
    /// Convert a firebase database DataSnapshot containing lecture information to a Lecture object
    /// </summary>
    public static Lecture FormalizeDBLectureData(DataSnapshot e)
    {
        var result = new Lecture("InvalidLecture");
        foreach (var x in e.Children)
            switch (x.Key)
            {
                case "code":
                    result.Code = x.Value.ToString();
                    break;
                case "name":
                    result.Name = x.Value.ToString();
                    break;
                case "location":
                    result.Location = x.Value.ToString();
                    break;
                case "instructor":
                    result.Instructor = x.Value.ToString();
                    break;
                case "time":
                    result.Time = x.Value.ToString();
                    break;
            }

        return result;
    }

    /// <summary>
    /// If the message contains a [event](.+) or [lecture](.+) pattern, return a list of the contents in square and round brackets
    /// </summary>
    public static List<string> GetActivityPattern(string message)
    {
        // Return two strings: first one is the type, second one is the ID (lecture's code, event's name)
        // Example: [event, EXPO]
        const string pattern = @"\[([a-zA-Z]+)\]\(([a-zA-Z0-9 ']+)\)";
        var match = Regex.Match(message, pattern);
        if (match.Success)
        {
            var type = match.Groups[1].Value;
            var id = match.Groups[2].Value;
            if ((type == "event") | (type == "lecture")) return new List<string> { type, id };
        }

        return new List<string> { "null", "" };
    }

    /// <summary>
    /// If the message contains a [event](.+) or [lecture](.+) pattern, the apply hex color to the message
    /// </summary>
    public static string PolishChatMessage(string message, string hex)
    {
        var pattern = GetActivityPattern(message);
        if ((pattern[0] == "null") & (pattern[1] == ""))
        {
            return message;
        }
        else
        {
            var originalString = "[" + pattern[0] + "](" + pattern[1] + ")";
            var activityInfo = "<color=" + hex + ">" + pattern[1] + "</color>";
            return message.Replace(originalString, activityInfo);
        }
    }
}