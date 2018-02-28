using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Models
{
    public class AvailableAssetsResponseModel
    {
        public IReadOnlyList<AssetModel> Assets { get; set; }
    }
}
