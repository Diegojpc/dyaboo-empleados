using Dyaboo.Domain.Entities;
using Dyaboo.Domain.Enums;

namespace Dyaboo.Domain.Tests;

public class CuttingOrderTests
{
    private static CuttingOrder NewOrder() =>
        CuttingOrder.Create("OC-20260701-TEST01", Guid.NewGuid());

    [Fact]
    public void Create_EmpiezaEnProceso()
    {
        var order = NewOrder();

        Assert.Equal(CuttingOrderStatus.InProgress, order.Status);
        Assert.Null(order.CompletedAt);
        Assert.Empty(order.Items);
    }

    [Fact]
    public void AddItem_CantidadInvalida_Lanza()
    {
        var order = NewOrder();

        Assert.Throws<ArgumentException>(() => order.AddItem(Guid.NewGuid(), 0));
        Assert.Throws<ArgumentException>(() => order.AddItem(Guid.NewGuid(), -5));
    }

    [Fact]
    public void Complete_RegistraCortadasYCierra()
    {
        var order = NewOrder();
        var item1 = order.AddItem(Guid.NewGuid(), 100);
        var item2 = order.AddItem(Guid.NewGuid(), 50);

        order.Complete(new Dictionary<Guid, int>
        {
            [item1.Id] = 98,   // mermas de tela
            [item2.Id] = 50,
        });

        Assert.Equal(CuttingOrderStatus.Completed, order.Status);
        Assert.NotNull(order.CompletedAt);
        Assert.Equal(98, item1.CutQuantity);
        Assert.Equal(150, order.TotalPlannedUnits);
        Assert.Equal(148, order.TotalCutUnits);
    }

    [Fact]
    public void Complete_SinItems_Lanza()
    {
        var order = NewOrder();

        Assert.Throws<InvalidOperationException>(() =>
            order.Complete(new Dictionary<Guid, int>()));
    }

    [Fact]
    public void Complete_DiccionarioIncompleto_Lanza()
    {
        var order = NewOrder();
        var item1 = order.AddItem(Guid.NewGuid(), 100);
        order.AddItem(Guid.NewGuid(), 50);

        Assert.Throws<InvalidOperationException>(() =>
            order.Complete(new Dictionary<Guid, int> { [item1.Id] = 100 }));
    }

    [Fact]
    public void Complete_DosVeces_Lanza()
    {
        var order = NewOrder();
        var item = order.AddItem(Guid.NewGuid(), 10);
        var quantities = new Dictionary<Guid, int> { [item.Id] = 10 };
        order.Complete(quantities);

        Assert.Throws<InvalidOperationException>(() => order.Complete(quantities));
    }

    [Fact]
    public void AddItem_TrasCompletar_Lanza()
    {
        var order = NewOrder();
        var item = order.AddItem(Guid.NewGuid(), 10);
        order.Complete(new Dictionary<Guid, int> { [item.Id] = 10 });

        Assert.Throws<InvalidOperationException>(() => order.AddItem(Guid.NewGuid(), 5));
    }
}
