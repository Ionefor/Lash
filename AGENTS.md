# Правила разработки Lash

## Архитектура

- Каждый модуль находится в `src/<Module>` и разделяется на `Domain`, `Application`, `Infrastructure`, `Presentation` и при необходимости `Messaging`.
- `Domain` не зависит от ASP.NET Core, EF Core, базы данных, транспорта и других слоёв.
- `Application` содержит use cases, порты, команды, запросы и валидацию. Он зависит только от Domain и абстракций.
- `Infrastructure` реализует порты Application, хранение данных, Identity, JWT, внешние сервисы и миграции.
- `Presentation` преобразует HTTP-запросы в команды и результаты use cases в HTTP-ответы. Бизнес-правила в контроллерах не размещаются.
- Новые пакеты должны поддерживать `net10.0`. Пакеты семейства WebFlow используют одну согласованную версию во всех проектах модуля.

## Application features

- Command feature размещается в `Application/Features/Commands/<FeatureName>` и содержит `<FeatureName>Command.cs`, `<FeatureName>Handler.cs` и при наличии входных ограничений `<FeatureName>CommandValidator.cs`.
- Query feature размещается в `Application/Features/Queries/<FeatureName>` и содержит `<FeatureName>Query.cs`, `<FeatureName>Handler.cs` и при наличии входных ограничений `<FeatureName>QueryValidator.cs`.
- Command и query содержат только входные данные use case. Они не содержат HTTP-типов, инфраструктурных типов, бизнес-правил и логирования.
- Handler реализует соответствующий `ICommandHandler` или `IQueryHandler`. Он сначала валидирует вход, затем вызывает порты Application, применяет правила use case и возвращает `Result` или `UnitResult`.
- Handler не знает о HTTP, EF Core, Identity, MassTransit, RabbitMQ, реализации хранилища или транспорте. Внешние операции выполняются через порты из `Application/Abstractions`.
- Переиспользуемые FluentValidation-правила размещаются в `Application/Extensions`. Модульные ошибки размещаются в `Application/Errors`, а интеграционные контракты — в проекте `Contracts`.
- Если use case принимает решение на основе текущего времени, handler получает `TimeProvider` через конструктор и использует `GetUtcNow()`. Прямой вызов `DateTimeOffset.UtcNow` или `DateTime.UtcNow` в Application запрещён.
- Unit-тесты feature повторяют путь исходников в `tests/<Module>/Lash.<Module>.UnitTests/Application/Features`. Они покрывают успешный сценарий, каждый изменённый error code/type/target и отсутствие вызова портов при невалидном вводе.

## Логирование по слоям

- `Domain` не использует `ILogger` и не пишет логи. Доменные правила выражаются через `Result` и `Error`.
- `Application` логирует бизнес-значимые результаты use case: `Information` для успешно завершённой операции, `Warning` для ожидаемого отказа или отклонения (валидация, конфликт, rate limit, неверные credentials), `Error` для нарушения обязательной конфигурации или состояния, при котором use case не может продолжаться.
- `Infrastructure` логирует внешние границы и технические операции: успешную доставку или публикацию значимого сообщения — `Information`, временный отказ внешнего сервиса — `Warning` с исключением, сбой bootstrap, миграции, seed или consumer — `Error` с исключением перед повторным выбросом.
- `Presentation` не дублирует логи use case. Неожиданные исключения логируются централизованно на HTTP-границе; стандартные 4xx и rate-limit ответы не логируются контроллерами.
- В шаблонах используйте английский язык и структурированные поля с устойчивыми именами: `{UserId}`, `{RoleName}`, `{Operation}`, `{SeederName}`. Не пишите email, пароль, confirmation/reset code, access/refresh token, JWT claims, connection string, SMTP credentials или персональные данные.
- Не создавайте строки, анонимные объекты, сериализацию или вычисления только ради лога. Используйте параметризованные вызовы `ILogger`; высокочастотные успешные технические действия оставляйте без лога или на `Debug`.
- Не логируйте одну и ту же ошибку на каждом слое: ожидаемый результат пишет владелец use case, техническое исключение — граница, которая его обрабатывает или повторно выбрасывает.

## Ошибки и исключения

- Ожидаемый результат use case возвращается через `Result<T, ErrorList>` или `UnitResult<ErrorList>` из CSharpFunctionalExtensions.
- Ожидаемая доменная операция возвращает `Result<T, Error>` или `UnitResult<Error>`.
- Для типовых ошибок используются `GeneralErrors` и `AuthErrors` из ErrorsFlow. Не создавайте новый код, если встроенный код точно передаёт смысл ошибки.
- Для смысла, специфичного для модуля, создавайте статический каталог `*ErrorCodes` и `*Errors` через `ErrorFactory.Create`. Код ошибки стабилен, имеет префикс модуля, например `users.required_role.not_configured`.
- `ErrorType` должен описывать смысл ошибки: `Validation`, `Conflict`, `NotFound`, `Unauthorized`, `Forbidden` или `Failure`. `InternalServer` формируется только на внешней границе при неожиданном сбое.
- Ошибки не содержат пароли, refresh-токены, connection strings, персональные данные и внутренние детали хранилища.
- Не выбрасывайте исключения для ошибок валидации, отсутствующих сущностей в use case, конфликтов, истёкших или отозванных токенов. Возвращайте ErrorsFlow-ошибку.
- Исключения допустимы для нарушения контракта программистом, непредвиденного технического сбоя и невозможного продолжения bootstrap: некорректной startup-конфигурации, ошибки миграции или недоступной БД. Эти исключения обрабатываются и логируются на границе приложения.
- Не обращайтесь к `Result.Value`, пока failure не исключён проверкой либо структурой кода. Не используйте текст внешнего исключения как текст API-ошибки.

## HTTP и безопасность

- Контроллеры передают `ErrorList` в `ApplicationController.Error`; вручную не сопоставляют ErrorType и HTTP-статусы.
- Не раскрывайте факт существования refresh-токена. Некорректный или отозванный refresh-токен возвращает `AuthErrors.RefreshTokenInvalid`, истёкший — `AuthErrors.RefreshTokenExpired`.
- Проверку access token без lifetime разрешается выполнять только в специально названном пути refresh-flow.
- JWT и startup options валидируются до обслуживания запросов. Секреты не добавляются в исходный код, миграции и тестовые фикстуры.

## Данные и тесты

- Миграции создаются только после изменения модели EF Core. Сгенерированные миграции вручную не редактируются без необходимости.
- Сидирование должно быть идемпотентным. Ошибка конфигурации seed-данных останавливает bootstrap с безопасным диагностическим сообщением.
- Unit-тесты размещаются в `tests/<Module>/Lash.<Module>.UnitTests`, повторяя путь тестируемого слоя. Используйте xUnit и имена `Method_Scenario_ExpectedResult`.
- Unit-тесты не используют реальную БД, сеть, файловую систему, Docker и текущее время. Для интеграций создаётся отдельный integration test project.
- Каждое изменение поведения ошибки покрывается тестом её кода, типа и target, а также тестом успешного сценария.

## Проверка

- Перед завершением работы выполняйте `dotnet build Lash.sln --configuration Release --no-restore` и тесты затронутого модуля.
- Для изменений форматирования выполняйте `dotnet format Lash.sln --verify-no-changes --no-restore`.
