using System;

public class Event
{
    public string name;
    public long time;
    public int duration;
    public string organizer;
    public string description;
    public string location;
    public bool isPublic;

    public Event(string name)
    {
        // Default evet
        this.name = name;
        this.time = 1695056400;  // Start date of project
        this.duration = 30;
        this.organizer = "NA";
        this.description = "NA";
        this.location = "NA";
        this.isPublic = false;
    }

    public Event(string name, long time, int duration, string organizer, string description, string location, bool isPublic)
    {
        // Event with all information
        this.name = name;
        this.time = time;
        this.duration = duration;
        this.organizer = organizer;
        this.description = description;
        this.location = location;
        this.isPublic = isPublic;
    }
}