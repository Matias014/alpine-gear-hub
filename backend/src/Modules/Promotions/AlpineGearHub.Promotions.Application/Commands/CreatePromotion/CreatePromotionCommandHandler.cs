using AlpineGearHub.Promotions.Application.DTOs;
using AlpineGearHub.Promotions.Application.Extensions;
using AlpineGearHub.Promotions.Application.Interfaces;
using AlpineGearHub.Promotions.Domain;
using AlpineGearHub.Promotions.Domain.Entities;
using AlpineGearHub.Promotions.Domain.Exceptions;
using AlpineGearHub.Promotions.Domain.Repositories;
using AlpineGearHub.Promotions.Domain.ValueObjects;
using MediatR;

namespace AlpineGearHub.Promotions.Application.Commands.CreatePromotion;

internal sealed class CreatePromotionCommandHandler(
    IPromotionRepository promotionRepository,
    IPaymentGateway paymentGateway) : IRequestHandler<CreatePromotionCommand, PromotionResponse>
{
    private const string Currency = "EUR";

    public async Task<PromotionResponse> Handle(CreatePromotionCommand request, CancellationToken cancellationToken)
    {
        if (await promotionRepository.HasActivePromotionAsync(request.ListingId, cancellationToken))
            throw new PromotionException("This listing already has an active or pending promotion.");

        var (amount, durationDays) = PromotionPricing.For(request.Tier);
        var promotion = Promotion.Create(request.ListingId, request.Tier, Money.Of(amount, Currency), durationDays);

        var intent = await paymentGateway.CreatePaymentIntentAsync(
            amount,
            Currency,
            new Dictionary<string, string>
            {
                ["promotionId"] = promotion.Id.ToString(),
                ["listingId"] = request.ListingId.ToString(),
            },
            cancellationToken);

        promotion.AttachPaymentIntent(intent.PaymentIntentId);

        await promotionRepository.AddAsync(promotion, cancellationToken);
        await promotionRepository.SaveChangesAsync(cancellationToken);

        return promotion.ToResponse(intent.ClientSecret);
    }
}
