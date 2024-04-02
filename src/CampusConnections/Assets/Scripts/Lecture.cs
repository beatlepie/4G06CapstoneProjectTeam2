public class Lecture
{
    public string code;
    public string name;
    public string instructor;
    public string time;
    public string location;
    public static string placeholder = "NA";

    public Lecture(string code)
    {
        this.code = code;
        name = placeholder;
        instructor = placeholder;
        time = placeholder;
        location = placeholder;
    }

    public Lecture(string code, string instructor, string location, string name, string time)
    {
        this.code = code;
        this.name = name;
        this.instructor = instructor;
        this.time = time;
        this.location = location;
    }
}