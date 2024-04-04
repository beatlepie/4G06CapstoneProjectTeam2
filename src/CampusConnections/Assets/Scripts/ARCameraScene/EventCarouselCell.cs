using FancyCarouselView.Runtime.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// This class assigns EventCarouselData value to Unity prefab view
/// Author: Zihao Du
/// Date: 2024-02-20
/// </summary>
public class EventCarouselCell : CarouselCell<EventCarouselData, EventCarouselCell>
{
    [FormerlySerializedAs("_image")] [SerializeField] private Image image;
    [FormerlySerializedAs("_name")] [SerializeField] private TMP_Text eventName;
    [FormerlySerializedAs("_description")] [SerializeField] private TMP_Text description;
    [FormerlySerializedAs("_location")] [SerializeField] private TMP_Text location;
    [FormerlySerializedAs("_organizer")] [SerializeField] private TMP_Text organizer;
    [FormerlySerializedAs("_button")] [SerializeField] private Button button;

    private EventCarouselData _data;

    protected override void Refresh(EventCarouselData data)
    {
        _data = data;
        image.sprite = Resources.Load<Sprite>("Carousel_Background/" + data.SpriteResourceKey);
        eventName.text = data.EventRef.Name;
        description.text = data.EventRef.Description;
        location.text = data.EventRef.Location;
        organizer.text = data.EventRef.Organizer;
    }

    protected override void OnVisibilityChanged(bool visibility)
    {
        if (visibility)
            button.onClick.AddListener(OnClick);
        else
            button.onClick.RemoveListener(OnClick);
    }

    private void OnClick()
    {
        _data?.Clicked?.Invoke();
    }
}