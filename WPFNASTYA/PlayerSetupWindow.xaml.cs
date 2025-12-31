using CardGame.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CardGame.GUI
{
    public partial class PlayerSetupWindow : Window
    {
        public string Player1Name { get; private set; }
        public string Player2Name { get; private set; }
        public string Player1Faction { get; private set; }
        public string Player2Faction { get; private set; }

        public Faction Player1FactionEnum => ConvertStringToFaction(Player1Faction);
        public Faction Player2FactionEnum => ConvertStringToFaction(Player2Faction);

        private Dictionary<string, string> _factionDescriptions;
        private Dictionary<string, string> _factionTitles;

        public PlayerSetupWindow()
        {
            InitializeComponent();
            InitializeFactionData();
            SetupEventHandlers();
            SetupTextBoxPlaceholders();

            // Устанавливаем начальные значения фракций
            Player1FactionBox.SelectedIndex = 0;
            Player2FactionBox.SelectedIndex = 1;

            // Устанавливаем начальные описания
            UpdateFactionDescription(1, "Люди");
            UpdateFactionDescription(2, "Звери");
        }

        private void InitializeFactionData()
        {
            _factionDescriptions = new Dictionary<string, string>
            {
                {
                    "Люди",
                    "Стратегия баланса и защиты\n• Универсальные карты\n• Хорошая защита\n• Стабильная стратегия"
                },
                {
                    "Звери",
                    "Агрессия и скорость\n• Быстрые атаки\n• Внезапные нападения\n• Высокая мобильность"
                },
                {
                    "Мифические",
                    "Магия и способности\n• Мощные заклинания\n• Уникальные эффекты\n• Магическая сила"
                },
                {
                    "Стихии",
                    "Контроль и выносливость\n• Контроль поля\n• Высокая живучесть\n• Элементальные силы"
                }
            };

            _factionTitles = new Dictionary<string, string>
            {
                { "Люди", "Королевство Людей" },
                { "Звери", "Племена Зверей" },
                { "Мифические", "Мифические Существа" },
                { "Стихии", "Стихийные Элементы" }
            };
        }

        private void SetupEventHandlers()
        {
            // Обработчики для ComboBox
            Player1FactionBox.SelectionChanged += (s, e) =>
            {
                var faction = GetSelectedFaction(1);
                if (!string.IsNullOrEmpty(faction))
                    UpdateFactionDescription(1, faction);
            };

            Player2FactionBox.SelectionChanged += (s, e) =>
            {
                var faction = GetSelectedFaction(2);
                if (!string.IsNullOrEmpty(faction))
                    UpdateFactionDescription(2, faction);
            };

            // Обработчики клавиш
            this.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                    BackButton_Click(this, new RoutedEventArgs());
                else if (e.Key == Key.Enter)
                    StartButton_Click(this, new RoutedEventArgs());
            };
        }

        private void SetupTextBoxPlaceholders()
        {
            // Игрок 1
            Player1NameBox.GotFocus += (s, e) =>
            {
                if (Player1NameBox.Text == "Введите имя")
                {
                    Player1NameBox.Text = "";
                    Player1NameBox.Foreground = Brushes.White;
                }
            };

            Player1NameBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(Player1NameBox.Text))
                {
                    Player1NameBox.Text = "Введите имя";
                    Player1NameBox.Foreground = Brushes.Gray;
                }
            };

            Player1NameBox.Text = "Введите имя";
            Player1NameBox.Foreground = Brushes.Gray;

            // Игрок 2
            Player2NameBox.GotFocus += (s, e) =>
            {
                if (Player2NameBox.Text == "Введите имя")
                {
                    Player2NameBox.Text = "";
                    Player2NameBox.Foreground = Brushes.White;
                }
            };

            Player2NameBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(Player2NameBox.Text))
                {
                    Player2NameBox.Text = "Введите имя";
                    Player2NameBox.Foreground = Brushes.Gray;
                }
            };

            Player2NameBox.Text = "Введите имя";
            Player2NameBox.Foreground = Brushes.Gray;
        }

        private void UpdateFactionDescription(int playerNumber, string faction)
        {
            if (!string.IsNullOrEmpty(faction) && _factionDescriptions.ContainsKey(faction))
            {
                if (playerNumber == 1)
                {
                    Player1FactionTitle.Text = _factionTitles.ContainsKey(faction) ? _factionTitles[faction] : faction;
                    Player1FactionDesc.Text = _factionDescriptions[faction];
                }
                else
                {
                    Player2FactionTitle.Text = _factionTitles.ContainsKey(faction) ? _factionTitles[faction] : faction;
                    Player2FactionDesc.Text = _factionDescriptions[faction];
                }
            }
        }

        private string GetSelectedFaction(int playerNumber)
        {
            ComboBox comboBox = playerNumber == 1 ? Player1FactionBox : Player2FactionBox;

            if (comboBox.SelectedItem is ComboBoxItem item)
            {
                if (item.Content is StackPanel panel && panel.Children.Count > 1)
                {
                    if (panel.Children[1] is TextBlock textBlock)
                    {
                        return textBlock.Text;
                    }
                }
                return item.Content.ToString();
            }

            return "";
        }

        private Faction ConvertStringToFaction(string factionStr)
        {
            return factionStr switch
            {
                "Люди" => Faction.Humans,
                "Звери" => Faction.Beasts,
                "Мифические" => Faction.Mythical,
                "Стихии" => Faction.Elements,
                _ => Faction.Humans
            };
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            string player1Name = Player1NameBox.Text.Trim();
            string player2Name = Player2NameBox.Text.Trim();

            // Проверяем плейсхолдеры
            if (player1Name == "Введите имя" || player2Name == "Введите имя" ||
                string.IsNullOrWhiteSpace(player1Name) || string.IsNullOrWhiteSpace(player2Name))
            {
                ShowMessage("Введите имена обоих магов!");
                return;
            }

            // Проверяем уникальность имен
            if (player1Name.ToLower() == player2Name.ToLower())
            {
                ShowMessage("Имена магов не должны совпадать!");
                return;
            }

            // Получаем выбранные фракции
            Player1Faction = GetSelectedFaction(1);
            Player2Faction = GetSelectedFaction(2);

            // Проверяем что фракции выбраны
            if (string.IsNullOrEmpty(Player1Faction) || string.IsNullOrEmpty(Player2Faction))
            {
                ShowMessage("Выберите фракции для обоих магов!");
                return;
            }

            Player1Name = player1Name;
            Player2Name = player2Name;

            // Показываем финальное подтверждение
            var result = MessageBox.Show(
                $"⚔ {Player1Name.ToUpper()} ({Player1Faction})\n" +
                $"⚔ {Player2Name.ToUpper()} ({Player2Faction})\n\n" +
                $"Готовы начать дуэль магов?",
                "НАЧАТЬ ДУЭЛЬ",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
            {
                DialogResult = true;
                Close();
            }
        }

        private void ShowMessage(string message)
        {
            MessageBox.Show(message, "Внимание",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus();
        }

        private void Player1FactionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Метод уже обрабатывается в анонимном обработчике
        }
    }
}