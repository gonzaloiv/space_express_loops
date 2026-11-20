using System;
using System.Text;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.EditorTools
{
    // The checks a rebaked job must pass: the sun-answer tests and the whole job set against its
    // contracts (an [Explicit] test, so it only runs when named). Runs them through the Test Runner and
    // keeps a one-line summary for the Job Baker.
    public sealed class PuzzleJobChecks : ICallbacks
    {
        private const string Tests = "DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode.";
        private const string SunAnswerClass = "^" + "DigitalLove\\.Game\\.SpaceShaping\\.Creator\\.Tests\\.EditMode\\.PuzzleSunAnswerTests\\.";
        private const string JobSetTest = Tests + "PuzzleJobBakeTests.JobSet_MeetsTheContract";

        private readonly StringBuilder failures = new();
        private TestRunnerApi api;
        private int passed;
        private int failed;

        public event Action Changed;

        public bool Running { get; private set; }

        public string Summary { get; private set; } = string.Empty;

        public void Run()
        {
            if (Running)
                return;

            Running = true;
            passed = 0;
            failed = 0;
            failures.Clear();
            Summary = "Running sun-answer and job-set checks…";
            api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.hideFlags = HideFlags.HideAndDontSave;
            api.RegisterCallbacks(this);
            api.Execute(new ExecutionSettings(
                new Filter { testMode = TestMode.EditMode, groupNames = new[] { SunAnswerClass } },
                new Filter { testMode = TestMode.EditMode, testNames = new[] { JobSetTest } }));
            Changed?.Invoke();
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
        }

        public void TestStarted(ITestAdaptor test)
        {
        }

        public void TestFinished(ITestResultAdaptor result)
        {
            if (result.HasChildren)
                return;

            if (result.TestStatus == TestStatus.Passed)
            {
                passed++;
                return;
            }

            if (result.TestStatus != TestStatus.Failed)
                return;

            failed++;
            failures.Append($"\n• {result.Name}: {FirstLine(result.Message)}");
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            Running = false;
            Summary = failed == 0
                ? $"Job checks passed ({passed} tests)."
                : $"Job checks FAILED: {failed} failed, {passed} passed.{failures}";
            api.UnregisterCallbacks(this);
            UnityEngine.Object.DestroyImmediate(api);
            api = null;
            Changed?.Invoke();
        }

        private static string FirstLine(string message)
        {
            if (string.IsNullOrEmpty(message))
                return "no message";

            int end = message.IndexOf('\n');
            return end < 0 ? message.Trim() : message.Substring(0, end).Trim();
        }
    }
}
