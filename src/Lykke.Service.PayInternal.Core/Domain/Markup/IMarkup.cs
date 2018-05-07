using System;

namespace Lykke.Service.PayInternal.Core.Domain.Markup
{
    public interface IMarkup : IMarkupValue
    {
        string AssetPairId { get; set; }

        MarkupIdentityType IdentityType { get; set; }

        string Identity { get; set; }

        DateTime CreatedOn { get; set; }
    }
}
