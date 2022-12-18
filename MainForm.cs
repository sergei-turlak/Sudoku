using Sudoku.Data;
using Sudoku.Game;
using Sudoku.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sudoku
{
    public partial class MainForm : Form
    {
        GameSession game;
        public MainForm()
        {
            InitializeComponent();

            CreateDials();
        }

        public void OnGameEnding(string text, string caption)
        {
            HistoryService historyService = new HistoryService();
            historyService.Add(game);
            game = null;
            MessageBox.Show(text, caption);
        }

        private void newButton_Click(object sender, EventArgs e)
        {
            easyButton.Tag = Level.Easy;
            mediumButton.Tag = Level.Medium;
            hardButton.Tag = Level.Hard;
            Level level = (Level)(sender as Button).Tag;

            game = new GameSession(level, 9);
            game.StartTimer();
            game.OnSuccessfulSolution += () => OnGameEnding("Оце він розумник :)", "Виконано!");
            game.OnFailedSolution += () => OnGameEnding("Почни знову і спробуй робити менше помилок", "Багато помилок!");

            for (int k = 0; k < fieldPanel.Controls.Count; k++)
            {
                fieldPanel.Controls[k].Controls[0].Text = "";
                fieldPanel.Controls[k].Controls[0].ForeColor = Color.SaddleBrown;
            }

            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                {
                    for (int k = 0; k < fieldPanel.Controls.Count; k++)
                    {
                        if (fieldPanel.Controls[k].TabIndex == i * 9 + j)
                        {
                            if (game.DraftField[i, j] != 0)
                                fieldPanel.Controls[k].Controls[0].Text = game.DraftField[i, j].ToString();
                            break;
                        }
                    }
                }

            mistakesLabel.Text = "Помилки: 0/" + (byte)game.Level;
            idLabel.Text = "id: " + game.ID;
            timeLabel.Tag = DateTime.Now;
            Timer timer = new Timer { Interval = 1000, Enabled = true, };
            timer.Tick += (sender2, e2) => { timeLabel.Text = $"{(DateTime.Now - (DateTime)timeLabel.Tag).Minutes} хв : {(DateTime.Now - (DateTime)timeLabel.Tag).Seconds} сек"; };
        }

        private void CreateDials()
        {
            for (int k = 0; k < 81; k++)
            {
                Label cell = (Label)fieldPanel.Controls[k].Controls[0];

                Panel dial = new Panel()
                {
                    Location = new Point(0, 0),
                    Size = new Size(cell.Width, cell.Height),
                    Visible = false
                };
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                    {
                        Label digit = new Label()
                        {
                            AutoSize = false,
                            TextAlign = ContentAlignment.MiddleCenter,
                            Size = new Size(cell.Width / 3 + 2, cell.Height / 3),
                            Location = new Point(j * (cell.Height / 3 + 1), i * (cell.Width / 3 - 1)),
                            Text = (i * 3 + j + 1).ToString(),
                            BorderStyle = BorderStyle.None,
                            Font = new Font("Unispace", 6.8F, FontStyle.Regular, GraphicsUnit.Point, ((byte)204)),
                        };
                        digit.MouseEnter += (sender2, e2) =>
                        {
                            (sender2 as Label).BackColor = Color.SaddleBrown;
                            (sender2 as Label).ForeColor = Color.LavenderBlush;

                            int cursor_i = (sender2 as Label).Parent.Parent.Parent.TabIndex / 9;
                            int cursor_j = (sender2 as Label).Parent.Parent.Parent.TabIndex % 9;
                            for (int l = 0; l < 81; l++)
                            {
                                fieldPanel.Controls[l].Controls[0].BackColor = Color.LavenderBlush;
                                if (fieldPanel.Controls[l].Controls[0].ForeColor != Color.Green)
                                fieldPanel.Controls[l].Controls[0].ForeColor = Color.SaddleBrown;

                                int current_i = fieldPanel.Controls[l].TabIndex / 9;
                                int current_j = fieldPanel.Controls[l].TabIndex % 9;

                                if (current_i == cursor_i && current_j == cursor_j)
                                    continue;
                                if (current_i == cursor_i || current_j == cursor_j)
                                {
                                    fieldPanel.Controls[l].Controls[0].BackColor = Color.DarkSalmon;
                                    if (fieldPanel.Controls[l].Controls[0].ForeColor != Color.Green)
                                        fieldPanel.Controls[l].Controls[0].ForeColor = Color.LavenderBlush;
                                    continue;
                                }

                                byte size = 3;
                                int innerSquare_i = cursor_i / size;
                                int innerSquare_j = cursor_j / size;
                                for (int w = 0; w < size; w++)
                                    for (int s = 0; s < size; s++)
                                    {
                                        if (innerSquare_i * size + w == current_i && innerSquare_j * size + s == current_j)
                                        {
                                            fieldPanel.Controls[l].Controls[0].BackColor = Color.DarkSalmon;
                                            if (fieldPanel.Controls[l].Controls[0].ForeColor != Color.Green)
                                                fieldPanel.Controls[l].Controls[0].ForeColor = Color.LavenderBlush;
                                        }
                                    }
                            }
                        };
                        digit.MouseLeave += (sender2, e2) =>
                        {
                            if ((sender2 as Label).ForeColor != Color.Green)
                                (sender2 as Label).BackColor = Color.LavenderBlush;
                            (sender2 as Label).ForeColor = Color.SaddleBrown;
                        };
                        digit.Click += (sender2, e2) =>
                        {
                            if (game == null) { MessageBox.Show("РОЗПОЧНІТЬ ГРУ!"); return; }
                            byte num = byte.Parse((sender2 as Label).Text);
                            int index = (sender2 as Label).Parent.Parent.Parent.TabIndex;
                            if (game.WriteNumInDraft(num, index / 9, index % 9))
                            {
                                (sender2 as Label).Parent.Visible = false;
                                (sender2 as Label).Parent.Parent.ForeColor = Color.Green;
                                (sender2 as Label).Parent.Parent.Text = num.ToString();
                            }
                            else
                            {
                                if (game == null) return;
                                mistakesLabel.Text = "Помилки: " + ((byte)game.Level - (byte)game.AllowableMistakesLeft) + "/" + (byte)game.Level;
                                (sender2 as Label).Parent.Visible = false;
                                (sender2 as Label).Parent.Parent.Text = num.ToString();
                                (sender2 as Label).Parent.Parent.BackColor = Color.Red;
                                Timer timer = new Timer();
                                timer.Interval = 1300;
                                timer.Start();
                                timer.Tick += (sender3, e3) =>
                                {
                                    (sender2 as Label).Parent.Parent.Text = "";
                                    (sender2 as Label).Parent.Parent.BackColor = Color.LavenderBlush;
                                    cell_MouseEnter((sender2 as Label).Parent.Parent, null);
                                    timer.Stop();
                                };
                            }
                        };
                        dial.Controls.Add(digit);
                    }
                cell.Controls.Add(dial);
            }
        }

        private void newButton_MouseEnter(object sender, EventArgs e)
        {
            newButton.Visible = false;
            easyButton.Visible = mediumButton.Visible = hardButton.Visible = true;
        }

        private void hardButton_MouseLeave(object sender, EventArgs e)
        {
            newButton.Visible = true;
            easyButton.Visible = mediumButton.Visible = hardButton.Visible = false;
        }

        private void cell_MouseEnter(object sender, EventArgs e)
        {
            if ((sender as Label).Text != "")
                return;

            for (int k = 0; k < 81; k++)
            {
                Panel dial = (Panel)fieldPanel.Controls[k].Controls[0].Controls[0];
                dial.Visible = false;
            }

            Label cell = (Label)sender;
            cell.Controls[0].Visible = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("\t\tФІНАЛЬНА РОБОТА. СУДОКУ\n\t\tТР-15 Турлак Сергій\n\nПРАВИЛА І МЕТА:" +
                " Мета головоломки — необхідно заповнити вільні клітинки цифрами від 1 до 9 так, щоб в кожному рядку, " +
                "в кожному стовпці і в кожному малому квадраті 3×3, кожна цифра зустрічалася лише один раз. "
                , "Інформація. Правила");
        }

        private void historyButton_Click(object sender, EventArgs e)
        {
            HistoryService historyService = new HistoryService();
            List<History> histories = historyService.GetAll();
            string output = "\t\tІСТОРІЯ ІГОР\n\n";
            if (histories.Count == 0)
                output += "Поки жодних ігор!";
            foreach (var history in histories)
            {
                output += $"ID: {history.ID}\n";
                output += $"Дата: {history.Date.ToString()}\n";
                output += $"Рівень складності: {history.Level.ToString()}\n";

                if (history.IsGameSuccessfullySolved)
                    output += $"Результат: успішно розгадано\n";
                else
                    output += $"Результат: не завершено через помилки\n";
                output += $"Зроблено {(byte)history.Level - history.AllowableMistakesLeft} помилок з {(byte)history.Level} можливих\n";
                output += $"Ігровий час: {history.PlayingTime.ToString()}\n\n";
            }
            MessageBox.Show(output, "Історія з бази даних");
        }
    }
}
