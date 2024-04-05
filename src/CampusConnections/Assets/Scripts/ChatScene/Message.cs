using UnityEngine;

/// <summary>
/// A data helper class that wraps and serializes user chat messages.
/// Author: Waseef Nayeem
/// Date: 2024-02-09
/// </summary>
[System.Serializable]
public class Message
{
    [SerializeField] private string content = "";

    public string Content
    {
        get => content;
        set => content = value;
    }
}