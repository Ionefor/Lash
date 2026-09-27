# Unit-тесты Lash.Web

Этот проект зеркалирует структуру `src/Lash.Web`. Тест для файла из
`src/Lash.Web/<Путь>/<Имя>.cs` размещается в `<Путь>/<Имя>Tests.cs` и использует
соответствующий namespace с префиксом `Lash.Web.UnitTests`.

## Фреймворк и границы

- Все тесты пишутся на xUnit: `[Fact]`, `[Theory]`, `[InlineData]` и `Assert` из
  `Xunit`. Не добавляйте NUnit или MSTest.
- Имена тестов соответствуют шаблону `Method_Scenario_ExpectedResult`.
- Unit-тесты изолируют внешние зависимости и не используют реальную БД, сеть,
  файловую систему, Docker или текущее время.
- Проверяйте наблюдаемое HTTP-поведение и типизированные ошибки, а не детали
  реализации.

## Проверка

```powershell
dotnet test tests/Lash.Web.UnitTests/Lash.Web.UnitTests.csproj -c Release --no-restore
```
