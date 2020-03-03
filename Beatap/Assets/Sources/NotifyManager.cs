using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotifyManager : MonoBehaviour
{
    public GameObject notifyPannel;
    public Text notifyText;

    private void Start()
    {
        notifyPannel.SetActive(false);
    }

    private void SetNotifyText(string text)
    {
        notifyText.text = text;
    }

    private IEnumerator NotifyCoroutine(string text, int time_sec = 4)
    {
        SetNotifyText(text);
        notifyPannel.SetActive(true);
        yield return new WaitForSeconds(time_sec);
        notifyPannel.SetActive(false);
    }

    public void Notify(string text, int time_sec = 2)
    {
        StartCoroutine(NotifyCoroutine(text, time_sec));
    }
}
