# UML Deployment Diagram

Диаграмма показывает развертывание демо-стенда в Docker Compose. Все прикладные контейнеры находятся в одной compose-сети и общаются по DNS-именам сервисов.

```mermaid
flowchart LR
    subgraph Browser["User Workstation"]
        UI["Frontend UI\nhttp://localhost:8080"]
    end

    subgraph DockerHost["Docker Host / docker-compose"]
        Integration["IntegrationService\nASP.NET Core Web API\nSwagger :8081"]
        ObjectSvc["ObjectService\nPython Flask :5001"]
        DepSvc["DepreciationService\nPython Flask :5002"]
        DisposalSvc["DisposalService\nNode.js Express :3000"]
        Postgres[("PostgreSQL 16\nfixed_assets host :5433\ncontainer :5432")]
        Redis[("Redis 7\ncache :6379")]
    end

    UI -->|"REST JSON"| Integration
    Integration -->|"GET /objects"| ObjectSvc
    Integration -->|"POST /depreciation/calculate"| DepSvc
    Integration -->|"POST /disposal"| DisposalSvc
    Integration -->|"cache assets:list"| Redis
    Integration -.->|"future EF Core persistence"| Postgres
```

## Deployment Notes

| Узел | Роль |
|---|---|
| Frontend UI | Демо-интерфейс оператора учета ОС |
| IntegrationService | API gateway/orchestrator, единая точка входа для UI |
| ObjectService | Источник карточек ОС |
| DepreciationService | Расчет годовой амортизации |
| DisposalService | Выполнение операции списания |
| PostgreSQL | Транзакционное хранилище данных |
| Redis | Быстрый кэш для справочных и часто читаемых данных |
