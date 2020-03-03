using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeText:MonoBehaviour {
    public float life_time = 0.3f;
    float time = 0f;

    // Use this for initialization
    void Start() {
        time = 0;
    }

    // Update is called once per frame
    void Update() {
        time += Time.deltaTime;
        GetComponent<TextMesh>().color = new Color(this.GetComponent<TextMesh>().color.r, this.GetComponent<TextMesh>().color.g, this.GetComponent<TextMesh>().color.b, (life_time-time)*10/3);
        if (time > life_time) {
            this.gameObject.SetActive(false);
        }
    }
}
