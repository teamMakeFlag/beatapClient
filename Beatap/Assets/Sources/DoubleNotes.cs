using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleNotes:MonoBehaviour {
    public GameObject JudgeBD;
    public int line;
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if ((GameManager.playing && GameManager.loaded_music) || PauseManager.isPause) {
            this.GetComponent<Rigidbody>().velocity = new Vector3(GameManager.total_speed * 100, 0, 0);
        }
        if (this.transform.position.x > 15 + (GameManager.judge_temp[GameManager.judge_level, 1] * GameManager.total_speed)) {
            this.gameObject.SetActive(false);
            GameManager.judge.bd++;
            //Debug.Log("BAD");
            GameManager.combo = 0;
            Instantiate(this.JudgeBD, new Vector3(14.5f, 1, this.transform.position.z - 2), Quaternion.Euler(90, 270, 0));
            GameManager.judgedNotes[line]++;
            if ((SongSelect.CurrentGauge == 2 || SongSelect.CurrentGauge == 4) && GameManager.gauge <= GameManager.gauge_hosei_border) {
                GameManager.gauge += GameManager.gauge_temp[SongSelect.CurrentGauge, 3] * GameManager.gauge_hosei_rate;
            }
            else {
                GameManager.gauge += GameManager.gauge_temp[SongSelect.CurrentGauge, 3];
            }
        }
    }
}
