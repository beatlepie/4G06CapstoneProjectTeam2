using System;

/// <summary>
/// This class specifies what information a lecture cell contains
/// Author: Zihao Du
/// Date: 2024-02-20
/// </summary>
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