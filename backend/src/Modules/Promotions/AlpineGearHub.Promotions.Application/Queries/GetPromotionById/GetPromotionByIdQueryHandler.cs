using AlpineGearHub.Promotions.Application.DTOs;
using AlpineGearHub.Promotions.Application.Extensions;
using AlpineGearHub.Promotions.Domain.Repositories;
using MediatR;

namespace AlpineGearHub.Promotions.Application.Queries.GetPromotionById;

internal sealed class GetPromotionByIdQueryHandler(IPromotionRepository promotionRepository)
    : IRequestHandler<GetPromotionByIdQuery, PromotionResponse?>
{
    public async Task<PromotionResponse?> Handle(GetPromotionByIdQuery request, CancellationToken cancellationToken)
    {
        var promotion = await promotionRepository.GetByIdAsync(request.PromotionId, cancellationToken);
        return promotion?.ToResponse();
    }
}
