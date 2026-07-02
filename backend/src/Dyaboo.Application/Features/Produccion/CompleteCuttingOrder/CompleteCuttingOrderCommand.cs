using Dyaboo.Application.Features.Produccion.GetCuttingOrders;
using MediatR;

namespace Dyaboo.Application.Features.Produccion.CompleteCuttingOrder;

public record CompleteCuttingOrderCommand(
    Guid CuttingOrderId,
    IReadOnlyList<CutCompletionInput> Items) : IRequest<CuttingOrderResult>;

public record CutCompletionInput(Guid ItemId, int CutQuantity);
