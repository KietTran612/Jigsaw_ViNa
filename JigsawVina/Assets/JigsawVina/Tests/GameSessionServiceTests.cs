using NUnit.Framework;
using JigsawVina.Core.Services;

namespace JigsawVina.Tests
{
    public class GameSessionServiceTests
    {
        [Test]
        public void Session_StoresCorrectly()
        {
            var session = new GameSessionService();
            session.SetSelectedPicture(5);
            session.SetSelectedDifficulty(1); // Normal

            Assert.AreEqual(5, session.SelectedPictureId);
            Assert.AreEqual(1, session.SelectedDifficultyId);
        }
    }
}
