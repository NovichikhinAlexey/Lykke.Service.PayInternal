using System;
using AutoMapper;
using Lykke.Service.EthereumCore.Client.Models;
using Lykke.Service.MarketProfile.Client.Models;
using Lykke.Service.PayInternal.AzureRepositories;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.AssetPair;
using Lykke.Service.PayInternal.Core.Domain.Groups;
using Lykke.Service.PayInternal.Core.Domain.History;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Domain.SupervisorMembership;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Services.Domain;
using Lykke.Service.PayInternal.Services.Mapping;
using UpdateRefundEthOutgoingTxCommand = Lykke.Service.PayInternal.Services.Domain.UpdateRefundEthOutgoingTxCommand;

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

            CreateEthereumPaymentMaps();
        }

        private void CreateEthereumPaymentMaps()
        {
            // incoming ethereum payment
            CreateMap<RegisterEthInboundTxCommand, CreateTransactionCommand>(MemberList.Destination)
                .ForMember(dest => dest.DueDate, opt => opt.Ignore())
                .ForMember(dest => dest.WalletAddress,
                    opt => opt.ResolveUsing<VirtualAddressResolver, string>(src => src.ToAddress))
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int)resContext.Items["Confirmations"]))
                .ForMember(dest => dest.SourceWalletAddresses, opt => opt.MapFrom(src => new[] { src.FromAddress }))
                .ForMember(dest => dest.TransferId, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.UseValue(TransactionType.Payment));

            CreateMap<RegisterEthInboundTxCommand, CreateCashInTransactionCommand>(MemberList.Destination)
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int)resContext.Items["Confirmations"]))
                .ForMember(dest => dest.SourceWalletAddresses, opt => opt.MapFrom(src => new[] { src.FromAddress }))
                .ForMember(dest => dest.Type, opt => opt.UseValue(TransactionType.CashIn))
                .ForMember(dest => dest.TransferId, opt => opt.Ignore())
                .ForMember(dest => dest.DueDate, opt => opt.Ignore())
                .ForMember(dest => dest.WalletAddress, opt => opt.Ignore());

            // incoming ethereum payment update
            CreateMap<RegisterEthInboundTxCommand, UpdateTransactionCommand>(MemberList.Destination)
                .ForMember(dest => dest.WalletAddress,
                    opt => opt.ResolveUsing<VirtualAddressResolver, string>(src => src.ToAddress))
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int)resContext.Items["Confirmations"]));

            CreateMap<RegisterEthInboundTxCommand, UpdateExchangeEthInboundTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int)resContext.Items["Confirmations"]))
                .ForMember(dest => dest.WalletAddress, opt => opt.Ignore());

            CreateMap<RegisterEthInboundTxCommand, UpdateSettlementEthInboundTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int)resContext.Items["Confirmations"]))
                .ForMember(dest => dest.WalletAddress,
                    opt => opt.ResolveUsing<VirtualAddressResolver, string>(src => src.FromAddress));

            // outgoing ethereum payment update
            CreateMap<UpdateEthOutgoingTxCommand, UpdatePaymentEthOutgoingTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.WalletAddress,
                    opt => opt.ResolveUsing<VirtualAddressResolver, string>(src => src.ToAddress))
                .ForMember(dest => dest.Confirmations, opt => opt.UseValue(0));

            CreateMap<UpdateEthOutgoingTxCommand, UpdateRefundEthOutgoingTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Confirmations, opt => opt.UseValue(0))
                .ForMember(dest => dest.WalletAddress,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.WalletAddress = (string)resContext.Items["WalletAddress"]));

            CreateMap<UpdateEthOutgoingTxCommand, UpdateExchangeEthOutgoingTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Confirmations, opt => opt.UseValue(0))
                .ForMember(dest => dest.WalletAddress, opt => opt.Ignore());

            CreateMap<UpdateEthOutgoingTxCommand, UpdateSettlementEthOutgoingTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Confirmations, opt => opt.UseValue(0))
                .ForMember(dest => dest.WalletAddress,
                    opt => opt.ResolveUsing<VirtualAddressResolver, string>(src => src.FromAddress));

            CreateMap<UpdateEthOutgoingTxCommand, UpdateCashoutEthOutgoingTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Confirmations, opt => opt.UseValue(0))
                .ForMember(dest => dest.WalletAddress, opt => opt.Ignore());

            // outgoing ethereum payment complete
            CreateMap<CompleteEthOutgoingTxCommand, CompletePaymentEthOutgoingTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int)resContext.Items["Confirmations"]))
                .ForMember(dest => dest.WalletAddress,
                    opt => opt.ResolveUsing<VirtualAddressResolver, string>(src => src.ToAddress));

            // outgoing ethereum refund complete
            CreateMap<CompleteEthOutgoingTxCommand, CompleteRefundEthOutgoingTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int)resContext.Items["Confirmations"]))
                .ForMember(dest => dest.WalletAddress,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.WalletAddress = (string)resContext.Items["WalletAddress"]));

            // outgoing ethereim exchange complete
            CreateMap<CompleteEthOutgoingTxCommand, CompleteExchangeEthOutgoingTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int)resContext.Items["Confirmations"]))
                .ForMember(dest => dest.WalletAddress, opt => opt.Ignore());

            // outgoing ethereim cashout complete
            CreateMap<CompleteEthOutgoingTxCommand, CompleteCashoutEthOutgoingTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int) resContext.Items["Confirmations"]))
                .ForMember(dest => dest.WalletAddress, opt => opt.Ignore());

            // outgoing ethereim settlement complete
            CreateMap<CompleteEthOutgoingTxCommand, CompleteSettlementEthOutgoingTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int)resContext.Items["Confirmations"]))
                .ForMember(dest => dest.WalletAddress, opt => opt.Ignore());

            CreateMap<CompleteEthOutgoingTxCommand, WalletHistoryCommand>(MemberList.Destination)
                .ForMember(dest => dest.TransactionHash, opt => opt.MapFrom(src => src.Hash))
                .ForMember(dest => dest.WalletAddress, opt => opt.MapFrom(src => src.FromAddress))
                .ForMember(dest => dest.AssetId,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.AssetId = (string)resContext.Items["AssetId"]));

            CreateMap<RegisterEthInboundTxCommand, WalletHistoryCommand>(MemberList.Destination)
                .ForMember(dest => dest.TransactionHash, opt => opt.MapFrom(src => src.Hash))
                .ForMember(dest => dest.WalletAddress, opt => opt.MapFrom(src => src.ToAddress));

            // outgoing ethereum payment not enough funds
            CreateMap<NotEnoughFundsEthOutgoingTxCommand, FailPaymentEthOutgoingTxCommand>(MemberList.Destination)
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
            CreateMap<NotEnoughFundsEthOutgoingTxCommand, FailCashoutEthOutgoingTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Amount, opt => opt.UseValue(0))
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int) resContext.Items["Confirmations"]))
                .ForMember(dest => dest.BlockId, opt => opt.Ignore())
                .ForMember(dest => dest.FirstSeen, opt => opt.Ignore())
                .ForMember(dest => dest.Hash, opt => opt.Ignore())
                .ForMember(dest => dest.WalletAddress, opt => opt.Ignore());

            // outgoing ethereum exchange not enough funds
            CreateMap<NotEnoughFundsEthOutgoingTxCommand, FailExchangeEthOutgoingTxCommand>(MemberList.Destination)
                .ForMember(dest => dest.Amount, opt => opt.UseValue(0))
                .ForMember(dest => dest.Confirmations,
                    opt => opt.ResolveUsing((src, dest, destMember, resContext) =>
                        dest.Confirmations = (int) resContext.Items["Confirmations"]))
                .ForMember(dest => dest.BlockId, opt => opt.Ignore())
                .ForMember(dest => dest.FirstSeen, opt => opt.Ignore())
                .ForMember(dest => dest.Hash, opt => opt.Ignore())
                .ForMember(dest => dest.WalletAddress, opt => opt.Ignore());
        }
    }
}
