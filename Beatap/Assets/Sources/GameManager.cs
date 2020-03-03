//     　　　 ∧ ,, ∧
//　　　　　(,,・∀・)
//　　　　～(_ u, u )
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Diagnostics;

public class GameManager:MonoBehaviour {
    //構造体
    struct Beat {//拍子(X分のY拍子)
        public int denominator;//拍子の分子
        public int numerator;//拍子の分母
    }
    public struct Judge {
        public int bd;
        public int gd;
        public int gr;
        public int pg;
    }
    public static Judge judge;

    public struct ResultInfo {
        public Judge JudgeNum;
        public int ClearType;//0:Failed 1:EasyClear 2:Clear 3:HardClear 4:EX-HardClear 5:Fullcombo 6:Perfect 7:MAX
        public string Song_name;
        public string Artist_name;
        public string Chart_name;
        public string Designer_name;
        public int Chart_level;
        public float Gauge;
        public Color32 gauge_color;//最終ゲージの色
        public int max_combo;
        public bool IsRegularChart;
        public string ChartId;
        public int fast_notes;
        public int slow_notes;
        public int left_notes;
    }
    public static ResultInfo ForResult;

    //設定済みゲームオブジェクトetc...
    public Launcher[] launchers;
    public GameObject EventLauncher;
    public GameObject JudgePG;
    public GameObject JudgeGR;
    public GameObject JudgeGD;
    public GameObject JudgeBD;
    public GameObject TimingFast;
    public GameObject TimingSlow;
    public GameObject ComboObj;
    public Slider gauge_ui;
    public Image gauge_fill;
    public Text gauge_value_text;
    public GameObject DangerPanel;

    //譜面読み込み用変数
    public static int[] notes_num;//ノーツ数(レーンごとに保存)
    int all_notes_num;//ノーツ数(全て)
    public static float gauge_hosei_rate;//補正時のダメージ倍率(Hard GaugeとGrade Gaugeの時に有効)
    public static float gauge_hosei_border;//補正がかかるゲージの残量
    List<List<float>> event_queue;//予約イベント
    int happened_event;//起動済みのイベントの数
    public static bool event_trigger;//イベント用トリガー
    public AudioClip HitSE;
    public static bool loaded_music;//曲の読み込み済みフラグ
    public static bool playing;//譜面の再生フラグ
    List<List<GameObject>> line_notes;//レーンごとの全ノーツを近い順に格納
    string song_name;
    string artist;
    string chart_name;
    int chart_level;
    string chart_designer;
    Beat beat;//拍子(?分の?)
    float bpm;
    float offset;
    public static float hispeed;//譜面内で別に定義されるスクロール速度
    public static float total_speed;//最終的なスクロール速度
    float last_notes_dist;//前のノーツが出現した距離
    TextMesh ComboText;//Combo数表示部分
    float total;

    //ゲームプレイ時のスコア、フラグなど
    public static int combo;
    int max_combo;
    public static float gauge;
    int[,] double_tap_time;//同時押し判定用タイマー
    public static int[] pushing_ln;//ロングノーツ判定用タイマー
    public static int[] judgedNotes;//判定済みのノーツ数(レーンごとに保存)
    public static int judge_level;
    public static int[,] judge_temp;
    public static float[,] gauge_temp;
    GameObject timing;//タイミング削除用
    int fast_notes;
    int slow_notes;

    void Start() {
        //初期化
        DangerPanel.gameObject.SetActive(false);
        judge.bd = 0;
        judge.gd = 0;
        judge.gr = 0;
        judge.pg = 0;
        timing = null;
        fast_notes = 0;
        slow_notes = 0;
        loaded_music = false;
        judge_temp = new int[5, 4] { { 45, 25, 15, 6 }, { 45, 15, 10, 4 }, { 45, 10, 5, 2 }, { 45, 1, 1, 1 }, { 45, 40, 24, 16 } };
        gauge_hosei_rate = 0.5f;
        gauge_hosei_border = 30f;
        hispeed = 1f;
        combo = 0;
        max_combo = 0;
        event_queue = new List<List<float>> { };
        line_notes = new List<List<GameObject>> { };
        happened_event = 0;
        event_trigger = false;
        double_tap_time = new int[5, 2] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        playing = false;
        judgedNotes = new int[5] { 0, 0, 0, 0, 0 };
        notes_num = new int[5] { 0, 0, 0, 0, 0 };
        pushing_ln = new int[5] { -1, -1, -1, -1, -1 };
        ComboText = ComboObj.GetComponent<TextMesh>();
        total = 300f;

        //ノーツオプションの設定
        int[] place_there = new int[5] { 0, 1, 2, 3, 4 };
        if (SongSelect.CurrentNoteOption == 1) {
            for (int i = 0; i < 5; i++) {
                place_there[i] = 4 - i;
            }
        }
        if (SongSelect.CurrentNoteOption == 2) {
            for (int i = 0; i < 4; i++) {
                int rnd = Random.Range(0, 5 - i);
                int tmp = place_there[rnd];
                place_there[rnd] = place_there[4 - i];
                place_there[4 - i] = tmp;
            }
        }
        if (SongSelect.CurrentNoteOption == 4) {
            int rnd = Random.Range(0, 5);
            for (int i = 0; i < 5; i++) {
                place_there[i] = (place_there[i] + rnd) % 5;
            }
        }

        //ゲージの初期化
        if (SongSelect.CurrentGauge < 2) {
            gauge = 0;
        }
        else {
            gauge = 100;
        }
        gauge_ui.value = gauge;
        gauge_value_text.text = gauge + "%";

        //曲ヘッダー
        song_name = SongSelect.songname;
        artist = SongSelect.artistname;
        ForResult.Song_name = song_name;
        ForResult.Artist_name = artist;

        //csv読み込み
        List<string[]> chart_csv_data = new List<string[]>();
        StreamReader sr = new StreamReader(SongSelect.chart_path, System.Text.Encoding.Default);
        while (sr.EndOfStream == false) {
            string line = sr.ReadLine();
            chart_csv_data.Add(line.Split(','));
        }
        sr.Close();

        //譜面ヘッダー読み込み
        chart_name = chart_csv_data[0][0];
        chart_level = int.Parse(chart_csv_data[0][1]);
        chart_designer = chart_csv_data[0][2];
        ForResult.Chart_name = chart_name;
        ForResult.Designer_name = chart_designer;
        ForResult.Chart_level = chart_level;
        ForResult.IsRegularChart = SongSelect.IsRegularChart;
        ForResult.ChartId = SongSelect.ChartId;
        bpm = float.Parse(chart_csv_data[0][3]);
        offset = float.Parse(chart_csv_data[0][4]) + 1 / (hispeed * SongSelect.hispeed);//offset値+ノーツの移動時間
        switch (chart_csv_data[0][5]) {
            case "easy":
                judge_level = 0;
                break;
            case "normal":
                judge_level = 1;
                break;
            case "hard":
                judge_level = 2;
                break;
            case "gambol":
                judge_level = 3;
                break;
            default:
                judge_level = 0;
                break;
        }
        if (SongSelect.CurrentAssistOption == 2 || SongSelect.CurrentAssistOption == 4 || SongSelect.CurrentAssistOption == 6 || SongSelect.CurrentAssistOption == 7) {
            judge_level = 4;
        }

        //譜面読み込み、ノーツ配置
        int load_line = 0;//読み込んだ行数
        load_line++;
        while (chart_csv_data[load_line][0] != "START") {
            if (chart_csv_data[load_line][0] == "TOTAL") {
                total = float.Parse(chart_csv_data[load_line][1]);
            }
            if (chart_csv_data[load_line][0] == "SCROLL") {
                hispeed = float.Parse(chart_csv_data[load_line][1]);
            }
            if (chart_csv_data[load_line][0] == "HOSEI_RATE") {
                gauge_hosei_rate = float.Parse(chart_csv_data[load_line][1]);
            }
            if (chart_csv_data[load_line][0] == "HOSEI_BORDER") {
                gauge_hosei_border = float.Parse(chart_csv_data[load_line][1]);
            }
            if (chart_csv_data[load_line][0] == "START_GAUGE") {
                gauge = float.Parse(chart_csv_data[load_line][1]);
            }
            if (chart_csv_data[load_line][0] == "GAUGE_TYPE") {
                SongSelect.CurrentGauge = int.Parse(chart_csv_data[load_line][1]);
            }
            load_line++;
            if (load_line > 500) {
                Debug.Log("[ERROR] STARTが500行の間に存在しません\n500行以内に記述してください");
                SceneManager.LoadScene("Scenes/SelectSong");
            }
        }
        int[] sran_long = new int[5] { -1, -1, -1, -1, -1 };
        bool[] legacy_check = new bool[5] { false, false, false, false, false };
        float last_notes_dist = (offset + 120f + PlayerPrefs.GetInt("localOffset", 0)) * SongSelect.hispeed * hispeed;//前のノーツが出現した距離
        load_line++;
        int loaded_beat = -1;//残りの読み込み待ち区間数
        int devide = -1;//1小節の分割数

        EventLauncher.GetComponent<EventLauncher>().LaunchEventNote(100f * SongSelect.hispeed * hispeed);//曲開始地点
        event_queue.Add(new List<float> { 0f, 0f });
        while (chart_csv_data[load_line][0] != "END") {
            Debug.Log(load_line + "行目を読み込み中...");
            total_speed = hispeed * SongSelect.hispeed;
            if (loaded_beat == -1) {
                beat.denominator = int.Parse(chart_csv_data[load_line][0]);
                beat.numerator = int.Parse(chart_csv_data[load_line][1]);
                devide = int.Parse(chart_csv_data[load_line][2]);
                loaded_beat = devide;
            }
            else {
                if (SongSelect.CurrentNoteOption == 3) {//S-RANDOM時の乱数生成
                    bool[] placed_num = new bool[5] { false, false, false, false, false };
                    bool[] placed_there = new bool[5] { false, false, false, false, false };
                    for (int i = 0; i < 5; i++) {
                        if (sran_long[i] != -1) {
                            placed_num[sran_long[i]] = true;
                            placed_there[i] = true;
                            place_there[i] = sran_long[i];
                        }
                    }
                    for (int i = 0; i < 5; i++) {
                        if (placed_num[i] == false) {
                            int rnd = Random.Range(0, 5);
                            if (placed_there[rnd] == false) {
                                place_there[rnd] = i;
                                placed_there[rnd] = true;
                            }
                        }
                    }
                }
                for (int line = 0; line < 5; line++) {//通常ノーツの設置
                    if (int.Parse(chart_csv_data[load_line][line]) == 1 || (int.Parse(chart_csv_data[load_line][line]) == 2 && (SongSelect.CurrentAssistOption == 1 || SongSelect.CurrentAssistOption == 4 || SongSelect.CurrentAssistOption == 5 || SongSelect.CurrentAssistOption == 7))) {
                        if (int.Parse(chart_csv_data[load_line][line]) == 1 || legacy_check[line] == false) {
                            launchers[place_there[line]].GetComponent<Launcher>().LaunchNote(place_there[line], last_notes_dist);
                            notes_num[place_there[line]]++;
                        }
                        legacy_check[line] = !legacy_check[line];
                    }
                    else if (int.Parse(chart_csv_data[load_line][line]) == 2) {//ロングノーツの設置
                        launchers[place_there[line]].GetComponent<Launcher>().LaunchLongNote(place_there[line], last_notes_dist, notes_num[place_there[line]]);
                        if (sran_long[line] == -1) {
                            sran_long[line] = place_there[line];
                        }
                        else {
                            sran_long[line] = -1;
                        }
                        notes_num[place_there[line]]++;
                    }
                    else if (int.Parse(chart_csv_data[load_line][line]) == 3) {//ダブルノーツの設置
                        if (SongSelect.CurrentAssistOption == 3 || SongSelect.CurrentAssistOption == 5 || SongSelect.CurrentAssistOption == 6 || SongSelect.CurrentAssistOption == 7) {
                            launchers[place_there[line]].GetComponent<Launcher>().LaunchNote(place_there[line], last_notes_dist);
                        }
                        else {
                            launchers[place_there[line]].GetComponent<Launcher>().LaunchDoubleNote(place_there[line], last_notes_dist);
                        }
                        notes_num[place_there[line]]++;
                    }
                }
                //イベントの予約
                if (chart_csv_data[load_line][5] == "SCROLL") {
                    EventLauncher.GetComponent<EventLauncher>().LaunchEventNote(last_notes_dist);
                    hispeed = float.Parse(chart_csv_data[load_line][6]);
                    event_queue.Add(new List<float> { 2f, float.Parse(chart_csv_data[load_line][6]) });
                    total_speed = hispeed * SongSelect.hispeed;
                }
                else if (chart_csv_data[load_line][5] == "BPMCHANGE") {
                    bpm = float.Parse(chart_csv_data[load_line][6]);
                }
                last_notes_dist += (240 * beat.denominator) / (bpm * devide * beat.numerator) * 100 * total_speed;//一区間あたりの距離の増加量を計算
            }
            load_line++;
            loaded_beat--;
        }
        EventLauncher.GetComponent<EventLauncher>().LaunchEventNote(last_notes_dist);
        event_queue.Add(new List<float> { 1f, 0f });

        //ノーツを近い順にソート
        for (int i = 0; i < 5; i++) {
            line_notes.Add(new List<GameObject>(GameObject.FindGameObjectsWithTag("Line" + (i + 1))));
            line_notes[i].Sort((a, b) => (int)(b.transform.position.x - a.transform.position.x));
        }

        //ゲージの増減量を設定
        all_notes_num = notes_num[0] + notes_num[1] + notes_num[2] + notes_num[3] + notes_num[4];
        gauge_temp = new float[5, 4] {
            { total / all_notes_num, total / all_notes_num, (total / 2) / all_notes_num, Mathf.Min(-6f, (-total * 6f) / all_notes_num) }, //Normal Gauge
            { total / all_notes_num, total / all_notes_num, (total / 2) / all_notes_num, Mathf.Min(-4f, (-total * 4f) / all_notes_num) }, //Easy Gauge
            { 0.3f, 0.25f, 0.15f, Mathf.Min(-9f, (-total * 9f) / all_notes_num) }, //Hard Gauge
            { 0.15f, 0.12f, Mathf.Min(-4f, (-total * 4f) / all_notes_num), Mathf.Min(-18f, (-total * 18f) / all_notes_num) }, //EX-Hard Gauge
            { 0.15f, 0.12f, 0.03f, Mathf.Min(-2f, (-total * 2f) / all_notes_num) } };//Grade Gauge
        hispeed = 1f;

        Debug.Log("譜面データの読み込み完了");
        playing = true;
    }

    void Update() {
        if (playing == false) {
            total_speed = 0f;
        }
        else if (playing == true && loaded_music == true) {
            total_speed = hispeed * SongSelect.hispeed;
            gauge = Mathf.Max(Mathf.Min(gauge, 100), 0);
            gauge_ui.value = gauge;
            if (gauge < 0 && SongSelect.CurrentGauge < 2) {
                gauge = 0;
            }
            if (max_combo < combo) {
                max_combo = combo;
            }
            switch (PlayerPrefs.GetInt("comboShow", 0)) {
                case 0:
                    ComboText.text = "" + combo;
                    break;
                case 1:
                    if (judgedNotes[0] + judgedNotes[1] + judgedNotes[2] + judgedNotes[3] + judgedNotes[4] > 0) {
                        ComboText.text = ((judge.pg * 100f + judge.gd * 50f) / (judgedNotes[0] + judgedNotes[1] + judgedNotes[2] + judgedNotes[3] + judgedNotes[4])).ToString("F2");
                    }
                    break;
                case 2:
                    ComboText.text = "";
                    break;
            }
            switch (SongSelect.CurrentGauge) {
                case 0://Normal
                    if (gauge >= 70) {
                        gauge_fill.color = new Color32(0, 255, 0, 255);
                        gauge_value_text.color = new Color32(0, 255, 0, 255);
                    }
                    else {
                        gauge_fill.color = new Color32(0, 160, 0, 255);
                        gauge_value_text.color = new Color32(255, 255, 255, 255);
                    }
                    break;
                case 1://Easy
                    if (gauge >= 70) {
                        gauge_fill.color = new Color32(255, 0, 255, 255);
                        gauge_value_text.color = new Color32(0, 255, 0, 255);
                    }
                    else {
                        gauge_fill.color = new Color32(175, 0, 255, 255);
                        gauge_value_text.color = new Color32(255, 255, 255, 255);
                    }
                    break;
                case 2://Hard
                    if (gauge <= gauge_hosei_border) {
                        gauge_fill.color = new Color32(255, 0, 0, 255);
                        gauge_value_text.color = new Color32(255, 0, 0, 255);
                        DangerPanel.SetActive(true);
                    }
                    else {
                        gauge_fill.color = new Color32(150, 0, 0, 255);
                        gauge_value_text.color = new Color32(255, 255, 255, 255);
                        DangerPanel.SetActive(false);
                    }
                    break;
                case 3://EX-Hard
                    if (gauge <= gauge_hosei_border) {
                        gauge_fill.color = new Color32(255, 75, 0, 255);
                        gauge_value_text.color = new Color32(255, 0, 0, 255);
                        DangerPanel.SetActive(true);
                    }
                    else {
                        gauge_fill.color = new Color32(255, 100, 0, 255);
                        gauge_value_text.color = new Color32(255, 255, 255, 255);
                        DangerPanel.SetActive(false);
                    }
                    break;
                case 4://Grade
                    if (gauge <= gauge_hosei_border) {
                        gauge_fill.color = new Color32(255, 0, 0, 255);
                        gauge_value_text.color = new Color32(255, 0, 0, 255);
                        DangerPanel.SetActive(true);
                    }
                    else {
                        gauge_fill.color = new Color32(150, 0, 0, 255);
                        gauge_value_text.color = new Color32(255, 255, 255, 255);
                        DangerPanel.SetActive(false);
                    }
                    break;
            }
            gauge_value_text.text = Mathf.Round(gauge) + "%";

            for (int i = 0; i < 5; i++) {
                if (double_tap_time[i, 0] > 0) {
                    double_tap_time[i, 0]--;
                }
                if (double_tap_time[i, 1] > 0) {
                    double_tap_time[i, 1]--;
                }
                if (Input.GetKeyDown(SongSelect.LineKey[i, 0]) && judgedNotes[i] < notes_num[i]) {
                    double_tap_time[i, 0] = 5;
                }
                if (Input.GetKeyDown(SongSelect.LineKey[i, 1]) && judgedNotes[i] < notes_num[i]) {
                    double_tap_time[i, 1] = 5;
                }
                if ((Input.GetKeyDown(SongSelect.LineKey[i, 0]) || Input.GetKeyDown(SongSelect.LineKey[i, 1])) && judgedNotes[i] < notes_num[i]) {
                    NotesJudge(i);
                }
                if (pushing_ln[i] > -1 && !(Input.GetKey(SongSelect.LineKey[i, 0]) || Input.GetKey(SongSelect.LineKey[i, 1])) && judgedNotes[i] < notes_num[i]) {
                    LNEndJudge(i);
                }
            }

            if (event_trigger) {
                event_trigger = false;
                switch (event_queue[happened_event][0]) {
                    case 0:
                        MusicManager.canPlayMusic = true;
                        break;
                    case 1:
                        ViewResult();
                        break;
                    case 2:
                        hispeed = event_queue[happened_event][1];
                        break;
                    default:
                        Debug.Log("[Warning] 定義していないイベントが呼ばれました\nこのイベントは無視されます");
                        break;
                }
                happened_event++;
            }

            if (gauge <= 0 && SongSelect.CurrentGauge > 1) {//ハードゲージ以上でゲージが0になった際にResultへ
                judge.bd += all_notes_num - judgedNotes[0] - judgedNotes[1] - judgedNotes[2] - judgedNotes[3] - judgedNotes[4];
                ForResult.left_notes = all_notes_num - judgedNotes[0] - judgedNotes[1] - judgedNotes[2] - judgedNotes[3] - judgedNotes[4];
                MusicManager.audioSource.Stop();
                ViewResult();
            }
        }
    }

    void NotesJudge(int line) {
        GameObject Note = line_notes[line][judgedNotes[line]];
        float dist = Mathf.Abs(15 - Note.transform.position.x);
        if (dist < judge_temp[judge_level, 0] * total_speed && (!Note.name.Contains("LongNote") || pushing_ln[line] == -1) && !(Note.name.Contains("DoubleNote") && (double_tap_time[line, 0] == 0 || double_tap_time[line, 1] == 0))) {//判定(スピード*ジャッジの値 と 距離 の差で判定)
            if (dist < judge_temp[judge_level, 1] * total_speed) {
                if (dist < judge_temp[judge_level, 2] * total_speed) {
                    if (dist < judge_temp[judge_level, 3] * total_speed) {
                        //Debug.Log("GREAT!!!");
                        judge.pg++;
                        gauge += gauge_temp[SongSelect.CurrentGauge, 0];
                        Instantiate(this.JudgePG, new Vector3(15, Note.transform.position.y + 1, Note.transform.position.z - 2), Quaternion.Euler(90, 270, 0));
                    }
                    else {
                        //Debug.Log("GREAT");
                        judge.gr++;
                        gauge += gauge_temp[SongSelect.CurrentGauge, 1];
                        Instantiate(this.JudgeGR, new Vector3(15, Note.transform.position.y + 1, Note.transform.position.z - 2), Quaternion.Euler(90, 270, 0));
                    }
                }
                else {
                    //Debug.Log("GOOD");
                    judge.gd++;
                    gauge += gauge_temp[SongSelect.CurrentGauge, 2];
                    Instantiate(this.JudgeGD, new Vector3(15, Note.transform.position.y + 1, Note.transform.position.z - 2), Quaternion.Euler(90, 270, 0));
                }
                if (Note.name.Contains("LongNote")) {
                    pushing_ln[line] = 30;
                }
                combo++;
                if (15 - Note.transform.position.x > judge_temp[judge_level, 3] * total_speed) {
                    if (timing != null) {
                        timing.gameObject.SetActive(false);
                    }
                    fast_notes++;
                    timing = Instantiate(this.TimingFast, new Vector3(15f, 5f, -1.05f), Quaternion.Euler(90, 270, 0));
                }
                else if (15 - Note.transform.position.x < -judge_temp[judge_level, 3] * total_speed) {
                    if (timing != null) {
                        timing.gameObject.SetActive(false);
                    }
                    slow_notes++;
                    timing = Instantiate(this.TimingSlow, new Vector3(15f, 5f, -1.05f), Quaternion.Euler(90, 270, 0));
                }
            }
            else {
                //Debug.Log("BAD");
                combo = 0;
                judge.bd++;
                Instantiate(this.JudgeBD, new Vector3(14.5f, 1, Note.transform.position.z - 2), Quaternion.Euler(90, 270, 0));
                if ((SongSelect.CurrentGauge == 2 || SongSelect.CurrentGauge == 4) && gauge <= gauge_hosei_border) {
                    gauge += gauge_temp[SongSelect.CurrentGauge, 3] * gauge_hosei_rate;
                }
                else {
                    gauge += gauge_temp[SongSelect.CurrentGauge, 3];
                }
                if (Note.name.Contains("LongNote")) {
                    pushing_ln[line] = 0;
                }
            }
            Note.gameObject.SetActive(false);
            judgedNotes[line]++;
        }
    }

    void LNEndJudge(int line) {
        if (pushing_ln[line] == 0) {
            pushing_ln[line] = -1;
            GameObject Note = line_notes[line][judgedNotes[line]];
            Debug.Log("BAD");
            combo = 0;
            judge.bd++;
            Instantiate(this.JudgeBD, new Vector3(14.5f, 1, Note.transform.position.z - 2), Quaternion.Euler(90, 270, 0));
            if ((SongSelect.CurrentGauge == 2 || SongSelect.CurrentGauge == 4) && gauge <= gauge_hosei_border) {
                gauge += gauge_temp[SongSelect.CurrentGauge, 3] * gauge_hosei_rate;
            }
            else {
                gauge += gauge_temp[SongSelect.CurrentGauge, 3];
            }
            Note.gameObject.SetActive(false);
            judgedNotes[line]++;
        }
        else {
            pushing_ln[line]--;
        }
    }

    public void ViewResult() { //GameManager君の終活
        ForResult.gauge_color = gauge_fill.color;
        if ((gauge >= 70 && SongSelect.CurrentGauge <= 1) || (gauge > 0 && SongSelect.CurrentGauge >= 2)) {
            ForResult.ClearType = SongSelect.CurrentGauge + 1;
            if (judge.bd == 0) {
                if (judge.gd == 0) {
                    if (judge.gr == 0) {
                        ForResult.ClearType = 8;
                    }
                    else {
                        ForResult.ClearType = 7;
                    }
                }
                else {
                    ForResult.ClearType = 6;
                }
            }
        }
        else {
            ForResult.ClearType = 0;
        }
        ForResult.Gauge = gauge;
        ForResult.JudgeNum = judge;
        ForResult.max_combo = max_combo;
        ForResult.fast_notes = fast_notes;
        ForResult.slow_notes = slow_notes;
        SceneManager.LoadScene("Scenes/Result");
    }
}