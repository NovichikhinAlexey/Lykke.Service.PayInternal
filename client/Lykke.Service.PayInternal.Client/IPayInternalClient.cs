using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models;
using Lykke.Service.PayInternal.Client.Models.Merchant;
using Lykke.Service.PayInternal.Client.Models.Order;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Lykke.Service.PayInternal.Client.Models.Transfer;

namespace Lykke.Service.PayInternal.Client
{
    public interface IPayInternalClient
    {
        Task<WalletAddressResponse> CreateAddressAsync(CreateWalletRequest request);

        Task<IEnumerable<WalletStateResponse>> GetNotExpiredWalletsAsync();

        Task CreateTransaction(CreateTransactionRequest request);

        Task UpdateTransaction(UpdateTransactionRequest request);
        
        /// <summary>
        /// Returns all merchants.
        /// </summary>
        /// <returns>The collection of merchants.</returns>
        Task<IReadOnlyList<MerchantModel>> GetMerchantsAsync();
        
        /// <summary>
        /// Returns merchant.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <returns>The merchant.</returns>
        Task<MerchantModel> GetMerchantByIdAsync(string merchantId);
        
        /// <summary>
        /// Creates merchant.
        /// </summary>
        /// <param name="request">The merchant create request.</param>
        /// <returns>The created merchant.</returns>
        Task<MerchantModel> CreateMerchantAsync(CreateMerchantRequest request);

        /// <summary>
        /// Updates a merchant.
        /// </summary>
        /// <param name="request">The merchant update request.</param>
        Task UpdateMerchantAsync(UpdateMerchantRequest request);

        /// <summary>
        /// Sets merchant public key.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <param name="content">The content of public key file.</param>
        Task SetMerchantPublicKeyAsync(string merchantId, byte[] content);

        /// <summary>
        /// Deletes a merchant.
        /// </summary>
        /// <param name="merchantId">The merchan id.</param>
        Task DeleteMerchantAsync(string merchantId);


        /// <summary>
        /// Returns an order by id.
        /// </summary>
        /// <param name="paymentRequestId">The payment request id.</param>
        /// <param name="orderId">The order id.</param>
        /// <returns>The payment request order.</returns>
        Task<OrderModel> GetOrderAsync(string paymentRequestId, string orderId);
        
        /// <summary>
        /// Returns merchant payment requests.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <returns>The collection of merchant payment requests.</returns>
        Task<IReadOnlyList<PaymentRequestModel>> GetPaymentRequestsAsync(string merchantId);
        
        /// <summary>
        /// Returns merchant payment request.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <param name="paymentRequestId">The payment request id.</param>
        /// <returns>The payment request.</returns>
        Task<PaymentRequestModel> GetPaymentRequestAsync(string merchantId, string paymentRequestId);

        /// <summary>
        /// Returns merchant payment request details.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <param name="paymentRequestId">The payment request id.</param>
        /// <returns>The payment request details.</returns>
        Task<PaymentRequestDetailsModel> GetPaymentRequestDetailsAsync(string merchantId, string paymentRequestId);

        /// <summary>
        ///  Returns merchant payment request by wallet address.
        /// </summary>
        /// <param name="walletAddress">Wallet address</param>
        /// <returns>The payment request.</returns>
        Task<PaymentRequestModel> GetPaymentRequestByAddressAsync(string walletAddress);
        
        /// <summary>
        /// Creates a payment request and wallet.
        /// </summary>
        /// <param name="model">The payment request creation information.</param>
        /// <returns>The payment request.</returns>
        Task<PaymentRequestModel> CreatePaymentRequestAsync(CreatePaymentRequestModel model);

        /// <summary>
        /// Creates an order if it does not exist or expired and returns payment request details.
        /// </summary>
        /// <returns>The payment request details.</returns>
        Task<PaymentRequestDetailsModel> ChechoutAsync(string merchantId, string paymentRequestId);

        /// <summary>
        /// Update transfer status.
        /// </summary>
        /// <param name="model">Transfer model.</param>
        /// <returns>The Transfer Info.</returns>
        Task<TransferRequest> UpdateTransferStatusAsync(UpdateTransferStatusModel model);

        /// <summary>
        /// Request to transfer all money.
        /// </summary>
        /// <param name="merchantId">Merchant Id.</param>
        /// <param name="destinationAddress">Destination Address.</param>
        /// <returns>The Transfer Info.</returns>
        Task<TransferRequest> TransfersRequestAllAsync(string merchantId, string destinationAddress);

        /// <summary>
        /// Request to transfer specify amount.
        /// </summary>
        /// <param name="merchantId">Merchant Id.</param>
        /// <param name="destinationAddress">Destination Address.</param>
        /// <param name="amount">Amount to send</param>
        /// <returns>The Transfer Info.</returns>
        Task<TransferRequest> TransfersRequestAmountAsync(string merchantId, string destinationAddress, string amount);

        /// <summary>
        /// Request to transfer from specify wallet. If Amount is 0, all money will be transfered
        /// </summary>
        /// <param name="merchantId">Merchant Id.</param>
        /// <param name="destinationAddress">Destination Address.</param>
        /// <param name="amount">Amount to send</param>
        /// <param name="sourceAddress">Source Address</param>
        /// <returns>The Transfer Info.</returns>
        Task<TransferRequest> TransfersRequestFromAddressAsync(string merchantId, string destinationAddress, string amount, string sourceAddress);

        /// <summary>
        /// Request to transfer from list of sources. If Amount is 0, all money will be transfered
        /// </summary>
        /// <param name="merchantId">Merchant Id.</param>
        /// <param name="destinationAddress">Destination Address.</param>
        /// <param name="amount">Amount to send</param>
        /// <param name="sourceAddressesList">Source Addresses List</param>
        /// <returns>The Transfer Info.</returns>
        Task<TransferRequest> TransfersRequestFromAddressesAsync(string merchantId, string destinationAddress, string amount,  List<string> sourceAddressesList);

        /// <summary>
        /// Request to transfer from list of sources with amounts
        /// </summary>
        /// <param name="merchantId">Merchant Id.</param>
        /// <param name="destinationAddress">Destination Address.</param>
        /// <param name="sourceAddressAmountList">Source Addresses with Amount List</param>
        /// <returns>The Transfer Info.</returns>
        Task<TransferRequest> TransfersRequestFromAddressesWithAmountAsync(string merchantId, string destinationAddress, List<SourceAmount> sourceAddressAmountList);

    }
}
