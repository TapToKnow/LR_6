# LR_№1
## Лабораторная работа №5. Новая функция «Экспорт и импорт коллекции»

Вариант 23 — «Управление музыкальной коллекцией».

### Функциональные требования

1. **Экспорт коллекции.** Пользователь может сохранить всю коллекцию в файл (кнопка «Экспорт»).
2. **Импорт коллекции.** Пользователь может загрузить коллекцию из файла (кнопка «Импорт»): заменить текущую коллекцию или добавить треки к ней. Треки, которые уже есть в коллекции, повторно не добавляются.
3. **Форматы файлов.** Поддерживаются JSON и CSV (разделитель `;`, кодировка UTF-8). Формат определяется по расширению файла.
4. **Резервное копирование.** Кнопка «Резервная копия» сохраняет коллекцию в папку `Backups` рядом с программой (`backup_ГГГГММДД_ччммсс.json`), кнопка «Восстановить» загружает последнюю резервную копию.
5. **Проверка данных.** Треки из файла проверяются так же, как при ручном вводе (пустые поля, формат и диапазон года). При ошибке выводится номер трека или строки, а коллекция не меняется.

### Тестирование

* Модульные тесты: проект `UnitTest` — `UnitTestExportImport` (16 тестов), `UnitTestBackup` (6 тестов).
* Тест-кейсы для ручной проверки: `Тест-кейсы/Тест_кейсы_к_ЛР_5.xlsx` (15 тест-кейсов).

## Лабораторная работа №6. Автоматизация тестирования пользовательского интерфейса

Инструменты: C#, MSTest, FlaUI 5 (UIA3). Тесты находятся в проекте `UITests` (.NET Framework 4.8).

### Запуск

1. Откройте `LR_№6.slnx` в Visual Studio. Пакеты NuGet (MSTest, FlaUI) восстановятся автоматически.
2. Соберите решение. Проект `UITests` ссылается на `LR_№6.csproj`, поэтому приложение `LR_№1.exe` собирается первым.
3. Запустите тесты в «Обозревателе тестов». Во время прогона не трогайте мышь и клавиатуру: тесты сами нажимают кнопки в окнах приложения.

Тесты ищут `LR_№1\bin\Debug\LR_№1.exe` (или `bin\Release`) в папках выше проекта тестов. Другой путь можно задать переменной окружения `MUSICCOLLECTION_EXE`.
Перед каждым тестом приложение запускается заново, а после теста закрывается. Резервные копии во время тестов сохраняются во временную папку (переменная `MUSICCOLLECTION_BACKUP_DIR`), реальная папка `Backups` не затрагивается.

### Поиск элементов

Элементам форм заданы имена (`Name`), которые FlaUI видит как `AutomationId`:

| Форма | AutomationId |
|---|---|
| Главная | `listView`, `addTrackButton`, `removeTrackButton`, `searchByArtistButton`, `sortByYearButton`, `exportButton`, `importButton`, `backupButton`, `restoreButton` |
| «Добавить трек» | `artistTextBox`, `titleTextBox`, `genreTextBox`, `yearTextBox`, `okButton`, `cancelButton` |
| «Поиск по исполнителю» | `searchArtistTextBox`, `okButton`, `cancelButton` |
| Окно сообщения Windows | текст — `65535`; кнопки: «ОК» — `2`, «Да» — `6`, «Нет» — `7`, «Отмена» — `2` |
| Диалоги сохранения и открытия файла | имя файла — `1001` / `1148`, тип файла — `FileTypeControlHost`, «Сохранить»/«Открыть» — `1`, «Отмена» — `2` |

### Тест-кейсы ЛР №3 (с исправлениями ЛР №4) — класс `MusicCollectionUITests`

| Тест | Тест-кейс | Проверка |
|---|---|---|
| `TC001_AddTrack_ValidData_TrackAdded` | TC-001 | трек с корректными данными появляется в таблице |
| `TC002_AddTrack_EmptyTitle_ShowsError` | TC-002 | пустое название → «Название не может быть пустым», трек не добавлен |
| `TC002_AddTrack_YearInFuture_ShowsError` | TC-002 | год больше текущего → сообщение об ошибке |
| `TC002_AddTrack_YearNotNumber_ShowsWarning` | TC-002 | год не числом → «Пожалуйста, введите корректный год.» |
| `TC003_RemoveTrack_Selected_TrackRemoved` | TC-003 | выбранный трек удаляется, «Трек удалён.» |
| `TC003_RemoveTrack_NothingSelected_ShowsMessage` | TC-003 | удаление без выбора → «Сначала выберите трек для удаления.» |
| `TC004_AddTrack_ShowsSuccessMessage` | TC-004 | сообщение «Трек добавлен.» |
| `TC005_List_ShowsAllTracksWithAttributes` | TC-005 | в таблице все треки и все четыре столбца |
| `TC006_List_UpdatesAfterAddAndRemove` | TC-006 | таблица обновляется после добавления и удаления |
| `TC007_SearchWindow_OpensWithAllControls` | TC-007 | окно поиска открывается, поле и кнопки помещаются в окно |
| `TC008_Search_ShowsOnlyArtistTracks` | TC-008 | поиск выводит только треки исполнителя |
| `TC008_Search_UnknownArtist_ShowsNotFound` | TC-008 | «Треки не найдены.» |
| `TC009_SortByYear_OrdersTracksAscending` | TC-009, TC-010 | треки отсортированы по году по возрастанию |

### Тест-кейсы ЛР №5 — класс `ExportImportUITests`

| Тест | Тест-кейс | Проверка |
|---|---|---|
| `TC_EXP_001_ExportToJson_FileContainsAllTracks` | TC-EXP-001 | файл JSON создан, в нём все треки |
| `TC_EXP_002_ExportToCsv_HeaderAndRows` | TC-EXP-002 | CSV: заголовок и строки треков |
| `TC_EXP_003_ExportEmptyCollection` | TC-EXP-003 | пустая коллекция → `[]` |
| `TC_EXP_004_ExportCancelled_NothingHappens` | TC-EXP-004 | отмена: файла и сообщений нет |
| `TC_IMP_001_ImportJson_Replace` | TC-IMP-001 | «Да» — коллекция заменена |
| `TC_IMP_002_ImportJson_Append` | TC-IMP-002 | «Нет» — треки добавлены к текущим |
| `TC_IMP_003_ImportDuplicates_Skipped` | TC-IMP-003 | дубликаты не добавляются |
| `TC_IMP_004_ImportCsv_IntoEmptyCollection` | TC-IMP-004 | импорт CSV в пустую коллекцию без вопроса о замене |
| `TC_IMP_005_ImportCorruptedFile_ShowsError` | TC-IMP-005 | повреждённый файл → сообщение, коллекция не изменилась |
| `TC_IMP_006_ImportInvalidTrack_CollectionUnchanged` | TC-IMP-006 | «Строка 2: Название не может быть пустым», коллекция не изменилась |
| `TC_IMP_007_ImportCancelled_CollectionUnchanged` | TC-IMP-007 | «Отмена» на вопросе — ничего не меняется |
| `TC_BAK_001_CreateBackup_FileCreated` | TC-BAK-001 | файл `backup_*.json` создан |
| `TC_BAK_002_TwoBackups_TwoFiles` | TC-BAK-002 | две копии — два файла |
| `TC_BAK_003_RestoreLatestBackup` | TC-BAK-003 | коллекция восстановлена из последней копии |
| `TC_BAK_004_RestoreWithoutBackups_ShowsError` | TC-BAK-004 | «Резервные копии не найдены.» |

Таблицы тест-кейсов: папка `Тест-кейсы` (ЛР №3, №4, №5).
