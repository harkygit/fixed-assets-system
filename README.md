# Fixed Assets System

Интеграционная система учета основных средств, построенная на микросервисной архитектуре. Проект демонстрирует полный поток жизненного цикла ОС: получение карточки объекта, расчет амортизации, списание, кэширование справочных данных, отказоустойчивые REST-вызовы и CI-проверки.

## Архитектура

| Компонент | Технологии | Назначение | Порт |
|---|---|---|---|
| Frontend UI | HTML, Bootstrap, JavaScript, Nginx | Демо-интерфейс оператора ОС | 8080 |
| IntegrationService | ASP.NET Core Web API, Swagger, Serilog, Polly, Redis | Интеграционный слой и оркестрация сценариев | 8081 |
| ObjectService | Python Flask | Каталог объектов основных средств | 5001 |
| DepreciationService | Python Flask | Расчет амортизации | 5002 |
| DisposalService | Node.js Express | Списание объектов ОС | 3000 |
| PostgreSQL | PostgreSQL 16 | Целевая транзакционная БД | 5433 -> 5432 |
| Redis | Redis 7 | Кэш объектов ОС и оптимизация чтения | 6379 |

Основной демо-поток:

```text
Frontend UI -> IntegrationService -> ObjectService
                              -> DepreciationService
                              -> DisposalService
                              -> Redis
                              -> PostgreSQL
```

## Быстрый старт

Требования:

- Docker Desktop или Docker Engine с Docker Compose
- .NET SDK 8 для локальной разработки IntegrationService
- Python 3.12 для локального запуска Flask-сервисов
- Node.js 20 для локального запуска Node.js-сервисов

Запуск всего стенда:

```bash
docker compose build
docker compose up -d
docker compose ps
```

Остановка:

```bash
docker compose down
```

Остановка с удалением томов PostgreSQL и Redis:

```bash
docker compose down -v
```

## Endpoint'ы

| Сервис | URL |
|---|---|
| Frontend UI | http://localhost:8080 |
| IntegrationService Swagger | http://localhost:8081/swagger |
| IntegrationService health | http://localhost:8081/health |
| Assets through integration layer | http://localhost:8081/api/assets |
| ObjectService | http://localhost:5001/objects |

Пример запуска интеграционного сценария:

```bash
curl -X POST http://localhost:8081/api/integration/fixed-assets/dispose \
  -H "Content-Type: application/json" \
  -d "{\"inventoryId\":\"OS001\",\"usefulLife\":5,\"disposalReason\":\"Физический износ\"}"
```

## Swagger/OpenAPI

Swagger включен в `integration_service/Program.cs`:

- `AddSwaggerGen()`
- `UseSwagger()`
- `UseSwaggerUI()`
- XML-документация из файла `integration_service.xml`

OpenAPI UI доступен по адресу:

```text
http://localhost:8081/swagger
```

## PostgreSQL

Конфигурация Docker Compose:

```yaml
POSTGRES_USER: admin
POSTGRES_PASSWORD: admin
POSTGRES_DB: fixed_assets
```

Строка подключения с хост-машины:

```text
Host=localhost;Port=5433;Database=fixed_assets;Username=admin;Password=admin
```

Строка подключения внутри Docker Compose-сети:

```text
Host=postgres;Port=5432;Database=fixed_assets;Username=admin;Password=admin
```

PostgreSQL используется как транзакционное хранилище доменных данных ОС. В текущем демо-стенде интеграционный сценарий работает через REST-сервисы, а схема БД подготовлена как инфраструктурный компонент для расширения EF Core-модели.

## Redis

Redis используется IntegrationService через `IDistributedCache`. Ключ `fixed-assets:assets:list` кэширует список объектов ОС на 5 минут.

Локальный адрес:

```text
localhost:6379
```

Docker-адрес внутри compose-сети:

```text
redis:6379
```

## Serilog и Polly

Serilog пишет структурированные логи в консоль и файл `logs/log.txt`.

Пример:

```text
[15:42:31 INF] Starting fixed asset workflow
[15:42:31 INF] Redis cache miss for assets:list
[15:42:31 INF] Loaded 2 fixed assets from ObjectService
[15:42:32 INF] Calculated yearly depreciation 16000 for asset cost 80000
[15:42:32 INF] Disposed fixed asset OS001
[15:42:32 INF] Fixed asset workflow completed
```

Polly используется для:

- exponential retry при временных ошибках ObjectService и DepreciationService;
- circuit breaker при повторных сбоях DisposalService.

## Тестирование

Базовая проверка стенда:

```bash
docker compose up -d --build
curl http://localhost:8081/health
curl http://localhost:8081/api/assets
```

Проверка .NET-проекта:

```bash
dotnet build integration_service/integration_service.csproj
```

Playwright E2E-тесты расположены в `tests/E2ETests`. Unit и integration-тесты находятся в `tests/UnitTests` и `tests/IntegrationTests`.

## GitHub Actions

CI pipeline находится в `.github/workflows/ci.yml` и выполняет:

- checkout репозитория;
- установку Python 3.12;
- установку Python-зависимостей;
- lint Python-кода;
- установку Node.js 20;
- установку Node.js-зависимостей;
- `docker compose build`;
- `docker compose up -d`;
- проверку запущенных контейнеров.

## Структура проекта

```text
fixed-assets-system/
├── .github/workflows/ci.yml
├── database/
├── depreciation_service/
│   ├── Dockerfile
│   └── app.py
├── disposal_service/
│   ├── Dockerfile
│   └── app.js
├── docs/
│   ├── architecture-description.md
│   ├── deployment-diagram.md
│   ├── swagger-examples.md
│   ├── presentation-plan.md
│   ├── defense-answers.md
│   └── demo-script.md
├── frontend/
│   ├── Dockerfile
│   ├── index.html
│   ├── app.js
│   └── style.css
├── integration_service/
│   ├── Adapters/
│   ├── DTO/
│   ├── Logging/
│   ├── Monitoring/
│   ├── Dockerfile
│   ├── Program.cs
│   └── integration_service.csproj
├── object_service/
│   ├── Dockerfile
│   └── app.py
├── performance/
├── tests/
├── docker-compose.yml
└── README.md
```

## Документация

- [Описание архитектуры](docs/architecture-description.md)
- [UML Deployment Diagram](docs/deployment-diagram.md)
- [Swagger examples](docs/swagger-examples.md)
- [План презентации](docs/presentation-plan.md)
- [Ответы на защиту](docs/defense-answers.md)
- [Demo script](docs/demo-script.md)
