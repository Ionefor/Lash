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

Readiness endpoint: `GET /health/ready`. It includes registered dependency health checks.

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
