using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LR__1
{
    public class AddTrackForm : Form
    {
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public string Year { get; set; } // Из int в string
        public AddTrackForm()
        {
            this.Name = "AddTrackForm";
            this.Text = "Добавить трек";
            this.Width = 300;
            this.Height = 200;
            var artistLabel = new Label
            {
                Text = "Исполнитель:",
                Location = new
            System.Drawing.Point(10, 10)
            };
            var titleLabel = new Label
            {
                Text = "Название:",
                Location = new
            System.Drawing.Point(10, 40)
            };
            var genreLabel = new Label
            {
                Text = "Жанр:",
                Location = new
            System.Drawing.Point(10, 70)
            };
            var yearLabel = new Label
            {
                Text = "Год:",
                Location = new System.Drawing.Point(10,
            100)
            };
            var artistTextBox = new TextBox
            {
                Name = "artistTextBox",
                Location = new System.Drawing.Point(120, 10),
                Width = 150
            };
            var titleTextBox = new TextBox
            {
                Name = "titleTextBox",
                Location = new System.Drawing.Point(120, 40),
                Width
            = 150
            };
            var genreTextBox = new TextBox
            {
                Name = "genreTextBox",
                Location = new System.Drawing.Point(120, 70),
                Width = 150
            };
            var yearTextBox = new TextBox
            {
                Name = "yearTextBox",
                Location = new System.Drawing.Point(120, 100),
                Width = 50
            };
            var okButton = new Button
            {
                Name = "okButton",
                Text = "OK",
                Location = new System.Drawing.Point(10,
            140),
                Size = new System.Drawing.Size(75, 23)
            };
            var cancelButton = new Button
            {
                Name = "cancelButton",
                Text = "Отмена",
                Location = new
            System.Drawing.Point(95, 140),
                Size = new System.Drawing.Size(75, 23)
            };
            okButton.Click += (sender, e) =>
            {
                if (int.TryParse(yearTextBox.Text, out int year))
                {
                    Artist = artistTextBox.Text;
                    Title = titleTextBox.Text;
                    Genre = genreTextBox.Text;
                    Year = yearTextBox.Text;
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("Пожалуйста, введите корректный год.");
                }
            };
            cancelButton.Click += (sender, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };
            this.Controls.Add(artistLabel);
            this.Controls.Add(titleLabel);
            this.Controls.Add(genreLabel);
            this.Controls.Add(yearLabel);
            this.Controls.Add(artistTextBox);
            this.Controls.Add(titleTextBox);
            this.Controls.Add(genreTextBox);
            this.Controls.Add(yearTextBox);
            this.Controls.Add(okButton);
            this.Controls.Add(cancelButton);
        }
    }

}
