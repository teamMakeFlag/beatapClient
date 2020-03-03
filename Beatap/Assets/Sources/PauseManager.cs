using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public Text PauseInfo;
    public GameObject PauseMenu;
    public static bool isPause;
    // Start is called before the first frame update
    void Start(){
        isPause = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isPause == false && Input.GetKeyDown(KeyCode.Escape)) {
            isPause = true;
            GameManager.playing = false;
            MusicManager.audioSource.Pause();
        }
        PauseInfo.gameObject.SetActive(!isPause);
        PauseMenu.gameObject.SetActive(isPause);
    }

    public void onClickResume() {
        isPause = false;
        GameManager.playing = true;
    }

    public void onClickRetry() {
        SceneManager.LoadScene("Scenes/GameScene");
    }

    public void onClickQuit() {
        SceneManager.LoadScene("Scenes/SelectSong");
    }
}
