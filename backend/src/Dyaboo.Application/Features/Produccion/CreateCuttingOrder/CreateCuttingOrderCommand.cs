using Dyaboo.Application.Features.Produccion.GetCuttingOrders;
using MediatR;

namespace Dyaboo.Application.Features.Produccion.CreateCuttingOrder;

public record CreateCuttingOrderCommand(
    Guid ProductReferenceId,
    IReadOnlyList<CutItemInput> Items,
    string? Notes) : IRequest<CuttingOrderResult>;

public record CutItemInput(Guid VariantId, int Quantity);
