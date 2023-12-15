using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        string email= index == -1 ? emailWithoutDot : emailWithoutDot.Substring(0, index) + "_" + emailWithoutDot.Substring(index + 1);
        return email;
    }
}
