using JigsawVina.Core.Services;

namespace JigsawVina.Presentation.Screens
{
    public class PictureSelectPresenter
    {
        private readonly GameSessionService _sessionService;

        public PictureSelectPresenter(PictureSelectView view, GameSessionService sessionService)
        {
            _sessionService = sessionService;
            view.OnPictureSelected += HandlePictureSelected;
        }

        private void HandlePictureSelected(int pictureId)
        {
            _sessionService.SetSelectedPicture(pictureId);
        }
    }
}
