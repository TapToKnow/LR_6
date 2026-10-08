using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace UITests
{
    // Общая часть UI-тестов: запуск приложения перед каждым тестом, закрытие после него
    // и вспомогательные методы для работы с окнами приложения через FlaUI.
    public abstract class AppTestBase
    {
        // Имя исполняемого файла тестируемого приложения (AssemblyName проекта LR_№6.csproj)
        private const string ExeName = "LR_№1.exe";
        // Можно указать путь к приложению явно через переменную окружения
        private const string ExePathVariable = "MUSICCOLLECTION_EXE";

        protected static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

        protected Application App;
        protected UIA3Automation Automation;
        protected Window MainWindow;
        // Временная папка теста: файлы экспорта/импорта и резервные копии
        protected string TempFolder;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void StartApplication()
        {
            TempFolder = Path.Combine(Path.GetTempPath(), "MusicCollectionUITests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(TempFolder);

            var startInfo = new ProcessStartInfo(FindApplication()) { UseShellExecute = false };
            // Резервные копии приложения складываются во временную папку теста
            startInfo.EnvironmentVariables["MUSICCOLLECTION_BACKUP_DIR"] = BackupFolder;

            App = Application.Launch(startInfo);
            Automation = new UIA3Automation();
            MainWindow = App.GetMainWindow(Automation, Timeout);
            Assert.IsNotNull(MainWindow, "Главное окно приложения не найдено.");
            MainWindow.Focus();
        }

        [TestCleanup]
        public void CloseApplication()
        {
            try
            {
                if (App != null && !App.HasExited)
                {
                    App.Close();
                    if (!App.HasExited)
                        App.Kill();
                }
            }
            catch (Exception)
            {
                // приложение уже закрыто
            }
            Automation?.Dispose();
            App?.Dispose();

            try
            {
                if (TempFolder != null && Directory.Exists(TempFolder))
                    Directory.Delete(TempFolder, true);
            }
            catch (IOException)
            {
                // файл может быть ещё занят закрывающимся приложением — временную папку можно оставить
            }
        }

        protected string BackupFolder => Path.Combine(TempFolder, "Backups");

        // Поиск LR_№1.exe: переменная окружения или папка bin\Debug (bin\Release) проекта LR_№1
        private static string FindApplication()
        {
            string fromVariable = Environment.GetEnvironmentVariable(ExePathVariable);
            if (!string.IsNullOrWhiteSpace(fromVariable) && File.Exists(fromVariable))
                return fromVariable;

            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null)
            {
                foreach (string configuration in new[] { "Debug", "Release" })
                {
                    string candidate = Path.Combine(dir.FullName, "LR_№1", "bin", configuration, ExeName);
                    if (File.Exists(candidate))
                        return candidate;
                }
                dir = dir.Parent;
            }
            throw new FileNotFoundException(
                $"Не найден {ExeName}. Соберите решение или укажите путь в переменной окружения {ExePathVariable}.");
        }

        // ---------- Поиск элементов ----------

        protected static AutomationElement Find(AutomationElement parent, string automationId)
        {
            var element = Retry.WhileNull(
                () => parent.FindFirstDescendant(cf => cf.ByAutomationId(automationId)),
                Timeout, TimeSpan.FromMilliseconds(200)).Result;
            Assert.IsNotNull(element, $"Элемент «{automationId}» не найден.");
            return element;
        }

        protected Button MainButton(string automationId) => Find(MainWindow, automationId).AsButton();

        // Ожидание модального окна главной формы (диалог, окно сообщения) с заголовком, содержащим title
        protected Window WaitModal(string title = null, Window owner = null)
        {
            owner = owner ?? MainWindow;
            var window = Retry.WhileNull(
                () => owner.ModalWindows.FirstOrDefault(w => title == null || (w.Title ?? "").Contains(title)),
                Timeout, TimeSpan.FromMilliseconds(200)).Result;
            Assert.IsNotNull(window, $"Не открылось окно «{title ?? "сообщение"}».");
            return window;
        }

        // Текст стандартного окна сообщения (MessageBox): элемент с AutomationId 65535
        protected static string MessageText(Window messageBox)
        {
            var text = messageBox.FindFirstDescendant(cf => cf.ByAutomationId("65535"));
            if (text != null)
                return text.Name;
            return string.Join(" ", messageBox
                .FindAllDescendants(cf => cf.ByControlType(ControlType.Text))
                .Select(t => t.Name));
        }

        // Стандартные AutomationId кнопок окна сообщения
        protected const string MessageBoxOk = "2";      // единственная кнопка «ОК»
        protected const string MessageBoxYes = "6";
        protected const string MessageBoxNo = "7";
        protected const string MessageBoxCancel = "2";

        protected static void Press(Window window, string automationId)
        {
            Find(window, automationId).AsButton().Click();
            Wait.UntilInputIsProcessed();
        }

        // Ждёт окно сообщения, проверяет текст и закрывает его кнопкой «ОК». Возвращает текст.
        protected string ConfirmMessage(string expectedText = null, Window owner = null)
        {
            var box = WaitModal(null, owner);
            string text = MessageText(box);
            if (expectedText != null)
                StringAssert.Contains(text, expectedText, $"Ожидалось сообщение «{expectedText}», получено «{text}».");
            Press(box, MessageBoxOk);
            return text;
        }

        // ---------- Сценарии работы с формами ----------

        // Открывает окно «Добавить трек», заполняет поля и нажимает «ОК» (сообщение не закрывается)
        protected void FillAddTrackForm(string artist, string title, string genre, string year)
        {
            MainButton("addTrackButton").Click();
            var form = WaitModal("Добавить трек");
            Find(form, "artistTextBox").AsTextBox().Text = artist;
            Find(form, "titleTextBox").AsTextBox().Text = title;
            Find(form, "genreTextBox").AsTextBox().Text = genre;
            Find(form, "yearTextBox").AsTextBox().Text = year;
            Press(form, "okButton");
        }

        // Добавляет трек и закрывает сообщение «Трек добавлен.»
        protected void AddTrack(string artist, string title, string genre, string year)
        {
            FillAddTrackForm(artist, title, genre, year);
            ConfirmMessage("Трек добавлен.");
        }

        // Строки таблицы треков: значения ячеек «Исполнитель | Название | Жанр | Год»
        protected List<string[]> ReadRows()
        {
            var list = Find(MainWindow, "listView");
            var rows = list.FindAllChildren(cf =>
                cf.ByControlType(ControlType.DataItem).Or(cf.ByControlType(ControlType.ListItem)));
            return rows.Select(row =>
            {
                var cells = row.FindAllChildren()
                    .Select(c => c.Name)
                    .Where(n => !string.IsNullOrEmpty(n))
                    .ToArray();
                return cells.Length > 0 ? cells : new[] { row.Name };
            }).ToList();
        }

        // Выделяет строку таблицы треков с указанным исполнителем
        protected void SelectRow(string artist)
        {
            var list = Find(MainWindow, "listView");
            var row = Retry.WhileNull(() => list.FindAllChildren(cf =>
                    cf.ByControlType(ControlType.DataItem).Or(cf.ByControlType(ControlType.ListItem)))
                .FirstOrDefault(r => r.Name == artist ||
                                     r.FindAllChildren().Any(c => c.Name == artist)),
                Timeout).Result;
            Assert.IsNotNull(row, $"Строка с исполнителем «{artist}» не найдена.");
            row.Click();
            Wait.UntilInputIsProcessed();
        }

        protected static string Row(string[] cells) => string.Join(" | ", cells);
    }
}
