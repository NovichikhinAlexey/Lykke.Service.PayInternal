using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.Asset;
using Lykke.Service.PayInternal.Client.Models.Markup;
using Lykke.Service.PayInternal.Client.Models.Merchant;
using Lykke.Service.PayInternal.Client.Models.Order;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Lykke.Service.PayInternal.Client.Models.Transactions;
using Lykke.Service.PayInternal.Client.Models.Wallets;
using Lykke.Service.PayInternal.Client.Models.Supervisor;

namespace Lykke.Service.PayInternal.Client
{
    /// <summary>
    /// Pay Internal client interface
    /// </summary>
    public interface IPayInternalClient
    {
        /// <summary>
        /// Returns wallet addresses that are not considered as expired
        /// </summary>
        /// <returns></returns>
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
        /// Creates an order if it does not exist or expired.
        /// </summary>
        /// <param name="model">The order creation information.</param>
        /// <returns>An active order related with payment request.</returns>
        Task<OrderModel> ChechoutOrderAsync(ChechoutRequestModel model);

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
        /// Transfers BTC from source addresses with amount provided to destination address without LykkePay fees
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<BtcTransferResponse> BtcFreeTransferAsync(BtcFreeTransferRequest request);

        /// <summary>
        /// Finds and returns all monitored (i.e., not expired and not fully confirmed yet) transactions.
        /// </summary>
        /// <returns>The list of monitored transactions.</returns>
        Task<IEnumerable<TransactionStateResponse>> GetAllMonitoredTransactionsAsync();

        /// <summary>
        /// Initiates a refund for the specified payment request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<RefundResponse> RefundAsync(RefundRequestModel request);

        /// <summary>
        /// Marks transaction as expired
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task SetTransactionExpiredAsync(TransactionExpiredRequest request);

        /// <summary>
        /// Returns list of assets available for merchant according to availability type and general asset settings
        /// </summary>
        /// <param name="merchantId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [Obsolete("Use ResolveSettlementAssetsAsync and ResolvePaymentAssetsAsync instead")]
        Task<AvailableAssetsResponse> ResolveAvailableAssetsAsync(string merchantId, AssetAvailabilityType type);

        /// <summary>
        /// Returns available settlement assets for merchant
        /// </summary>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        Task<AvailableAssetsResponse> GetAvailableSettlementAssetsAsync(string merchantId);

        /// <summary>
        /// Returns available payment assets for merchant and settlement asset id
        /// </summary>
        /// <param name="merchantId"></param>
        /// <param name="settlementAssetId"></param>
        /// <returns></returns>
        Task<AvailableAssetsResponse> GetAvailablePaymentAssetsAsync(string merchantId, string settlementAssetId);

        /// <summary>
        /// Returns asset general settings
        /// </summary>
        Task<IEnumerable<AssetGeneralSettingsResponse>> GetAssetGeneralSettingsAsync();

        /// <summary>
        /// Returns merchant asset settings
        /// </summary>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        Task<AssetMerchantSettingsResponse> GetAssetMerchantSettingsAsync(string merchantId);

        /// <summary>
        ///  Updates asset general settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task SetAssetGeneralSettingsAsync(UpdateAssetGeneralSettingsRequest request);

        /// <summary>
        /// Updates merchant asset settings
        /// </summary>
        /// <param name="settingsRequest"></param>
        /// <returns></returns>
        Task SetAssetMerchantSettingsAsync(UpdateAssetMerchantSettingsRequest settingsRequest);

        /// <summary>
        /// Cancels payment request
        /// </summary>
        /// <param name="merchantId">Merchant id</param>
        /// <param name="paymentRequestId">Payment request id</param>
        /// <returns></returns>
        Task CancelAsync(string merchantId, string paymentRequestId);

        /// <summary>
        /// Marks wallet as expired
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task SetWalletExpiredAsync(BlockchainWalletExpiredRequest request);

        /// <summary>
        /// Returns markup values for merchant and asset pair
        /// </summary>
        /// <param name="merchantId">Merchant id</param>
        /// <param name="assetPairId">Asset pair id</param>
        /// <returns></returns>
        Task<MarkupResponse> ResolveMarkupByMerchantAsync(string merchantId, string assetPairId);

        /// <summary>
        /// Returns the default markup values for all asset pairs
        /// </summary>
        /// <returns></returns>
        Task<IReadOnlyList<MarkupResponse>> GetDefaultMarkupsAsync();

        /// <summary>
        /// Returns the default markup values for asset pair id
        /// </summary>
        /// <param name="assetPairId">Asset pair id</param>
        /// <returns></returns>
        Task<MarkupResponse> GetDefaultMarkupAsync(string assetPairId);

        /// <summary>
        /// Updates markup values for asset pair id
        /// </summary>
        /// <param name="assetPairId">Asset pair id</param>
        /// <param name="request">Markup values</param>
        /// <returns></returns>
        Task SetDefaultMarkupAsync(string assetPairId, UpdateMarkupRequest request);

        /// <summary>
        /// Returns all markup values for merchant
        /// </summary>
        /// <param name="merchantId">Merchant id</param>
        /// <returns></returns>
        Task<IReadOnlyList<MarkupResponse>> GetMarkupsForMerchantAsync(string merchantId);

        /// <summary>
        /// Returns markup value for merchant and asset pair id
        /// </summary>
        /// <param name="merchantId">Merchant id</param>
        /// <param name="assetPairId">Asset pair id</param>
        /// <returns></returns>
        Task<MarkupResponse> GetMarkupForMerchantAsync(string merchantId, string assetPairId);

        /// <summary>
        /// Updates markup values for merchant and asset pair
        /// </summary>
        /// <param name="merchantId">Merchant id</param>
        /// <param name="assetPairId">Asset pair id</param>
        /// <param name="request">Markup values</param>
        /// <returns></returns>
        Task SetMarkupForMerchantAsync(string merchantId, string assetPairId, UpdateMarkupRequest request);
        /// <summary>
        /// Return list of supervising merchants for employee
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        Task<SupervisingMerchantsResponse> GetSupervisingAsync(string employeeId);
        /// <summary>
        /// Delete all supervising merchants from employee
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        Task DeleteSupervisingAsync(string employeeId);
        /// <summary>
        /// Add supervising merchants for employee
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task SetSupervisingAsync(CreateSupervisingEmployeeRequest request);
    }
}
