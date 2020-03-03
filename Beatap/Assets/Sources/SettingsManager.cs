using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Diagnostics;
using System;

public class SettingsManager:MonoBehaviour {
    public static bool SettingsActive = false;
    public GameObject SettingsPanel;
    public Text OffsetShow;
    public Text ComboText;
    public Text TimingText;
    private int localOffset;
    private int timingShow;
    private int comboShow;

    // Start is called before the first frame update
    void Start() {
        SettingsPanel.gameObject.SetActive(SettingsActive);
        localOffset = PlayerPrefs.GetInt("localOffset", 0);
        comboShow = PlayerPrefs.GetInt("comboShow", 0);
    }

    // Update is called once per frame
    void Update() {
        //Debug.Log(SongSelect.LocalOffset);
        OffsetShow.text = "" + localOffset;
        switch (comboShow) {
            case 0:
                ComboText.text = "コンボ数";
                break;
            case 1:
                ComboText.text = "パーセント";
                break;
            case 2:
                ComboText.text = "なし";
                break;
        }
        if (Input.GetKeyDown(KeyCode.S)) {
            SettingsActive = !SettingsActive;
            SettingsPanel.gameObject.SetActive(SettingsActive);
            if (!SettingsActive) {
                PlayerPrefs.SetInt("localOffset", localOffset);
                PlayerPrefs.SetInt("comboShow", comboShow);
                PlayerPrefs.Save();
            }
        }
    }

    public void OffsetPlus() {
        localOffset++;
        if (localOffset > 100) {
            localOffset = 100;
        }
    }

    public void OffsetMinus() {
        localOffset--;
        if (localOffset < -100) {
            localOffset = -100;
        }
    }

    public void ComboShowToggle() {
        comboShow = (comboShow + 1) % 3;
    }
}
