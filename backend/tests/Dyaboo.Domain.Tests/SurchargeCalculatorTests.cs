using Dyaboo.Domain.Enums;
using Dyaboo.Domain.Services;

namespace Dyaboo.Domain.Tests;

public class SurchargeCalculatorTests
{
    [Theory]
    [InlineData(2025, 6, 30, 0.75)]  // antes de la Ley 2466
    [InlineData(2025, 7, 1,  0.80)]
    [InlineData(2026, 6, 30, 0.80)]
    [InlineData(2026, 7, 1,  0.90)]
    [InlineData(2027, 6, 30, 0.90)]
    [InlineData(2027, 7, 1,  1.00)]
    public void GetSundayHolidayPercent_EsProgresivoSegunFecha(int y, int m, int d, decimal expected)
    {
        Assert.Equal(expected, SurchargeCalculator.GetSundayHolidayPercent(new DateOnly(y, m, d)));
    }

    [Theory]
    [InlineData(OvertimeType.ExtraDiurna,     0.25)]
    [InlineData(OvertimeType.ExtraNocturna,   0.75)]
    [InlineData(OvertimeType.RecargoNocturno, 0.35)]
    public void GetSurchargePercent_TiposFijos(OvertimeType type, decimal expected)
    {
        Assert.Equal(expected, SurchargeCalculator.GetSurchargePercent(type, new DateOnly(2026, 7, 2)));
    }

    [Fact]
    public void GetSurchargePercent_AcumulaExtraConDominical()
    {
        var fecha = new DateOnly(2026, 7, 2); // dominical vigente: 90%
        Assert.Equal(1.15m, SurchargeCalculator.GetSurchargePercent(OvertimeType.ExtraDiurnaDominical, fecha));
        Assert.Equal(1.65m, SurchargeCalculator.GetSurchargePercent(OvertimeType.ExtraNocturnaDominical, fecha));
    }

    [Fact]
    public void CalculateAmount_ExtraPagaHoraCompletaMasRecargo()
    {
        // 2h extra diurna a $10.000/h → 2 × 10.000 × 1,25 = 25.000
        var amount = SurchargeCalculator.CalculateAmount(
            OvertimeType.ExtraDiurna, new DateOnly(2026, 7, 2), 2m, 10_000m);
        Assert.Equal(25_000m, amount);
    }

    [Fact]
    public void CalculateAmount_RecargoPagaSoloElRecargo()
    {
        // 8h de recargo nocturno a $10.000/h → 8 × 10.000 × 0,35 = 28.000
        var amount = SurchargeCalculator.CalculateAmount(
            OvertimeType.RecargoNocturno, new DateOnly(2026, 7, 2), 8m, 10_000m);
        Assert.Equal(28_000m, amount);

        // 8h dominicales el 2026-07-05 → 8 × 10.000 × 0,90 = 72.000
        var dominical = SurchargeCalculator.CalculateAmount(
            OvertimeType.DominicalFestivo, new DateOnly(2026, 7, 5), 8m, 10_000m);
        Assert.Equal(72_000m, dominical);
    }

    [Fact]
    public void CalculateAmount_MismaNovedadCambiaSegunFecha()
    {
        // Extra diurna dominical: 28-jun-2026 (80%) vs 5-jul-2026 (90%)
        var junio = SurchargeCalculator.CalculateAmount(
            OvertimeType.ExtraDiurnaDominical, new DateOnly(2026, 6, 28), 2m, 10_000m);
        var julio = SurchargeCalculator.CalculateAmount(
            OvertimeType.ExtraDiurnaDominical, new DateOnly(2026, 7, 5), 2m, 10_000m);

        Assert.Equal(41_000m, junio); // 2 × 10.000 × (1 + 0,25 + 0,80)
        Assert.Equal(43_000m, julio); // 2 × 10.000 × (1 + 0,25 + 0,90)
    }
}
