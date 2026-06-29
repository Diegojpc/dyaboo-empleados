# Módulo PLM — Product Lifecycle Management

## Responsabilidad

Gestiona el ciclo de vida de las **referencias textiles**: creación de productos con todas sus variantes Talla × Color, y su galería de imágenes.

## Flujo: Crear referencia de producto

```mermaid
sequenceDiagram
    actor U as Líder PLM
    participant FE as Frontend
    participant API as PlmController
    participant MED as MediatR
    participant HDL as CreateProductReferenceHandler
    participant DB as PostgreSQL

    U->>FE: Completa formulario\n(nombre, código, categoría,\nvariantes Talla × Color)
    FE->>API: POST /api/plm/product-references\n[Authorize]\nBody: CreateProductReferenceCommand
    API->>MED: Send(command)
    MED->>HDL: Handle(command, ct)
    HDL->>HDL: Valida que no exista\nCode duplicado
    HDL->>HDL: Genera SKU por variante\n"{Code}-{SizeCode}-{ColorHex}"
    HDL->>DB: INSERT ProductReference\n+ N ProductVariants
    DB-->>HDL: entidades guardadas
    HDL-->>API: ProductReferenceId (Guid)
    API-->>FE: 201 Created { id }
    FE-->>U: Muestra referencia creada
```

## Flujo: Listar referencias con imágenes

```mermaid
sequenceDiagram
    participant FE as Frontend
    participant API as PlmController
    participant HDL as GetProductReferencesHandler
    participant DB as PostgreSQL
    participant MINIO as MinIO

    FE->>API: GET /api/plm/product-references
    API->>HDL: Send(GetProductReferencesQuery)
    HDL->>DB: SELECT con proyección\n(sin Include() ciego)
    Note over HDL,DB: Proyección: Id, Name, Code,\nCategory, CreatedAt,\nVariants(SKU, Size, Color, Stock),\nImages(FileName, DisplayOrder)
    DB-->>HDL: datos proyectados
    HDL->>HDL: Por cada imagen:\nstorage.GetPublicUrl(fileName)\n→ http://localhost:9000/dyaboo-assets/{file}
    HDL-->>API: List<ProductReferenceDto>
    API-->>FE: 200 JSON array
    FE->>MINIO: GET imágenes por URL directa\n(no pasan por la API)
```

## Flujo: Subir imagen

```mermaid
sequenceDiagram
    actor U as Usuario
    participant FE as ImageGallery.tsx
    participant API as PlmController
    participant MINIO as MinIO

    U->>FE: Arrastra foto o hace click\nen zona de upload
    FE->>FE: Valida tipo (image/*)\ny tamaño
    FE->>API: POST /api/plm/product-references/{id}/images\nmultipart/form-data\n[Authorize]
    API->>API: Genera fileName único:\n"{Guid}_{sanitized_name}"
    API->>MINIO: PutObjectAsync(\n  stream, fileName,\n  contentType)
    MINIO-->>API: OK
    API->>DB: INSERT ProductImage\n(referenceId, fileName, displayOrder)
    DB-->>API: savedImage
    API-->>FE: 201 { id, url, fileName }
    FE->>FE: Actualiza galería sin reload
```

## Flujo: Eliminar imagen

```mermaid
sequenceDiagram
    actor U as Usuario
    participant FE as ImageGallery.tsx
    participant API as PlmController
    participant DB as PostgreSQL
    participant MINIO as MinIO

    U->>FE: Click en "×" sobre la imagen
    FE->>API: DELETE /api/plm/product-references/{refId}/images/{imageId}
    API->>DB: SELECT ProductImage WHERE Id = imageId
    DB-->>API: { fileName, ... }
    API->>MINIO: RemoveObjectAsync(fileName)
    MINIO-->>API: OK
    API->>DB: DELETE FROM ProductImages\nWHERE Id = imageId
    DB-->>API: deleted
    API-->>FE: 204 No Content
    FE->>FE: Remueve imagen de la UI
```

## Modelo de dominio

```mermaid
classDiagram
    class ProductReference {
        +Guid Id
        +string Name
        +string Code
        +ProductCategory Category
        +string Description
        +ICollection~ProductVariant~ Variants
        +ICollection~ProductImage~ Images
    }

    class ProductVariant {
        +Guid Id
        +string SKU
        +SizeValueObject Size
        +ColorValueObject Color
        +int CurrentStock
        +decimal MaterialCostPerUnit
        +AdjustStock(int qty)
    }

    class SizeValueObject {
        +string Code
        +string Label
    }

    class ColorValueObject {
        +string Name
        +string HexCode
    }

    class ProductImage {
        +Guid Id
        +string FileName
        +int DisplayOrder
    }

    class ProductCategory {
        <<enumeration>>
        Camisa
        Pantalon
        Vestido
        Chaqueta
        Accesorio
    }

    ProductReference "1" --> "N" ProductVariant
    ProductReference "1" --> "N" ProductImage
    ProductVariant --> SizeValueObject
    ProductVariant --> ColorValueObject
```

## Endpoints REST

| Método | URL | Descripción |
|---|---|---|
| `GET` | `/api/plm/product-references` | Listar todas las referencias |
| `POST` | `/api/plm/product-references` | Crear referencia con variantes |
| `POST` | `/api/plm/product-references/{id}/images` | Subir imagen (multipart) |
| `DELETE` | `/api/plm/product-references/{id}/images/{imgId}` | Eliminar imagen |
