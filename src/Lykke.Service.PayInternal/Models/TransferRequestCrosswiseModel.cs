using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Lykke.Service.PayInternal.Models
{
    /// <summary>
    /// The data to be obtained by TransferRequestController via http-request for crosswise transfer.
    /// </summary>
    public class TransferRequestCrosswiseModel : ITransferRequestModel
    {
        /// <summary>
        /// ID of the Merchant who is responsible for money transfer.
        /// </summary>
        [Required]
        public string MerchantId { get; set; }
        /// <summary>
        /// The side who pays the fee. If Merchant pays, then the Client receives the full sum
        /// of money as it is pointed in request for the transfer, and the fee is additionaly 
        /// paid from Merchant's wallets(s). Alternatively, the Client gets his (money - fee),
        /// and the Merchant is not responsible for anything.
        /// </summary>
        [Required]
        public TransferFeePayerEnum FeePayer { get; set; }
        /// <summary>
        /// The list of source addresses, belonging to the Merchant.
        /// </summary>
        [Required]
        public List<AddressAmount> Sources { get; set; }
        /// <summary>
        /// The list of destination addresses, (supposably) belonging to the Client.
        /// </summary>
        [Required]
        public List<AddressAmount> Destinations { get; set; }

        /// <summary>
        /// Indicates the first revealed validation error.
        /// </summary>
        /// <returns>The first encountered format mismatch or null if nothing was found.</returns>
        /// <remarks>The mismatches that can be found here are:
        /// * if one of the destination addresses has the amount less than or equal to 0;
        /// * if one of the source addresses has the amount less than or equal to 0;
        /// * the sum of source amounts is not equal to the sum of destination amounts.</remarks>
        public string CheckAmountsValidity()
        {
            decimal destAmountSum = 0M;
            foreach (AddressAmount dst in Destinations)
            {
                destAmountSum += dst.Amount;
                if (dst.Amount <= 0)
                    return "Some destination address has the requested transfer amount <= 0. The transfer is impossible.";
            }

            decimal sourceAmountSum = 0M;
            foreach (AddressAmount src in Sources)
            {
                sourceAmountSum += src.Amount;
                if (src.Amount <= 0)
                    return "Some source address has the transfer amount <= 0. The transfer is impossible.";
            }

            if (destAmountSum != sourceAmountSum)
                return "The requested sum for crosswise transfer doesn't coincide with the sum of the specified source addresses' money amounts. The transfer is impossible.";

            return null; // No errors on this level of analysis. But transfer may fail deeper for some reasons.
        }

        /// <summary>
        /// Converts the model data to transfer request object with multiple crosswise transactions: each transaction may contain 
        /// several sources and one destination address, but different transactions (may) have different destination addresses.
        /// </summary>
        /// <returns>The newly created instance of <see cref="TransferRequest"/>.</returns>
        public ITransferRequest ToTransferRequest()
        {
            // Please, note: there is no additional call for CheckAmountsValidity(). It is assumed that the caller code already tested it.
            var result = new TransferRequest()
            {
                TransferId = Guid.NewGuid().ToString(),
                TransferStatus = TransferStatus.InProgress,
                TransferStatusError = TransferStatusError.NotError,
                CreateDate = DateTime.Now,
                MerchantId = MerchantId
            };

            result.TransactionRequests = new List<ITransactionRequest>();
            decimal tmpDestinationAmount, iterationAddedAmount;
            // We need some temporary copy of list of sources for we do iteratively change it below.
            var tmpSources = (from S in Sources
                              select new AddressAmount()
                              {
                                  Address = S.Address,
                                  Amount = S.Amount
                              }).ToList();

            for (int i = 0; i < Destinations.Count; i++)
            {
                var newMultipleTransaction = new TransactionRequest();
                newMultipleTransaction.Amount = Destinations[i].Amount;
                newMultipleTransaction.CountConfirm = 1; // TODO: maybe it would be more awesome to move this constant to settings
                newMultipleTransaction.Currency = LykkeConstants.BitcoinAssetId;
                newMultipleTransaction.DestinationAddress = Destinations[i].Address;
                newMultipleTransaction.SourceAmounts = new List<IAddressAmount>();
                
                tmpDestinationAmount = Destinations[i].Amount; // See below

                // Now we run iterational algo for fulfilling the requested destination amount from available sources.
                while (tmpDestinationAmount > 0)
                {
                    if (!tmpSources.Any()) return null; // Something's going wrong: we still have not satisfied destination, but there a no available sources

                    for (int j = 0; j < tmpSources.Count; j++)
                    {
                        if (tmpSources[j].Amount >= tmpDestinationAmount) iterationAddedAmount = tmpDestinationAmount;
                        else iterationAddedAmount = tmpSources[j].Amount;

                        newMultipleTransaction.SourceAmounts.Add(new AddressAmount()
                        {
                            Address = tmpSources[j].Address,
                            Amount = iterationAddedAmount
                        });

                        tmpDestinationAmount -= iterationAddedAmount;
                        tmpSources[j].Amount -= iterationAddedAmount;

                        if (tmpDestinationAmount <= 0) break;
                    }

                    tmpSources.RemoveAll(S => S.Amount <= 0); // Removing all empty Sources. S.Amount can't be < 0, but let it be here to avoid any risk of infinite loop.
                }

                result.TransactionRequests.Add(newMultipleTransaction);
            }

            // Please, note: the FeePayer property is not curretly supported by ITransferRequest and lower levels of infrastructure. Is to be impemented in future.
            return result;
        }
    }
}
