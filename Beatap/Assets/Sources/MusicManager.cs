using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Diagnostics;

public class MusicManager:MonoBehaviour {
    public static AudioClip Music;
    public static AudioSource audioSource;
    public static bool canPlayMusic;
    // Start is called before the first frame update
    void Start() {
        canPlayMusic = false;
        DontDestroyOnLoad(this);
        StartCoroutine(GetAudioClip());
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update() {
        if (canPlayMusic == true && PauseManager.isPause == false) {
            audioSource.PlayOneShot(Music);
            canPlayMusic = false;
        }
    }

    IEnumerator GetAudioClip() {
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + SongSelect.song_path, AudioType.UNKNOWN);
        yield return www.SendWebRequest();
        Music = DownloadHandlerAudioClip.GetContent(www);
        GameManager.loaded_music = true;
        Debug.Log("楽曲の読み込み完了");
    }
}
