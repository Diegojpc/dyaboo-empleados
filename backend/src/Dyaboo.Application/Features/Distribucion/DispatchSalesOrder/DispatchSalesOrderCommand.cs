using MediatR;

namespace Dyaboo.Application.Features.Distribucion.DispatchSalesOrder;

public record DispatchSalesOrderCommand(Guid SalesOrderId) : IRequest<DispatchResult>;

// ── Resultado ──────────────────────────────────────────────────────────────

/// <summary>Resultado del despacho: sirve como lista de picking para bodega.</summary>
public record DispatchResult(
    Guid OrderId,
    string OrderCode,
    int TotalUnitsDispatched,
    IReadOnlyList<PickingLine> PickingLines);

public record PickingLine(
    string LocationCode,
    string Sku,
    int Quantity);
