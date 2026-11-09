using System;
using System.Collections.Generic;
using DigitalLove.Casual.Flow.Setup;
using TMPro;
using UnityEngine;

namespace DigitalLove.Game.UI.SplashScreen
{
    // The boot's setup rows ("> SEAT ........ OK"). A row types up to its last word when its step
    // begins and finishes with that word when the step ends. Reports are queued and typed in order, so
    // steps that finish in the same frame still read one by one.
    [Serializable]
    public class BootChecklist
    {
        [Serializable]
        private class Row
        {
            [SerializeField] private SetupStep step;
            [SerializeField] private TMP_Text text;

            public SetupStep Step => step;
            public TMP_Text Text => text;

            // Everything before the last word: the row while its step is still running.
            public int PendingCount => Mathf.Max(0, text.text.LastIndexOf(' '));
        }

        [SerializeField] private Row[] rows;

        private Queue<(TMP_Text line, int count)> reports;

        private Queue<(TMP_Text line, int count)> Reports => reports ??= new Queue<(TMP_Text, int)>();

        public bool IsIdle => Reports.Count == 0;

        public void Hide()
        {
            Reports.Clear();
            foreach (Row row in rows)
                row.Text.gameObject.SetActive(false);
        }

        public void Begin(SetupStep step)
        {
            Row row = Find(step);
            if (row != null)
                Reports.Enqueue((row.Text, row.PendingCount));
        }

        public void End(SetupStep step)
        {
            Row row = Find(step);
            if (row != null)
                Reports.Enqueue((row.Text, int.MaxValue));
        }

        // The next row to type and how far.
        public (TMP_Text line, int count) Next() => Reports.Dequeue();

        private Row Find(SetupStep step) => Array.Find(rows, row => row.Step == step);
    }
}
