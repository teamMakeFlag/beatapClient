using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keybeam:MonoBehaviour {
    public GameObject[] KeyBeam = new GameObject[5];

    public static bool[,] LineFlag = new bool[5, 2] { { false, false }, { false, false }, { false, false }, { false, false }, { false, false } };
    // Start is called before the first frame update
    void Start() {
        for (int i = 0; i < 5; i++) {
            KeyBeam[i].gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update() {
        if (PauseManager.isPause == false) {
            for (int i = 0; i < 5; i++) {
                for (int j = 0; j < 2; j++) {
                    if (Input.GetKey(SongSelect.LineKey[i, j])) {
                        LineFlag[i, j] = true;
                    }
                    else {
                        LineFlag[i, j] = false;
                    }
                }
            }
            for (int i = 0; i < 5; i++) {
                if (LineFlag[i, 0] || LineFlag[i, 1]) {
                    KeyBeam[i].gameObject.SetActive(true);
                }
                else {
                    KeyBeam[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
