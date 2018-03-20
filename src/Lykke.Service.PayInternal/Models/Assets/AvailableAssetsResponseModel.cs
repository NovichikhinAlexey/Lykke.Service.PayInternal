using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Models.Assets
{
    public class AvailableAssetsResponseModel
    {
        public IReadOnlyList<string> Assets { get; set; }
    }
}
