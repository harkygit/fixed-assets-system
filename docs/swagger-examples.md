# Swagger/OpenAPI Examples

Swagger UI:

```text
http://localhost:8081/swagger
```

## GET /health

### Response 200

```json
{
  "status": "Healthy",
  "service": "IntegrationService",
  "checkedAt": "2026-05-27T12:30:00.0000000+00:00"
}
```

## GET /api/assets

Возвращает список объектов ОС через интеграционный слой. Первый вызов идет в ObjectService и кладет результат в Redis, следующие вызовы читают кэш.

### Response 200

```json
[
  {
    "inventoryId": "OS001",
    "name": "Ноутбук Lenovo ThinkPad T14",
    "cost": 80000
  },
  {
    "inventoryId": "OS002",
    "name": "МФУ HP LaserJet Pro",
    "cost": 25000
  }
]
```

## POST /api/integration/fixed-assets/dispose

Запускает полный сценарий:

```text
IntegrationService -> ObjectService -> DepreciationService -> DisposalService
```

### Request

```json
{
  "inventoryId": "OS001",
  "usefulLife": 5,
  "disposalReason": "Физический износ"
}
```

### Response 200

```json
{
  "correlationId": "6f01d71f5a19468b94b6a824f4e0f314",
  "asset": {
    "inventoryId": "OS001",
    "name": "Ноутбук Lenovo ThinkPad T14",
    "cost": 80000
  },
  "depreciation": {
    "yearlyDepreciation": 16000
  },
  "disposal": {
    "message": "Объект OS001 списан",
    "reason": "Физический износ"
  },
  "status": "Completed"
}
```

## Downstream API Examples

### ObjectService: GET /objects

```json
[
  {
    "inventory_id": "OS001",
    "name": "Ноутбук Lenovo ThinkPad T14",
    "cost": 80000
  }
]
```

### DepreciationService: POST /depreciation/calculate

Request:

```json
{
  "cost": 80000,
  "useful_life": 5
}
```

Response:

```json
{
  "yearly_depreciation": 16000
}
```

### DisposalService: POST /disposal

Request:

```json
{
  "inventory_id": "OS001",
  "reason": "Физический износ"
}
```

Response:

```json
{
  "message": "Объект OS001 списан",
  "reason": "Физический износ"
}
```

## Example Logs

```text
[15:42:31 INF] Starting Fixed Assets IntegrationService
[15:42:38 INF] Request starting HTTP/1.1 GET /api/assets
[15:42:38 INF] Redis cache miss for assets:list
[15:42:38 INF] Loaded 2 fixed assets from ObjectService
[15:42:40 INF] Starting fixed asset workflow
[15:42:40 INF] Redis cache hit for assets:list
[15:42:40 INF] Calculated yearly depreciation 16000 for asset cost 80000
[15:42:40 INF] Disposed fixed asset OS001
[15:42:40 INF] Fixed asset workflow completed
```
