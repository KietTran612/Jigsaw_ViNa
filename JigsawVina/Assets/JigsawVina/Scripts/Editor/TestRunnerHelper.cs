#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace JigsawVina.Editor
{
    public static class TestRunnerHelper
    {
        [MenuItem("JigsawVina/Run EditMode Tests")]
        public static string RunEditModeTests()
        {
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            var filter = new Filter { testMode = TestMode.EditMode };
            
            api.RegisterCallbacks(new TestCallback());
            api.Execute(new ExecutionSettings(filter));
            return "Tests started";
        }
        
        private class TestCallback : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun)
            {
                Debug.Log("[TestRunner] Run started");
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                Debug.Log($"[TestRunner] Run finished. Pass: {result.PassCount}, Fail: {result.FailCount}, Skip: {result.SkipCount}");
            }

            public void TestStarted(ITestAdaptor test) {}

            public void TestFinished(ITestResultAdaptor result)
            {
                if (result.ResultState == "Passed")
                {
                    Debug.Log($"[TestRunner] Test passed: {result.Name}");
                }
                else
                {
                    Debug.LogError($"[TestRunner] Test failed: {result.Name}\n{result.Message}\n{result.StackTrace}");
                }
            }
        }
    }
}
#endif
