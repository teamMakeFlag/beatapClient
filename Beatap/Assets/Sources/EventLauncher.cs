using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Event ID:Event(引数はx)
 * 0:譜面の開始(引数不要)
 * 1:譜面の終了(引数不要)
 * 2:スクロール速度をx倍
 * 
 * 
 * 
 * 
 * 
 * 
 */

public class EventLauncher:MonoBehaviour {
    public GameObject EventNote;
    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update() {
    }

    public void LaunchEventNote(float dist) {
        var transform = this.EventNote.transform.position;
        GameObject EventNotes = Instantiate(this.EventNote, new Vector3(15 - dist, transform.y, transform.z), Quaternion.identity);
    }
}
