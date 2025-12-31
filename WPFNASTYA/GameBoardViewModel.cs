using CardGame.Core.GameState;
using CardGame.Core.Models;
using CardGame.GUI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CardGame.GUI.ViewModels
{
    public class GameBoardViewModel : INotifyPropertyChanged
    {
        private GameState _gameState;
        private Player _player;
        private Player _opponent;
        private ICard _selectedCard;
        private CreatureCard _selectedAttacker;
        private CreatureCard _selectedDefender;
        private string _statusMessage;
        private double _messageOpacity;
        private bool _isMessageVisible;

        public event PropertyChangedEventHandler PropertyChanged;

        // Команды
        public ICommand EndTurnCommand { get; }
        public ICommand AttackCommand { get; }
        public ICommand UseHeroPowerCommand { get; }
        public ICommand PlayCardCommand { get; }
        public ICommand SelectCardCommand { get; }
        public ICommand SelectCreatureCommand { get; }
        public ICommand SaveGameCommand { get; }
        public ICommand BackToMenuCommand { get; }
        public ICommand DirectAttackCommand { get; }
        public ICommand ClearSelectionCommand { get; }

        // Свойства
        public GameState GameState
        {
            get => _gameState;
            set
            {
                if (_gameState != null)
                {
                    _gameState.OnStateChanged -= GameState_OnStateChanged;
                    UnsubscribeFromPlayerEvents(_player);
                    UnsubscribeFromPlayerEvents(_opponent);
                }

                _gameState = value;

                if (_gameState != null)
                {
                    _gameState.OnStateChanged += GameState_OnStateChanged;
                }

                UpdatePlayers();
                OnPropertyChanged();
            }
        }

        public Player Player
        {
            get => _player;
            set
            {
                if (_player != value)
                {
                    UnsubscribeFromPlayerEvents(_player);
                    _player = value;
                    SubscribeToPlayerEvents(_player);
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PlayerName));
                    OnPropertyChanged(nameof(PlayerFaction));
                    OnPropertyChanged(nameof(CanUseHeroPower));
                    OnPropertyChanged(nameof(CanPlaySelectedCard));
                }
            }
        }

        public Player Opponent
        {
            get => _opponent;
            set
            {
                if (_opponent != value)
                {
                    UnsubscribeFromPlayerEvents(_opponent);
                    _opponent = value;
                    SubscribeToPlayerEvents(_opponent);
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(OpponentName));
                    OnPropertyChanged(nameof(OpponentFaction));
                }
            }
        }

        // Вычисляемые свойства для привязки в XAML
        public string TurnInfo => GameState != null ? $"Ход {GameState.TurnNumber} • {Player?.Name}" : "Игра не начата";
        public string GameStatusText => GameState?.Status.ToString() ?? "Неактивно";
        public string PlayerName => Player?.Name ?? "Игрок";
        public string PlayerFaction => Player?.Faction.ToString() ?? "Неизвестно";
        public string OpponentName => Opponent?.Name ?? "Противник";
        public string OpponentFaction => Opponent?.Faction.ToString() ?? "Неизвестно";

        // Количество карт в колодах (для привязки в XAML)
        public int PlayerDeckCount => Player?.Deck?.Count ?? 0;
        public int OpponentDeckCount => Opponent?.Deck?.Count ?? 0;

        public bool CanUseHeroPower => Player?.HasUsedHeroPower == false;
        public bool CanPlaySelectedCard => SelectedCard != null && Player?.Mana >= SelectedCard.ManaCost;
        public bool CanAttack => SelectedAttacker != null && (SelectedDefender != null || CanAttackHeroDirectly());
        public bool CanDirectAttack => SelectedAttacker != null && CanAttackHeroDirectly();

        // Сообщения
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public double MessageOpacity
        {
            get => _messageOpacity;
            set
            {
                _messageOpacity = value;
                OnPropertyChanged();
            }
        }

        public bool IsMessageVisible
        {
            get => _isMessageVisible;
            set
            {
                _isMessageVisible = value;
                OnPropertyChanged();
            }
        }

        // КОЛЛЕКЦИИ ДЛЯ ПРИВЯЗКИ В XAML
        // Используем эти коллекции в XAML вместо Player.Hand и Player.Board
        public ObservableCollection<ICard> PlayerHand { get; } = new ObservableCollection<ICard>();
        public ObservableCollection<CreatureCard> PlayerBoard { get; } = new ObservableCollection<CreatureCard>();
        public ObservableCollection<CreatureCard> OpponentBoard { get; } = new ObservableCollection<CreatureCard>();
        public ObservableCollection<string> GameLog { get; } = new ObservableCollection<string>();

        public ICard SelectedCard
        {
            get => _selectedCard;
            set
            {
                _selectedCard = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanPlaySelectedCard));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public CreatureCard SelectedAttacker
        {
            get => _selectedAttacker;
            set
            {
                _selectedAttacker = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanAttack));
                OnPropertyChanged(nameof(CanDirectAttack));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public CreatureCard SelectedDefender
        {
            get => _selectedDefender;
            set
            {
                _selectedDefender = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanAttack));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public GameBoardViewModel()
        {
            // Инициализация команд
            EndTurnCommand = new RelayCommand(
                _ => ExecuteEndTurn(),
                _ => GameState?.Status == Core.GameState.GameStatus.Active);

            AttackCommand = new RelayCommand(
                _ => ExecuteAttack(),
                _ => CanAttack);

            UseHeroPowerCommand = new RelayCommand(
                _ => ExecuteUseHeroPower(),
                _ => CanUseHeroPower);

            PlayCardCommand = new RelayCommand<ICard>(
                ExecutePlayCard,
                CanExecutePlayCard);

            SelectCardCommand = new RelayCommand<ICard>(ExecuteSelectCard);

            SelectCreatureCommand = new RelayCommand<CreatureCard>(ExecuteSelectCreature);

            SaveGameCommand = new RelayCommand(_ => ExecuteSaveGame());

            BackToMenuCommand = new RelayCommand(_ => ExecuteBackToMenu());

            DirectAttackCommand = new RelayCommand(
                _ => ExecuteDirectAttack(),
                _ => CanDirectAttack);

            ClearSelectionCommand = new RelayCommand(_ => ExecuteClearSelection());
        }

        // Подписка на события игрока
        private void SubscribeToPlayerEvents(Player player)
        {
            if (player == null) return;

            player.PropertyChanged += Player_PropertyChanged;
            player.OnHealthChanged += Player_OnHealthChanged;
            player.OnManaChanged += Player_OnManaChanged;
        }

        // Отписка от событий игрока
        private void UnsubscribeFromPlayerEvents(Player player)
        {
            if (player == null) return;

            player.PropertyChanged -= Player_PropertyChanged;
            player.OnHealthChanged -= Player_OnHealthChanged;
            player.OnManaChanged -= Player_OnManaChanged;
        }

        // Обработчики событий
        private void GameState_OnStateChanged(GameState gameState)
        {
            // МГНОВЕННОЕ ОБНОВЛЕНИЕ ПРИ ИЗМЕНЕНИИ СОСТОЯНИЯ ИГРЫ
            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateAllUI();
            });
        }

        private void Player_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                OnPropertyChanged(nameof(PlayerName));
                OnPropertyChanged(nameof(PlayerFaction));
                OnPropertyChanged(nameof(CanUseHeroPower));
                OnPropertyChanged(nameof(CanPlaySelectedCard));
                OnPropertyChanged(nameof(PlayerDeckCount));
                OnPropertyChanged(nameof(OpponentDeckCount));
                CommandManager.InvalidateRequerySuggested();
            });
        }

        private void Player_OnHealthChanged(Player player)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                OnPropertyChanged(nameof(Player));
                OnPropertyChanged(nameof(Opponent));
                CommandManager.InvalidateRequerySuggested();
            });
        }

        private void Player_OnManaChanged(Player player)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                OnPropertyChanged(nameof(Player));
                OnPropertyChanged(nameof(Opponent));
                OnPropertyChanged(nameof(CanPlaySelectedCard));
                CommandManager.InvalidateRequerySuggested();
            });
        }

        // ОБНОВЛЕНИЕ ИГРОКОВ И КОЛЛЕКЦИЙ
        private void UpdatePlayers()
        {
            Player = GameState?.CurrentPlayer;
            Opponent = GameState?.OpponentPlayer;
        }

        private void UpdateCollections()
        {
            // РУКА ИГРОКА
            PlayerHand.Clear();
            if (Player?.Hand != null)
            {
                foreach (var card in Player.Hand)
                {
                    PlayerHand.Add(card);
                }
            }

            // ПОЛЕ ИГРОКА
            PlayerBoard.Clear();
            if (Player?.Board != null)
            {
                foreach (var creature in Player.Board)
                {
                    PlayerBoard.Add(creature);
                }
            }

            // ПОЛЕ ПРОТИВНИКА
            OpponentBoard.Clear();
            if (Opponent?.Board != null)
            {
                foreach (var creature in Opponent.Board)
                {
                    OpponentBoard.Add(creature);
                }
            }

            // ЛОГ ИГРЫ
            UpdateGameLog();
        }

        private void UpdateGameLog()
        {
            GameLog.Clear();
            if (GameState?.GameLog != null)
            {
                var lastLogs = GameState.GameLog.TakeLast(10);
                foreach (var log in lastLogs)
                {
                    GameLog.Add(log);
                }
            }
        }

        // ПОЛНОЕ ОБНОВЛЕНИЕ UI
        public void UpdateAllUI()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // 1. Обновляем ссылки на игроков
                    UpdatePlayers();

                    // 2. Обновляем коллекции
                    UpdateCollections();

                    // 3. Уведомляем об изменении всех свойств
                    NotifyAllProperties();

                    // 4. Обновляем состояние команд
                    CommandManager.InvalidateRequerySuggested();

                    Debug.WriteLine("UpdateAllUI выполнен");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка в UpdateAllUI: {ex}");
            }
        }

        private void NotifyAllProperties()
        {
            // Основные свойства
            OnPropertyChanged(nameof(Player));
            OnPropertyChanged(nameof(Opponent));

            // Вычисляемые свойства
            OnPropertyChanged(nameof(PlayerName));
            OnPropertyChanged(nameof(PlayerFaction));
            OnPropertyChanged(nameof(OpponentName));
            OnPropertyChanged(nameof(OpponentFaction));
            OnPropertyChanged(nameof(TurnInfo));
            OnPropertyChanged(nameof(GameStatusText));
            OnPropertyChanged(nameof(PlayerDeckCount));
            OnPropertyChanged(nameof(OpponentDeckCount));

            // Состояния команд
            OnPropertyChanged(nameof(CanUseHeroPower));
            OnPropertyChanged(nameof(CanPlaySelectedCard));
            OnPropertyChanged(nameof(CanAttack));
            OnPropertyChanged(nameof(CanDirectAttack));
        }

        // МЕТОДЫ КОМАНД
        private void ExecuteEndTurn()
        {
            GameState?.EndTurn();
            ClearSelection();
            UpdateAllUI(); // ОБНОВЛЯЕМ UI ПОСЛЕ КАЖДОГО ДЕЙСТВИЯ
            ShowMessage("Ход завершен");
        }

        private void ExecuteAttack()
        {
            if (SelectedAttacker == null || (SelectedDefender == null && !CanAttackHeroDirectly()))
            {
                ShowMessage("Выберите существо и цель для атаки");
                return;
            }

            var result = GameState?.Attack(SelectedAttacker, SelectedDefender);
            if (result?.IsSuccessful == true)
            {
                ClearSelection();
                UpdateAllUI(); // ОБНОВЛЯЕМ UI ПОСЛЕ АТАКИ
                ShowMessage("Атака выполнена успешно!");
            }
            else
            {
                ShowMessage(result?.Error ?? "Ошибка атаки");
            }
        }

        private void ExecuteDirectAttack()
        {
            // Проверяем ВСЕ возможные null значения
            if (GameState == null)
            {
                ShowMessage("Игра не инициализирована");
                return;
            }

            if (SelectedAttacker == null)
            {
                ShowMessage("Выберите существо для атаки");
                return;
            }

            // Сохраняем ссылку на атакующего до вызова атаки
            var attacker = SelectedAttacker;
            var attackerName = attacker.Name;
            var attackerAttack = attacker.Attack;

            if (!CanAttackHeroDirectly())
            {
                ShowMessage("Сначала уничтожьте всех существ с Taunt!");
                return;
            }

            try
            {
                var result = GameState.Attack(attacker, null);

                if (result != null && result.IsSuccessful)
                {
                    ClearSelection();
                    UpdateAllUI();
                    ShowMessage($"Герой атакован на {attackerAttack} урона существом {attackerName}!");
                }
                else
                {
                    ShowMessage(result?.Error ?? "Ошибка прямой атаки");
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка при атаке: {ex.Message}");
                Debug.WriteLine($"Ошибка в ExecuteDirectAttack: {ex}");
            }
        }
        private void ExecuteUseHeroPower()
        {
            if (Player != null)
            {
                Player.HasUsedHeroPower = true;
                UpdateAllUI(); // ОБНОВЛЯЕМ UI
                ShowMessage("Способность героя использована");
            }
        }

        private bool CanExecutePlayCard(ICard card)
        {
            if (card == null || Player == null || GameState?.CurrentPlayer != Player)
                return false;

            bool hasEnoughMana = Player.Mana >= card.ManaCost;
            bool hasSpaceForCreature = true;

            if (card is CreatureCard)
                hasSpaceForCreature = Player.Board?.Count < 7;

            bool isInHand = Player.Hand?.Contains(card) == true;

            return hasEnoughMana && hasSpaceForCreature && isInHand;
        }

        public void ExecutePlayCard(ICard card)
        {
            if (card == null)
            {
                ShowMessage("Выберите карту для игры");
                return;
            }

            if (Player?.Mana < card.ManaCost)
            {
                ShowMessage($"Недостаточно маны! Нужно: {card.ManaCost}");
                return;
            }

            var result = GameState?.PlayCard(card, null);

            if (result?.IsSuccess == true)
            {
                SelectedCard = null;
                UpdateAllUI(); // НЕМЕДЛЕННОЕ ОБНОВЛЕНИЕ UI
                ShowMessage($"{card.Name} разыграна!");
            }
            else
            {
                ShowMessage(result?.Error ?? "Не удалось разыграть карту");
            }
        }

        public void ExecuteSelectCard(ICard card)
        {
            try
            {
                if (SelectedCard == card)
                {
                    SelectedCard = null;
                    ShowMessage("Выбор карты снят");
                }
                else
                {
                    SelectedCard = card;
                    string message = CanExecutePlayCard(card)
                        ? $"Выбрана карта: {card.Name} (Можно сыграть)"
                        : $"Выбрана карта: {card.Name} (Недостаточно маны)";
                    ShowMessage(message);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка в ExecuteSelectCard: {ex}");
            }
        }

        public void ExecuteSelectCreature(CreatureCard creature)
        {
            if (creature == null) return;

            if (PlayerBoard.Contains(creature))
            {
                SelectedAttacker = creature;
                SelectedDefender = null;
                ShowMessage($"Выбрано существо для атаки: {creature.Name}");
            }
            else if (OpponentBoard.Contains(creature))
            {
                SelectedDefender = creature;
                ShowMessage($"Выбрана цель: {creature.Name}");

                if (SelectedAttacker != null && AttackCommand.CanExecute(null))
                {
                    ExecuteAttack();
                }
            }
        }

        private void ExecuteSaveGame()
        {
            ShowMessage("Сохранение игры еще не реализовано");
        }

        private void ExecuteBackToMenu()
        {
            try
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();

                foreach (Window window in Application.Current.Windows)
                {
                    if (window is GameBoardWindow)
                    {
                        window.Close();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при возврате в меню: {ex}");
                Application.Current.Shutdown();
            }
        }

        public void ExecuteClearSelection()
        {
            ClearSelection();
            UpdateAllUI(); // ОБНОВЛЯЕМ UI
            ShowMessage("Выбор сброшен");
        }
        private bool CanAttackHeroDirectly()
        {
            // Добавьте проверку на null
            if (SelectedAttacker == null || Opponent?.Board == null)
                return false;

            return !Opponent.Board.Any(c => c != null && c.IsTaunt && c.Health > 0);
        }

        public void InitializeGame(GameState gameState)
        {
            GameState = gameState;
            UpdateAllUI(); // НЕМЕДЛЕННОЕ ОБНОВЛЕНИЕ ПРИ ИНИЦИАЛИЗАЦИИ
        }

        public void ClearSelection()
        {
            SelectedCard = null;
            SelectedAttacker = null;
            SelectedDefender = null;
            OnPropertyChanged(nameof(CanAttack));
            OnPropertyChanged(nameof(CanDirectAttack));
            OnPropertyChanged(nameof(CanPlaySelectedCard));
        }

        public async void ShowMessage(string message)
        {
            StatusMessage = message;
            IsMessageVisible = true;
            MessageOpacity = 1;

            OnPropertyChanged(nameof(StatusMessage));
            OnPropertyChanged(nameof(IsMessageVisible));
            OnPropertyChanged(nameof(MessageOpacity));

            await Task.Delay(3000);

            for (double opacity = 1; opacity > 0; opacity -= 0.1)
            {
                MessageOpacity = opacity;
                OnPropertyChanged(nameof(MessageOpacity));
                await Task.Delay(50);
            }

            IsMessageVisible = false;
            OnPropertyChanged(nameof(IsMessageVisible));
        }

        public virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}