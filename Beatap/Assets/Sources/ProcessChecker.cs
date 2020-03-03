using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class ProcessChecker : MonoBehaviour
{
    private CommunicationManager communicationManager;

    private Process Duplicated()
    {
        Process currentProcess = Process.GetCurrentProcess();
        Process[] processes = Process.GetProcessesByName(
            currentProcess.ProcessName
        );
        if (processes.Length > 1)
        {
            foreach (Process process in processes)
            {
                if (process.Id != currentProcess.Id) return process;
            }
        }
        return null;
    }

    private void CalledFromUrl(string[] args)
    {
        if(args.Length >= 1)
        {
            string mode = args[1];
            switch (mode)
            {
                case "download":
                {
                    string download_type = args[2];
                    switch (download_type)
                    {
                        case "music":
                        {
                            string musicId = args[3].Replace("\r", "").Replace("\n", "");
                            StartCoroutine(communicationManager.MusicDownloadCoroutine(musicId));
                            break;
                        }
                        case "chart":
                        {
                            string musicId = args[3].Replace("\r", "").Replace("\n", "");
                            string chartId = args[4].Replace("\r", "").Replace("\n", "");
                            StartCoroutine(communicationManager.ChartDownloadCoroutine(musicId, chartId));
                            break;
                        }
                    }
                    break;
                }
            }
        }
    }

    private string ExecuteCommand(string command, string args)
    {
        Process process = new Process();
        process.StartInfo.FileName = command;
        process.StartInfo.Arguments = args;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;

        process.Start();

        string result = process.StandardOutput.ReadToEnd();
        result.Replace("\r\n", "\n");

        return result;
    }

    void Start()
    {
        communicationManager = GameObject.Find("CommunicationManager").GetComponent<CommunicationManager>();
    }

    void Update()
    {
        Process otherProcess = Duplicated();
        if (otherProcess != null)
        {
            string GET_ARGS_COMMAND = "wmic";
            string GET_ARGS_COMMAND_ARGS = $"process where \"processid = {otherProcess.Id}\" get commandline /format:value";
            string result = ExecuteCommand(GET_ARGS_COMMAND, GET_ARGS_COMMAND_ARGS);
            string argsStr = result.Split('=')[1];
            string[] args = argsStr.Split(' ');
            otherProcess.Kill();

            CalledFromUrl(args);
        }
    }
}
