using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 테스트용. gitIgnore 추가.
/// </summary>
public class Test_Script : Singleton<Test_Script>
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            LogColor("Test1");

            System.Diagnostics.Process process = new();
            System.Diagnostics.ProcessStartInfo processStartInfo = new();

            processStartInfo.FileName = @"cmd";
            processStartInfo.CreateNoWindow = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.Arguments = "/c cd tools & Test1.bat";

            process.StartInfo = processStartInfo;
            process.Start();

            string resultValue = process.StandardOutput.ReadToEnd();            

            process.WaitForExit(1000 * 60 * 5);
            Thread.Sleep(6000);
            //process.Close();

            UnityEngine.Debug.Log(resultValue);

            Test_CMD2(process, processStartInfo);            
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            Test_Bat(new System.Diagnostics.Process(), new System.Diagnostics.ProcessStartInfo());
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

        //cmd에 보낼 정보.
        process.StandardInput.Write(@"dir/w" + System.Environment.NewLine);
        process.StandardInput.Close();

        //cmd에서 읽어올 정보.
        string resultValue = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        process.Close();

        UnityEngine.Debug.Log(resultValue);
    }
    #endregion

    #region Batch Command.
    private static void Test_Bat(System.Diagnostics.Process process, System.Diagnostics.ProcessStartInfo processStartInfo)
    {
        UnityEngine.Debug.Log("Start Test_Bat");

        processStartInfo.FileName = @"cmd";
        processStartInfo.CreateNoWindow = true;
        processStartInfo.UseShellExecute = false;
        processStartInfo.RedirectStandardOutput = true;
        processStartInfo.RedirectStandardInput = true;
        processStartInfo.RedirectStandardError = true;
        processStartInfo.Arguments = "/c cd ../tools & Test1.bat";

        process.StartInfo = processStartInfo;

        process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e) {
            UnityEngine.Debug.LogError(e.Data);
        };
        process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e) {
            UnityEngine.Debug.LogError(e.Data);
        };
        process.Exited += delegate (object sender, System.EventArgs e) {
            UnityEngine.Debug.LogError(e.ToString());
        };
        process.Start();

        process.WaitForExit();
        process.Close();

        UnityEngine.Debug.Log("Finish Test_Bat");
    }
    #endregion

    #region CMD.
    private static void Test_CMD(System.Diagnostics.Process process, System.Diagnostics.ProcessStartInfo processStartInfo)
    {
        LogColor("Test");

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
    private static void Test_CMD1(System.Diagnostics.Process process, System.Diagnostics.ProcessStartInfo processStartInfo)
    {
        LogColor("Test1");

        processStartInfo.FileName = @"cmd";
        processStartInfo.CreateNoWindow = true;
        processStartInfo.UseShellExecute = false;
        processStartInfo.RedirectStandardOutput = true;
        processStartInfo.RedirectStandardInput = true;
        processStartInfo.RedirectStandardError = true;
        processStartInfo.Arguments = "/c cd tools & Test.bat";

        process.StartInfo = processStartInfo;
        process.Start();

        string resultValue = process.StandardOutput.ReadToEnd();

        process.WaitForExit();
        process.Close();

        UnityEngine.Debug.Log(resultValue);
    }
    private static void Test_CMD2(System.Diagnostics.Process process, System.Diagnostics.ProcessStartInfo processStartInfo)
    {
        LogColor("Test2");

        processStartInfo.FileName = @"cmd";
        processStartInfo.CreateNoWindow = true;
        processStartInfo.UseShellExecute = false;
        processStartInfo.RedirectStandardOutput = true;
        processStartInfo.RedirectStandardInput = true;
        processStartInfo.RedirectStandardError = true;
        processStartInfo.Arguments = "/c cd tools & Test2.bat";

        process.StartInfo = processStartInfo;
        process.Start();

        //process.StandardInput.WriteLine("1");
        //process.StandardInput.WriteLine("2");
        //process.StandardInput.WriteLine("3");
        //process.StandardInput.WriteLine("4");
        //process.StandardInput.WriteLine("5");
        //process.StandardInput.Close();

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