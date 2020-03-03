using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class ResultManager:MonoBehaviour {
    private CommunicationManager communicationManager;
    public Text ClearFlag = null;
    public Text JudgeGreatest = null;
    public Text JudgeGreat = null;
    public Text JudgeGood = null;
    public Text JudgeBad = null;
    public Text ClearType = null;
    public Text SongName = null;
    public Image SongImage = null;
    public Text ArtistName = null;
    public Text ChartName = null;
    public Text ChartDesignerName = null;
    public Text ChartLevel = null;
    public Text Score = null;
    public Text EXScore = null;
    public Text MaxCombo = null;
    public Text Fast = null;
    public Text Slow = null;
    public Text HiSpeed = null;
    public Text NoteOption = null;
    public Text AssistOption = null;
    public Text LeftNotes = null;
    public Slider gauge_ui;
    public Image gauge_fill;
    int notes_num;
    void Start() {
        ClearFlag = ClearFlag.GetComponent<Text>();
        JudgeGreatest = JudgeGreatest.GetComponent<Text>();
        JudgeGreat = JudgeGreat.GetComponent<Text>();
        JudgeGood = JudgeGood.GetComponent<Text>();
        JudgeBad = JudgeBad.GetComponent<Text>();
        ClearType = ClearType.GetComponent<Text>();
        SongName = SongName.GetComponent<Text>();
        SongImage = SongImage.GetComponent<Image>();
        ArtistName = ArtistName.GetComponent<Text>();
        ChartName = ChartName.GetComponent<Text>();
        ChartDesignerName = ChartDesignerName.GetComponent<Text>();
        ChartLevel = ChartLevel.GetComponent<Text>();
        Score = Score.GetComponent<Text>();
        EXScore = EXScore.GetComponent<Text>();
        MaxCombo = MaxCombo.GetComponent<Text>();
        Fast = Fast.GetComponent<Text>();
        Slow = Slow.GetComponent<Text>();
        HiSpeed = HiSpeed.GetComponent<Text>();
        NoteOption = NoteOption.GetComponent<Text>();
        AssistOption = AssistOption.GetComponent<Text>();

        switch (GameManager.ForResult.ClearType) {
            case 0:
                ClearFlag.text = "Failed";
                break;
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
                ClearFlag.text = "Clear";
                break;
            case 6:
                ClearFlag.text = "FullCombo Clear!";
                break;
            case 7:
                ClearFlag.text = "Perfect Clear!";
                break;
            case 8:
                ClearFlag.text = "MAX Clear!!!";
                break;
            default:
                ClearFlag.text = "Failed";
                Debug.Log("[WARNING] 定義されていないクリアタイプです\nこのリザルトのクリアタイプはFailedとして扱われます");
                break;
        }
        switch (SongSelect.CurrentGauge) {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
                ClearType.text = SongSelect.GaugeName[SongSelect.CurrentGauge];
                break;
            default:
                ClearType.text = "Easy";
                Debug.Log("[WARNING] 定義されていないゲージタイプです\nこのリザルトのゲージタイプはEasyゲージとして扱われます");
                break;
        }
        gauge_fill.color = GameManager.ForResult.gauge_color;
        JudgeGreatest.text = GameManager.ForResult.JudgeNum.pg.ToString();
        JudgeGreat.text = GameManager.ForResult.JudgeNum.gr.ToString();
        JudgeGood.text = GameManager.ForResult.JudgeNum.gd.ToString();
        JudgeBad.text = GameManager.ForResult.JudgeNum.bd.ToString();
        SongName.text = GameManager.ForResult.Song_name;
        if(SongSelect.songimage != null)
        {
            SongImage.sprite = Sprite.Create(SongSelect.songimage, new Rect(0, 0, SongSelect.songimage.width, SongSelect.songimage.height), Vector2.zero);
        }
        ArtistName.text = GameManager.ForResult.Artist_name;
        ChartName.text = GameManager.ForResult.Chart_name;
        ChartDesignerName.text = GameManager.ForResult.Designer_name;
        ChartLevel.text = "Lv" + GameManager.ForResult.Chart_level;
        int exScore = GameManager.ForResult.JudgeNum.pg * 2 + GameManager.ForResult.JudgeNum.gr;
        EXScore.text = exScore.ToString();
        notes_num = GameManager.ForResult.JudgeNum.pg + GameManager.ForResult.JudgeNum.gr + GameManager.ForResult.JudgeNum.gd + GameManager.ForResult.JudgeNum.bd;
        float score_float = ((GameManager.ForResult.JudgeNum.pg * 2f + GameManager.ForResult.JudgeNum.gr) / (notes_num * 2f) * 100f);
        Score.text = score_float.ToString("F2") + "%";
        gauge_ui.value = GameManager.ForResult.Gauge;
        MaxCombo.text = GameManager.ForResult.max_combo.ToString();
        Fast.text = GameManager.ForResult.fast_notes.ToString();
        Slow.text = GameManager.ForResult.slow_notes.ToString();
        HiSpeed.text = SongSelect.hispeed.ToString("F");
        NoteOption.text = SongSelect.OptionName[0, SongSelect.CurrentNoteOption];
        AssistOption.text = SongSelect.OptionName[1, SongSelect.CurrentAssistOption];
        if (GameManager.ForResult.ClearType == 0 && SongSelect.CurrentGauge >= 2) {
            LeftNotes.text = GameManager.ForResult.left_notes + " notes left";
        }
        else {
            LeftNotes.text = "";
        }

        // ハイスコアかどうかを確認
        string songName = SongSelect.songname;
        string chartName = SongSelect.chartname;
        string username = PlayerPrefs.GetString("username", "default");
        string localDataModStr = username + "__" + songName + "__" + chartName + "__";
        bool isHighestScore = false;
        bool isHighestExScore = false;
        bool isHighestClearType = false;
        float lastHighestScore = PlayerPrefs.GetFloat(localDataModStr + "score", 0f);
        int lastHighestExScore = PlayerPrefs.GetInt(localDataModStr + "exScore", 0);
        int lastHighestClearType = PlayerPrefs.GetInt(localDataModStr + "clearType", 0);
        isHighestScore = score_float > lastHighestScore;
        isHighestExScore = exScore > lastHighestExScore;
        isHighestClearType = GameManager.ForResult.ClearType > lastHighestClearType;

        // ローカルにハイスコアを保存
        if (SongSelect.CurrentAssistOption == 0 && SongSelect.CurrentGauge != 4)
        {
            Color highestColor;
            ColorUtility.TryParseHtmlString("#ff5758", out highestColor);
            if (isHighestExScore)
            {
                PlayerPrefs.SetInt(localDataModStr + "exScore", exScore);
                PlayerPrefs.Save();
                // EXスコアの文字を赤色にする
                EXScore.color = highestColor;
                Debug.Log("EXスコア更新！");
            }
            if (isHighestScore)
            {
                PlayerPrefs.SetFloat(localDataModStr + "score", score_float);
                PlayerPrefs.Save();
                // スコアの文字を赤色にする
                Score.color = highestColor;
                Debug.Log("スコア更新！");
            }
            if (isHighestClearType)
            {
                PlayerPrefs.SetInt(localDataModStr + "clearType", GameManager.ForResult.ClearType);
                PlayerPrefs.Save();
                // クリアゲージの文字を赤色にする
                ClearType.color = highestColor;
                Debug.Log("クリアゲージ更新！");
            }
        }

        // サーバーにリザルトをアップロード
        if (GameManager.ForResult.IsRegularChart && SongSelect.CurrentAssistOption == 0 && SongSelect.CurrentGauge != 4)
        {
            communicationManager = communicationManager = GameObject.Find("CommunicationManager").GetComponent<CommunicationManager>();
            StartCoroutine(communicationManager.ResultUpdateCoroutine(
                GameManager.ForResult.ChartId,
                score_float / 100f,
                exScore,
                GameManager.ForResult.Gauge / 100f,
                GameManager.ForResult.ClearType,
                GameManager.ForResult.JudgeNum.pg,
                GameManager.ForResult.JudgeNum.gr,
                GameManager.ForResult.JudgeNum.gd,
                GameManager.ForResult.JudgeNum.bd,
                GameManager.ForResult.max_combo
            ));
        }
    }
    void Update() {
        if (Input.GetKeyDown(SongSelect.DecideKey)) {
            MusicManager.audioSource.Stop();
            SceneManager.LoadScene("Scenes/SelectSong");
        }
    }
}
