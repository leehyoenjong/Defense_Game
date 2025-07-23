using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageTextController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI[] _text;
    Queue<TextMeshProUGUI> _textqueue = new Queue<TextMeshProUGUI>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var maxcount = _text.Length;
        for (int i = 0; i < maxcount; i++)
        {
            _textqueue.Enqueue(_text[i]);
        }
    }

    public void CreateText(int damage)
    {
        if (_textqueue.Count <= 0)
        {
            return;
        }

        var text = _textqueue.Dequeue();
        text.gameObject.SetActive(true);
        text.text = damage.ToString();
    }

    public void OnComplete(TextMeshProUGUI text)
    {
        text.gameObject.SetActive(false);
        _textqueue.Enqueue(text);
    }
}
