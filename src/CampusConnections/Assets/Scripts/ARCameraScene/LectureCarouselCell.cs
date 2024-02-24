﻿using FancyCarouselView.Runtime.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LectureCarouselCell : CarouselCell<LectureCarouselData, LectureCarouselCell>
{
    [SerializeField] private Image _image;
    [SerializeField] private TMP_Text _code;
    [SerializeField] private TMP_Text _name;
    [SerializeField] private TMP_Text _location;
    [SerializeField] private TMP_Text _instructor;
    [SerializeField] private Button _button;

    private LectureCarouselData _data;

    protected override void Refresh(LectureCarouselData data)
    {
        _data = data;
        _image.sprite = Resources.Load<Sprite>("Carousel_Background/" + data.SpriteResourceKey);
        _code.text = data.Lecture.code;
        _name.text = data.Lecture.name;
        _location.text = data.Lecture.location;
        _instructor.text = data.Lecture.instructor;
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