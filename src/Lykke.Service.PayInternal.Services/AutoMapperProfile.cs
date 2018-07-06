using System;
using AutoMapper;
using Common;
using Lykke.Service.EthereumCore.Client.Models;
using Lykke.Service.MarketProfile.Client.Models;
using Lykke.Service.PayCallback.Client.InvoiceConfirmation;
using Lykke.Service.PayInternal.AzureRepositories;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.AssetPair;
using Lykke.Service.PayInternal.Core.Domain.Confirmations;
using Lykke.Service.PayInternal.Core.Domain.Groups;
using Lykke.Service.PayInternal.Core.Domain.History;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Domain.SupervisorMembership;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Transaction.Ethereum;
using Lykke.Service.PayInternal.Core.Domain.Transaction.Ethereum.Common;
using Lykke.Service.PayInternal.Core.Domain.Transaction.Ethereum.Context;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Services.Domain;
using Lykke.Service.PayInternal.Services.Mapping;
using DateTimeUtils = Lykke.Service.PayInternal.Core.DateTimeUtils;

namespace Lykke.Service.PayInternal.Services
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<IPaymentRequestTransaction, PaymentRequestRefundTransaction>(MemberList.Destination)
                .ForMember(dest => dest.Hash, opt => opt.MapFrom(src => src.TransactionId))
                .ForMember(dest => dest.NumberOfConfirmations, opt => opt.MapFrom(src => src.Confirmations))
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.CreatedOn));

            CreateMap<IPaymentRequestTransaction, TransferCommand>(MemberList.Destination)
                .ForMember(dest => dest.Amounts, opt => opt.ResolveUsing<RefundAmountResolver>());

            CreateMap<ICreateLykkeTransactionRequest, CreateLykkeTransactionCommand>(MemberList.Destination)
                .ForMember(dest => dest.Type,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Type = (TransactionType) resContext.Items["TransactionType"]));

            CreateMap<IUpdateTransactionRequest, UpdateTransactionCommand>(MemberList.Destination)
                .ForMember(dest => dest.WalletAddress,
                    opt => opt.ResolveUsing<VirtualAddressResolver, string>(src => src.WalletAddress));

            CreateMap<TransferAmount, TransferFromDepositRequest>(MemberList.Destination)
                .ForMember(dest => dest.DepositAddress, opt => opt.MapFrom(src => src.Source))
                .ForMember(dest => dest.DestinationAddress, opt => opt.MapFrom(src => src.Destination))
                .ForMember(dest => dest.TokenAddress,
                    opt => opt.ResolveUsing((src, dest, destMemeber, resContext) =>
                        dest.TokenAddress = (string) resContext.Items["TokenAddress"]));

            CreateMap<TransferAmount, AirlinesTransferFromDepositRequest>(MemberList.Destination)
                .ForMember(dest => dest.DepositAddress, opt => opt.MapFrom(src => src.Source))
                .ForMember(dest => dest.DestinationAddress, opt => opt.MapFrom(src => src.Destination))
                .ForMember(dest => dest.TokenAmount,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) => dest.TokenAmount =
                        src.Amount?.ToContract(
                            (int) resContext.Items["AssetMultiplier"],
                            (int) resContext.Items["AssetAccuracy"])))
                .ForMember(dest => dest.TokenAddress,
                    opt => opt.ResolveUsing((src, dest, destMemeber, resContext) =>
                        dest.TokenAddress = (string) resContext.Items["TokenAddress"]));

            CreateMap<ICreateTransactionCommand, PaymentRequestTransaction>(MemberList.Source)
                .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.Hash))
                .ForMember(dest => dest.PaymentRequestId,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.PaymentRequestId = ((IPaymentRequest) resContext.Items["PaymentRequest"])?.Id))
                .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.DueDate,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        src.DueDate ?? ((IPaymentRequest) resContext.Items["PaymentRequest"])?.DueDate));

            CreateMap<BlockchainTransactionResult, TransferTransaction>(MemberList.Destination);

            CreateMap<TransferTransaction, TransferTransactionResult>(MemberList.Destination);

            CreateMap<ITransfer, TransferResult>(MemberList.Destination)
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.CreatedOn));

            CreateMap<ICreateTransactionCommand, UpdateTransactionCommand>(MemberList.Destination);

            CreateMap<IPaymentRequest, RequestMarkup>(MemberList.Destination)
                .ForMember(dest => dest.Percent, opt => opt.MapFrom(src => src.MarkupPercent))
                .ForMember(dest => dest.Pips, opt => opt.MapFrom(src => src.MarkupPips))
                .ForMember(dest => dest.FixedFee, opt => opt.MapFrom(src => src.MarkupFixedFee));

            CreateMap<IMerchantsSupervisorMembership, MerchantGroup>(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.DisplayName, opt => opt.Ignore())
                .ForMember(dest => dest.MerchantGroupUse, opt => opt.UseValue(MerchantGroupUse.Supervising))
                .ForMember(dest => dest.Merchants, opt => opt.MapFrom(src => string.Join(Constants.Separator, src.Merchants)))
                .ForMember(dest => dest.OwnerId, opt => opt.MapFrom(src => src.MerchantId));

            CreateMap<AddAssetPairRateCommand, AssetPairRate>(MemberList.Destination)
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<AssetPairModel, AssetPairRate>(MemberList.Destination)
                .ForMember(dest => dest.CreatedOn,
                    opt => opt.MapFrom(src => DateTimeUtils.Largest(src.AskPriceTimestamp, src.BidPriceTimestamp)))
                .ForMember(dest => dest.BaseAssetId,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.BaseAssetId = (string) resContext.Items["BaseAssetId"]))
                .ForMember(dest => dest.QuotingAssetId,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.QuotingAssetId = (string) resContext.Items["QuotingAssetId"]));

            CreateMap<CashoutConfirmationCommand, InvoiceConfirmation>(MemberList.Destination)
                .ForMember(dest => dest.CashOut, opt => opt.MapFrom(src => new CashOut
                {
                    Amount = src.Amount,
                    Currency = src.Asset
                }))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.EmployeeEmail))
                .ForMember(dest => dest.BlockchainHash, opt => opt.MapFrom(src => src.TransactionHash))
                .ForMember(dest => dest.SettledInBlockchainDateTime, opt => opt.MapFrom(src => src.SettlementDateTime))
                .ForMember(dest => dest.InvoiceList, opt => opt.Ignore());

            CreateMap<IPaymentRequestTransaction, CashoutConfirmationCommand>(MemberList.Destination)
                .ForMember(dest => dest.TransactionHash, opt => opt.MapFrom(src => src.TransactionId))
                .ForMember(dest => dest.SettlementDateTime, opt => opt.MapFrom(src => src.FirstSeen ?? src.CreatedOn))
                .ForMember(dest => dest.Asset,
                    opt => opt.MapFrom(src =>
                        src.ContextData.DeserializeJson<CashoutTransactionContext>().DesiredAsset))
                .ForMember(dest => dest.EmployeeEmail,
                    opt => opt.MapFrom(
                        src => src.ContextData.DeserializeJson<CashoutTransactionContext>().EmployeeEmail));

            CreateEthereumPaymentMaps();

            CreateHistoryMaps();
        }

        private void CreateEthereumPaymentMaps()
        {
            // incoming ethereum payment
            CreateMap<RegisterInTxCommand, CreateTransactionCommand>(MemberList.Destination)
                .ForMember(dest => dest.DueDate, opt => opt.Ignore())
                .ForMember(dest => dest.WalletAddress,
                    opt => opt.ResolveUsing<VirtualAddressResolver, string>(src => src.ToAddress))
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int) resContext.Items["Confirmations"]))
                .ForMember(dest => dest.SourceWalletAddresses, opt => opt.MapFrom(src => new[] {src.FromAddress}))
                .ForMember(dest => dest.TransferId, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.UseValue(TransactionType.Payment))
                .ForMember(dest => dest.ContextData, opt => opt.Ignore());

            CreateMap<RegisterInTxCommand, RegisterCashinTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int) resContext.Items["Confirmations"]))
                .ForMember(dest => dest.SourceWalletAddresses, opt => opt.MapFrom(src => new[] {src.FromAddress}))
                .ForMember(dest => dest.Type, opt => opt.UseValue(TransactionType.CashIn))
                .ForMember(dest => dest.TransferId, opt => opt.Ignore())
                .ForMember(dest => dest.DueDate, opt => opt.Ignore())
                .ForMember(dest => dest.WalletAddress, opt => opt.Ignore())
                .ForMember(dest => dest.ContextData, opt => opt.Ignore());

            // incoming ethereum payment update
            CreateMap<RegisterInTxCommand, UpdateTransactionCommand>(MemberList.Destination)
                .ForMember(dest => dest.WalletAddress,
                    opt => opt.ResolveUsing<VirtualAddressResolver, string>(src => src.ToAddress))
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int)resContext.Items["Confirmations"]));

            CreateMap<RegisterInTxCommand, UpdateExchangeInTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int)resContext.Items["Confirmations"]))
                .ForMember(dest => dest.WalletAddress, opt => opt.Ignore());

            CreateMap<RegisterInTxCommand, UpdateSettlementInTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int)resContext.Items["Confirmations"]))
                .ForMember(dest => dest.WalletAddress,
                    opt => opt.ResolveUsing<VirtualAddressResolver, string>(src => src.FromAddress));

            // outgoing ethereum payment update
            CreateMap<UpdateOutTxCommand, UpdatePaymentOutTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.WalletAddress,
                    opt => opt.ResolveUsing<VirtualAddressResolver, string>(src => src.ToAddress))
                .ForMember(dest => dest.Confirmations, opt => opt.UseValue(0));

            CreateMap<UpdateOutTxCommand, UpdateRefundOutTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Confirmations, opt => opt.UseValue(0))
                .ForMember(dest => dest.WalletAddress,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.WalletAddress = (string) resContext.Items["WalletAddress"]));

            CreateMap<UpdateOutTxCommand, UpdateExchangeOutTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Confirmations, opt => opt.UseValue(0))
                .ForMember(dest => dest.WalletAddress, opt => opt.Ignore());

            CreateMap<UpdateOutTxCommand, UpdateSettlementOutTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Confirmations, opt => opt.UseValue(0))
                .ForMember(dest => dest.WalletAddress,
                    opt => opt.ResolveUsing<VirtualAddressResolver, string>(src => src.FromAddress));

            CreateMap<UpdateOutTxCommand, UpdateCashoutTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Confirmations, opt => opt.UseValue(0))
                .ForMember(dest => dest.WalletAddress, opt => opt.Ignore());

            // outgoing ethereum payment complete
            CreateMap<CompleteOutTxCommand, CompletePaymentOutTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int)resContext.Items["Confirmations"]))
                .ForMember(dest => dest.WalletAddress,
                    opt => opt.ResolveUsing<VirtualAddressResolver, string>(src => src.ToAddress));

            // outgoing ethereum refund complete
            CreateMap<CompleteOutTxCommand, CompleteRefundOutTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int)resContext.Items["Confirmations"]))
                .ForMember(dest => dest.WalletAddress,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.WalletAddress = (string)resContext.Items["WalletAddress"]));

            // outgoing ethereim exchange complete
            CreateMap<CompleteOutTxCommand, CompleteExchangeOutTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int)resContext.Items["Confirmations"]))
                .ForMember(dest => dest.WalletAddress, opt => opt.Ignore());

            // outgoing ethereim cashout complete
            CreateMap<CompleteOutTxCommand, CompleteCashoutTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int) resContext.Items["Confirmations"]))
                .ForMember(dest => dest.WalletAddress, opt => opt.Ignore());

            // outgoing ethereim settlement complete
            CreateMap<CompleteOutTxCommand, CompleteSettlementOutTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int)resContext.Items["Confirmations"]))
                .ForMember(dest => dest.WalletAddress, opt => opt.Ignore());

            // outgoing ethereum payment not enough funds
            CreateMap<NotEnoughFundsOutTxCommand, FailPaymentOutTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Amount, opt => opt.UseValue(0))
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int) resContext.Items["Confirmations"]))
                .ForMember(dest => dest.BlockId, opt => opt.Ignore())
                .ForMember(dest => dest.FirstSeen, opt => opt.Ignore())
                .ForMember(dest => dest.Hash, opt => opt.Ignore())
                .ForMember(dest => dest.WalletAddress,
                    opt => opt.ResolveUsing<VirtualAddressResolver, string>(src => src.ToAddress));

            // outgoing ethereum cashout not enough funds
            CreateMap<NotEnoughFundsOutTxCommand, FailCashoutTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Amount, opt => opt.UseValue(0))
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int) resContext.Items["Confirmations"]))
                .ForMember(dest => dest.BlockId, opt => opt.Ignore())
                .ForMember(dest => dest.FirstSeen, opt => opt.Ignore())
                .ForMember(dest => dest.Hash, opt => opt.Ignore())
                .ForMember(dest => dest.WalletAddress, opt => opt.Ignore());

            // outgoing ethereum exchange not enough funds
            CreateMap<NotEnoughFundsOutTxCommand, FailExchangeOutTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Amount, opt => opt.UseValue(0))
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int) resContext.Items["Confirmations"]))
                .ForMember(dest => dest.BlockId, opt => opt.Ignore())
                .ForMember(dest => dest.FirstSeen, opt => opt.Ignore())
                .ForMember(dest => dest.Hash, opt => opt.Ignore())
                .ForMember(dest => dest.WalletAddress, opt => opt.Ignore());
        }

        private void CreateHistoryMaps()
        {
            CreateMap<CompleteOutTxCommand, WalletHistoryCommand>(MemberList.Destination)
               .ForMember(dest => dest.TransactionHash, opt => opt.MapFrom(src => src.Hash))
               .ForMember(dest => dest.WalletAddress, opt => opt.MapFrom(src => src.FromAddress))
               .ForMember(dest => dest.AssetId,
                   opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                       dest.AssetId = (string)resContext.Items["AssetId"]));

            // WalletHistoryCashoutCommand is being mapped from two maps
            // Map 1
            CreateMap<CompleteOutTxCommand, WalletHistoryCashoutCommand>(MemberList.Destination)
                .ForMember(dest => dest.TransactionHash, opt => opt.MapFrom(src => src.Hash))
                .ForMember(dest => dest.WalletAddress, opt => opt.MapFrom(src => src.FromAddress))
                .ForMember(dest => dest.AssetId, opt => opt.Ignore())
                .ForMember(dest => dest.DesiredAsset, opt => opt.Ignore())
                .ForMember(dest => dest.EmployeeEmail, opt => opt.Ignore());

            // Map2
            CreateMap<IPaymentRequestTransaction, WalletHistoryCashoutCommand>(MemberList.Destination)
                .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.DesiredAsset,
                    opt => opt.MapFrom(src =>
                        src.ContextData.DeserializeJson<CashoutTransactionContext>().DesiredAsset))
                .ForMember(dest => dest.EmployeeEmail,
                    opt => opt.MapFrom(
                        src => src.ContextData.DeserializeJson<CashoutTransactionContext>().EmployeeEmail))
                .ForMember(dest => dest.TransactionHash, opt => opt.Ignore())
                .ForMember(dest => dest.WalletAddress, opt => opt.Ignore());

            CreateMap<RegisterInTxCommand, WalletHistoryCommand>(MemberList.Destination)
                .ForMember(dest => dest.TransactionHash, opt => opt.MapFrom(src => src.Hash))
                .ForMember(dest => dest.WalletAddress, opt => opt.MapFrom(src => src.ToAddress));
        }
    }
}
