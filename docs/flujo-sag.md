# Módulo SAG — Sistema de Administración y Gestión

## Responsabilidad

Calcula los **costos de producción** por orden y mantiene el **inventario financiero** con el valor de stock actual por variante.

## Flujo: Calcular costo de producción

```mermaid
sequenceDiagram
    actor U as Líder Producción
    participant FE as Frontend (SAG page)
    participant API as SagController
    participant HDL as CalculateProductionCostHandler
    participant DB as PostgreSQL

    U->>FE: Selecciona referencia,\ningresa cantidades y costo\nde mano de obra por variante,\n% overhead
    FE->>API: POST /api/sag/production-costs\n[Authorize]\nBody: CalculateProductionCostCommand
    API->>HDL: Send(command)

    HDL->>DB: SELECT ProductReference\nINCLUDE Variants\nWHERE Id = referenceId
    DB-->>HDL: ProductReference con variantes

    HDL->>HDL: Por cada línea (variante):\ncostoMaterial = variant.MaterialCostPerUnit * qty\ncostoLabor = laborCostPerUnit * qty\ncostoOverhead = (material + labor) * overhead%\ntotalLinea = material + labor + overhead

    HDL->>HDL: Crea ProductionOrder\n(OrderCode = "ORD-{timestamp}")\nCrea ProductionOrderLines

    HDL->>DB: INSERT ProductionOrder\n+ ProductionOrderLines
    DB-->>HDL: saved

    HDL-->>API: ProductionCostResult
    API-->>FE: 200 { orderId, orderCode,\ntotalUnits, summary,\nlines[] }
    FE-->>U: Muestra desglose de costos
```

## Cálculo de costos — detalle

```mermaid
flowchart TB
    subgraph INPUT["Entrada por variante"]
        Q["Cantidad (qty)"]
        MAT["Costo material/u\n(del dominio: variant.MaterialCostPerUnit)"]
        LAB["Costo labor/u\n(ingresado por el usuario)"]
        OVH["% Overhead\n(aplicado a todo)"]
    end

    subgraph CALC["Cálculo por línea"]
        CM["Costo material total\n= MaterialCostPerUnit × qty"]
        CL["Costo labor total\n= LaborCostPerUnit × qty"]
        CO["Costo overhead total\n= (CM + CL) × overhead%"]
        TL["Total línea\n= CM + CL + CO"]
    end

    subgraph SUMMARY["Resumen de orden"]
        TCM["TotalMaterialCost\n= Σ CM de todas las líneas"]
        TCL["TotalLaborCost\n= Σ CL de todas las líneas"]
        TCO["TotalOverheadCost\n= Σ CO de todas las líneas"]
        GT["GrandTotal\n= TCM + TCL + TCO"]
        CPU["CostPerUnit\n= GrandTotal / TotalUnits"]
    end

    Q & MAT --> CM
    Q & LAB --> CL
    CM & CL & OVH --> CO
    CM & CL & CO --> TL

    CM --> TCM
    CL --> TCL
    CO --> TCO
    TCM & TCL & TCO --> GT
    GT --> CPU
```

## Flujo: Inventario financiero

```mermaid
sequenceDiagram
    participant FE as Frontend (SAG page)
    participant API as SagController
    participant HDL as GetFinancialInventoryHandler
    participant DB as PostgreSQL

    FE->>API: GET /api/sag/financial-inventory
    API->>HDL: Send(query)

    HDL->>DB: SELECT ProductReferences\nProjection: References → Variants\n(sin Include, proyección SQL)
    Note over HDL,DB: Solo campos necesarios:\nId, Name, Code,\nVariants: SKU, Size, Color,\nCurrentStock, MaterialCostPerUnit

    DB-->>HDL: datos proyectados

    HDL->>HDL: Por cada variante:\nInventoryValue = CurrentStock × MaterialCostPerUnit

    HDL-->>API: List<FinancialInventoryDto>
    API-->>FE: 200 JSON
    FE-->>FE: Muestra tabla con:\n- Swatch de color (círculo de color)\n- Nombre del color\n- Stock actual\n- Valor de inventario
```

## Modelo de datos SAG

```mermaid
classDiagram
    class ProductionCostResult {
        +Guid OrderId
        +string OrderCode
        +string ProductReferenceName
        +string Currency
        +int TotalUnits
        +decimal OverheadPercentage
        +CostSummary Summary
        +List~CostLineResult~ Lines
        +DateTime CalculatedAt
    }

    class CostSummary {
        +decimal TotalMaterialCost
        +decimal TotalLaborCost
        +decimal TotalOverheadCost
        +decimal GrandTotal
        +decimal CostPerUnit
    }

    class CostLineResult {
        +Guid VariantId
        +string SKU
        +string Size
        +string Color
        +int Quantity
        +decimal MaterialCostPerUnit
        +decimal LaborCostPerUnit
        +decimal OverheadCostPerUnit
        +decimal TotalCostPerUnit
        +decimal TotalLineCost
    }

    class VariantFinancialDetail {
        +string SKU
        +string SizeCode
        +string SizeLabel
        +string ColorName
        +string ColorHex
        +int CurrentStock
        +decimal MaterialCostPerUnit
        +decimal InventoryValue
    }

    ProductionCostResult --> CostSummary
    ProductionCostResult --> CostLineResult
```

## Endpoints REST

| Método | URL | Descripción |
|---|---|---|
| `POST` | `/api/sag/production-costs` | Calcular y registrar orden de producción |
| `GET` | `/api/sag/financial-inventory` | Inventario financiero de todas las referencias |

## Vinculación con otros módulos

- **PLM**: Lee `ProductReference.Variants` para obtener el costo de material por variante (`MaterialCostPerUnit`).
- **WMS**: Al asignar stock (`AssignStock`), el handler llama a `variant.AdjustStock(qty)` que actualiza `CurrentStock` en el dominio — el mismo campo que SAG usa para calcular valor de inventario.
