using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LNBridge:MonoBehaviour {
    public int line;
    public int note_num;
    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update() {
        if ((GameManager.playing && GameManager.loaded_music) || PauseManager.isPause) {
            this.GetComponent<Rigidbody>().velocity = new Vector3(GameManager.total_speed * 100, 0, 0);
        }
        
        if (note_num <= GameManager.judgedNotes[line] && GameManager.pushing_ln[line] == -1) {
            this.gameObject.SetActive(false);
        }
    }

    public void getInfo(int get_line, int get_note_num) {
        line = get_line;
        note_num = get_note_num;
    }
}
