# UML Activity Diagram

```mermaid
flowchart TD

A([Старт])

A --> B[Получение данных]

B --> C[Проверка Redis Cache]

C -->|Cache Hit| D[Возврат данных 50ms]

C -->|Cache Miss| E[Запрос PostgreSQL 500ms]

E --> F[Сохранение в Redis]

F --> G[Асинхронная интеграция]

G --> H[Параллельные HTTP запросы]

H --> I([Завершение 800ms])
```

---

# Добавь Redis в docker-compose

```yaml id="w6j9pc"
redis:
  image: redis:7
  ports:
    - "6379:6379"
```

# NuGet пакеты
```bash
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
dotnet add package BenchmarkDotNet
```