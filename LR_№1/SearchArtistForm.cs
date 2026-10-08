using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LR__1
{
    public class SearchArtistForm : Form
    {
        public string Artist { get; set; }
        public SearchArtistForm()
        {
            this.Name = "SearchArtistForm";
            this.Text = "Поиск по исполнителю";
            this.Width = 300;
            this.Height = 150;
            var artistLabel = new Label
            {
                Text = "Исполнитель:",
                Location = new
            System.Drawing.Point(10, 10)
            };
            var artistTextBox = new TextBox
            {
                Name = "searchArtistTextBox",
                Location = new System.Drawing.Point(10, 35),
                Width
            = 260
            };
            var okButton = new Button
            {
                Name = "okButton",
                Text = "OK",
                Location = new System.Drawing.Point(10,
            60),
                Size = new System.Drawing.Size(75, 23)
            };
            var cancelButton = new Button
            {
                Name = "cancelButton",
                Text = "Отмена",
                Location = new
            System.Drawing.Point(95, 60),
                Size = new System.Drawing.Size(75, 23)
            };
            okButton.Click += (sender, e) =>
            {
                Artist = artistTextBox.Text;
                DialogResult = DialogResult.OK;
                Close();
            };
            cancelButton.Click += (sender, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };
            this.Controls.Add(artistLabel);
            this.Controls.Add(artistTextBox);
            this.Controls.Add(okButton);
            this.Controls.Add(cancelButton);
        }
    }
}
