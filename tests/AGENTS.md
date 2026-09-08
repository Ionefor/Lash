# Тесты Lash

## Структура

- Тесты группируются сначала по модулю, затем по типу тестирования:
  `tests/<Module>/Lash.<Module>.UnitTests` и, при появлении реальной потребности,
  `tests/<Module>/Lash.<Module>.IntegrationTests`.
- Внутри тестового проекта папки зеркалируют слои исходного модуля: `Domain`,
  `Application`, `Infrastructure`, `Messaging`, `Presentation`.
- Namespace также отражает путь. Например, тест для `src/Users/Lash.Users.Domain/User.cs`
  находится в `tests/Users/Lash.Users.UnitTests/Domain/UserTests.cs` и имеет namespace
  `Lash.Users.UnitTests.Domain`.
- Не создавайте `.gitkeep` и другие файлы-заглушки для пустых папок. Папка появляется в Git
  вместе с первым реальным тестом.

## Как добавить unit-тест

1. Выберите проект `Lash.<Module>.UnitTests` своего модуля.
2. Создайте файл в папке слоя, повторяя дальнейший путь исходного кода.
3. Если проект тестов ещё не ссылается на тестируемый слой, добавьте только нужный
   `ProjectReference`; не подключайте все проекты модуля заранее.
4. Используйте xUnit, схему Arrange / Act / Assert и имя
   `Method_Scenario_ExpectedResult`.
5. Unit-тест не обращается к реальной БД, сети, RabbitMQ, файловой системе, Docker или
   текущему времени. Внешние зависимости заменяются test double; детерминированные данные
   располагаются рядом с тестом в `Fixtures`.
6. Проверьте тесты командой:

   ```powershell
   dotnet test tests/<Module>/Lash.<Module>.UnitTests/Lash.<Module>.UnitTests.csproj -c Release
   ```

## Как добавить новый модуль

1. Создайте `tests/<Module>/Lash.<Module>.UnitTests`.
2. Добавьте в него xUnit-проект на `net10.0` и включите его в `Lash.sln` под solution folder
   `tests/<Module>`.
3. Создайте первую папку слоя только вместе с первым тестом. Остальные появятся по мере
   необходимости.
4. Для тестов с настоящими PostgreSQL, RabbitMQ, HTTP-хостом или контейнерами создавайте
   отдельный `Lash.<Module>.IntegrationTests`, а не расширяйте unit-проект.

## Границы тестов

- Domain тестируется без DI и инфраструктуры.
- Application тестируется с mock/fake портов.
- Infrastructure и Presentation попадают в unit-тесты только когда их можно изолировать;
  сценарии с реальными внешними компонентами относятся к integration-тестам.
- Проверяйте наблюдаемое поведение и типизированные `Result`/`Error`, а не внутренние детали
  реализации.
