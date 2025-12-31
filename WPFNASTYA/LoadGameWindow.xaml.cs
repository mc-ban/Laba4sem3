using CardGame.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CardGame.GUI
{
    public partial class LoadGameWindow : Window
    {
        private readonly GameSaveManager _saveManager;
        private SaveGameInfo _selectedSave;
        private List<SaveGameInfo> _saves;

        public SaveGameInfo SelectedSave => _selectedSave;

        public LoadGameWindow(GameSaveManager saveManager)
        {
            InitializeComponent();
            _saveManager = saveManager;
            LoadSaves();
        }

        private void LoadSaves()
        {
            try
            {
                _saves = _saveManager.GetSaveFiles();
                SavesList.ItemsSource = _saves;

                if (_saves.Count == 0)
                {
                    NoSavesText.Visibility = Visibility.Visible;
                    SavesList.Visibility = Visibility.Collapsed;
                }
                else
                {
                    NoSavesText.Visibility = Visibility.Collapsed;
                    SavesList.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке списка сохранений: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
                Close();
            }
        }

        private void SaveItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is Border border)
            {
                // Снимаем выделение со всех элементов
                foreach (var item in SavesList.Items)
                {
                    if (SavesList.ItemContainerGenerator.ContainerFromItem(item) is ContentPresenter presenter)
                    {
                        if (presenter.ContentTemplate.FindName("SaveItemBorder", presenter) is Border itemBorder)
                        {
                            itemBorder.BorderBrush = (SolidColorBrush)FindResource("CopperBrush");
                            itemBorder.Background = (SolidColorBrush)FindResource("40000000");
                        }
                    }
                }

                // Выделяем выбранный элемент
                border.BorderBrush = Brushes.Gold;
                border.Background = new SolidColorBrush(Color.FromArgb(80, 184, 111, 47));

                // Сохраняем выбранное сохранение
                _selectedSave = (SaveGameInfo)border.DataContext;
                LoadButton.IsEnabled = true;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string saveName)
            {
                var result = MessageBox.Show($"Удалить сохранение '{saveName}'?",
                                           "Подтверждение удаления",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _saveManager.DeleteSave(saveName);
                        LoadSaves();
                        _selectedSave = null;
                        LoadButton.IsEnabled = false;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось удалить сохранение: {ex.Message}",
                                      "Ошибка",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                    }
                }
            }
            e.Handled = true;
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSave != null)
            {
                try
                {
                    // Закрываем окно загрузки с результатом
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
            else if (e.Key == Key.Enter && LoadButton.IsEnabled)
            {
                LoadButton_Click(this, new RoutedEventArgs());
            }
        }
    }
}