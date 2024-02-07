using System;

public class LectureCarouselData
{
    public string SpriteResourceKey { get; }
    public Lecture Lecture { get; }
    public Action Clicked { get; }

    public LectureCarouselData(string spriteResourceKey, Lecture lec, Action clicked)
    {
        SpriteResourceKey = spriteResourceKey;
        Lecture = lec;
        Clicked = clicked;
    }
}
