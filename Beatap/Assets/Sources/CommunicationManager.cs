using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.Networking;
using UniRx.Async;

public class CommunicationManager: MonoBehaviour
{
    private readonly string SERVER_PROTCOL = "https://";
    private readonly string SERVER_URL = "beatap.herokuapp.com";
    public readonly string MUSIC_FOLDER_MOD_STR = "BeatapMusic_";
    public readonly string CHART_FILE_MOD_STR = "BeatapChart_";

    private NotifyManager notifyManager;

    private void Start()
    {
        notifyManager = GameObject.Find("NotifyManager").GetComponent<NotifyManager>();
    }

    private async UniTask<UnityWebRequest> PostAsync(string url, WWWForm postData)
    {
        UnityWebRequest request = UnityWebRequest.Post(url, postData);
        await request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError || request.responseCode / 100 != 2) //response codeが200系でなかったら弾く
        {
            Debug.Log("COMMUNICATION ERROR!");
            Debug.Log("response code:" + request.responseCode);
            Debug.Log("body:" + request.downloadHandler.text);

            return null;
        }
        return request;
    }

    private async UniTask<UnityWebRequest> GetAsync(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        await request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError || request.responseCode / 100 != 2) //response codeが200系でなかったら弾く
        {
            Debug.Log("COMMUNICATION ERROR!");
            Debug.Log("response code:" + request.responseCode);
            Debug.Log("body:" + request.downloadHandler.text);

            return null;
        }

        return request;
    }

    private async UniTask<UnityWebRequest> LoginAsync(string username, string password)
    {
        string url = SERVER_PROTCOL + SERVER_URL + "/api/login/";

        WWWForm postData = new WWWForm();
        postData.AddField("username", username);
        postData.AddField("password", password);

        return await PostAsync(url, postData);
    }

    private async UniTask<UnityWebRequest> LogoutAsync()
    {
        string url = SERVER_PROTCOL + SERVER_URL + "/api/logout/";

        return await GetAsync(url);
    }

    private async UniTask<UnityWebRequest> ResultUpdateAsync(string chartId, float score, int exScore, float gauge, int clearStatusNum, int pg, int gr, int gd, int bd, int maxCombo)
    {
        string url = SERVER_PROTCOL + SERVER_URL + "/api/result/update/";

        WWWForm postData = new WWWForm();
        postData.AddField("chart_id", chartId);
        postData.AddField("score", score.ToString());
        postData.AddField("ex_score", exScore.ToString());
        postData.AddField("gauge", gauge.ToString());
        postData.AddField("clear_status_num", clearStatusNum.ToString());
        postData.AddField("pg", pg.ToString());
        postData.AddField("gr", gr.ToString());
        postData.AddField("gd", gd.ToString());
        postData.AddField("bd", bd.ToString());
        postData.AddField("max_combo", maxCombo.ToString());

        return await PostAsync(url, postData);
    }

    private async UniTask<UnityWebRequest> MusicDownloadAsync(string musicId)
    {
        string url = SERVER_PROTCOL + SERVER_URL + "/api/music/download/";

        url += musicId + "/";

        return await GetAsync(url);
    }

    private async UniTask<UnityWebRequest> ChartDownloadAsync(string chartId)
    {
        string url = SERVER_PROTCOL + SERVER_URL + "/api/chart/download/";

        url += chartId + "/";

        return await GetAsync(url);
    }

    public IEnumerator LoginCoroutine(string username, string password)
    {
        UniTask<UnityWebRequest> loginTask = LoginAsync(username, password);

        yield return new WaitUntil(() => loginTask.IsCompleted);

        UnityWebRequest request = loginTask.Result;
        if (request == null)
        {
            Debug.Log("ログインに失敗しました");
            notifyManager.Notify("ログインに失敗しました");
        }
        else
        {
            PlayerPrefs.SetString("username", username);
            PlayerPrefs.Save();

            Debug.Log("ログインに成功しました");
            notifyManager.Notify("ログインに成功しました");
        }
    }

    public IEnumerator LogoutCoroutine()
    {
        UniTask<UnityWebRequest> logoutTask = LogoutAsync();

        yield return new WaitUntil(() => logoutTask.IsCompleted);

        UnityWebRequest request = logoutTask.Result;
        if (request == null)
        {
            Debug.Log("ログアウトに失敗しました");
            notifyManager.Notify("ログアウトに失敗しました");
        }
        else
        {
            PlayerPrefs.SetString("username", null);
            PlayerPrefs.Save();

            Debug.Log("ログアウトに成功しました");
            notifyManager.Notify("ログアウトに成功しました");
        }
    }

    public IEnumerator ResultUpdateCoroutine(string chartId, float score, int exScore, float gauge, int clearStatusNum, int pg, int gr, int gd, int bd, int maxCombo)
    {
        UniTask<UnityWebRequest> resultUpdateTask = ResultUpdateAsync(chartId, score, exScore, gauge, clearStatusNum, pg, gr, gd, bd, maxCombo);

        yield return new WaitUntil(() => resultUpdateTask.IsCompleted);

        UnityWebRequest request = resultUpdateTask.Result;
        if (request == null)
        {
            Debug.Log("スコア更新に失敗しました");
            notifyManager.Notify("スコア更新に失敗しました");
        }
        else
        {

            Debug.Log("スコア更新に成功しました");
            notifyManager.Notify("スコア更新に成功しました");
        }
    }

    public IEnumerator MusicDownloadCoroutine(string musicId)
    {
        UniTask<UnityWebRequest> musicDownloadTask = MusicDownloadAsync(musicId);

        yield return new WaitUntil(() => musicDownloadTask.IsCompleted);

        UnityWebRequest request = musicDownloadTask.Result;
        if (request == null)
        {
            Debug.Log("曲のダウンロードに失敗しました");
            notifyManager.Notify("曲のダウンロードに失敗しました");
        }
        else
        {
            string resourcesPath = Application.dataPath + @"\Resources";
            string musicFolderPath = resourcesPath + $@"\Songs\{MUSIC_FOLDER_MOD_STR + musicId}";

            // zipファイルをダウンロード
            MemoryStream memoryStream = new MemoryStream(request.downloadHandler.data);
            ZipArchive zipArchive = new ZipArchive(memoryStream);

            // zipファイルを解凍
            Directory.CreateDirectory(musicFolderPath);
            foreach (ZipArchiveEntry entry in zipArchive.Entries)
            {
                string entryPath = musicFolderPath + $@"\{entry.FullName}";

                // すでにダウンロード済みだったら更新する
                entry.ExtractToFile(entryPath, true);
            }

            Debug.Log("曲のダウンロードに成功しました");
            notifyManager.Notify("曲のダウンロードに成功しました");
        }
    }

    public IEnumerator ChartDownloadCoroutine(string musicId, string chartId)
    {
        UniTask<UnityWebRequest> chartDownloadTask = ChartDownloadAsync(chartId);

        yield return new WaitUntil(() => chartDownloadTask.IsCompleted);

        UnityWebRequest request = chartDownloadTask.Result;
        if (request == null)
        {
            Debug.Log("譜面のダウンロードに失敗しました");
            notifyManager.Notify("譜面のダウンロードに失敗しました");
        }
        else
        {
            string resourcesPath = Application.dataPath + @"\Resources";
            string musicFolderPath = resourcesPath + $@"\Songs\{MUSIC_FOLDER_MOD_STR + musicId}";
            string chartFilePath = musicFolderPath + $@"\{CHART_FILE_MOD_STR + chartId}.csv";

            // 曲をまだダウンロードしてなかったらダウンロードする
            if (!Directory.Exists(musicFolderPath))
            {
                UniTask<UnityWebRequest> musicDownloadTask = MusicDownloadAsync(musicId);

                yield return new WaitUntil(() => musicDownloadTask.IsCompleted);

                UnityWebRequest musicRequest = musicDownloadTask.Result;
                if (request == null)
                {
                    Debug.Log("曲のダウンロードに失敗しました");
                    notifyManager.Notify("曲のダウンロードに失敗しました");
                }
                else
                {

                    // zipファイルをダウンロード
                    MemoryStream memoryStream = new MemoryStream(musicRequest.downloadHandler.data);
                    ZipArchive zipArchive = new ZipArchive(memoryStream);

                    // zipファイルを解凍
                    Directory.CreateDirectory(musicFolderPath);
                    foreach (ZipArchiveEntry entry in zipArchive.Entries)
                    {
                        string entryPath = musicFolderPath + $@"\{entry.FullName}";

                        // すでにダウンロード済みだったら更新する
                        entry.ExtractToFile(entryPath, true);
                    }

                    Debug.Log("曲のダウンロードに成功しました");
                    notifyManager.Notify("曲のダウンロードに成功しました");
                }
            }

            // csvをダウンロード
            File.WriteAllText(chartFilePath, request.downloadHandler.text);

            Debug.Log("譜面のダウンロードに成功しました");
            notifyManager.Notify("譜面のダウンロードに成功しました");
        }
    }
}
