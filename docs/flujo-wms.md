# Módulo WMS — Warehouse Management System

## Responsabilidad

Gestiona la **asignación física de unidades** en la bodega. Distribuye variantes de productos en ubicaciones (Pasillo / Estante / Nivel) con llenado secuencial inteligente.

## Flujo: Asignar stock a ubicaciones

```mermaid
sequenceDiagram
    actor U as Líder Bodega
    participant FE as Frontend (WMS page)
    participant API as WmsController
    participant HDL as AssignStockHandler
    participant DB as PostgreSQL

    U->>FE: Selecciona referencia\ne ingresa cantidades\npor variante
    FE->>API: POST /api/wms/stock-assignments\n[Authorize]\nBody: AssignStockCommand
    API->>HDL: Send(command)

    HDL->>DB: SELECT ProductReference\nINCLUDE Variants
    DB-->>HDL: referencia con variantes

    HDL->>HDL: Valida que todas las variantes\npertenezcan a la referencia
    HDL->>HDL: Calcula totalRequested\n= Σ quantities

    HDL->>DB: SELECT WarehouseLocations\nWHERE IsActive AND CurrentStock < Capacity\nORDER BY Aisle, Shelf, Level
    DB-->>HDL: Lista de ubicaciones ordenadas

    HDL->>HDL: Valida totalAvailable ≥ totalRequested

    loop Por cada variante en el pedido
        loop Mientras queden unidades que asignar
            HDL->>HDL: Busca siguiente ubicación\ncon espacio disponible
            HDL->>HDL: location.Accommodate(remaining)\n→ actualiza CurrentStock en memoria
            HDL->>HDL: Crea StockAssignment(locationId, variantId, qty)
        end
        HDL->>HDL: variant.AdjustStock(totalAsignado)\n→ actualiza stock en memoria
    end

    HDL->>DB: INSERT StockAssignments[]
    HDL->>DB: SaveChanges\n(también persiste cambios en\nVariants.CurrentStock y\nLocations.CurrentStock)
    DB-->>HDL: saved

    HDL-->>API: AssignStockResult
    API-->>FE: 200 { assignments[], totalUnits, locationsUsed }
    FE-->>U: Muestra resumen de asignaciones
```

## Algoritmo de llenado secuencial

```mermaid
flowchart TD
    START["Inicio:\nlocationIdx = 0\npor cada variante"]
    SKIP["locationIdx++\n(avanza a siguiente)"]
    CHECK{¿Location[idx]\nestá llena?}
    ACCOM["location.Accommodate(remaining)\n→ assigned = min(remaining, availableSpace)\n→ location.CurrentStock += assigned\n→ remaining -= assigned"]
    CREATE["Crea StockAssignment\n(location, variant, assigned)"]
    MORE{¿remaining > 0?}
    NEXT_VAR["Siguiente variante"]

    START --> CHECK
    CHECK -->|Sí| SKIP --> CHECK
    CHECK -->|No| ACCOM --> CREATE --> MORE
    MORE -->|Sí| CHECK
    MORE -->|No| NEXT_VAR
```

Las ubicaciones se cargan **ordenadas** por `Aisle → Shelf → Level` desde la BD, lo que garantiza llenado en orden físico lógico (A-01-01, A-01-02, ..., A-02-01, ..., B-01-01, etc.).

## Flujo: Estado de bodega

```mermaid
sequenceDiagram
    participant FE as Frontend (WMS page)
    participant API as WmsController
    participant HDL as GetWarehouseStatusHandler
    participant DB as PostgreSQL

    FE->>API: GET /api/wms/warehouse-status
    API->>HDL: Send(query)
    HDL->>DB: Proyección de WarehouseLocations\n+ StockAssignments\n+ Variants (SKU, Size, Color)
    Note over HDL,DB: Usa proyecciones EF Core\nNUNCA Include() ciego —\nen alta velocidad (WMS policy)
    DB-->>HDL: datos proyectados
    HDL->>HDL: Calcula métricas:\n- OccupancyRate %\n- TotalCapacity\n- UsedCapacity\n- EmptyLocations
    HDL-->>API: WarehouseStatusDto
    API-->>FE: 200 JSON
    FE-->>FE: Muestra mapa de bodega\ny tabla de ubicaciones
```

## Modelo de dominio

```mermaid
classDiagram
    class WarehouseLocation {
        +Guid Id
        +WarehouseLocationCode LocationCode
        +int Capacity
        +int CurrentStock
        +bool IsActive
        +int AvailableSpace
        +bool IsFull
        +Accommodate(int qty) int
    }

    class WarehouseLocationCode {
        +string Aisle
        +int Shelf
        +int Level
        +string Code
    }

    class StockAssignment {
        +Guid Id
        +Guid WarehouseLocationId
        +Guid VariantId
        +int Quantity
        +DateTime AssignedAt
        +Create(locationId, variantId, qty) StockAssignment$
    }

    class AssignmentDetail {
        +string LocationCode
        +string Aisle
        +int Shelf
        +int Level
        +string SKU
        +string Size
        +string Color
        +int QuantityAssigned
        +int LocationRemainingSpace
    }

    WarehouseLocation --> WarehouseLocationCode
    WarehouseLocation "1" --> "N" StockAssignment
    StockAssignment --> AssignmentDetail
```

## Estructura de la bodega (datos seeded)

| Dimensión | Valor |
|---|---|
| Pasillos | A, B, C, D, E, F |
| Estantes por pasillo | 10 (1–10) |
| Niveles por estante | 10 (1–10) |
| **Total ubicaciones** | **600** |
| Capacidad por ubicación | 50 unidades |
| Capacidad total bodega | 30.000 unidades |

Código de ubicación: `{Pasillo}-{Estante:D2}-{Nivel:D2}` → ejemplo: `A-03-07`

## Endpoints REST

| Método | URL | Descripción |
|---|---|---|
| `POST` | `/api/wms/stock-assignments` | Asignar unidades a ubicaciones |
| `GET` | `/api/wms/warehouse-status` | Estado actual de bodega |

## Vinculación con otros módulos

- **PLM**: Lee `ProductReference` y sus `Variants` para saber qué se va a asignar.
- **SAG**: Al asignar, llama `variant.AdjustStock()` que actualiza `CurrentStock` — mismo campo que SAG usa en el inventario financiero.
