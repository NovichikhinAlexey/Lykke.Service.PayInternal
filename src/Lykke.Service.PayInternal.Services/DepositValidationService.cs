using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Order;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayTransferValidation.Client;
using Lykke.Service.PayTransferValidation.Client.Models.Validation;

namespace Lykke.Service.PayInternal.Services
{
    public class DepositValidationService : IDepositValidationService
    {
        private readonly IPaymentRequestService _paymentRequestService;
        private readonly IOrderService _orderService;
        private readonly IPayTransferValidationClient _payTransferValidationClient;
        private readonly ILog _log;

        public DepositValidationService(
            [NotNull] IPaymentRequestService paymentRequestService,
            [NotNull] IPayTransferValidationClient payTransferValidationClient,
            [NotNull] IOrderService orderService,
            [NotNull] ILogFactory logFactory)
        {
            _paymentRequestService = paymentRequestService;
            _payTransferValidationClient = payTransferValidationClient;
            _orderService = orderService;
            _log = logFactory.CreateLog(this);
        }

        public async Task<bool> ValidateDepositTransferAsync(ValidateDepositTransferCommand cmd)
        {
            IPaymentRequest paymentRequest = await _paymentRequestService.FindAsync(cmd.WalletAddress);

            if (paymentRequest == null)
                throw new PaymentRequestNotFoundException(cmd.WalletAddress);

            IOrder actualOrder = await _orderService.GetActualAsync(paymentRequest.Id, DateTime.UtcNow);

            if (actualOrder == null)
            {
                actualOrder = await _orderService.GetLatestOrCreateAsync(paymentRequest);

                _log.Warning("New order has been created during deposit transfer validation", context: cmd.ToDetails());
            }

            ValidationResultModel validationResult;

            try
            {
                validationResult  = await _payTransferValidationClient.Api.ValidateAsync(
                    new ValidationContextModel
                    {
                        MerchantId = paymentRequest.MerchantId,
                        AssetId = paymentRequest.PaymentAssetId,
                        TransferAmount = cmd.TransferAmount,
                        ExpectedAmount = actualOrder.PaymentAmount
                    });
            }
            catch (ClientApiException e) when (e.HttpStatusCode == HttpStatusCode.NotFound)
            {
                _log.Error(e, $"Probably, something is wrong with merchant validation configuration: {e.Message}",
                    $"MerchantId = {paymentRequest.MerchantId}");

                return false;
            }

            foreach (var algorithmValidationResultModel in validationResult.Results)
            {
                if (!algorithmValidationResultModel.IsSuccess)
                {
                    _log.ErrorWithDetails("Deposit transfer validation failed",
                        context: new
                        {
                            cmd.WalletAddress,
                            cmd.Blockchain,
                            cmd.TransferAmount,
                            algorithmValidationResultModel.Rule,
                            algorithmValidationResultModel.Error
                        });
                }
            }

            return validationResult.Results.All(x => x.IsSuccess);
        }
    }
}
