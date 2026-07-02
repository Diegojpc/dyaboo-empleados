using Dyaboo.Application.Features.Produccion.GetSewingOrders;
using MediatR;

namespace Dyaboo.Application.Features.Produccion.ReceiveSewingOrder;

public record ReceiveSewingOrderCommand(
    Guid SewingOrderId,
    IReadOnlyList<ReceptionInput> Items) : IRequest<SewingOrderResult>;

public record ReceptionInput(Guid ItemId, int Approved, int Rejected);
