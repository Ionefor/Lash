# Тесты модуля Users

Этот проект зеркалирует проекты модуля из `src/Users`:

| Путь теста | Тестируемый проект |
| --- | --- |
| `Domain` | `src/Users/Lash.Users.Domain` |
| `Application` | `src/Users/Lash.Users.Application` |
| `Infrastructure` | `src/Users/Lash.Users.Infrastructure` |
| `Messaging` | `src/Users/Lash.Users.Messaging` |
| `Presentation` | `src/Users/Lash.Users.Presentation` |

Сохраняйте дальнейшую структуру каталогов и namespace относительно тестируемого
исходного файла. Например, тест для
`src/Users/Lash.Users.Application/Features/Commands/Login/LoginHandler.cs`
располагается в `Application/Features/Commands/Login/LoginHandlerTests.cs` и
использует namespace `Lash.Users.UnitTests.Application.Features.Commands.Login`.

## Фреймворк и границы

- Все тесты пишутся на xUnit: `[Fact]`, `[Theory]`, `[InlineData]` и `Assert` из
  `Xunit`. Не добавляйте NUnit или MSTest.
- Имена тестов соответствуют шаблону `Method_Scenario_ExpectedResult`.
- Unit-тесты не используют реальную БД, сеть, файловую систему, Docker или текущее
  время; внешние зависимости заменяются mock/fake.
- `Domain` тестируется без DI и инфраструктуры. Для `Application` используйте
  mock/fake портов. Сценарии с реальными внешними компонентами относятся к
  integration-тестам.
- При изменении поведения ошибки проверяйте её код, тип, target и успешный сценарий.

## Проверка

```powershell
dotnet test tests/Users/Lash.Users.UnitTests/Lash.Users.UnitTests.csproj -c Release --no-restore
```
