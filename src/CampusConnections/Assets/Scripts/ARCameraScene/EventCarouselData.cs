using System;

public class EventCarouselData
{
    public string SpriteResourceKey { get; }
    public Event eve { get; }
    public Action Clicked { get; }

    public EventCarouselData(string spriteResourceKey, Event e, Action clicked)
    {
        SpriteResourceKey = spriteResourceKey;
        eve = e;
        Clicked = clicked;
    }
}