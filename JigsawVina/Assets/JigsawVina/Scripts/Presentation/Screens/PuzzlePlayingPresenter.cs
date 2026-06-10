using JigsawVina.Core.Services;

namespace JigsawVina.Presentation.Screens
{
    public class PuzzlePlayingPresenter
    {
        private readonly PuzzlePlayingView _view;
        private readonly GameSessionService _sessionService;
        private readonly IStaticDataService _staticDataService;

        public PuzzlePlayingPresenter(
            PuzzlePlayingView view,
            GameSessionService sessionService,
            IStaticDataService staticDataService)
        {
            _view = view;
            _sessionService = sessionService;
            _staticDataService = staticDataService;
        }

        public void Initialize()
        {
            var picture = _staticDataService.GetPictureById(_sessionService.SelectedPictureId);
            string difficultyName = _sessionService.SelectedDifficultyId switch
            {
                0 => "Easy (24 pieces)",
                1 => "Normal (48 pieces)",
                2 => "Hard (96 pieces)",
                _ => "Debug"
            };

            _view.Setup(picture.DisplayName ?? "Unknown", difficultyName);
        }
    }
}
