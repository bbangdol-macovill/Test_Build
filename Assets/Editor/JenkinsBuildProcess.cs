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
            GenericBuild(FindEnabledEditorScenes(), $"./Build/IOS/", BuildTarget.iOS, opt);
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


            var apkName = pathToBuildProject.Split(new[] { '/' }).Last();// apk 파일 이름 얻기

            System.Console.WriteLine("OnAndroidBuildFinish!!" + apkName);

            //Google Drice 업로드 하는 부분. 되던게 갑자기 안되고 있어 다른걸로 대체해야 함.
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