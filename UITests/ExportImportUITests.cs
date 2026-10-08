using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace UITests
{
    // Автоматизация тест-кейсов ЛР №5: экспорт, импорт и резервное копирование коллекции
    [TestClass]
    public class ExportImportUITests : AppTestBase
    {
        private const string JsonTwoTracks =
            "[{\"Artist\":\"Кипелов\",\"Title\":\"Вавилон\",\"Genre\":\"Метал\",\"Year\":\"2004\"}," +
            "{\"Artist\":\"Ария\",\"Title\":\"Штиль\",\"Genre\":\"Рок\",\"Year\":\"1997\"}]";

        // ---------- Вспомогательные методы ----------

        private string TempFile(string name) => Path.Combine(TempFolder, name);

        private string CreateFile(string name, string content)
        {
            string path = TempFile(name);
            File.WriteAllText(path, content, new UTF8Encoding(false));
            return path;
        }

        // Текст окна; если окно уже закрывается — пустая строка
        private static string SafeText(Window window)
        {
            try { return MessageText(window) ?? ""; }
            catch (Exception) { return ""; }
        }

        // Ждёт окно сообщения/вопроса с указанным текстом
        private Window WaitMessage(string expectedText)
        {
            var window = Retry.WhileNull(
                () => MainWindow.ModalWindows.FirstOrDefault(w => SafeText(w).Contains(expectedText)),
                Timeout, TimeSpan.FromMilliseconds(200)).Result;
            Assert.IsNotNull(window, $"Не появилось сообщение «{expectedText}».");
            return window;
        }

        private void ConfirmText(string expectedText) => Press(WaitMessage(expectedText), MessageBoxOk);

        // Поле «Имя файла» стандартного диалога Windows (сохранение — 1001, открытие — 1148)
        private static TextBox FileNameBox(Window dialog)
        {
            var edit = Retry.WhileNull(() =>
                    dialog.FindFirstDescendant(cf => cf.ByAutomationId("1001").And(cf.ByControlType(ControlType.Edit))) ??
                    dialog.FindFirstDescendant(cf => cf.ByAutomationId("1148").And(cf.ByControlType(ControlType.Edit))),
                Timeout).Result;
            Assert.IsNotNull(edit, "Не найдено поле «Имя файла» в диалоге.");
            return edit.AsTextBox();
        }

        // Экспорт через кнопку «Экспорт»: тип файла выбирается в диалоге, имя вводится полностью
        private void ExportTo(string path, bool csv)
        {
            MainButton("exportButton").Click();
            var dialog = WaitModal("Экспорт коллекции");
            if (csv)
                Find(dialog, "FileTypeControlHost").AsComboBox().Select("Файлы CSV (*.csv)");
            FileNameBox(dialog).Text = path;
            Press(dialog, "1"); // «Сохранить»
        }

        // Выбор файла в диалоге «Импорт коллекции»
        private void ChooseImportFile(string path)
        {
            MainButton("importButton").Click();
            var dialog = WaitModal("Импорт коллекции");
            FileNameBox(dialog).Text = path;
            Press(dialog, "1"); // «Открыть»
        }

        // Ответ на вопрос «Заменить текущую коллекцию...»
        private void AnswerReplaceQuestion(string buttonId)
        {
            Press(WaitMessage("Заменить текущую коллекцию"), buttonId);
        }

        private void AddTwoTracks()
        {
            AddTrack("Кипелов", "Вавилон", "Метал", "2004");
            AddTrack("Ария", "Штиль", "Рок", "1997");
        }

        // ---------- Экспорт ----------

        // TC-EXP-001: экспорт коллекции в JSON
        [TestMethod]
        public void TC_EXP_001_ExportToJson_FileContainsAllTracks()
        {
            AddTwoTracks();
            string path = TempFile("collection.json");

            ExportTo(path, csv: false);

            ConfirmText("Коллекция экспортирована (2 тр.).");
            Assert.IsTrue(File.Exists(path), "Файл JSON не создан.");
            string json = File.ReadAllText(path, Encoding.UTF8);
            StringAssert.Contains(json, "\"Кипелов\"");
            StringAssert.Contains(json, "\"Штиль\"");
        }

        // TC-EXP-002: экспорт коллекции в CSV
        [TestMethod]
        public void TC_EXP_002_ExportToCsv_HeaderAndRows()
        {
            AddTwoTracks();
            string path = TempFile("collection.csv");

            ExportTo(path, csv: true);

            ConfirmText("Коллекция экспортирована (2 тр.).");
            var lines = File.ReadAllLines(path, Encoding.UTF8);
            CollectionAssert.AreEqual(
                new[] { "Исполнитель;Название;Жанр;Год", "Кипелов;Вавилон;Метал;2004", "Ария;Штиль;Рок;1997" },
                lines);
        }

        // TC-EXP-003: экспорт пустой коллекции
        [TestMethod]
        public void TC_EXP_003_ExportEmptyCollection()
        {
            string path = TempFile("empty.json");

            ExportTo(path, csv: false);

            ConfirmText("Коллекция экспортирована (0 тр.).");
            Assert.AreEqual("[]", File.ReadAllText(path).Trim());
        }

        // TC-EXP-004: отмена экспорта
        [TestMethod]
        public void TC_EXP_004_ExportCancelled_NothingHappens()
        {
            AddTwoTracks();

            MainButton("exportButton").Click();
            var dialog = WaitModal("Экспорт коллекции");
            Press(dialog, "2"); // «Отмена»

            Retry.WhileTrue(() => MainWindow.ModalWindows.Length > 0, Timeout);
            Assert.AreEqual(0, MainWindow.ModalWindows.Length, "После отмены не должно быть сообщений.");
            Assert.AreEqual(0, Directory.GetFiles(TempFolder).Length, "Файл не должен создаваться.");
            Assert.AreEqual(2, ReadRows().Count);
        }

        // ---------- Импорт ----------

        // TC-IMP-001: импорт JSON с заменой коллекции
        [TestMethod]
        public void TC_IMP_001_ImportJson_Replace()
        {
            string path = CreateFile("collection.json", JsonTwoTracks);
            AddTrack("Singer", "Song", "Style", "2020");

            ChooseImportFile(path);
            AnswerReplaceQuestion(MessageBoxYes);

            ConfirmText("Импортировано треков: 2.");
            var artists = ReadRows().Select(r => r[0]).ToArray();
            CollectionAssert.AreEqual(new[] { "Кипелов", "Ария" }, artists);
        }

        // TC-IMP-002: импорт JSON с добавлением к текущей коллекции
        [TestMethod]
        public void TC_IMP_002_ImportJson_Append()
        {
            string path = CreateFile("collection.json", JsonTwoTracks);
            AddTrack("Singer", "Song", "Style", "2020");

            ChooseImportFile(path);
            AnswerReplaceQuestion(MessageBoxNo);

            ConfirmText("Импортировано треков: 2.");
            var artists = ReadRows().Select(r => r[0]).ToArray();
            CollectionAssert.AreEqual(new[] { "Singer", "Кипелов", "Ария" }, artists);
        }

        // TC-IMP-003: повторный импорт не создаёт дубликатов
        [TestMethod]
        public void TC_IMP_003_ImportDuplicates_Skipped()
        {
            AddTwoTracks();
            string path = CreateFile("collection.json", JsonTwoTracks);

            ChooseImportFile(path);
            AnswerReplaceQuestion(MessageBoxNo);

            ConfirmText("Импортировано треков: 0.");
            Assert.AreEqual(2, ReadRows().Count);
        }

        // TC-IMP-004: импорт CSV в пустую коллекцию (без вопроса о замене)
        [TestMethod]
        public void TC_IMP_004_ImportCsv_IntoEmptyCollection()
        {
            string path = CreateFile("collection.csv",
                "Исполнитель;Название;Жанр;Год\r\nКипелов;Вавилон;Метал;2004\r\nАрия;Штиль;Рок;1997\r\n");

            ChooseImportFile(path);

            ConfirmText("Импортировано треков: 2.");
            var rows = ReadRows().Select(Row).ToArray();
            CollectionAssert.AreEqual(
                new[] { "Кипелов | Вавилон | Метал | 2004", "Ария | Штиль | Рок | 1997" }, rows);
        }

        // TC-IMP-005: импорт повреждённого файла
        [TestMethod]
        public void TC_IMP_005_ImportCorruptedFile_ShowsError()
        {
            string path = CreateFile("broken.json", "это не JSON");
            AddTrack("Singer", "Song", "Style", "2020");

            ChooseImportFile(path);
            AnswerReplaceQuestion(MessageBoxYes);

            ConfirmText("Файл повреждён или не является коллекцией в формате JSON.");
            Assert.AreEqual(1, ReadRows().Count, "Коллекция не должна измениться.");
        }

        // TC-IMP-006: файл с некорректным треком — коллекция не меняется
        [TestMethod]
        public void TC_IMP_006_ImportInvalidTrack_CollectionUnchanged()
        {
            string path = CreateFile("invalid.csv", "Ария;Штиль;Рок;1997\r\nКипелов;;Метал;2004\r\n");
            AddTrack("Singer", "Song", "Style", "2020");

            ChooseImportFile(path);
            AnswerReplaceQuestion(MessageBoxYes);

            ConfirmText("Строка 2: Название не может быть пустым");
            var rows = ReadRows();
            Assert.AreEqual(1, rows.Count);
            Assert.AreEqual("Singer", rows[0][0]);
        }

        // TC-IMP-007: отмена импорта на вопросе о замене
        [TestMethod]
        public void TC_IMP_007_ImportCancelled_CollectionUnchanged()
        {
            string path = CreateFile("collection.json", JsonTwoTracks);
            AddTrack("Singer", "Song", "Style", "2020");

            ChooseImportFile(path);
            AnswerReplaceQuestion(MessageBoxCancel);

            Retry.WhileTrue(() => MainWindow.ModalWindows.Length > 0, Timeout);
            Assert.AreEqual(0, MainWindow.ModalWindows.Length);
            Assert.AreEqual(1, ReadRows().Count);
        }

        // ---------- Резервное копирование ----------

        // TC-BAK-001: создание резервной копии
        [TestMethod]
        public void TC_BAK_001_CreateBackup_FileCreated()
        {
            AddTwoTracks();

            MainButton("backupButton").Click();

            string text = SafeText(WaitMessage("Резервная копия создана"));
            StringAssert.Contains(text, BackupFolder);
            ConfirmText("Резервная копия создана");
            var files = Directory.GetFiles(BackupFolder, "backup_*.json");
            Assert.AreEqual(1, files.Length);
            StringAssert.Contains(File.ReadAllText(files[0], Encoding.UTF8), "Кипелов");
        }

        // TC-BAK-002: каждая резервная копия — отдельный файл
        [TestMethod]
        public void TC_BAK_002_TwoBackups_TwoFiles()
        {
            AddTwoTracks();

            MainButton("backupButton").Click();
            ConfirmText("Резервная копия создана");
            MainButton("backupButton").Click();
            ConfirmText("Резервная копия создана");

            Assert.AreEqual(2, Directory.GetFiles(BackupFolder, "backup_*.json").Length);
        }

        // TC-BAK-003: восстановление из последней резервной копии
        [TestMethod]
        public void TC_BAK_003_RestoreLatestBackup()
        {
            AddTwoTracks();
            MainButton("backupButton").Click();
            ConfirmText("Резервная копия создана");
            AddTrack("Singer", "Song", "Style", "2020");
            Assert.AreEqual(3, ReadRows().Count);

            MainButton("restoreButton").Click();
            Press(WaitMessage("Заменить текущую коллекцию последней резервной копией?"), MessageBoxYes);

            ConfirmText("Коллекция восстановлена, треков: 2.");
            var artists = ReadRows().Select(r => r[0]).ToArray();
            CollectionAssert.AreEqual(new[] { "Кипелов", "Ария" }, artists);
        }

        // TC-BAK-004: восстановление при отсутствии резервных копий
        [TestMethod]
        public void TC_BAK_004_RestoreWithoutBackups_ShowsError()
        {
            AddTrack("Singer", "Song", "Style", "2020");

            MainButton("restoreButton").Click();
            Press(WaitMessage("Заменить текущую коллекцию последней резервной копией?"), MessageBoxYes);

            ConfirmText("Резервные копии не найдены.");
            Assert.AreEqual(1, ReadRows().Count);
        }
    }
}
