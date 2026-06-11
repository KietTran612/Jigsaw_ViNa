namespace JigsawVina.Core.Services
{
    public class GameSessionService
    {
        public int SelectedPictureId { get; private set; }
        public int SelectedDifficultyId { get; private set; }
        public float LastElapsedTimeSeconds { get; set; }
        public int LastStarCount { get; set; }
        public bool IsRewardProcessed { get; set; }

        public void SetSelectedPicture(int pictureId)
        {
            SelectedPictureId = pictureId;
            IsRewardProcessed = false;
        }

        public void SetSelectedDifficulty(int difficultyId)
        {
            SelectedDifficultyId = difficultyId;
            IsRewardProcessed = false;
        }

        public void BeginPuzzle()
        {
            LastElapsedTimeSeconds = 0f;
            LastStarCount = 0;
            IsRewardProcessed = false;
        }
    }
}
