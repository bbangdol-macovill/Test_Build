using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

/// <summary>
/// ????????. gitIgnore ????.
/// </summary>
public class Test_Script : Singleton<Test_Script>
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            string url = serverURL + "dev/" + Test_GetPlatformString() + "/";

            LogColor(url);
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            Test_CMD(new System.Diagnostics.Process(), new System.Diagnostics.ProcessStartInfo());

            //string path = System.Environment.CurrentDirectory + "/tool" + "DeleteAssetBundle.bat";
            //UnityEngine.Debug.Log("DeleteAssetBundle : " + path);
            //UnityEngine.Debug.Log("DeleteAssetBundle : " + System.Environment.CurrentDirectory);
            //System.Diagnostics.Process process = new System.Diagnostics.Process();
            //System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            //startInfo.FileName = "cmd.exe";
            //startInfo.CreateNoWindow = true;
            //startInfo.Arguments = "/c cd tool & DeleteAssetBundle.bat";
            //process.StartInfo = startInfo;
            //process.Start();
            //process.WaitForExit();
        }
    }

    #region Path.
    private string serverURL = "https://oz-patch.s3.ap-northeast-2.amazonaws.com/";

    private string Test_GetPlatformString()
    {
#if UNITY_ANDROID
        return "Android";
#elif UNITY_IOS
        return "IOS";
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        return "StandaloneWindows";
#endif
        return string.Empty;
    }
    #endregion

    #region git Batch.
    private void TestGitBatch()
    {
        System.Diagnostics.Process process = new();
        System.Diagnostics.ProcessStartInfo startInfo = new()
        {
            FileName = @"cmd",
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardInput = true,
            RedirectStandardError = true,
            Arguments = "/c cd ../tools & Test.bat"
        };

        process.StartInfo = startInfo;
        process.Start();

        //cmd?? ???? ????.
        process.StandardInput.Write(@"dir/w" + System.Environment.NewLine);
        process.StandardInput.Close();

        //cmd???? ?????? ????.
        string resultValue = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        process.Close();

        UnityEngine.Debug.Log(resultValue);
    }
    #endregion

    #region Batch Command.
    private static void Test_Bat(System.Diagnostics.Process process, System.Diagnostics.ProcessStartInfo processStartInfo)
    {
        processStartInfo.FileName = @"cmd";
        processStartInfo.CreateNoWindow = true;
        processStartInfo.UseShellExecute = false;
        processStartInfo.RedirectStandardOutput = true;
        processStartInfo.RedirectStandardInput = true;
        processStartInfo.RedirectStandardError = true;
        processStartInfo.Arguments = "/c cd ../tools & Test.bat";

        process.StartInfo = processStartInfo;
        process.Start();

        process.WaitForExit();
        process.Close();
    }
    #endregion

    #region CMD.
    private static void Test_CMD(System.Diagnostics.Process process, System.Diagnostics.ProcessStartInfo processStartInfo)
    {
        processStartInfo.FileName = @"cmd";
        processStartInfo.CreateNoWindow = false;
        processStartInfo.UseShellExecute = false;
        processStartInfo.RedirectStandardOutput = true;
        processStartInfo.RedirectStandardInput = true;
        processStartInfo.RedirectStandardError = true;

        process.StartInfo = processStartInfo;
        process.Start();

        process.StandardInput.WriteLine("aws s3 rm s3://oz-patch/dev/Android/" + 10 + "--recursive --exclude=\"*\" --include=\"" + 10 + "/*.*\"");
        process.StandardInput.WriteLine("s3 rm s3://oz-patch/dev --recursive --exclude=\"*\" --include=\"Android/*.*\"");
        process.StandardInput.Close();

        string resultValue = process.StandardOutput.ReadToEnd();

        process.WaitForExit();
        process.Close();

        UnityEngine.Debug.Log(resultValue);
    }
    #endregion

    #region Log.
    public enum LogColorType
    {
        None,
        Red,
        Cyan,
        Magenta,
        White
    }
    public static void LogColor(LogColorType logColorType, string logString)
    {
        switch (logColorType)
        {
            case LogColorType.None:
                UnityEngine.Debug.Log(logString);
                break;
            case LogColorType.Red:
                UnityEngine.Debug.Log("<color=red>" + logString + "</color>");
                break;
            case LogColorType.Cyan:
                UnityEngine.Debug.Log("<color=cyan>" + logString + "</color>");
                break;
            case LogColorType.Magenta:
                UnityEngine.Debug.Log("<color=magenta>" + logString + "</color>");
                break;
            case LogColorType.White:
                UnityEngine.Debug.Log("<color=white>" + logString + "</color>");
                break;
            default:
                break;
        }
    }
    public static void LogColor(string logString)
    {
        UnityEngine.Debug.Log("<color=lightblue>" + logString + "</color>");
    }
    #endregion
}