# Lash.Web

Единый HTTP-host для модулей Lash. Сейчас подключён модуль Users.

Перед запуском необходимо задать через user secrets, переменные окружения или безопасное хранилище секретов:

- `ConnectionStrings__UsersDatabase` — строка подключения к PostgreSQL;
- `Jwt__Issuer`, `Jwt__Audience`, `Jwt__Key` — параметры подписи access token;
- `RolePermissions` — набор доступных прав и назначений прав ролям для сидирования;
- `Admin__Email` и `Admin__Password` — учётные данные начального администратора, если он нужен.

Миграции и сидирование выключены по умолчанию. Для контролируемого локального bootstrap установите `DatabaseInitialization__ApplyMigrationsOnStartup=true`. В production их следует выполнять отдельной deploy-job до запуска реплик приложения.

Liveness endpoint: `GET /health/live`.

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
