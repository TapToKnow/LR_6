using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace UITests
{
    // Автоматизация тест-кейсов из ЛР №3 (с учётом исправлений ЛР №4):
    // добавление, удаление, отображение, поиск и сортировка треков.
    [TestClass]
    public class MusicCollectionUITests : AppTestBase
    {
        // TC-001: добавление трека с корректными данными
        [TestMethod]
        public void TC001_AddTrack_ValidData_TrackAdded()
        {
            // Arrange + Act
            FillAddTrackForm("Singer", "Song", "Style", "2020");

            // Assert: сообщение и строка в таблице
            ConfirmMessage("Трек добавлен.");
            var rows = ReadRows();
            Assert.AreEqual(1, rows.Count, "В таблице должен быть один трек.");
            Assert.AreEqual("Singer | Song | Style | 2020", Row(rows[0]));
        }

        // TC-002 (ЛР4): сообщение об ошибке при пустом поле «Название»
        [TestMethod]
        public void TC002_AddTrack_EmptyTitle_ShowsError()
        {
            FillAddTrackForm("Singer", "", "Style", "2020");

            var box = WaitModal("Ошибка ввода");
            StringAssert.Contains(MessageText(box), "Название не может быть пустым");
            Press(box, MessageBoxOk);
            Assert.AreEqual(0, ReadRows().Count, "Трек с пустым названием не должен добавляться.");
        }

        // Проверка граничных значений года: год в будущем
        [TestMethod]
        public void TC002_AddTrack_YearInFuture_ShowsError()
        {
            FillAddTrackForm("Singer", "Song", "Style", (DateTime.Now.Year + 1).ToString());

            var box = WaitModal("Ошибка ввода");
            StringAssert.Contains(MessageText(box), "Год должен быть от 1 до");
            Press(box, MessageBoxOk);
            Assert.AreEqual(0, ReadRows().Count);
        }

        // Год не числом: окно добавления не закрывается и просит ввести корректный год
        [TestMethod]
        public void TC002_AddTrack_YearNotNumber_ShowsWarning()
        {
            MainButton("addTrackButton").Click();
            var form = WaitModal("Добавить трек");
            Find(form, "artistTextBox").AsTextBox().Text = "Singer";
            Find(form, "titleTextBox").AsTextBox().Text = "Song";
            Find(form, "genreTextBox").AsTextBox().Text = "Style";
            Find(form, "yearTextBox").AsTextBox().Text = "две тысячи";
            Press(form, "okButton");

            ConfirmMessage("Пожалуйста, введите корректный год.", form);
            Press(form, "cancelButton");
            Assert.AreEqual(0, ReadRows().Count);
        }

        // TC-003: удаление выбранного трека
        [TestMethod]
        public void TC003_RemoveTrack_Selected_TrackRemoved()
        {
            AddTrack("Singer", "Song", "Style", "2020");
            AddTrack("Кипелов", "Вавилон", "Метал-рок", "2004");

            SelectRow("Singer");
            MainButton("removeTrackButton").Click();

            ConfirmMessage("Трек удалён.");
            var rows = ReadRows();
            Assert.AreEqual(1, rows.Count);
            Assert.AreEqual("Кипелов", rows[0][0]);
        }

        // Удаление без выбора трека
        [TestMethod]
        public void TC003_RemoveTrack_NothingSelected_ShowsMessage()
        {
            AddTrack("Singer", "Song", "Style", "2020");

            MainButton("removeTrackButton").Click();

            ConfirmMessage("Сначала выберите трек для удаления.");
            Assert.AreEqual(1, ReadRows().Count);
        }

        // TC-004: сообщение об успешном добавлении трека
        [TestMethod]
        public void TC004_AddTrack_ShowsSuccessMessage()
        {
            FillAddTrackForm("Singer", "Song", "Style", "1989");

            string text = ConfirmMessage();
            Assert.AreEqual("Трек добавлен.", text.Trim());
        }

        // TC-005: в таблице отображаются все треки со всеми атрибутами
        [TestMethod]
        public void TC005_List_ShowsAllTracksWithAttributes()
        {
            AddTrack("Singer", "Song", "Style", "2020");
            AddTrack("Кипелов", "Вавилон", "Метал-рок", "2004");

            var rows = ReadRows().Select(Row).ToList();

            CollectionAssert.AreEqual(
                new[] { "Singer | Song | Style | 2020", "Кипелов | Вавилон | Метал-рок | 2004" },
                rows);
        }

        // TC-006: отображение обновляется после добавления и удаления
        [TestMethod]
        public void TC006_List_UpdatesAfterAddAndRemove()
        {
            AddTrack("Singer", "Song", "Style", "2020");
            AddTrack("Кипелов", "Вавилон", "Метал-рок", "2004");
            Assert.AreEqual(2, ReadRows().Count);

            AddTrack("Ария", "Штиль", "Рок", "1997");
            Assert.AreEqual(3, ReadRows().Count, "После добавления должно стать 3 трека.");

            SelectRow("Ария");
            MainButton("removeTrackButton").Click();
            ConfirmMessage("Трек удалён.");
            Assert.AreEqual(2, ReadRows().Count, "После удаления должно остаться 2 трека.");
        }

        // TC-007: окно «Поиск по исполнителю» открывается, все элементы видны
        [TestMethod]
        public void TC007_SearchWindow_OpensWithAllControls()
        {
            MainButton("searchByArtistButton").Click();

            var form = WaitModal("Поиск по исполнителю");
            var textBox = Find(form, "searchArtistTextBox");
            var ok = Find(form, "okButton");
            var cancel = Find(form, "cancelButton");
            Assert.IsFalse(textBox.IsOffscreen, "Поле ввода исполнителя должно быть видно.");
            Assert.IsFalse(ok.IsOffscreen, "Кнопка «ОК» должна помещаться в окно.");
            Assert.IsFalse(cancel.IsOffscreen, "Кнопка «Отмена» должна помещаться в окно.");
            Assert.IsTrue(form.BoundingRectangle.Contains(cancel.BoundingRectangle),
                "Кнопка «Отмена» должна полностью помещаться в окно.");

            Press(form, "cancelButton");
        }

        // TC-008: поиск выводит все треки исполнителя
        [TestMethod]
        public void TC008_Search_ShowsOnlyArtistTracks()
        {
            AddTrack("Кипелов", "Вавилон", "Метал-рок", "2004");
            AddTrack("Singer", "Song", "Style", "2020");
            AddTrack("Кипелов", "Я свободен", "Рок", "1997");

            MainButton("searchByArtistButton").Click();
            var form = WaitModal("Поиск по исполнителю");
            Find(form, "searchArtistTextBox").AsTextBox().Text = "Кипелов";
            Press(form, "okButton");
            ConfirmMessage("Найденные треки:");

            var rows = ReadRows();
            Assert.AreEqual(2, rows.Count);
            Assert.IsTrue(rows.All(r => r[0] == "Кипелов"));
        }

        // Поиск исполнителя, которого нет в коллекции
        [TestMethod]
        public void TC008_Search_UnknownArtist_ShowsNotFound()
        {
            AddTrack("Singer", "Song", "Style", "2020");

            MainButton("searchByArtistButton").Click();
            var form = WaitModal("Поиск по исполнителю");
            Find(form, "searchArtistTextBox").AsTextBox().Text = "Ария";
            Press(form, "okButton");

            ConfirmMessage("Треки не найдены.");
        }

        // TC-009, TC-010: сортировка по году и обновление таблицы
        [TestMethod]
        public void TC009_SortByYear_OrdersTracksAscending()
        {
            AddTrack("Singer", "Song", "Style", "2020");
            AddTrack("Ария", "Штиль", "Рок", "1989");
            AddTrack("Кипелов", "Вавилон", "Метал-рок", "2004");

            MainButton("sortByYearButton").Click();
            ConfirmMessage("Сортировка по году выполнена.");

            var years = ReadRows().Select(r => r[r.Length - 1]).ToArray();
            CollectionAssert.AreEqual(new[] { "1989", "2004", "2020" }, years);
        }
    }
}
