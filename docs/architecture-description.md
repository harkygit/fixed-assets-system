# Описание архитектуры интеграционного решения

## Назначение

Fixed Assets System автоматизирует интеграционный сценарий учета основных средств: оператор выбирает объект ОС, система получает карточку объекта, рассчитывает амортизацию и выполняет списание. Решение построено как набор слабосвязанных сервисов, объединенных REST API и интеграционным слоем.

## Компоненты

| Компонент | Ответственность |
|---|---|
| Frontend UI | Визуальный сценарий демонстрации, вызов IntegrationService |
| IntegrationService | Оркестрация бизнес-потока, кэширование, устойчивые HTTP-вызовы, Swagger |
| ObjectService | Выдача списка объектов основных средств |
| DepreciationService | Расчет амортизации линейным методом |
| DisposalService | Списание объекта ОС |
| PostgreSQL | Целевая транзакционная БД |
| Redis | Кэш списка объектов ОС |
| GitHub Actions | CI pipeline для сборки и проверки контейнеров |

## REST API

Взаимодействие между сервисами синхронное и основано на REST/JSON:

```text
GET  /api/assets
POST /api/integration/fixed-assets/dispose
GET  /objects
POST /depreciation/calculate
POST /disposal
```

Интеграционный слой скрывает от UI внутреннюю топологию сервисов. UI работает только с `IntegrationService`, а все downstream-вызовы выполняются адаптерами:

- `ObjectAdapter`
- `DepreciationAdapter`
- `DisposalAdapter`

## Интеграционный слой

IntegrationService реализует orchestration pattern:

1. Принимает команду от UI.
2. Получает список ОС из Redis или ObjectService.
3. Находит нужный объект по `inventoryId`.
4. Вызывает DepreciationService для расчета амортизации.
5. Вызывает DisposalService для списания.
6. Возвращает агрегированный response.

Такой подход упрощает UI и концентрирует политики интеграции в одном месте.

## PostgreSQL

PostgreSQL 16 подключен как транзакционное хранилище. Для production-развития в нем размещаются:

- карточки основных средств;
- операции амортизации;
- операции списания;
- audit trail;
- состояние Saga/компенсаций.

Рекомендуемый доступ из .NET: EF Core с миграциями, optimistic concurrency и отдельными DbContext на bounded context.

## Redis

Redis 7 используется как распределенный кэш. IntegrationService кэширует список объектов ОС:

```text
fixed-assets:assets:list
TTL: 5 minutes
```

Эффект:

- меньше HTTP-вызовов к ObjectService;
- быстрее первичная загрузка UI после прогрева;
- меньше нагрузка на сервис каталога.

## Docker Compose

`docker-compose.yml` поднимает:

- frontend;
- integration-service;
- object-service;
- depreciation-service;
- disposal-service;
- postgres;
- redis.

Контейнеры общаются по service name: `object-service`, `depreciation-service`, `disposal-service`, `redis`, `postgres`. PostgreSQL опубликован на host-порту `5433`, а внутри compose-сети остается доступен как `postgres:5432`.

## GitHub Actions

CI pipeline выполняет сборку и smoke-проверку контейнерного стенда. Для enterprise-подхода pipeline можно расширить:

- `dotnet build`;
- `dotnet test`;
- Playwright E2E;
- Docker image scan;
- публикация OpenAPI artifact;
- deployment в staging.

## Polly

Polly защищает межсервисные HTTP-вызовы:

- retry с exponential backoff для временных 5xx/network ошибок;
- circuit breaker для DisposalService при повторных отказах.

Это предотвращает каскадные сбои и дает downstream-сервисам время восстановиться.

## Serilog

Serilog используется для структурированного логирования:

- console sink для Docker logs;
- rolling file sink `logs/log.txt`;
- enrichment `Application`, `MachineName`, `ThreadId`;
- correlation scope в интеграционном сценарии.

Пример:

```text
[INF] Starting fixed asset workflow { CorrelationId: "c71c...", InventoryId: "OS001" }
[INF] Redis cache hit for assets:list
[INF] Calculated yearly depreciation 16000 for asset cost 80000
[INF] Fixed asset workflow completed
```
