using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class SongSelect:MonoBehaviour {
#if UNITY_EDITOR
        #if UNITY_EDITOR_WIN
            const string sep = "\\";
            const string SONGS_FOLDER_PATH = "\\Resources\\Songs";
        #else
            const string sep = "/";
            const string SONGS_FOLDER_PATH = "/Resources/Songs";
        #endif
    #else
        #if UNITY_STANDALONE_WIN
            const string sep = "\\";
            const string SONGS_FOLDER_PATH = "\\Resources\\Songs";
        #elif UNITY_STANDALONE_LINUX
            const string sep = "/";
            const string SONGS_FOLDER_PATH = "/Resources/Songs";
        #endif
    #endif
    public static KeyCode[,] LineKey = new KeyCode[5, 2] { { KeyCode.Q, KeyCode.Y }, { KeyCode.W, KeyCode.U }, { KeyCode.E, KeyCode.I }, { KeyCode.R, KeyCode.O }, { KeyCode.T, KeyCode.P } };
    public static KeyCode HispeedDownKey = KeyCode.Q;
    public static KeyCode HispeedUpKey = KeyCode.W;
    public static KeyCode ChangeNoteOptionKey = KeyCode.E;
    public static KeyCode ChangeAssistOptionKey = KeyCode.R;
    public static KeyCode ChangeGaugeKey = KeyCode.T;
    //public static KeyCode ChangeSettings = KeyCode.Z;
    //public static KeyCode ChangeValues = KeyCode.X;
    public static KeyCode DecideKey = KeyCode.Space;

    public static int CurrentNoteOption = 0;
    public static int CurrentAssistOption = 0;
    public static int CurrentGauge = 0; //0=Normal 1=Easy 2=Hard 3=EXHard
    public static string[,] OptionName = new string[2, 8] { { "OFF", "Mirror", "Random", "S-Random", "R-Random", "", "", "" }, { "OFF", "Legacy Note", "Expand Judge", "Double to Single", "Legacy & Expand", "Legacy & Single", "Expand & Single", "All Assist" } };
    public static string[] GaugeName = { "Normal", "Easy", "Hard", "EX-Hard", "Grade" };

    public Text SongName = null;
    public Text ArtistName = null;
    public Text DesignerName = null;
    public Text ChartName = null;
    public Text Level = null;
    public Image SongImage = null;
    public Text Hispeed = null;
    public Text BPM = null;
    public Text ErrorMsg = null;
    public Text Gauge = null;
    public Text NoteOption = null;
    public Text AssistOption = null;
    public Text BestGauge = null;
    public Text BestScore = null;
    public Text BestExScore = null;
    public static string chart_path;
    public static string song_path;
    public static string ChartId = "";
    public static bool IsRegularChart = false;
    public static string songname;
    public static string artistname;
    private string designername;
    public static string chartname;
    public bool isread = false;
    private int level;
    public static float hispeed = 1f;
    private float bpm;
    public static Texture2D songimage;
    private string nowdir;
    private List<string> audiopaths = new List<string>();
    private List<List<string>> chartpaths = new List<List<string>>();
    private List<string> songnames = new List<string>();
    private List<string> artistnames = new List<string>();
    private List<List<string>> designernames = new List<List<string>>();
    private List<List<string>> chartnames = new List<List<string>>();
    private List<List<string>> chartjudges = new List<List<string>>();
    private List<List<float>> chartbpms = new List<List<float>>();
    private List<List<int>> chartlevels = new List<List<int>>();
    private List<Texture2D> songimages = new List<Texture2D>();
    private List<string> errormsgs = new List<string>();
    private string[] ApprovalExt = {".ogg", ".csv", ".jpg"};
    private string[] ApprovalAudioExt = {".mp3", ".ogg", ".wav", ".aif",".MP3",".OGG",".WAV",".AIF"};
    private string[] ApprovalImageExt = {".psd",".tga",".tiff",".png",".pict",".jpg",".jpeg",".iff",".hdr",".gif",".exr",".bmp",".PSD",".TGA",".TIFF",".PNG",".PICT",".JPG",".JPEG",".IFF",".HDR",".GIF",".EXR",".BMP"};
    public static int selected_song = 0;
    public static int selected_difficult = 0;

    private NotifyManager notifyManager;

    // Start is called before the first frame update
    void Start() {
        notifyManager = GameObject.Find("NotifyManager").GetComponent<NotifyManager>();
        LoadSongData();
        ShowData();
    }
    
    void LoadSongData()
    {
        nowdir = Application.dataPath+SONGS_FOLDER_PATH+sep;
        DirectoryInfo directory = new DirectoryInfo(nowdir);
        DirectoryInfo[] information_d = directory.GetDirectories();
        Boolean isImageRead = false;
        Boolean isAudioRead = false;
        Boolean isMTextRead = false;
        string errormsg = "";
        foreach (DirectoryInfo info in information_d) {
            List<string> cpath = new List<string>();
            List<string> cname = new List<string>();
            List<string> cjudge = new List<string>();
            List<float> cbpm = new List<float>();
            List<int> clevel = new List<int>();
            List<string> cdesigner = new List<string>();
            foreach (FileInfo inf in info.GetFiles()) {
                string infstr = inf + "";
                if (infstr.LastIndexOf("music.txt") == infstr.Length - 9) {
                    StreamReader sr = new StreamReader(@infstr, System.Text.Encoding.Default);
                    string value = sr.ReadToEnd();
                    sr.Close();
                    string[] values = value.Split('\n');
                    songnames.Add(values[0]);
                    artistnames.Add(values[1]);
                    isMTextRead = true;
                }
                else if (CheckExt(ApprovalAudioExt, infstr)) {
                    if (!isAudioRead) {
                        audiopaths.Add(infstr);
                        isAudioRead = true;
                    }
                    else if (isAudioRead) {
                        errormsg += "複数のオーディオファイルが検出されました.\n";
                    }
                }
                else if (CheckExt(infstr) == ".csv") {
                    cpath.Add(infstr);
                    StreamReader sr = new StreamReader(@infstr, System.Text.Encoding.Default);
                    string value = sr.ReadToEnd();
                    sr.Close();
                    string[] values = value.Split(',');
                    cname.Add(values[0]);
                    clevel.Add(int.Parse(values[1]));
                    cdesigner.Add(values[2]);
                    cbpm.Add(float.Parse(values[3]));
                    cjudge.Add(values[5]);
                }
                else if (CheckExt(ApprovalImageExt, infstr)) {
                    if (!isImageRead) {
                        songimages.Add(Resources.Load("Textures" + sep + "noimage") as Texture2D);
                        int songimages_index = songimages.Count - 1;
                        StartCoroutine(GetImage(infstr, songimages_index));
                        isImageRead = true;
                    }
                    else if (isImageRead) {
                        errormsg += "複数の画像ファイルが検出されました.\n";
                    }
                }
            }
            chartpaths.Add(cpath);
            chartnames.Add(cname);
            chartjudges.Add(cjudge);
            chartbpms.Add(cbpm);
            chartlevels.Add(clevel);
            designernames.Add(cdesigner);
            if (!isImageRead) {
                songimages.Add(Resources.Load("Textures" + sep + "noimage") as Texture2D);
            }
            if (!isAudioRead) {
                audiopaths.Add("");
                errormsg += "オーディオファイルが見つかりませんでした.\n";
            }
            if (!isMTextRead) {
                errormsg += "「music.txt」が見つかりません.";
                songnames.Add("");
                artistnames.Add("");
            }
            errormsgs.Add(errormsg);
            isImageRead = false;
            isAudioRead = false;
        }
        isread = true;
    }

    void ShowData() {
        songname = songnames[selected_song];
        artistname = artistnames[selected_song];
        designername = designernames[selected_song][selected_difficult];
        chartname = chartnames[selected_song][selected_difficult];
        level = chartlevels[selected_song][selected_difficult];
        bpm = chartbpms[selected_song][selected_difficult];
        songimage = songimages[selected_song];
        SongName = SongName.GetComponent<Text>();
        ArtistName = ArtistName.GetComponent<Text>();
        DesignerName = DesignerName.GetComponent<Text>();
        ChartName = ChartName.GetComponent<Text>();
        Level = Level.GetComponent<Text>();
        Hispeed = Hispeed.GetComponent<Text>();
        SongImage = SongImage.GetComponent<Image>();
        BPM = BPM.GetComponent<Text>();
        ErrorMsg = ErrorMsg.GetComponent<Text>();
        NoteOption = NoteOption.GetComponent<Text>();
        AssistOption = AssistOption.GetComponent<Text>();
        BestGauge = BestGauge.GetComponent<Text>();
        BestScore = BestScore.GetComponent<Text>();
        BestExScore = BestExScore.GetComponent<Text>();
        SongName.text = songname;
        ArtistName.text = artistname;
        DesignerName.text = designername;
        ChartName.text = chartname;
        Level.text = "Lv" + level.ToString();
        Hispeed.text = hispeed.ToString("F");
        BPM.text = "BPM:" + bpm.ToString();
        ErrorMsg.text = errormsgs[selected_song];
        SongImage.sprite = Sprite.Create(songimage, new Rect(0, 0, songimage.width, songimage.height), Vector2.zero);
        NoteOption.text = OptionName[0, CurrentNoteOption];
        AssistOption.text = OptionName[1, CurrentAssistOption];
        Gauge.text = GaugeName[CurrentGauge];

        // 最高スコア関連
        string username = PlayerPrefs.GetString("username", "default");
        string localDataModStr = username + "__" + songname + "__" + chartname + "__";
        switch (PlayerPrefs.GetInt(localDataModStr + "clearType", 0))
        {
            case 0:
                BestGauge.text = "Failed";
                break;
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
                BestGauge.text = "Clear";
                break;
            case 6:
                BestGauge.text = "FullCombo Clear!";
                break;
            case 7:
                BestGauge.text = "Perfect Clear!";
                break;
            case 8:
                BestGauge.text = "MAX Clear!!!";
                break;
            default:
                BestGauge.text = "Failed";
                Debug.Log("[WARNING] 定義されていないクリアタイプです\nこのリザルトのクリアタイプはFailedとして扱われます");
                break;
        }
        BestScore.text = PlayerPrefs.GetFloat(localDataModStr + "score", 00.00f).ToString("F2") + "%";
        BestExScore.text = PlayerPrefs.GetInt(localDataModStr + "exScore", 0).ToString();
    }

    string CheckExt(string str) {
        foreach (string ext in ApprovalExt) {
            if (str.Length - ext.Length == str.LastIndexOf(ext))
                return ext;
        }
        return "";
    }
    Boolean CheckExt(string[] exts, string str) {
        foreach (string ext in exts) {
            if (str.Length - ext.Length == str.LastIndexOf(ext))
                return true;
        }
        return false;
    }

    IEnumerator GetImage(string path, int songimages_index) {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + path);
        yield return www.SendWebRequest();
        songimages[songimages_index] = DownloadHandlerTexture.GetContent(www);
        if(isread)ShowData();
    }
    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(ChangeNoteOptionKey) && SettingsManager.SettingsActive == false)
        {
            CurrentNoteOption = (CurrentNoteOption + 1) % 5;
            NoteOption.text = OptionName[0, CurrentNoteOption];
        }
        if (Input.GetKeyDown(ChangeAssistOptionKey) && SettingsManager.SettingsActive == false)
        {
            CurrentAssistOption = (CurrentAssistOption + 1) % 8;
            AssistOption.text = OptionName[1, CurrentAssistOption];
        }
        if (Input.GetKeyDown(ChangeGaugeKey) && SettingsManager.SettingsActive == false) {
            switch (CurrentGauge) {
                case 0: // Normal -> Hard
                    CurrentGauge = 2;
                    break;
                case 1: // Easy -> Normal
                    CurrentGauge = 0;
                    break;
                case 2: // Hard -> EX-Hard
                    CurrentGauge = 3;
                    break;
                case 3: // EX-Hard -> Easy
                    CurrentGauge = 1;
                    break;
                case 4: // Grade -> Normal
                    CurrentGauge = 0;
                    break;
            }
            if (Input.GetKey(KeyCode.LeftShift) && SettingsManager.SettingsActive == false) {
                CurrentGauge = 4;
                Debug.Log("注意 ゲージの種類をGradeにした場合、リザルトは保存されません!");
                notifyManager.Notify("注意 ゲージの種類をGradeにした場合、リザルトは保存されません!");
            }
            Gauge.text = GaugeName[CurrentGauge];
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) && SettingsManager.SettingsActive == false) {
            if (selected_song == 0)
                selected_song = songnames.Count - 1;
            else
                selected_song--;
            selected_difficult = 0;
            ShowData();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) && SettingsManager.SettingsActive == false) {
            if (selected_song == songnames.Count - 1)
                selected_song = 0;
            else
                selected_song++;
            selected_difficult = 0;
            ShowData();
        }
        if (errormsgs[selected_song] == "" && Input.GetKeyDown(KeyCode.UpArrow) && SettingsManager.SettingsActive == false) {
            if (selected_difficult == chartlevels[selected_song].Count - 1)
                selected_difficult = 0;
            else
                selected_difficult++;
            ShowData();
        }
        if (errormsgs[selected_song] == "" && Input.GetKeyDown(KeyCode.DownArrow) && SettingsManager.SettingsActive == false) {
            if (selected_difficult == 0)
                selected_difficult = chartlevels[selected_song].Count - 1;
            else
                selected_difficult--;
            ShowData();
        }
        if (errormsgs[selected_song] == "" && Input.GetKeyDown(DecideKey) && SettingsManager.SettingsActive == false) {
            chart_path = chartpaths[selected_song][selected_difficult];
            song_path = audiopaths[selected_song];
            // 譜面のidと譜面がサーバーからダウンロードされたものかどうかを取得
            chartpaths[selected_song][selected_difficult] = chartpaths[selected_song][selected_difficult].Replace(sep, "/");
            string[] splitedChartPath = chartpaths[selected_song][selected_difficult].Split('/');
            string chartFileName = splitedChartPath[splitedChartPath.Length-1];
            chartFileName = chartFileName.Replace(".csv", "");
            chartFileName = chartFileName.Replace(".CSV", "");
            if (chartFileName.IndexOf("_") != -1)
            {
                IsRegularChart = chartFileName.Split('_')[0] == "BeatapChart";
                ChartId = chartFileName.Split('_')[1];
            }
            SceneManager.LoadScene("Scenes/GameScene");
        }
        if (errormsgs[selected_song] == "" && Input.GetKeyDown(HispeedUpKey) && SettingsManager.SettingsActive == false) {
            hispeed += 0.05f;
            if (hispeed > 3f) {
                hispeed = 3f;
            }
            hispeed = Mathf.Round(100 * hispeed) / 100;
            Hispeed = Hispeed.GetComponent<Text>();
            Hispeed.text = hispeed.ToString("F");
        }
        if (errormsgs[selected_song] == "" && Input.GetKeyDown(HispeedDownKey) && SettingsManager.SettingsActive == false) {
            hispeed -= 0.05f;
            if (hispeed < 0.1f) {
                hispeed = 0.1f;
            }
            hispeed = Mathf.Round(100 * hispeed) / 100;
            Hispeed = Hispeed.GetComponent<Text>();
            Hispeed.text = hispeed.ToString("F");
        }
        if (Input.GetKeyDown(KeyCode.Escape) && SettingsManager.SettingsActive == false)
        {
            if(MusicManager.audioSource != null)
            {
                MusicManager.audioSource.Stop();
            }
            SceneManager.LoadScene("Scenes/Title");
        }
    }
}
