using System;

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