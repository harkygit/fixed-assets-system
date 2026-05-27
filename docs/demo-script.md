# Demo Script

## 1. Запуск стенда

```bash
docker compose build
docker compose up -d
docker compose ps
```

Ожидаемый результат:

```text
frontend-ui            running
integration-service    running
object-service         running
depreciation-service   running
disposal-service       running
postgres-db            running
redis-cache            running
```

## 2. Открытие frontend

Открыть:

```text
http://localhost:8080
```

Показать:

- таблицу объектов ОС;
- статус загрузки через IntegrationService;
- кнопку `Списать`.

## 3. Демонстрация API

Проверить health endpoint:

```bash
curl http://localhost:8081/health
```

Проверить список ОС:

```bash
curl http://localhost:8081/api/assets
```

Запустить интеграционный сценарий:

```bash
curl -X POST http://localhost:8081/api/integration/fixed-assets/dispose \
  -H "Content-Type: application/json" \
  -d "{\"inventoryId\":\"OS001\",\"usefulLife\":5,\"disposalReason\":\"Физический износ\"}"
```

## 4. Swagger

Открыть:

```text
http://localhost:8081/swagger
```

Показать:

- `GET /health`;
- `GET /api/assets`;
- `POST /api/integration/fixed-assets/dispose`;
- request/response schema из XML-комментариев.

## 5. Тесты

Проверить .NET-сборку:

```bash
dotnet build integration_service/integration_service.csproj
```

Проверить контейнерный smoke test:

```bash
docker compose ps
docker compose logs integration-service
```

При наличии Playwright-зависимостей:

```bash
npx playwright test
```

## 6. GitHub Actions

Открыть `.github/workflows/ci.yml` и показать этапы:

- checkout;
- setup Python;
- setup Node.js;
- docker compose build;
- docker compose up;
- docker ps.

Объяснить, что pipeline воспроизводит базовую проверку demo-стенда.

## 7. Redis cache

Первый вызов:

```bash
curl http://localhost:8081/api/assets
```

В логах IntegrationService:

```text
Redis cache miss for assets:list
Loaded 2 fixed assets from ObjectService
```

Повторный вызов:

```bash
curl http://localhost:8081/api/assets
```

В логах:

```text
Redis cache hit for assets:list
```

## 8. Polly Retry и Circuit Breaker

Для демонстрации retry можно временно остановить ObjectService:

```bash
docker stop object-service
curl http://localhost:8081/api/assets
docker start object-service
```

Объяснение:

- retry выполняет повторные попытки при сетевых ошибках;
- circuit breaker защищает DisposalService от постоянной нагрузки при повторяющихся 5xx/network сбоях;
- Serilog фиксирует ошибки и correlation context.

## 9. Завершение

```bash
docker compose down
```

Ключевой вывод: демонстрация показывает полный путь `Frontend -> IntegrationService -> ObjectService -> DepreciationService -> DisposalService`, инфраструктуру Docker/Redis/PostgreSQL, API-контракт Swagger и инженерные практики надежности.
