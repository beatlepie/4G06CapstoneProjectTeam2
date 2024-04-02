using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class ToggleManager : MonoBehaviour
{
    public int selected;
    public GameObject option1;
    public GameObject option2;
    public GameObject ChatUI;
    public GameObject RequestUI;
    [SerializeField] private Color selectedBackground;
    [SerializeField] private Color unselectedBackground;
    [SerializeField] private Color selectedTextColor;
    [SerializeField] private Color unselectedTextColor;

    private void Start()
    {
        selected = 1;
        option1.GetComponentInChildren<TMP_Text>().color = selectedTextColor;
        option1.GetComponent<Image>().color = selectedBackground;
        option2.GetComponentInChildren<TMP_Text>().color = unselectedTextColor;
        option2.GetComponent<Image>().color = unselectedBackground;
    }

    public void selectOption1()
    {
        if (selected == 1) return;
        selected = 1;
        option1.GetComponentInChildren<TMP_Text>().color = selectedTextColor;
        option1.GetComponent<Image>().color = selectedBackground;
        option2.GetComponentInChildren<TMP_Text>().color = unselectedTextColor;
        option2.GetComponent<Image>().color = unselectedBackground;
        ChatUI.SetActive(true);
        RequestUI.SetActive(false);
    }

    public void selectOption2()
    {
        if (selected == 2) return;
        selected = 2;
        option2.GetComponentInChildren<TMP_Text>().color = selectedTextColor;
        option2.GetComponent<Image>().color = selectedBackground;
        option1.GetComponentInChildren<TMP_Text>().color = unselectedTextColor;
        option1.GetComponent<Image>().color = unselectedBackground;
        ChatUI.SetActive(false);
        RequestUI.SetActive(true);
    }
}