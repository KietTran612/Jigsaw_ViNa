using System.Collections.Generic;
using System.Linq;
using JigsawVina.Core.Data;

namespace JigsawVina.Core.Services
{
    public class StaticDataService : IStaticDataService
    {
        private readonly List<PictureConfig> _pictures = new()
        {
            // Picture 1: Ho Guom, Picture 2: Vinh Ha Long
            new PictureConfig(1, "ho_guom", "H\u1ed3 G\u01b0\u01a1m", "Textures/ho_guom"),
            new PictureConfig(2, "ha_long", "V\u1ecbnh H\u1ea1 Long", "Textures/ha_long")
        };

        private readonly Dictionary<(int PictureId, int DifficultyId), PictureDifficultyConfig> _difficulties = new()
        {
            [(1, 0)] = new PictureDifficultyConfig(1, 0, "D\u1ec5", 6, 4, 1),
            [(1, 1)] = new PictureDifficultyConfig(1, 1, "Trung b\u00ecnh", 8, 6, 2),
            [(1, 2)] = new PictureDifficultyConfig(1, 2, "Kh\u00f3", 12, 8, 3),
            [(2, 0)] = new PictureDifficultyConfig(2, 0, "D\u1ec5", 6, 4, 1),
            [(2, 1)] = new PictureDifficultyConfig(2, 1, "Trung b\u00ecnh", 8, 6, 2),
            [(2, 2)] = new PictureDifficultyConfig(2, 2, "Kh\u00f3", 12, 8, 3)
        };

        public IReadOnlyList<PictureConfig> GetAllPictures() => _pictures;

        public PictureConfig GetPictureById(int id)
        {
            return _pictures.FirstOrDefault(p => p.Id == id);
        }

        public PictureDifficultyConfig GetPictureDifficulty(int pictureId, int difficultyId)
        {
            var picture = GetPictureById(pictureId);
            if (picture.Id == 0)
            {
                throw new KeyNotFoundException($"Picture with ID {pictureId} not found.");
            }

            if (_difficulties.TryGetValue((pictureId, difficultyId), out var config))
            {
                return config;
            }

            throw new KeyNotFoundException(
                $"Difficulty with ID {difficultyId} was not configured for picture {pictureId}.");
        }
    }
}
