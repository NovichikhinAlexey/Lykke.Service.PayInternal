using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Services.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Lykke.Service.PayInternal.Models.Transfers
{
    public class CrosswiseTransferRequest : ITransferRequestModel
    {
        [Required]
        public string PaymentRequestId { get; set; }
        [Required]
        public string MerchantId { get; set; }
        [Required]
        public string AssetId { get; set; }
        [Required]
        public int FeeRate { get; set; }
        [Required]
        public decimal FixedFee { get; set; }
        [Required]
        public List<AddressAmount> Sources { get; set; }
        [Required]
        public List<AddressAmount> Destinations { get; set; }

        public TransferRequestModelValidationResult CheckAmountsValidity()
        {
            if (FeeRate < 0)
                return TransferRequestModelValidationResult.NegativeFeeRate;

            if (FixedFee < 0)
                return TransferRequestModelValidationResult.NegativeFixedFee;

            var destAmountSum = 0M;
            foreach (var dst in Destinations)
            {
                destAmountSum += dst.Amount;
                if (dst.Amount <= 0)
                    return TransferRequestModelValidationResult.NegatveDestinationAmount;
            }

            var sourceAmountSum = 0M;
            foreach (var src in Sources)
            {
                sourceAmountSum += src.Amount;
                if (src.Amount <= 0)
                    return TransferRequestModelValidationResult.NegativeSourceAmount;
            }

            return destAmountSum != sourceAmountSum ? 
                TransferRequestModelValidationResult.NotEqualSourceDestAmounts : 
                TransferRequestModelValidationResult.Valid;
        }

        public IMultipartTransfer ToDomain()
        {
            // Please, note: there is no additional call for CheckAmountsValidity(). It is assumed that the caller code already tested it.
            var result = new MultipartTransfer
            {
                PaymentRequestId = PaymentRequestId,
                TransferId = Guid.NewGuid().ToString(),
                MerchantId = MerchantId,
                CreationDate = DateTime.UtcNow,
                AssetId = AssetId,
                FeeRate = FeeRate,
                FixedFee = FixedFee,
                Parts = new List<TransferPart>()
            };

            // We need some temporary copy of list of sources for we do iteratively change it below.
            var tmpSources = (from s in Sources
                              select new AddressAmount
                              {
                                  Address = s.Address,
                                  Amount = s.Amount
                              }).ToList();

            for (int i = 0; i < Destinations.Count; i++)
            {
                var newMultipleTransaction = new TransferPart
                {
                    Destination = new AddressAmount
                    {
                        Amount = Destinations[i].Amount,
                        Address = Destinations[i].Address
                    },
                    Sources = new List<AddressAmount>()
                };

                var tmpDestinationAmount = Destinations[i].Amount;

                // Now we run iterational algo for fulfilling the requested destination amount from available sources.
                while (tmpDestinationAmount > 0)
                {
                    if (!tmpSources.Any()) return null; // Something's going wrong: we still have not satisfied destination, but there a no available sources

                    for (int j = 0; j < tmpSources.Count; j++)
                    {
                        var iterationAddedAmount = tmpSources[j].Amount >= tmpDestinationAmount ? 
                            tmpDestinationAmount : 
                            tmpSources[j].Amount;

                        newMultipleTransaction.Sources.Add(new AddressAmount
                        {
                            Address = tmpSources[j].Address,
                            Amount = iterationAddedAmount
                        });

                        tmpDestinationAmount -= iterationAddedAmount;
                        tmpSources[j].Amount -= iterationAddedAmount;

                        if (tmpDestinationAmount <= 0) break;
                    }

                    tmpSources.RemoveAll(s => s.Amount <= 0); // Removing all empty Sources. S.Amount can't be < 0, but let it be here to avoid any risk of infinite loop.
                }

                result.Parts.Add(newMultipleTransaction);
            }

            return result;
        }
    }
}
