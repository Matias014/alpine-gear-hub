using AlpineGearHub.Promotions.Application.DTOs;
using MediatR;

namespace AlpineGearHub.Promotions.Application.Queries.GetPromotionById;

public record GetPromotionByIdQuery(Guid PromotionId) : IRequest<PromotionResponse?>;
