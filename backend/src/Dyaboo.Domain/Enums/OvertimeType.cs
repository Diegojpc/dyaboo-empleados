namespace Dyaboo.Domain.Enums;

/// <summary>
/// Tipos de novedad de horas según el RIT (Arts. 33-34) y la Ley 2466/2025.
/// Los tipos Extra* pagan la hora completa más el recargo; los tipos de solo
/// recargo (RecargoNocturno, DominicalFestivo) pagan únicamente el recargo
/// porque la hora base ya está incluida en el salario.
/// </summary>
public enum OvertimeType
{
    ExtraDiurna = 1,
    ExtraNocturna,
    RecargoNocturno,
    DominicalFestivo,
    ExtraDiurnaDominical,
    ExtraNocturnaDominical
}
