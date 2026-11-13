using System.Collections.Generic;
using DigitalLove.Audio;
using DigitalLove.Casual.Levels;
using DigitalLove.DataAccess;
using DigitalLove.FlowControl;
using DigitalLove.Game.GesturePrompts;
using DigitalLove.Game.Persistence;
using DigitalLove.Game.PuzzleSelection;
using DigitalLove.Game.SpaceShaping;
using DigitalLove.Game.UI.Console;
using DigitalLove.Global;
using DigitalLove.Localization;
using DigitalLove.TTS;
using Reflex.Attributes;
using UnityEngine;

namespace DigitalLove.Game.Flow
{
    public class PuzzleSelectionState : MonoState
    {
        [SerializeField] private MonoState nextState;
        [SerializeField] private LevelSelector levelSelector;
        [SerializeField] private PuzzleMapView view;
        [SerializeField] private TtsPresenter ttsPresenter;
        [SerializeField] private GesturePromptPresenter prompts;
        [SerializeField] private SynthCuePlayer systemPlayer;
        [SerializeField] private SynthPatch selectCue;
        [SerializeField] private SynthPatch openCue;
        [SerializeField] private SynthPatch typeCue;
        [SerializeField] private JobCardPanel card;
        [SerializeField] private SectorReadout sector;
        [SerializeField] private MapChapterTitles chapterTitles;
        [Tooltip("Types the episode bulletin before the map, once per chapter.")]
        [SerializeField] private ConsoleScreen console;
        [SerializeField, Min(0f)] private float tickCooldownSecs = 0.06f;

        [Header("Debug")]
        [SerializeField] private DebugBool selectRandom;
        [SerializeField] private PuzzleData puzzleToSet;

        [Inject] private MemoryDataClient memoryDataClient;
        [Inject] private PlayerDataProvider playerDataProvider;

        private const string LevelsTable = "Levels";

        private PuzzleMap map;
        private bool autoEntryUsed;
        private bool active;

        public override void Enter()
        {
            active = true;
            ttsPresenter.Stop();
            view.SelectionChanged += OnSelectionChanged;
            card.Opened += OnCardOpened;
            card.LineTyped += OnLineTyped;
            ApplyPendingCloud();
            PlayerData playerData = memoryDataClient.Get<PlayerData>();

            if (selectRandom.Value)
            {
                EnterJob(levelSelector.GetLevelByGlobalIndex<PuzzleData>(
                    Random.Range(0, levelSelector.TotalLevelsCount)));
                return;
            }

            if (puzzleToSet != null)
            {
                EnterJob(puzzleToSet);
                return;
            }

            if (view.CurrentMap != null)
            {
                AdoptMap();
                return;
            }

            RunProgress progress = RunProgressCookies.Build(levelSelector, playerData);
            if (progress.IsFresh && !autoEntryUsed)
            {
                autoEntryUsed = true;
                EnterJob(levelSelector.GetLevelByGlobalIndex<PuzzleData>(0));
                return;
            }

            if (!TryPlayBulletin(playerData, progress))
                ShowMap(progress);
        }

        // Once a chapter's first job is solved, the console types the episode's bulletin (the world the
        // player now works in) before the map comes back. Once per chapter; none authored, none played.
        private bool TryPlayBulletin(PlayerData playerData, RunProgress progress)
        {
            string chapter = RunProgressCookies.DueBulletinChapter(levelSelector, playerData);
            if (chapter == null)
                return false;

            List<string> lines =
                LocalizationUtil.GetValuesInKeyOrder(LevelsTable, ChapterData.BulletinKeyFor(chapter));
            if (lines.Count == 0)
                return false;

            RunProgressCookies.MarkBulletinSeen(playerData, chapter);
            console.PlayMessage(lines, () =>
            {
                if (active)
                    ShowMap(progress);
            });
            return true;
        }

        private void ApplyPendingCloud()
        {
            PlayerData pending = playerDataProvider.ApplyPendingCloud();
            if (pending != null)
                memoryDataClient.Put(pending);
        }

        private void ShowMap(RunProgress progress)
        {
            view.EnterRequested += OnEnterRequested;

            map = RunMap.Build(levelSelector, progress);
            view.Show(map);
            chapterTitles.Show(map);
            ShowSector(progress);
            prompts.Show(GestureKind.Enter);
        }

        private void AdoptMap()
        {
            map = view.CurrentMap;
            view.EnterRequested += OnEnterRequested;
            chapterTitles.Show(map);
            ShowSector(RunProgressCookies.Build(levelSelector, memoryDataClient.Get<PlayerData>()));
            prompts.Show(GestureKind.Enter);
        }

        private void ShowSector(RunProgress progress)
        {
            PlayerData playerData = memoryDataClient.Get<PlayerData>();
            bool typed = RunProgressCookies.SeeCoverage(playerData, progress.SolvedCount);
            sector.Show(progress.SolvedCount, progress.Count, typed);
        }

        public override void Exit()
        {
            view.EnterRequested -= OnEnterRequested;
            view.SelectionChanged -= OnSelectionChanged;
            card.Opened -= OnCardOpened;
            card.LineTyped -= OnLineTyped;
            active = false;
            prompts.HideAll();
            sector.Hide();
            chapterTitles.Hide();

            if (map == null)
                return;

            view.Hide();
            map = null;
        }

        private void OnEnterRequested()
        {
            if (!map.CanEnter)
            {
                view.Refuse();
                return;
            }

            EnterJob(map.SelectedPuzzle);
        }

        // Any entry uses up the fresh-save auto-entry, so leaving a job always lands on the map.
        private void EnterJob(PuzzleData puzzle)
        {
            autoEntryUsed = true;
            memoryDataClient.Remove<PuzzleRestart>();
            memoryDataClient.Put(puzzle);
            parent.SetCurrentState(nextState.RouteId);
        }

        private void OnSelectionChanged() => Tick(selectCue);

        private void OnCardOpened() => Tick(openCue);

        private void OnLineTyped() => Tick(typeCue);

        private void Tick(SynthPatch patch) => systemPlayer.PlayTick(patch, tickCooldownSecs);
    }
}
