﻿using BeatLeader.API.Methods;
using BeatLeader.Interop;
using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using System.Linq;
using BeatLeader.Models.Replay;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class ReplayPanel : ReeUIComponentV2 {
        #region Components

        [UIValue("settings-panel"), UsedImplicitly]
        private ReplayerSettingsPanel _settingsPanel = null!;

        private void Awake() {
            _settingsPanel = Instantiate<ReplayerSettingsPanel>(transform);
        }

        #endregion

        #region Initialize/Dispose

        protected override void OnInitialize() {
            InitializePlayButton();

            DownloadReplayRequest.AddProgressListener(OnDownloadProgressChanged);
            ReplayerMenuLoader.AddStateListener(OnLoaderStateChanged);
            LeaderboardState.AddSelectedBeatmapListener(OnSelectedBeatmapChanged);
        }

        protected override void OnDispose() {
            DownloadReplayRequest.RemoveProgressListener(OnDownloadProgressChanged);
            ReplayerMenuLoader.RemoveStateListener(OnLoaderStateChanged);
            LeaderboardState.RemoveSelectedBeatmapListener(OnSelectedBeatmapChanged);
        }

        #endregion

        #region SetReplayOrScore

        public void SetScore(Score score) {
            ReplayerMenuLoader.NotifyDataWasSelected(new(score));
        }

        public void SetReplayPath(string? filePath) {
            var pathIsNotNull = filePath is not null;
            SetPlayButtonInteractable(pathIsNotNull);
            if (!pathIsNotNull) return;
            ReplayerMenuLoader.NotifyDataWasSelected(new(filePath!));
        }

        #endregion

        #region Events
        
        public bool followMenuLoaderDownloadState = true;

        private void OnSelectedBeatmapChanged(bool selectedAny, LeaderboardKey leaderboardKey, IDifficultyBeatmap beatmap) {
            _buttonCanBeInteractable = SongCoreInterop.ValidateRequirements(beatmap);
        }

        private void OnLoaderStateChanged(ReplayerMenuLoader.LoaderState state, Score? score, Replay? replay) {
            //if (!followMenuLoaderDownloadState) {
            //    SetPlayButtonInteractable(state is not ReplayerMenuLoader.LoaderState.Downloading);
            //    return;
            //}
            SetPlayButtonInteractable(state 
                is not ReplayerMenuLoader.LoaderState.Downloading 
                and not ReplayerMenuLoader.LoaderState.Started
                and not ReplayerMenuLoader.LoaderState.Uninitialized);

            switch (state) {
                case ReplayerMenuLoader.LoaderState.DownloadRequired:
                case ReplayerMenuLoader.LoaderState.ReadyToPlay:
                    _playButton.SetButtonText("Watch Replay");
                    break;
            }
        }

        private void OnDownloadProgressChanged(float uploadProgress, float downloadProgress, float overallProgress) {
            if (!followMenuLoaderDownloadState) return;
            _playButton.SetButtonText($"{downloadProgress * 100:F0}<size=60%>%");
        }

        #endregion

        #region PlayButton

        [UIComponent("play-button"), UsedImplicitly]
        private Button _playButton = null!;

        private bool _buttonCanBeInteractable = true;

        private void InitializePlayButton() {
            _playButton.onClick.AddListener(ReplayerMenuLoader.NotifyPlayButtonWasPressed);
            _playButton.SetButtonText("Watch Replay");
            SetPlayButtonInteractable(false);
        }

        private void SetPlayButtonInteractable(bool value) {
            _playButton.interactable = value && _buttonCanBeInteractable;
        }

        #endregion

        #region SetActive

        public void SetActive(bool value) {
            Active = value;
            SetPlayButtonInteractable(true);
        }

        #endregion

        #region Active

        private bool _active = true;

        [UIValue("active"), UsedImplicitly]
        private bool Active {
            get => _active;
            set {
                if (_active.Equals(value)) return;
                _active = value;
                NotifyPropertyChanged();
            }
        }

        #endregion
    }
}