namespace JigsawVina.Core.Services
{
    public class GameSessionService
    {
        public int SelectedPictureId { get; private set; }
        public int SelectedDifficultyId { get; private set; }
        public float LastElapsedTimeSeconds { get; set; }
        public int LastStarCount { get; set; }

        public void SetSelectedPicture(int pictureId)
        {
            SelectedPictureId = pictureId;
        }

        public void SetSelectedDifficulty(int difficultyId)
        {
            SelectedDifficultyId = difficultyId;
        }
    }
}
