using Dyaboo.Application.Features.Produccion.GetSewingOrders;
using MediatR;

namespace Dyaboo.Application.Features.Produccion.CreateSewingOrder;

public record CreateSewingOrderCommand(
    Guid CuttingOrderId,
    Guid ConfeccionistaId) : IRequest<SewingOrderResult>;
