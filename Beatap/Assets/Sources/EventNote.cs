using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventNote:MonoBehaviour {
    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update() {
        if ((GameManager.playing && GameManager.loaded_music) || PauseManager.isPause) {
            this.GetComponent<Rigidbody>().velocity = new Vector3(GameManager.total_speed * 100, 0, 0);
        }
        if (this.transform.position.x > 15) {
            GameManager.event_trigger = true;
            this.gameObject.SetActive(false);
        }
    }
}
