using FancyCarouselView.Runtime.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventCarouselCell : CarouselCell<EventCarouselData, EventCarouselCell>
{
    [SerializeField] private Image _image;
    [SerializeField] private TMP_Text _name;
    [SerializeField] private TMP_Text _description;
    [SerializeField] private TMP_Text _location;
    [SerializeField] private TMP_Text _organizer;
    [SerializeField] private Button _button;

    private EventCarouselData _data;

    protected override void Refresh(EventCarouselData data)
    {
        _data = data;
        _image.sprite = Resources.Load<Sprite>("Carousel_Background/" + data.SpriteResourceKey);
        _name.text = data.eve.name;
        _description.text = data.eve.description;
        _location.text = data.eve.location;
        _organizer.text = data.eve.organizer;
    }

    protected override void OnVisibilityChanged(bool visibility)
    {
        if (visibility)
            _button.onClick.AddListener(OnClick);
        else
            _button.onClick.RemoveListener(OnClick);
    }

    private void OnClick()
    {
        _data?.Clicked?.Invoke();
    }
}
