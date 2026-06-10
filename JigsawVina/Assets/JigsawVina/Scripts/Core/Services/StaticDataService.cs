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

        public IReadOnlyList<PictureConfig> GetAllPictures() => _pictures;

        public PictureConfig GetPictureById(int id)
        {
            return _pictures.FirstOrDefault(p => p.Id == id);
        }
    }
}
