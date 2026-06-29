# Pipeline de Imágenes con MinIO

## Arquitectura de almacenamiento

Las imágenes **nunca pasan por la API en descarga** — se sirven directamente desde MinIO al browser. La API solo actúa como proxy de subida y gestiona el registro en BD.

```mermaid
graph LR
    subgraph UPLOAD["📤 Subida (pasa por API)"]
        USER["Usuario\n(arrastrar o click)"] --> FE["Frontend\n(multipart form)"]
        FE --> API["API .NET\n(PlmController)"]
        API --> MINIO["MinIO\n(bucket: dyaboo-assets)"]
        API --> DB["PostgreSQL\n(tabla: ProductImages)"]
    end

    subgraph SERVE["📥 Descarga (directa)"]
        DB2["PostgreSQL\n(FileName guardado)"] --> API2["API .NET\n(GetProductReferences)"]
        API2 --> FE2["Frontend\n(URL pública construida)"]
        FE2 -->|"HTTP GET directo"| MINIO2["MinIO\n(:9000/dyaboo-assets/{file})"]
    end
```

## Flujo detallado de subida

```mermaid
sequenceDiagram
    actor U as Usuario
    participant FE as ImageGallery.tsx
    participant API as PlmController
    participant STORE as IStorageService
    participant MINIO as MinIO S3
    participant DB as PostgreSQL

    U->>FE: Selecciona imagen(es)
    FE->>FE: Valida type="image/*"
    FE->>API: POST /api/plm/product-references/{id}/images\nContent-Type: multipart/form-data\nAuthorization: Bearer {token}

    API->>API: Genera fileName:\n"{Guid}_{sanitizedOriginalName}"\n(Guid.NewGuid() evita colisiones)

    API->>STORE: UploadAsync(stream, fileName,\ncontentType, cancellationToken)
    STORE->>MINIO: PutObjectAsync(\n  bucket: "dyaboo-assets",\n  objectName: fileName,\n  stream, contentType)
    MINIO-->>STORE: OK

    API->>DB: INSERT ProductImage\n{ ProductReferenceId, FileName,\n  DisplayOrder = maxOrder + 1 }
    DB-->>API: { Id, FileName, DisplayOrder }

    API->>STORE: GetPublicUrl(fileName)
    STORE-->>API: "http://localhost:9000/dyaboo-assets/{fileName}"

    API-->>FE: 201 Created\n{ id, url, fileName }
    FE->>FE: Agrega imagen al estado\n(sin reload de página)
    FE-->>U: Imagen visible en galería
```

## Flujo de descarga (servicio de URLs)

```mermaid
sequenceDiagram
    participant FE as Frontend
    participant API as GetProductReferencesHandler
    participant DB as PostgreSQL
    participant MINIO as MinIO

    FE->>API: GET /api/plm/product-references
    API->>DB: SELECT refs + images\n(proyección: solo FileName)
    DB-->>API: [{..., images: [{fileName, displayOrder}]}]

    loop Por cada imagen
        API->>API: storage.GetPublicUrl(fileName)\n→ "{PublicUrl}/{Bucket}/{fileName}"\n→ "http://localhost:9000/dyaboo-assets/{fileName}"
    end

    API-->>FE: [{..., images: [{url: "http://...", ...}]}]

    loop Cada imagen en la galería
        FE->>MINIO: GET http://localhost:9000/dyaboo-assets/{fileName}
        MINIO-->>FE: imagen binaria (JPEG/PNG/WebP)
    end
```

## Configuración del bucket

```mermaid
flowchart TD
    START["API Startup\nMinioInitializer.InitializeAsync()"]
    CHECK{¿Bucket\n'dyaboo-assets'\nexiste?}
    CREATE["BucketExistsArgs\nMakeBucketArgs\nCrear bucket"]
    POLICY["SetPolicyAsync:\nBucket public-read policy\n(JSON S3 bucket policy)"]
    DONE["Bucket listo\npara recibir objetos"]

    START --> CHECK
    CHECK -->|No| CREATE --> POLICY --> DONE
    CHECK -->|Sí| DONE
```

La política del bucket permite lectura pública sin autenticación a cualquier objeto en `dyaboo-assets/*`. Esto habilita que los browsers accedan a las imágenes directamente.

## Interfaz `IStorageService`

```csharp
// Definida en Application Layer — sin dependencia de MinIO
public interface IStorageService
{
    Task UploadAsync(Stream stream, string fileName,
                     string contentType, CancellationToken ct);
    Task DeleteAsync(string fileName, CancellationToken ct);
    string GetPublicUrl(string fileName);
}
```

`MinioStorageService` en Infrastructure implementa esta interfaz con el SDK de MinIO. Si en el futuro se cambia a AWS S3 o Azure Blob, solo se reemplaza la implementación — el resto del sistema no cambia.

## Variables de entorno relevantes

| Variable | Propósito |
|---|---|
| `Minio__Endpoint` | Host:puerto MinIO desde la API (`minio:9000` en Docker) |
| `Minio__AccessKey` | Credencial de acceso |
| `Minio__SecretKey` | Credencial secreta |
| `Minio__Bucket` | Nombre del bucket (`dyaboo-assets`) |
| `Minio__PublicUrl` | URL base para URLs públicas (`http://localhost:9000`) |
| `Minio__UseSSL` | `false` en desarrollo, `true` con HTTPS en producción |
| `NEXT_PUBLIC_MINIO_URL` | URL MinIO usada por el browser (igual a `PublicUrl`) |

## Formatos soportados

Cualquier `Content-Type: image/*` es aceptado por el backend (no hay validación de formato específica — se guarda tal cual). El browser puede renderizar JPEG, PNG, WebP, GIF, AVIF.

## Eliminación de imágenes

Al eliminar una imagen, el flujo es siempre:
1. Obtener `FileName` de PostgreSQL
2. Eliminar objeto de MinIO (`RemoveObjectAsync`)
3. Eliminar registro de PostgreSQL

Este orden garantiza que si falla el paso 3, la próxima vez que se intente eliminar se podrá reintentar (el registro aún existe). Si falla el paso 2, el archivo queda huérfano en MinIO (mejora futura: job de limpieza).
