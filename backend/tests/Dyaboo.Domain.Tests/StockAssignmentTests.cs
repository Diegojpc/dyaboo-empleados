using Dyaboo.Domain.Entities;

namespace Dyaboo.Domain.Tests;

public class StockAssignmentTests
{
    [Fact]
    public void Create_SaldoIgualACantidadAsignada()
    {
        var assignment = StockAssignment.Create(Guid.NewGuid(), Guid.NewGuid(), 80);

        Assert.Equal(80, assignment.Quantity);
        Assert.Equal(80, assignment.RemainingQuantity);
    }

    [Fact]
    public void Consume_Parcial_DecrementaSaldo()
    {
        var assignment = StockAssignment.Create(Guid.NewGuid(), Guid.NewGuid(), 80);

        var consumed = assignment.Consume(30);

        Assert.Equal(30, consumed);
        Assert.Equal(50, assignment.RemainingQuantity);
        Assert.Equal(80, assignment.Quantity);  // el histórico no cambia
    }

    [Fact]
    public void Consume_MayorAlSaldo_DevuelveSoloElSaldo()
    {
        var assignment = StockAssignment.Create(Guid.NewGuid(), Guid.NewGuid(), 20);

        var consumed = assignment.Consume(50);

        Assert.Equal(20, consumed);
        Assert.Equal(0, assignment.RemainingQuantity);
    }

    [Fact]
    public void Consume_CantidadInvalida_Lanza()
    {
        var assignment = StockAssignment.Create(Guid.NewGuid(), Guid.NewGuid(), 20);

        Assert.Throws<ArgumentException>(() => assignment.Consume(0));
        Assert.Throws<ArgumentException>(() => assignment.Consume(-1));
    }
}
