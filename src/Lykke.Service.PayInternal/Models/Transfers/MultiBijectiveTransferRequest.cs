using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Services.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Lykke.Service.PayInternal.Models.Transfers
{
    public class MultiBijectiveTransferRequest : ITransferRequestModel
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
        public List<BiAddressAmount> BiAddresses { get; set; }

        public TransferRequestModelValidationResult CheckAmountsValidity()
        {
            if (FeeRate < 0)
                return TransferRequestModelValidationResult.NegativeFeeRate;

            if (FixedFee < 0)
                return TransferRequestModelValidationResult.NegativeFixedFee;

            return BiAddresses.Any(x => x.Amount <= 0) ? 
                TransferRequestModelValidationResult.NegatveDestinationAmount : 
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


            // Just translate BiAddresses to a list of single-source transactions
            foreach (var biad in BiAddresses)
            {
                var newTransaction = new TransferPart
                {
                    Destination =
                    {
                        Amount = biad.Amount,
                        Address = biad.DestinationAddress
                    },
                    Sources = new List<AddressAmount>
                    {
                        new AddressAmount
                        {
                            Address = biad.SourceAddress,
                            Amount = biad.Amount
                        }
                    }
                };


                result.Parts.Add(newTransaction);
            }

            return result;
        }
    }
}
