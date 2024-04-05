/// <summary>
/// Data record class for Event objects. Provides a local data type for Event objects retrieved from the DB.
/// Author: Zihao Du
/// Date: 2024-02-20
/// </summary>
public class Event
{
    public string Name;
    public long Time;
    public int Duration;
    public string Organizer;
    public string Description;
    public string Location;
    public bool IsPublic;
    public const long DefaultTime = 1695056400; // Start date of project
    public const int DefaultDuration = 30; // Minimum 30 minutes
    public const string Placeholder = "NA";

    public Event(string name)
    {
        // Default event
        this.Name = name;
        Time = DefaultTime;
        Duration = DefaultDuration;
        Organizer = Placeholder;
        Description = Placeholder;
        Location = Placeholder;
        IsPublic = false;
    }

    public Event(string name, long time, int duration, string organizer, string description, string location,
        bool isPublic)
    {
        // Event with all information
        this.Name = name;
        this.Time = time;
        this.Duration = duration;
        this.Organizer = organizer;
        this.Description = description;
        this.Location = location;
        this.IsPublic = isPublic;
    }
}