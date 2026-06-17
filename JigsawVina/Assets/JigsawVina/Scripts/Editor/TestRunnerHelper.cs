#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using System.IO;

namespace JigsawVina.Editor
{
    [InitializeOnLoad]
    public static class TestRunnerHelper
    {
        private static readonly TestCallback _callback = new TestCallback();
        private static readonly System.Text.StringBuilder _logSb = new System.Text.StringBuilder();
        private static TestRunnerApi _callbackApi;
        private static TestRunnerApi _executionApi;

        static TestRunnerHelper()
        {
            _callbackApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            _callbackApi.RegisterCallbacks(_callback);
            Application.logMessageReceived += HandleLog;
        }

        private static void HandleLog(string logString, string stackTrace, LogType type)
        {
            _logSb.AppendLine($"[{type}] {logString}");
            if (type == LogType.Error || type == LogType.Exception)
            {
                _logSb.AppendLine(stackTrace);
            }
        }

        [MenuItem("JigsawVina/Run EditMode Tests")]
        public static void RunEditModeTests()
        {
            RunTests(TestMode.EditMode);
        }

        public static void RunTask37Tests()
        {
            RunTests(TestMode.EditMode, new[]
            {
                "JigsawVina.Tests.ProgressionTests",
                "JigsawVina.Tests.StaticDataServiceTests"
            });
        }

        public static void RunTask38Tests()
        {
            RunTests(TestMode.EditMode, new[]
            {
                "JigsawVina.Tests.PictureSelectFlowTests"
            });
        }

        public static void RunTask39Tests()
        {
            RunTests(TestMode.EditMode, new[]
            {
                "JigsawVina.Tests.DifficultySelectFlowTests"
            });
        }

        public static void RunTask40Tests()
        {
            RunTests(TestMode.EditMode, new[]
            {
                "JigsawVina.Tests.LifetimeScopeRegistrationTests"
            });
        }

        public static void RunTask43Tests()
        {
            RunTests(TestMode.EditMode, new[]
            {
                "JigsawVina.Tests.DropRewardTests"
            });
        }

        public static void RunTask44Tests()
        {
            RunTests(TestMode.EditMode, new[]
            {
                "JigsawVina.Tests.DropRewardTests",
                "JigsawVina.Tests.ProgressionTests"
            });
        }

        public static void RunTask45Tests()
        {
            RunTests(TestMode.EditMode, new[]
            {
                "JigsawVina.Tests.CollectionFlowTests",
                "JigsawVina.Tests.PictureSelectFlowTests",
                "JigsawVina.Tests.LifetimeScopeRegistrationTests"
            });
        }

        public static void RunTask47Tests()
        {
            RunTests(TestMode.EditMode, new[]
            {
                "JigsawVina.Tests.DailyRewardTests"
            });
        }

        public static void RunTask49Tests()
        {
            RunTests(TestMode.EditMode, new[]
            {
                "JigsawVina.Tests.SaveDataServiceTests"
            });
        }

        public static void RunTask50Tests()
        {
            RunTests(TestMode.EditMode, new[]
            {
                "JigsawVina.Tests.SaveDataServiceTests",
                "JigsawVina.Tests.LocalizationServiceTests",
                "JigsawVina.Tests.GameplayPauseTests",
                "JigsawVina.Tests.GameplayFlowTests"
            });
        }

        [MenuItem("JigsawVina/Run PlayMode Tests")]
        public static void RunPlayModeTests()
        {
            RunTests(TestMode.PlayMode);
        }

        private static void RunTests(TestMode mode, string[] testNames = null)
        {
            _logSb.Clear();
            _logSb.AppendLine($"=== {mode} Test Run Logs ===");

            if (_executionApi != null)
            {
                Object.DestroyImmediate(_executionApi);
            }

            _executionApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            string assemblyName = mode == TestMode.EditMode
                ? "JigsawVina.Tests"
                : "JigsawVina.PlayModeTests";
            Debug.Log($"[TestRunner] Starting {mode} tests from {assemblyName}");
            _executionApi.Execute(new ExecutionSettings(new Filter
            {
                testMode = mode,
                assemblyNames = new[] { assemblyName },
                testNames = testNames
            }));
        }
        
        private class TestCallback : ICallbacks
        {
            private System.Text.StringBuilder sb = new System.Text.StringBuilder();

            public void RunStarted(ITestAdaptor testsToRun)
            {
                sb.Clear();
                sb.AppendLine("[TestRunner] Run started");
                Debug.Log("[TestRunner] Run started");
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                sb.AppendLine($"[TestRunner] Run finished. Pass: {result.PassCount}, Fail: {result.FailCount}, Skip: {result.SkipCount}");
                Debug.Log($"[TestRunner] Run finished. Pass: {result.PassCount}, Fail: {result.FailCount}, Skip: {result.SkipCount}");
                try
                {
                    string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "TestResults"));
                    Directory.CreateDirectory(directory);
                    string path = Path.Combine(directory, "latest-test-results.txt");
                    File.WriteAllText(path, sb.ToString() + "\n\n" + _logSb.ToString());
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Failed to write test results file: {ex.Message}");
                }
            }

            public void TestStarted(ITestAdaptor test) {}

            public void TestFinished(ITestResultAdaptor result)
            {
                if (result.ResultState == "Passed" || result.ResultState == "Success")
                {
                    sb.AppendLine($"[TestRunner] Test passed: {result.Name}");
                    Debug.Log($"[TestRunner] Test passed: {result.Name}");
                }
                else
                {
                    sb.AppendLine($"[TestRunner] Test failed: {result.Name}\n{result.Message}\n{result.StackTrace}");
                    Debug.LogError($"[TestRunner] Test failed: {result.Name}\n{result.Message}\n{result.StackTrace}");
                }
            }
        }
    }
}
#endif
