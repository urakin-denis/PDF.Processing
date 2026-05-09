# PDF Processing Service

Два сервиса для обработки PDF:

- **API Gateway** (`PDF.Processing.Service.Api`): загрузка PDF → сохранение в MinIO → запись outbox → публикация события в RabbitMQ.
- **Background Worker** (`PDF.Processing.Service.Worker`): потребляет события из RabbitMQ → скачивает PDF из MinIO → извлекает текст → сохраняет в PostgreSQL → обновляет статус.

Внешние зависимости поднимаются в Docker: **PostgreSQL**, **RabbitMQ**, **MinIO**.

## Быстрый старт (one-step)

Требования:
- Docker Desktop

Запуск:

```bash
docker compose up --build
```

После старта:
- **API**: `http://localhost:8080`
- **Swagger UI**: `http://localhost:8080/swagger`
- **RabbitMQ Management**: `http://localhost:15672` (логин/пароль: `guest` / `guest`)
- **MinIO Console**: `http://localhost:9001` (логин/пароль: `minioadmin` / `minioadmin`)

Примечание:
- Миграции EF Core применяются автоматически при старте API/Worker (для dev/docker).

## REST API

Базовый путь: `/api/v1`

### Загрузка PDF

`POST /api/v1/pdfs` (multipart/form-data)

Пример:

```bash
curl -F "file=@./sample.pdf" http://localhost:8080/api/v1/pdfs
```

Ответ: `202 Accepted`

### Список PDF

`GET /api/v1/pdfs?page=1&pageSize=50`

```bash
curl http://localhost:8080/api/v1/pdfs
```

### Метаданные + статус

`GET /api/v1/pdfs/{id}`

```bash
curl http://localhost:8080/api/v1/pdfs/<guid>
```

### Текст PDF

`GET /api/v1/pdfs/{id}/text`

- если документ ещё не обработан, вернётся `409 Conflict` с текущим статусом
- если обработан — `200 OK` и поле `text`

```bash
curl http://localhost:8080/api/v1/pdfs/<guid>/text
```

## Статусы обработки

- `Uploaded` — файл сохранён в MinIO, запись документа создана, outbox заполнен.
- `Queued` — событие опубликовано в RabbitMQ (outbox published).
- `Processing` — Worker начал обработку.
- `Succeeded` — текст извлечён и сохранён.
- `Failed` — ошибка, будет повтор (до лимита).
- `DeadLettered` — превышен лимит попыток / невосстановимая ошибка, сообщение ушло в **DLQ**.
