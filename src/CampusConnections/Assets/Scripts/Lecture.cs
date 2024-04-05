/// <summary>
/// Data record class for Lecture objects. Provides a local data type for Lecture objects retrieved from the DB.
/// Author: Zihao Du
/// Date: 2023-11-19
/// </summary>
public class Lecture
{
    public string Code;
    public string Name;
    public string Instructor;
    public string Time;
    public string Location;
    public const string Placeholder = "NA";

    public Lecture(string code)
    {
        this.Code = code;
        Name = Placeholder;
        Instructor = Placeholder;
        Time = Placeholder;
        Location = Placeholder;
    }

    public Lecture(string code, string instructor, string location, string name, string time)
    {
        this.Code = code;
        this.Name = name;
        this.Instructor = instructor;
        this.Time = time;
        this.Location = location;
    }
}