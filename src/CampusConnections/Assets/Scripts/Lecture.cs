public class Lecture
{
    public string code;
    public string name;
    public string instructor;
    public string time;
    public string location;

    public Lecture(string code)
    {
        this.code = code;
        this.name = "NA";
        this.instructor = "NA";
        this.time = "NA";
        this.location = "NA";
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