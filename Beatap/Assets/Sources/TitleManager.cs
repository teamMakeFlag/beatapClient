using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Diagnostics;

public class TitleManager : MonoBehaviour
{
    private CommunicationManager communicationManager;

    public GameObject SettingPanel;
    public InputField UsernameInput;
    public InputField PasswordInput;


    public void ActiveSettingPanel()
    {
        SettingPanel.SetActive(true);
    }

    public void DeactiveSettingPanel()
    {
        SettingPanel.SetActive(false);
    }

    public void StartLogin()
    {
        string username = UsernameInput.text;
        string password = PasswordInput.text;
        PlayerPrefs.SetString("username", username);
        PlayerPrefs.SetString("password", password);
        PlayerPrefs.Save();
        StartCoroutine(communicationManager.LoginCoroutine(username, password));
    }

    public void StartLogout()
    {
        PlayerPrefs.DeleteKey("username");
        PlayerPrefs.DeleteKey("password");
        StartCoroutine(communicationManager.LogoutCoroutine());
        UpdateInputFieldValue();
    }

    private void UpdateInputFieldValue()
    {
        UsernameInput.text = PlayerPrefs.GetString("username", "");
        PasswordInput.text = PlayerPrefs.GetString("password", "");
    }

    void Start()
    {
        communicationManager = GameObject.Find("CommunicationManager").GetComponent<CommunicationManager>();
        SettingPanel.SetActive(false);
        UpdateInputFieldValue();
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene("Scenes/SelectSong");
            }
        }
    }
}
