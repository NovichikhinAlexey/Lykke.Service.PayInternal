using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.Merchant;
using Lykke.Service.PayInternal.Client.Models.Order;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Lykke.Service.PayInternal.Client.Models.Refunds;
using Lykke.Service.PayInternal.Client.Models.Transactions;
using Lykke.Service.PayInternal.Client.Models.Wallets;

namespace Lykke.Service.PayInternal.Client
{
    public interface IPayInternalClient
    {
        Task<WalletAddressResponse> CreateAddressAsync(CreateWalletRequest request);

        Task<IEnumerable<WalletStateResponse>> GetNotExpiredWalletsAsync();

        /// <summary>
        /// Creates transaction of payment type
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task CreatePaymentTransactionAsync(CreateTransactionRequest request);

        /// <summary>
        /// Updates transaction
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task UpdateTransactionAsync(UpdateTransactionRequest request);
        
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
        /// Transfers BTC from source addresses with amount provided to destination address without LykkePay fees
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<BtcTransferResponse> BtcFreeTransferAsync(BtcFreeTransferRequest request);

        /// <summary>
        /// Finds and returns all monitored (i.e., not expired and not fully confirmed yet) transactions.
        /// </summary>
        /// <returns>The list of monitored transactions.</returns>
        Task<IEnumerable<TransactionStateResponse>> GetAllMonitoredTransactions();

        /// <summary>
        /// Request transfer from a list of some source address(es) to a list of destination address(es) with amounts specified.
        /// </summary>
        /// <param name="request">The data containing serialized model object.</param>
        /// <returns>The object representing the state of transfer request execution.</returns>
        Task<MultipartTransferResponse> CrosswiseTransferAsync(CrosswiseTransferRequest request);

        /// <summary>
        /// Request transfer consistent of a list of signle-source and single-destination transactions with amounts specified for every address pair.
        /// </summary>
        /// <param name="request">The data containing serialized model object.</param>
        /// <returns>The object representing the state of transfer request execution.</returns>
        Task<MultipartTransferResponse> MultiBijectiveTransferAsync( MultipartTransferResponse request);

        /// <summary>
        /// Creates a new refund request for the specified payment request and (optionally) wallet address.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<RefundResponse> CreateRefundRequestAsync(RefundRequestModel request);

        /// <summary>
        /// Gets the current state of the specified refund request.
        /// </summary>
        /// <param name="merchantId"></param>
        /// <param name="refundId"></param>
        /// <returns></returns>
        Task<RefundResponse> GetRefundAsync(string merchantId, string refundId);
    }
}
