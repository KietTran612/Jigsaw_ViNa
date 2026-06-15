using System;
using System.Globalization;

namespace JigsawVina.Core.Services
{
    public class LocalDateProvider : ILocalDateProvider
    {
        public string GetCurrentLocalDateString()
        {
            return DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}
