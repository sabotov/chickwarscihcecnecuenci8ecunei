# Анализ и минимальная архитектура локальной боевой системы (Unity)

## Что нужно из требований
- Бой должен запускаться **по кнопке**.
- Данные (персонажи, монстры, скиллы) должны храниться **локально в Unity**.
- Формат хранения может быть JSON.
- NGUI использовать не обязательно.
- На сцене уже есть поле, `Tile` и `FieldScriptWrapper`.

## Предложенная схема
1. `LocalBattleDatabase` загружает `TextAsset` JSON в память.
2. `BattleButtonController` (вешается на UI Button или любой объект) запускает метод `StartBattle()`.
3. `BattleActionResolver` проводит пошаговый 1v1 бой до победителя.
4. `SimpleAiController` выбирает доступный скилл (без кулдауна, с максимальной силой).
5. После боя можно обновить поле (`Tile`) — в примере подсвечивается первая клетка.

## Где что находится
- `Core/LocalBattleModels.cs` — модели данных JSON и runtime-сущности.
- `Core/LocalBattleDatabase.cs` — загрузка JSON и доступ к данным.
- `Actions/BattleActionResolver.cs` — расчёт боя.
- `AI/SimpleAiController.cs` — выбор действия AI.
- `SideControllers/BattleButtonController.cs` — запуск боя по кнопке и интеграция с `FieldScriptWrapper`.
- `Core/sample-battle-data.json` — пример локальной базы монстров/скиллов.

## Как подключить в Unity (быстро)
1. Создай `TextAsset` из `sample-battle-data.json` (или используй свой JSON).
2. На объект сцены добавь `LocalBattleDatabase` и назначь `battleDataJson`.
3. На UI кнопку добавь `BattleButtonController`:
   - `database` -> ссылка на `LocalBattleDatabase`
   - `playerMonsterId` и `enemyMonsterId` -> ID из JSON
   - опционально `fieldScriptWrapper` -> ссылка на объект поля
4. Нажатие кнопки вызывает `StartBattle()` и печатает лог боя в Console.

## Почему это удобно
- Полностью локально (оффлайн), без сервера.
- Простая точка входа: один метод для кнопки.
- Легко масштабировать: добавить эффекты, очередность, типы урона, 2x2/3x3 сетку, PvE волны.
- Можно постепенно подменить старые части без тотального переписывания.
