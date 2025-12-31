using CardGame.Core.GameState;
using CardGame.Core.Models;
using CardGame.Core.Serialization;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using WPFNASTYA;

namespace CardGame.GUI
{
    public partial class MainWindow : Window
    {
        private GameSaveManager _saveManager;

        public MainWindow()
        {
            InitializeComponent();
            _saveManager = new GameSaveManager();
        }
        private void LoadGameButton_Click(object sender, RoutedEventArgs e)
        {
            // Используем окно загрузки игр
            var loadWindow = new LoadGameWindow(_saveManager);

            if (loadWindow.ShowDialog() == true && loadWindow.SelectedSave != null)
            {
                try
                {
                    // Загружаем выбранную игру - используем имя файла для поиска
                    var gameState = _saveManager.LoadGame(loadWindow.SelectedSave.FileName);

                    // Создаем игровое окно
                    var gameWindow = new GameBoardWindow();
                    gameWindow.InitializeGame(gameState);
                    gameWindow.Show();

                    // Закрываем главное меню
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось загрузить игру: {ex.Message}",
                                  "Ошибка загрузки",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
        }
        // Добавьте кнопку "Продолжить игру" для автосохранения
        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var gameState = _saveManager.LoadLastAutosave();

                if (gameState != null)
                {
                    var gameWindow = new GameBoardWindow();
                    gameWindow.InitializeGame(gameState);
                    gameWindow.Show();

                    this.Close();
                }
                else
                {
                    MessageBox.Show("Нет сохраненной игры для продолжения",
                                  "Продолжить игру",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить последнюю игру: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }
        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно настройки игроков
            var setupWindow = new PlayerSetupWindow();

            // Показываем окно как диалоговое окно
            var result = setupWindow.ShowDialog();

            // Если пользователь нажал "Начать игру"
            if (result == true)
            {
                // Получаем выбранные данные
                string player1Name = setupWindow.Player1Name;
                string player2Name = setupWindow.Player2Name;
                string player1FactionStr = setupWindow.Player1Faction;
                string player2FactionStr = setupWindow.Player2Faction;

                // Конвертируем строки фракций в перечисление Faction
                Faction player1Faction = ConvertStringToFaction(player1FactionStr);
                Faction player2Faction = ConvertStringToFaction(player2FactionStr);

                // Создаем карты с помощью фабрики
                var cardFactory = new CardFactory();

                // Создаем игроков с выбранными параметрами
                var player1 = new Player(player1Name, player1Faction);
                var player2 = new Player(player2Name, player2Faction);

                // Инициализируем колоды
                player1.InitializeDeck(cardFactory.CreateStarterDeck(player1Faction));
                player2.InitializeDeck(cardFactory.CreateStarterDeck(player2Faction));

                // Создаем состояние игры
                var gameState = new GameState(player1, player2);

                // Автосохранение перед началом
                try
                {
                    _saveManager.Autosave(gameState);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Автосохранение не удалось: {ex}");
                }
                // Открываем игровое окно
                var gameWindow = new GameBoardWindow();
                gameWindow.InitializeGame(gameState);
                gameWindow.Show();

                this.Close();
            }
            
            // Автосохранение перед началом
            
        }

        // Вспомогательный метод для конвертации строки в Faction
        private Faction ConvertStringToFaction(string factionStr)
        {
            return factionStr switch
            {
                "Люди" => Faction.Humans,
                "Звери" => Faction.Beasts,
                "Мифические" => Faction.Mythical,
                "Стихии" => Faction.Elements,
                _ => Faction.Humans // Значение по умолчанию
            };
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти из игры?",
                                       "Подтверждение выхода",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }
}