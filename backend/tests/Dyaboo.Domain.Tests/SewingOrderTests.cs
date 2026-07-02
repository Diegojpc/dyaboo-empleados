using Dyaboo.Domain.Entities;
using Dyaboo.Domain.Enums;

namespace Dyaboo.Domain.Tests;

public class SewingOrderTests
{
    private static SewingOrder NewOrder() =>
        SewingOrder.Create("OCF-20260701-TEST01", Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void Create_EmpiezaAsignada()
    {
        var order = NewOrder();

        Assert.Equal(SewingOrderStatus.Assigned, order.Status);
        Assert.Null(order.ReceivedAt);
    }

    [Fact]
    public void Receive_SumaNoCoincide_Lanza()
    {
        var order = NewOrder();
        var item = order.AddItem(Guid.NewGuid(), 100);

        Assert.Throws<InvalidOperationException>(() =>
            order.Receive(new Dictionary<Guid, (int, int)> { [item.Id] = (90, 5) }));
    }

    [Fact]
    public void Receive_CantidadesNegativas_Lanza()
    {
        var order = NewOrder();
        var item = order.AddItem(Guid.NewGuid(), 100);

        Assert.Throws<ArgumentException>(() =>
            order.Receive(new Dictionary<Guid, (int, int)> { [item.Id] = (105, -5) }));
    }

    [Fact]
    public void Receive_Correcto_RegistraCalidadYCierra()
    {
        var order = NewOrder();
        var item1 = order.AddItem(Guid.NewGuid(), 100);
        var item2 = order.AddItem(Guid.NewGuid(), 40);

        order.Receive(new Dictionary<Guid, (int, int)>
        {
            [item1.Id] = (93, 7),
            [item2.Id] = (40, 0),
        });

        Assert.Equal(SewingOrderStatus.Received, order.Status);
        Assert.NotNull(order.ReceivedAt);
        Assert.Equal(140, order.TotalSent);
        Assert.Equal(133, order.TotalApproved);
        Assert.Equal(7, order.TotalRejected);
    }

    [Fact]
    public void Receive_DosVeces_Lanza()
    {
        var order = NewOrder();
        var item = order.AddItem(Guid.NewGuid(), 10);
        var reception = new Dictionary<Guid, (int, int)> { [item.Id] = (10, 0) };
        order.Receive(reception);

        Assert.Throws<InvalidOperationException>(() => order.Receive(reception));
    }

    [Fact]
    public void AddItem_TrasRecibir_Lanza()
    {
        var order = NewOrder();
        var item = order.AddItem(Guid.NewGuid(), 10);
        order.Receive(new Dictionary<Guid, (int, int)> { [item.Id] = (10, 0) });

        Assert.Throws<InvalidOperationException>(() => order.AddItem(Guid.NewGuid(), 5));
    }
}
