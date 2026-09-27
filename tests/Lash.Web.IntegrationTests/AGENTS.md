# Интеграционные тесты Lash.Web

Этот проект зеркалирует структуру `src/Lash.Web`. Тест для файла из
`src/Lash.Web/<Путь>/<Имя>.cs` размещается в `<Путь>/<Имя>Tests.cs` и использует
соответствующий namespace с префиксом `Lash.Web.IntegrationTests`.

## Фреймворк и границы

- Все тесты пишутся на xUnit: `[Fact]`, `[Theory]`, `[InlineData]` и `Assert` из
  `Xunit`. Не добавляйте NUnit или MSTest.
- Имена тестов соответствуют шаблону `Method_Scenario_ExpectedResult`.
- Здесь размещаются только сценарии, которым необходимы интеграционные границы:
  HTTP-хост, реальная БД, брокер или контейнер. Для изолируемой логики используйте
  `Lash.Web.UnitTests`.
- Фикстуры не содержат секретов, токенов, паролей или персональных данных.

## Проверка

```powershell
dotnet test tests/Lash.Web.IntegrationTests/Lash.Web.IntegrationTests.csproj -c Release --no-restore
```
