# Fixed Assets System

Система учета основных средств (ОС).

## Описание проекта

Проект реализует интеграцию нескольких модулей для автоматизации учета основных средств организации.

Основные возможности системы:

- регистрация объектов ОС;
- расчет амортизации;
- проведение инвентаризации;
- списание объектов;
- формирование отчетности.

---

# Архитектура проекта

Проект состоит из 3 микросервисов:

| Модуль | Технология | Порт |
|---|---|---|
| ObjectService | Python + Flask | 5001 |
| DepreciationService | Python + Flask | 5002 |
| DisposalService | Node.js + Express | 3000 |

---

# Структура проекта

```text
fixed-assets-system/
│
├── object_service/
│   └── app.py
│
├── depreciation_service/
│   └── app.py
│
├── disposal_service/
│   └── app.js
│
├── requirements.txt
└── README.md
```

# Установка и запуск
1. Клонирование репозитория
```bash
git clone https://github.com/username/fixed-assets-system.git
cd fixed-assets-system
```

# Запуск Python-сервисов
Установка зависимостей
```bash
pip install -r requirements.txt
```

# Запуск ObjectService
```bash
cd object_service
python app.py
```

Сервис будет доступен:
http://localhost:5001

# Запуск DepreciationService
```bash
cd depreciation_service
python app.py
```

Сервис будет доступен:
http://localhost:5002

# Запуск Node.js сервиса
Установка зависимостей
```bash
npm install express
```

# Запуск DisposalService
```bash
node app.js
```

Сервис будет доступен:
http://localhost:3000

# API Endpoints
ObjectService
Получение списка объектов
```http
GET /objects
```

# DepreciationService
Расчет амортизации
```http
POST /depreciation/calculate
```

Пример JSON:
```json
{
  "cost": 120000,
  "useful_life": 5
}
```

# DisposalService
Списание объекта
```http
POST /disposal
```

Пример JSON:
```json
{
  "inventory_id": "OS001",
  "reason": "Поломка"
}
```

Используемые технологии
- Python
- Flask
- Node.js
- Express
- REST API


---

### `requirements.txt`

```txt id="k8m56k"
Flask==3.0.3
requests==2.32.3