# AI Platform SaaS — Self-Hosted B2B

Платформа AI-агентов в виде self-hosted Kubernetes-образа для B2B SaaS: agent runtime, инструменты, аудит, политики, RAG, UI и control plane (лицензия, конфиг). Деплой через Helm, BYOC, интеграция с внешним LLM.

## Стек

- **Backend:** .NET 10 (ASP.NET Core)
- **Frontend:** React 18 + Vite 5, TypeScript
- **БД:** PostgreSQL (агенты, политики, инструменты), MongoDB (аудит)
- **Кэш:** Redis (опционально)
- **Очереди:** Kafka (опционально, для событий аудита)
- **Инфраструктура:** Kubernetes, Helm, GitLab CI

## Структура репозитория

```
├── src/                    # Backend .NET
│   ├── AiPlatform.sln
│   ├── AiPlatform.Api/      # REST API, контроллеры
│   ├── AiPlatform.Core/    # Домен, репозитории
│   ├── AiPlatform.Agents/   # Runtime агентов, LLM, инструменты, политики
│   ├── AiPlatform.Rag/      # RAG-сервис
│   └── AiPlatform.ControlPlane/  # Лицензия, конфиг
├── frontend/               # React + Vite UI
├── deploy/helm/ai-platform/ # Helm chart
├── docker/                 # Dockerfile.api, Dockerfile.ui
├── docker-compose.yml      # Инфраструктура (Postgres, Mongo, Redis, Kafka) + опционально app
├── .env.example            # Переменные для LLM при запуске через compose
└── .gitlab-ci.yml          # Сборка и публикация образов
```

## Docker Compose (инфраструктура)

Запуск только инфраструктуры (PostgreSQL, MongoDB, Redis):

```bash
docker compose up -d
```

Строки подключения для приложения (при запуске API локально):

- **Postgres:** `Host=localhost;Port=5432;Database=aiplatform;Username=postgres;Password=postgres`
- **MongoDb:** `mongodb://localhost:27017`
- **Redis:** `localhost:6379`

Запуск инфраструктуры и приложения (API + UI в контейнерах):

```bash
# При необходимости задайте в .env: LLM_BASE_URL, LLM_API_KEY, LLM_DEFAULT_MODEL
docker compose --profile app up -d
```

- API: http://localhost:5000  
- UI: http://localhost:3000 (проксирует /api на API)

Kafka (опционально):

```bash
docker compose --profile kafka up -d
```

## Локальный запуск

### Требования

- .NET 10 SDK
- Node.js 20+
- PostgreSQL 15+
- MongoDB 6+

### Backend

```bash
# Настройте appsettings.Development.json: ConnectionStrings, Llm__BaseUrl, Llm__ApiKey
cd src/AiPlatform.Api
dotnet run
```

API: http://localhost:5000, Swagger: http://localhost:5000/swagger

### Frontend

```bash
cd frontend
npm install
npm run dev
```

UI: http://localhost:3000 (проксирует /api на backend).

### Переменные окружения / конфиг (BYOC)

- **ConnectionStrings:Postgres** — строка подключения к PostgreSQL
- **ConnectionStrings:MongoDb** — строка подключения к MongoDB
- **ConnectionStrings:Redis** — опционально
- **Llm:BaseUrl** — базовый URL LLM API (OpenAI-совместимый)
- **Llm:ApiKey** — API-ключ LLM
- **Llm:DefaultModel** — модель по умолчанию (например, gpt-4o-mini)
- **License:** — Edition, ExpiresAt, MaxAgents, MaxTenants, Features (в appsettings или env)

## Деплой в Kubernetes (Helm)

1. Установите PostgreSQL и MongoDB (свои инстансы или подчарты).
2. Создайте Secret с ключом LLM:
   ```bash
   kubectl create secret generic ai-platform-secrets --from-literal=llm-api-key=YOUR_KEY
   ```
3. Укажите в `values.yaml`:
   - `connectionStrings.postgres`, `connectionStrings.mongoDb`
   - `api.env` (Llm__BaseUrl и т.д.)
   - образы API и UI после сборки
4. Установка:
   ```bash
   helm upgrade --install ai-platform ./deploy/helm/ai-platform \
     --set api.image.repository=your-registry/ai-platform-api \
     --set ui.image.repository=your-registry/ai-platform-ui
   ```
5. Включите Ingress и укажите host в `values.yaml` (ingress.hosts).

## GitLab CI

- **build:** сборка .NET и frontend
- **test:** тесты .NET (при наличии)
- **docker:** сборка и push образов API и UI в Registry (ветки main/master)

В `variables` задайте `CI_REGISTRY_IMAGE` (например, `registry.example.com/group/ai-platform`).

## API (кратко)

| Метод | Путь | Описание |
|-------|------|----------|
| GET/POST | /api/v1/agents | Список, создание агентов |
| GET/PUT/DELETE | /api/v1/agents/{id} | Агент по ID |
| POST | /api/v1/agents/{id}/run | Запуск агента (сообщение → ответ + tool calls) |
| GET/POST | /api/v1/tools | Регистрация и список инструментов |
| GET | /api/v1/audit | Аудит (фильтры: tenantId, agentId, eventType, from, to) |
| GET/POST | /api/v1/policies | Политики (allow/deny по tool pattern) |
| POST/GET | /api/v1/rag/collections/{id}/... | RAG: индексация, поиск |
| GET | /api/v1/control-plane/license | Статус лицензии |
| GET | /api/v1/control-plane/config | Конфиг (key=...) |

Запросы можно передавать с `?tenantId=...` для мультитенантности.

## Лицензия

Платформа читает лицензию из конфига (License:Edition, ExpiresAt, Features). В production лицензию можно подставлять из внешнего хранилища или секретов при деплое.
