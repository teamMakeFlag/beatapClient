using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher:MonoBehaviour {
    public GameObject Note;
    public GameObject LongNote;
    public GameObject LNBridge;
    public GameObject DoubleNote;
    public string[] TagNames = new string[5] { "Line1", "Line2", "Line3", "Line4", "Line5" };
    public float place_dist = -1f;
    // Start is called before the first frame update
    void Start() {}

    // Update is called once per frame
    void Update() {}

    public void LaunchNote(int line, float dist) {
        var transform = this.Note.transform.position;
        GameObject Notes = Instantiate(this.Note, new Vector3(15 - dist, transform.y, transform.z), Quaternion.identity);
        Notes.tag = TagNames[line];
    }

    public void LaunchLongNote(int line, float dist, int note_num) {
        var transform = this.LongNote.transform.position;
        GameObject LongNotes = Instantiate(this.LongNote, new Vector3(15 - dist, transform.y, transform.z), Quaternion.Euler(90, 0, 0));
        LongNotes.tag = TagNames[line];
        if (place_dist != -1f) {
            GameObject LNBridges = Instantiate(this.LNBridge, new Vector3((30 - dist - place_dist) / 2.0f, transform.y, transform.z), Quaternion.identity);
            LNBridge script = LNBridges.GetComponent<LNBridge>();
            script.getInfo(line, note_num);
            LNBridges.transform.localScale = new Vector3((dist - place_dist) / 10f, 1, 0.2f);
            place_dist = -1f;
        }
        else {
            place_dist = dist;
        }
    }

    public void LaunchDoubleNote(int line, float dist) {
        var transform = this.DoubleNote.transform.position;
        GameObject DoubleNotes = Instantiate(this.DoubleNote, new Vector3(15 - dist, transform.y, transform.z), Quaternion.identity);
        DoubleNotes.tag = TagNames[line];
    }
}
