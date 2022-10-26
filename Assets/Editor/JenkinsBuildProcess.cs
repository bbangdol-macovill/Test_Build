using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Diagnostics;
using UnityEngine.AddressableAssets.Initialization;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Callbacks;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BuildProcessor
{
    public class JenkinsBuildProcessor
    {
        public const string AddressableBuilderName = "Addressable Build";

        private class EditorCoroutine
        {
            private IEnumerator routine;

            private EditorCoroutine(IEnumerator routine)
            {
                this.routine = routine;
            }

            public static EditorCoroutine Start(IEnumerator routine)
            {
                EditorCoroutine coroutine = new EditorCoroutine(routine);
                coroutine.Start();
                return coroutine;
            }

            private void Start()
            {
                UnityEditor.EditorApplication.update += Update;
            }

            public void Stop()
            {
                UnityEditor.EditorApplication.update -= Update;
            }

            private void Update()
            {
                if (!routine.MoveNext())
                {
                    Stop();
                }
            }
        }

        [MenuItem("Tools/CI/Build iOS Debug")]
        public static void BuildIOS()
        {
            // 1. Build Number Automatically Up
            AddBuildVersion(BuildTarget.iOS);
            BuildOptions opt = BuildOptions.None;
            GenericBuild_iOS(FindEnabledEditorScenes(), $"./Build/IOS/", BuildTarget.iOS, opt);
        }

        [MenuItem("Tools/CI/Build And Debug")]
        public static void BuildAnd()
        {
            System.Console.WriteLine("Beginning BuildAnd");

            // 1. Build Number Automatically Up
            //var newBuildVersionCode = AddBuildVersion(BuildTarget.Android);

            //PlayerSettings.Android.useCustomKeystore = true;
            // PlayerSettings.Android.keystoreName = "AndroidKey.keystore";
            //PlayerSettings.Android.keystorePass = "**KeyStore Password**";
            //PlayerSettings.Android.keyaliasName = "**Alias name**";
            //PlayerSettings.Android.keyaliasPass = "**Alias password**";
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

            BuildOptions opt = BuildOptions.None;
            GenericBuild(FindEnabledEditorScenes(), $"./Build/Android/", BuildTarget.Android, opt);

            System.Console.WriteLine("End Of BuildAnd");
        }

        [MenuItem("Tools/CI/CMDTest")]
        public static void CMDTest()
        {
            System.Diagnostics.Process process = new();
            //System.Diagnostics.ProcessStartInfo processStartInfo = new();

            string resultValue = string.Empty;

            process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e) {
                UnityEngine.Debug.LogError(e.Data);
            };
            process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e) {
                UnityEngine.Debug.LogError(e.Data);
            };
            process.Exited += delegate (object sender, System.EventArgs e) {
                UnityEngine.Debug.LogError(e.ToString());
            };

            process.StartInfo.FileName = @"cmd";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardError = true;

            process.StartInfo.Arguments = "/c git add Assets/AddressableAssetsData/*";
            process.Start();
            process.WaitForExit();
            resultValue = process.StandardOutput.ReadToEnd();
            UnityEngine.Debug.Log("Add : " + resultValue);
            process.Close();

            string arguments = "git commit -m JenkinsAndroidBuild_develop_" + PlayerSettings.Android.bundleVersionCode.ToString();
            process.StartInfo.Arguments = "/c " + arguments;
            process.Start();
            process.WaitForExit();
            resultValue = process.StandardOutput.ReadToEnd();
            UnityEngine.Debug.Log("Commit : " + resultValue);
            process.Close();

            process.StartInfo.Arguments = "/c git push origin develop";
            process.Start();
            process.WaitForExit();
            resultValue = process.StandardOutput.ReadToEnd();
            UnityEngine.Debug.Log("Push : " + resultValue);
            process.Close();
        }

        [MenuItem("Tools/CI/Shell Test")]
        public static void CMDShellTest()
        {
            System.Diagnostics.Process process = new();

            string resultValue = string.Empty;

            process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e) {
                UnityEngine.Debug.LogError(e.Data);
            };
            process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e) {
                UnityEngine.Debug.LogError(e.Data);
            };
            process.Exited += delegate (object sender, System.EventArgs e) {
                UnityEngine.Debug.LogError(e.ToString());
            };

            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardError = true;

            process.StartInfo.Arguments = "-c" + " \"" + "git add Assets/AddressableAssetsData/*" + " \"";
            process.Start();
            process.WaitForExit();
            resultValue = process.StandardOutput.ReadToEnd();
            UnityEngine.Debug.Log("Add : " + resultValue);
            process.Close();

            process.StartInfo.Arguments = "-c" + " \"" + "git add ProjectSettings/ProjectSettings.asset" + " \"";
            process.Start();
            process.WaitForExit();
            resultValue = process.StandardOutput.ReadToEnd();
            UnityEngine.Debug.Log("Add : " + resultValue);
            process.Close();

            string arguments = "-c" + " \"" + "git commit -m JenkinsiOSBuild_develop_" + PlayerSettings.iOS.buildNumber + " \"";
            process.StartInfo.Arguments = arguments;
            process.Start();
            process.WaitForExit();
            resultValue = process.StandardOutput.ReadToEnd();
            UnityEngine.Debug.Log("Commit : " + resultValue);
            process.Close();

            process.StartInfo.Arguments = "-c" + " \"" + "git push origin develop" + " \"";
            process.Start();
            process.WaitForExit();
            resultValue = process.StandardOutput.ReadToEnd();
            UnityEngine.Debug.Log("Push : " + resultValue);
            process.Close();
        }
        [MenuItem("Tools/CI/Python Test")]
        public static void CMDPythonTest()
        {
            System.Diagnostics.Process process = new();

            string reopenTerminal = $"tell application \\\"Terminal\\\" to if not (exists window 1) then reopen";
            string activateTerminal = $"tell application \\\"Terminal\\\" to activate";
            string toDoScript1 = $"tell application \\\"Terminal\\\" to do script \\\"cd ~/Projects/Test_Project/Tools/PythonScripts\\\" in window 1";
            string toDoScript2 = $"tell application \\\"Terminal\\\" to do script \\\"python3 -u UploadToGoogleDrive_iOS.py\\\" in window 1";
            //string osaScript = $"osascript -e \'{reopenTerminal}\' -e \'{activateTerminal}\' -e \'{toDoScript}\' -e \'{endTell}\'";
            string osaScript = $"osascript -e \'{reopenTerminal}\' -e \'{activateTerminal}\' -e \'{toDoScript1}\' -e \'{toDoScript2}\'";
            string argument = $" -c \"{osaScript}\"";

            System.Diagnostics.ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                FileName = "/bin/bash",
                CreateNoWindow = false,
                Arguments = argument,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true

            };
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
        }

        private static string AddBuildVersion(BuildTarget buildTarget)
        {
            CheckBuildTarget(buildTarget);

            string oldBuildNumber;

            switch (buildTarget)
            {
                case BuildTarget.Android:
                    oldBuildNumber = PlayerSettings.Android.bundleVersionCode.ToString();
                    PlayerSettings.Android.bundleVersionCode = (int.Parse(oldBuildNumber) + 1);
                    return PlayerSettings.Android.bundleVersionCode.ToString();
                case BuildTarget.iOS:
                    oldBuildNumber = PlayerSettings.iOS.buildNumber;
                    PlayerSettings.iOS.buildNumber = (int.Parse(oldBuildNumber) + 1).ToString();
                    AssetDatabase.SaveAssets();
                    return PlayerSettings.iOS.buildNumber;
            }

            return "null";
        }

        private static void CheckBuildTarget(BuildTarget buildTarget)
        {
            if (buildTarget == BuildTarget.Android || buildTarget == BuildTarget.iOS)
                return;

            throw new Exception($"Is not supported Platform, {buildTarget}");
        }

        private static void GenericBuild(string[] scenes, string targetPath, BuildTarget buildTarget, BuildOptions buildOptions)
        {
            //copy all table files 
            string path = System.Environment.CurrentDirectory + "../data/" + "CopyAll.bat";
            UnityEngine.Debug.Log(path);
            UnityEngine.Debug.Log(System.Environment.CurrentDirectory);

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.CreateNoWindow = true;
            //startInfo.WorkingDirectory = @"C:\";
            startInfo.Arguments = "/c cd .. & cd data & CopyAll.bat";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

            // addressable build
            for (int i = 0; i < AddressableAssetSettingsDefaultObject.Settings.DataBuilders.Count; i++)
            {
                var m = AddressableAssetSettingsDefaultObject.Settings.GetDataBuilder(i);
                if (m.Name == AddressableBuilderName)
                {
                    AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilderIndex = i;
                    break;
                }
            }

            AddressableAssetSettings.BuildPlayerContent();


            // build
            if (File.Exists(targetPath) == false)
                Directory.CreateDirectory(targetPath);

            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);


            string timestring = string.Format("{0}_{1}", DateTime.Now.ToString("yyyy") + DateTime.Now.ToString("MM") + DateTime.Now.ToString("dd"), DateTime.Now.ToString("HH") + DateTime.Now.ToString("mm") + DateTime.Now.ToString("ss"));
            string target = targetPath + string.Format("/{0}_{1}_{2}.apk", PlayerSettings.productName, PlayerSettings.bundleVersion, timestring);
            BuildPipeline.BuildPlayer(scenes, target, buildTarget, buildOptions);
        }
        private static void GenericBuild_iOS(string[] scenes, string targetPath, BuildTarget buildTarget, BuildOptions buildOptions)
        {
            // addressable build
            for (int i = 0; i < AddressableAssetSettingsDefaultObject.Settings.DataBuilders.Count; i++)
            {
                var m = AddressableAssetSettingsDefaultObject.Settings.GetDataBuilder(i);
                if (m.Name == AddressableBuilderName)
                {
                    AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilderIndex = i;
                    break;
                }
            }

            AddressableAssetSettings.BuildPlayerContent();

            // build
            if (File.Exists(targetPath) == false)
                Directory.CreateDirectory(targetPath);

            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);

            string timestring = string.Format("{0}_{1}", DateTime.Now.ToString("yyyy") + DateTime.Now.ToString("MM") + DateTime.Now.ToString("dd"), DateTime.Now.ToString("HH") + DateTime.Now.ToString("mm") + DateTime.Now.ToString("ss"));
            string target = targetPath + string.Format("/{0}_{1}_{2}.apk", PlayerSettings.productName, PlayerSettings.bundleVersion, timestring);
            //BuildPipeline.BuildPlayer(scenes, target, buildTarget, buildOptions);

            System.Diagnostics.Process process = new();
            System.Diagnostics.ProcessStartInfo processStartInfo = new();

            string reopenTerminal = $"tell application \\\"Terminal\\\" to if not (exists window 1) then reopen";
            string activateTerminal = $"tell application \\\"Terminal\\\" to activate";
            string changeDirectory = $"tell application \\\"Terminal\\\" to do script \\\"cd ~/Projects/Test_Project/Tools/PythonScripts\\\" in window 1";
            string runPhython = $"tell application \\\"Terminal\\\" to do script \\\"python3 -u UploadToGoogleDrive_iOS.py\\\" in window 1";
            string osaScript = $"osascript -e \'{reopenTerminal}\' -e \'{activateTerminal}\' -e \'{changeDirectory}\' -e \'{runPhython}\'";
            string argument = $" -c \"{osaScript}\"";

            processStartInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                FileName = "/bin/bash",
                CreateNoWindow = false,
                Arguments = argument,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true

            };
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
        }

        private static string[] FindEnabledEditorScenes()
        {
            List<string> EditorScenes = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled)
                    continue;

                EditorScenes.Add(scene.path);
            }

            return EditorScenes.ToArray();
        }

        [PostProcessBuild(100)]
        public static void OnAndroidBuildFinish(BuildTarget target, string pathToBuildProject)
        {
            System.Console.WriteLine("OnAndroidBuildFinish!!");
            //if (target == BuildTarget.Android)
            //{
            //    EditorCoroutine.Start(UploadApkAsync(pathToBuildProject));
            //}
        }

        private static IEnumerator UploadApkAsync(string pathToBuildProject)
        {
            //-CustomArgs:Language=en_US;Version=1.02
            string parentId = CommandLineReader.GetCustomArgument("ParentId");
            if (string.IsNullOrEmpty(parentId))
            {
                UnityEngine.Debug.Log("Stop UploadApkAsync :" + parentId);
                yield break;
            }


            var apkName = pathToBuildProject.Split(new[] { '/' }).Last();// apk ???? ???? ????

            System.Console.WriteLine("OnAndroidBuildFinish!!" + apkName);

            //Google Drice ?????? ???? ????. ?????? ?????? ?????? ???? ???????? ???????? ??.
            //if (File.Exists(pathToBuildProject))
            //{
            //    var apk = new UnityGoogleDrive.Data.File
            //    { Name = apkName, Content = File.ReadAllBytes(pathToBuildProject), Parents = new List<string> { parentId, } };
            //    var req = UnityGoogleDrive.GoogleDriveFiles.Create(apk);
            //    req.OnDone += response =>
            //    {
            //        System.Console.WriteLine("UploadApkAsync Req Done!!" + req.IsError + "!!" + req.IsDone);

            //        if (req.IsError) UnityEngine.Debug.LogError(req.Error);
            //        if (req.IsDone)
            //        {
            //            UnityEngine.Debug.Log("Upload Success!! " + apkName);
            //            UnityEngine.Debug.Log(response.WebViewLink);
            //            if (!string.IsNullOrEmpty(response.WebViewLink))
            //                Application.OpenURL(req.ResponseData.WebViewLink);
            //        }
            //    };

            //    System.Console.WriteLine("UploadApkAsync Send!!" + apkName);

            //    yield return req.Send();//upload

            //    while (!req.IsDone)
            //        yield return 0;

            //    System.Console.WriteLine("UploadApkAsync End Of Upload Process!!");
            //}
            //else
            //{
            //    System.Console.WriteLine("Error UploadApk " + pathToBuildProject);

            //    UnityEngine.Debug.Log($"Built File Path:{pathToBuildProject} File Not Exists");
            //}

            EditorApplication.Exit(0);
        }
    }
}