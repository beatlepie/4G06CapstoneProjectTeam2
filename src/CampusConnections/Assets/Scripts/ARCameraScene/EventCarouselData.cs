using System;

/// <summary>
/// This class specifies what information an event cell contains
/// Author: Zihao Du
/// Date: 2024-02-20
/// </summary>
public class EventCarouselData
{
    public string SpriteResourceKey { get; }
    public Event EventRef { get; }
    public Action Clicked { get; }

    public EventCarouselData(string spriteResourceKey, Event e, Action clicked)
    {
        SpriteResourceKey = spriteResourceKey;
        EventRef = e;
        Clicked = clicked;
    }
}