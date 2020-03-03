using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LongNotes:MonoBehaviour {
    public GameObject JudgeBD;
    public GameObject JudgePG;
    public int line;
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if ((GameManager.playing && GameManager.loaded_music) || PauseManager.isPause) {
            this.GetComponent<Rigidbody>().velocity = new Vector3(GameManager.total_speed * 100, 0, 0);
            if (this.transform.position.x > 15 + (GameManager.judge_temp[GameManager.judge_level, 1] * GameManager.total_speed) && GameManager.pushing_ln[line] == -1) {
                this.gameObject.SetActive(false);
                GameManager.judge.bd++;
                GameManager.judgedNotes[line]++;
                GameManager.pushing_ln[line] = 0;
                //Debug.Log("BAD");
                GameManager.combo = 0;
                Instantiate(this.JudgeBD, new Vector3(14.5f, 1, this.transform.position.z - 2), Quaternion.Euler(90, 270, 0));
                if ((SongSelect.CurrentGauge == 2 || SongSelect.CurrentGauge == 4) && GameManager.gauge <= GameManager.gauge_hosei_border) {
                    GameManager.gauge += GameManager.gauge_temp[SongSelect.CurrentGauge, 3] * GameManager.gauge_hosei_rate;
                }
                else {
                    GameManager.gauge += GameManager.gauge_temp[SongSelect.CurrentGauge, 3];
                }
            }
            else if (this.transform.position.x > 15 && GameManager.pushing_ln[line] > 0) {
                this.gameObject.SetActive(false);
                GameManager.judge.pg++;
                //Debug.Log("GREAT!!!");
                GameManager.combo++;
                GameManager.pushing_ln[line] = -1;
                Instantiate(this.JudgePG, new Vector3(15, 1, this.transform.position.z - 2), Quaternion.Euler(90, 270, 0));
                GameManager.judgedNotes[line]++;
                GameManager.gauge += GameManager.gauge_temp[SongSelect.CurrentGauge, 0];
            }
        }
    }
}
