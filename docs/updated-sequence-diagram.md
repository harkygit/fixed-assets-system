# Updated UML Sequence Diagram

```mermaid
sequenceDiagram

actor User

participant UI
participant IntegrationService
participant DepreciationService
participant DisposalService
participant Saga

User->>UI:
Запуск процесса

UI->>IntegrationService:
Создать операцию

IntegrationService->>DepreciationService:
Расчет амортизации

DepreciationService-->>IntegrationService:
Ошибка

IntegrationService->>Saga:
Компенсация

Saga->>DisposalService:
Отмена операции

DisposalService-->>Saga:
Успешно

Saga-->>User:
Ошибка обработана


---

# `debug-logs.txt`

```text id="u4w9rm"
[INF] Старт процесса
[INF] Проверка остатков выполнена

[ERR] Ошибка подключения к сервису амортизации

[WRN] Retry attempt 1
[WRN] Retry attempt 2
[WRN] Retry attempt 3

[ERR] Circuit Breaker activated

[WRN] Запуск компенсации

[INF] Компенсация успешно завершена

[INF] Пользователь уведомлен