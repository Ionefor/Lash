# Lash.Web

Единый HTTP-host для модулей Lash. Сейчас подключён модуль Users.

Перед запуском необходимо задать через user secrets, переменные окружения или безопасное хранилище секретов:

- `ConnectionStrings__UsersDatabase` — строка подключения к PostgreSQL;
- `Jwt__Issuer`, `Jwt__Audience`, `Jwt__Key` — параметры подписи access token;
- `Email__Host`, `Email__Port`, `Email__UserName`, `Email__Password`, `Email__FromAddress`, `Email__UseSsl` — параметры SMTP;
- `RabbitMq__Host`, `RabbitMq__VirtualHost`, `RabbitMq__UserName`, `RabbitMq__Password` — параметры RabbitMQ;
- `RolePermissions` — набор доступных прав и назначений прав ролям для сидирования;
- `Admin__Email` и `Admin__Password` — учётные данные начального администратора, если он нужен.

Миграции и сидирование выключены по умолчанию. Для контролируемого локального bootstrap установите `DatabaseInitialization__ApplyMigrationsOnStartup=true`. В production их следует выполнять отдельной deploy-job до запуска реплик приложения.

Liveness endpoint: `GET /health/live`. It reports only whether the HTTP process can respond.

Readiness endpoint: `GET /health/ready`. It includes PostgreSQL and the MassTransit/RabbitMQ bus; when either is unavailable, it returns `503 Service Unavailable` so an orchestrator does not send traffic to the replica.

В окружениях Development и Testing документация API доступна по `/swagger`, а OpenAPI v1 — по `/swagger/v1/swagger.json`. Кнопка **Войти** в верхней панели принимает email и пароль, вызывает `POST /api/v1/auth/login` и автоматически добавляет полученный access token в Swagger UI. Пароль не сохраняется в браузерном хранилище. Для ручного ввода токена по-прежнему доступна кнопка **Authorize**; префикс `Bearer` интерфейс добавит сам.

Пример несекретной части конфигурации:

```json
{
  "RolePermissions": {
    "Permissions": {
      "all": ["users.read", "users.manage"]
    },
    "Roles": {
      "Admin": ["users.read", "users.manage"],
      "Master": ["users.read"],
      "Client": []
    }
  }
}
```
