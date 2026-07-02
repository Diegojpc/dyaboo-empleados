using Dyaboo.Domain.Entities;
using Dyaboo.Domain.Enums;

namespace Dyaboo.Domain.Tests;

public class SalesOrderTests
{
    private static SalesOrder NewOrder() =>
        SalesOrder.Create("PED-20260701-TEST01", Guid.NewGuid());

    private static SalesOrder NewConfirmedOrder()
    {
        var order = NewOrder();
        order.AddItem(Guid.NewGuid(), 10, 50_000m);
        order.Confirm();
        return order;
    }

    [Fact]
    public void Confirm_SinItems_Lanza()
    {
        var order = NewOrder();

        Assert.Throws<InvalidOperationException>(order.Confirm);
    }

    [Fact]
    public void CaminoFeliz_DraftConfirmedDispatchedDelivered()
    {
        var order = NewOrder();
        order.AddItem(Guid.NewGuid(), 10, 50_000m);

        order.Confirm();
        Assert.Equal(SalesOrderStatus.Confirmed, order.Status);
        Assert.NotNull(order.ConfirmedAt);

        order.MarkDispatched();
        Assert.Equal(SalesOrderStatus.Dispatched, order.Status);
        Assert.NotNull(order.DispatchedAt);

        order.MarkDelivered();
        Assert.Equal(SalesOrderStatus.Delivered, order.Status);
        Assert.NotNull(order.DeliveredAt);
    }

    [Fact]
    public void MarkDispatched_DesdeDraft_Lanza()
    {
        var order = NewOrder();
        order.AddItem(Guid.NewGuid(), 1, 100m);

        Assert.Throws<InvalidOperationException>(order.MarkDispatched);
    }

    [Fact]
    public void MarkDelivered_DesdeConfirmed_Lanza()
    {
        var order = NewConfirmedOrder();

        Assert.Throws<InvalidOperationException>(order.MarkDelivered);
    }

    [Fact]
    public void Cancel_TrasDespachar_Lanza()
    {
        var order = NewConfirmedOrder();
        order.MarkDispatched();

        Assert.Throws<InvalidOperationException>(order.Cancel);
    }

    [Fact]
    public void Cancel_EnBorradorOConfirmado_Funciona()
    {
        var draft = NewOrder();
        draft.Cancel();
        Assert.Equal(SalesOrderStatus.Cancelled, draft.Status);

        var confirmed = NewConfirmedOrder();
        confirmed.Cancel();
        Assert.Equal(SalesOrderStatus.Cancelled, confirmed.Status);
    }

    [Fact]
    public void AddItem_TrasConfirmar_Lanza()
    {
        var order = NewConfirmedOrder();

        Assert.Throws<InvalidOperationException>(() => order.AddItem(Guid.NewGuid(), 1, 100m));
    }

    [Fact]
    public void Totales_CalculadosSobreItems()
    {
        var order = NewOrder();
        var item = order.AddItem(Guid.NewGuid(), 10, 45_000m);
        order.AddItem(Guid.NewGuid(), 5, 60_000m);

        Assert.Equal(450_000m, item.LineTotal);
        Assert.Equal(750_000m, order.Total);
        Assert.Equal(15, order.TotalUnits);
    }

    [Fact]
    public void AddItem_PrecioNegativo_Lanza()
    {
        var order = NewOrder();

        Assert.Throws<ArgumentException>(() => order.AddItem(Guid.NewGuid(), 1, -1m));
    }
}
