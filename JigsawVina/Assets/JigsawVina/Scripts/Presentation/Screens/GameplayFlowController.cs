using System;
using Cysharp.Threading.Tasks;
using JigsawVina.Core.Services;
using VContainer;
using VContainer.Unity;
using UnityEngine;

namespace JigsawVina.Presentation.Screens
{
    public class GameplayFlowController : IStartable, ITickable, IDisposable
    {
        private readonly PuzzlePlayingView _puzzlePlayingView;
        private readonly RewardSummaryView _rewardSummaryView;
        private readonly PuzzlePlayingPresenter _puzzlePlayingPresenter;
        private readonly RewardSummaryPresenter _rewardSummaryPresenter;
        private readonly SceneLoader _sceneLoader;

        private bool _isPlaying;

        public GameplayFlowController(
            PuzzlePlayingView puzzlePlayingView,
            RewardSummaryView rewardSummaryView,
            PuzzlePlayingPresenter puzzlePlayingPresenter,
            RewardSummaryPresenter rewardSummaryPresenter,
            SceneLoader sceneLoader)
        {
            _puzzlePlayingView = puzzlePlayingView;
            _rewardSummaryView = rewardSummaryView;
            _puzzlePlayingPresenter = puzzlePlayingPresenter;
            _rewardSummaryPresenter = rewardSummaryPresenter;
            _sceneLoader = sceneLoader;
        }

        public void Start()
        {
            _puzzlePlayingView.SetActive(true);
            _rewardSummaryView.SetActive(false);
            _puzzlePlayingPresenter.Initialize();
            _isPlaying = true;

            _puzzlePlayingPresenter.OnPuzzleCompleted += HandlePuzzleCompleted;
            _puzzlePlayingPresenter.OnBackRequested += HandleBack;
            _puzzlePlayingPresenter.OnQuitRequested += HandleQuit;

            _rewardSummaryView.OnReturnClicked += HandleReturnClicked;
        }

        public void Tick()
        {
            if (_isPlaying)
            {
                _puzzlePlayingPresenter.Tick();
            }
        }

        public void Dispose()
        {
            if (_puzzlePlayingPresenter != null)
            {
                _puzzlePlayingPresenter.OnPuzzleCompleted -= HandlePuzzleCompleted;
                _puzzlePlayingPresenter.OnBackRequested -= HandleBack;
                _puzzlePlayingPresenter.OnQuitRequested -= HandleQuit;
            }

            if (_rewardSummaryView != null)
            {
                _rewardSummaryView.OnReturnClicked -= HandleReturnClicked;
            }
        }

        private void HandleBack()
        {
            _isPlaying = false;
            _puzzlePlayingPresenter.Cleanup();
            _sceneLoader.LoadSceneAsync("Home").Forget();
        }

        private void HandleQuit()
        {
            _isPlaying = false;
            _puzzlePlayingPresenter.Cleanup();
            _sceneLoader.LoadSceneAsync("Home").Forget();
        }

        private void HandleReturnClicked()
        {
            _puzzlePlayingPresenter.Cleanup();
            _sceneLoader.LoadSceneAsync("Home").Forget();
        }

        private void HandlePuzzleCompleted(float elapsedSeconds)
        {
            _isPlaying = false;
            _puzzlePlayingView.DisableAllInput();

            _rewardSummaryPresenter.ProcessRewards(elapsedSeconds);
            ShowRewardSequence().Forget();
        }

        private async UniTaskVoid ShowRewardSequence()
        {
            Debug.Log("[FlowController] ShowRewardSequence started");
            try
            {
                var token = _puzzlePlayingView.destroyCancellationToken;
                Debug.Log("[FlowController] Playing Win Animation...");
                await _puzzlePlayingView.BoardView.PlayWinAnimationAsync(token);
                Debug.Log("[FlowController] Win Animation finished, delaying...");
                await UniTask.Delay(500, ignoreTimeScale: true, cancellationToken: token);
                Debug.Log("[FlowController] Delay finished");
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[FlowController] ShowRewardSequence cancelled");
                return;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FlowController] ShowRewardSequence error: {ex}");
                return;
            }

            Debug.Log("[FlowController] Transitioning to reward view");
            _puzzlePlayingView.SetActive(false);
            _rewardSummaryView.SetActive(true);
            _rewardSummaryPresenter.DisplayProcessedReward();
            Debug.Log("[FlowController] Reward view active and processed");
        }
    }
}
