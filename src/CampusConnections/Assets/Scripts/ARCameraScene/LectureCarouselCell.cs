using FancyCarouselView.Runtime.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LectureCarouselCell : CarouselCell<LectureCarouselData, LectureCarouselCell>
{
    [FormerlySerializedAs("_image")] [SerializeField] private Image image;
    [FormerlySerializedAs("_code")] [SerializeField] private TMP_Text code;
    [FormerlySerializedAs("_name")] [SerializeField] private TMP_Text lectureName;
    [FormerlySerializedAs("_location")] [SerializeField] private TMP_Text location;
    [FormerlySerializedAs("_instructor")] [SerializeField] private TMP_Text instructor;
    [FormerlySerializedAs("_button")] [SerializeField] private Button button;

    private LectureCarouselData _data;

    protected override void Refresh(LectureCarouselData data)
    {
        _data = data;
        image.sprite = Resources.Load<Sprite>("Carousel_Background/" + data.SpriteResourceKey);
        code.text = data.Lecture.Code;
        lectureName.text = data.Lecture.Name;
        location.text = data.Lecture.Location;
        instructor.text = data.Lecture.Instructor;
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