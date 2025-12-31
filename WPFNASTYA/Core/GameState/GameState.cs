using CardGame.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CardGame.Core.GameEngine;

namespace CardGame.Core.GameState
{
    // Класс, представляющий состояние всей игры
    [Serializable]
    public class GameState : IGameUpdateListener
    {
        // Основные свойства игры
        public string GameId { get; set; }          // Уникальный ID игры
        public Player Player1 { get; set; }         // Первый игрок
        public Player Player2 { get; set; }         // Второй игрок
        public Player CurrentPlayer { get; set; }   // Игрок, который ходит сейчас
        public Player OpponentPlayer { get; set; }  // Игрок, ожидающий хода
        public int TurnNumber { get; set; } = 1;    // Номер текущего хода
        public GameStatus Status { get; set; }      // Текущий статус игры
        public DateTime CreatedAt { get; set; }     // Время создания игры
        public DateTime LastUpdated { get; set; }   // Время последнего обновления
        public List<string> GameLog { get; set; } = new List<string>(); // Лог событий

        // Событие для уведомления об изменении состояния игры
        [field: NonSerialized]
        public event Action<GameState> OnStateChanged;

        // Генератор случайных чисел
        private Random _random = new Random();

        // Конструктор создает новую игру с двумя игроками
        private bool _isProcessingAction = false;

        void IGameUpdateListener.OnGameUpdate()
        {
            // Этот метод вызывается движком 60 раз в секунду
            // Используем для периодических проверок

            // Например, проверка состояния игры
            CheckGameOver();

            // Обновление времени
            LastUpdated = DateTime.Now;
        }
        // Конструктор
        public GameState(Player player1, Player player2, bool isLoading = false)
        {
            GameId = Guid.NewGuid().ToString();
            Player1 = player1;
            Player2 = player2;
            CurrentPlayer = player1;
            OpponentPlayer = player2;
            Status = GameStatus.Active;
            CreatedAt = DateTime.Now;
            LastUpdated = DateTime.Now;

            // Регистрация в движке
            GameEngine.GameEngine.Instance.RegisterListener(this);

            if (!isLoading)
            {
                InitializeGame();
            }
            else
            {
                Log($"Загружена сохраненная игра: {Player1.Name} vs {Player2.Name}");
            }

            // Подписка на события игроков
            SubscribeToPlayerEvents(player1);
            SubscribeToPlayerEvents(player2);
        }

        private void SubscribeToPlayerEvents(Player player)
        {
            if (player == null) return;

            player.OnHealthChanged += (p) =>
            {
                GameEngine.GameEngine.Instance.ForceImmediateUpdate($"Health changed for {p.Name}");
            };

            player.OnManaChanged += (p) =>
            {
                GameEngine.GameEngine.Instance.ForceImmediateUpdate($"Mana changed for {p.Name}");
            };

            player.HandChanged += () =>
            {
                GameEngine.GameEngine.Instance.ForceImmediateUpdate($"Hand changed for {player.Name}");
            };

            player.BoardChanged += () =>
            {
                GameEngine.GameEngine.Instance.ForceImmediateUpdate($"Board changed for {player.Name}");
            };
        }

        // Метод из RFOnline: немедленное обновление после действия
        public PlayResult PlayCard(ICard card, CreatureCard target = null)
        {
            if (_isProcessingAction)
                return PlayResult.FailedResult("Действие уже выполняется");

            if (Status != GameStatus.Active)
                return PlayResult.FailedResult("Игра не активна");

            _isProcessingAction = true;
            try
            {
                var result = CurrentPlayer.PlayCard(card, target, this);

                if (result.IsSuccess)
                {
                    Log(result.Message ?? $"{CurrentPlayer.Name} разыгрывает {card.Name}");

                    // RFOnline стиль: принудительное обновление ВСЕГО
                    GameEngine.GameEngine.Instance.ForceImmediateUpdate($"Card played: {card.Name}");

                    CheckGameOver();
                    OnStateChanged?.Invoke(this);
                }
                else
                {
                    Log($"Ошибка: {result.Error}");
                }

                return result;
            }
            finally
            {
                _isProcessingAction = false;
            }
        }

        // RFOnline стиль атаки
        // Инициализирует начальное состояние игры
        private void InitializeGame()
        {
            // Стартовая раздача карт
            CurrentPlayer.DrawCard(3);
            OpponentPlayer.DrawCard(4);

            Log($"Начало игры: {Player1.Name} vs {Player2.Name}");
            Log($"{Player2.Name} получает дополнительную карту");
        }

        // Запускает ход текущего игрока
        public void StartTurn()
        {
            try
            {
                Debug.WriteLine($"Starting turn {TurnNumber} for {CurrentPlayer.Name}");

                // Восстановление маны для нового хода
                CurrentPlayer.ResetMana();

                // Взятие карты в начале хода
                CurrentPlayer.DrawCard();

                // Сбрасываем флаг способности героя
                CurrentPlayer.HasUsedHeroPower = false;

                // Сброс состояний существ
                foreach (var creature in CurrentPlayer.Board)
                {
                    creature.ResetForNewTurn();
                }

                // Логирование начала хода
                Log($"=== Ход {TurnNumber} ===");
                Log($"Ходит: {CurrentPlayer.Name} (Мана: {CurrentPlayer.Mana}/{CurrentPlayer.MaxMana})");

                Debug.WriteLine($"Turn started. Mana: {CurrentPlayer.Mana}/{CurrentPlayer.MaxMana}, Hand: {CurrentPlayer.Hand.Count}");

                // Уведомляем об изменении состояния
                OnStateChanged?.Invoke(this);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GameState.StartTurn: {ex}");
                Log($"Ошибка при начале хода: {ex.Message}");
            }
        }

        // Завершает ход текущего игрока и передает ход противнику
        public void EndTurn()
        {
            try
            {
                Log($"{CurrentPlayer.Name} завершает ход");

                // Сбрасываем флаг способности героя
                CurrentPlayer.HasUsedHeroPower = false;

                // Смена текущего и оппонента местами
                (CurrentPlayer, OpponentPlayer) = (OpponentPlayer, CurrentPlayer);

                // Увеличиваем счетчик ходов
                TurnNumber++;

                // Начинаем ход следующего игрока
                StartTurn();

                // Проверка условий завершения игры
                CheckGameOver();

                // Уведомляем об изменении состояния
                OnStateChanged?.Invoke(this);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GameState.EndTurn: {ex}");
                Log($"Ошибка при завершении хода: {ex.Message}");
            }
        }

        // Позволяет текущему игроку разыграть карту
    
        // Основной метод для выполнения атаки существом

        public AttackResult Attack(CreatureCard attacker, CreatureCard defender = null)
        {
            if (_isProcessingAction)
                return AttackResult.Failed("Действие уже выполняется");           // Проверка базовых условий для атаки
            if (Status != GameStatus.Active)
                return AttackResult.Failed("Игра не активна");

            if (!CurrentPlayer.Board.Contains(attacker))
                return AttackResult.Failed("Существо не на вашем поле");

            if (!attacker.CanAttack || attacker.IsExhausted)
                return AttackResult.Failed("Это существо не может атаковать");

            if (attacker.IsFrozen)
                return AttackResult.Failed("Существо заморожено");

            // Правило Taunt (Провокация)
            if (OpponentPlayer.HasTauntCreatures())
            {
                var tauntCreatures = OpponentPlayer.GetTauntCreatures();
                if (defender == null || !defender.IsTaunt)
                {
                    return AttackResult.Failed($"Вы должны атаковать существо с Taunt: {string.Join(", ", tauntCreatures.Select(c => c.Name))}");
                }
            }

            // Определяем тип атаки: по герою или по существу
            
            _isProcessingAction = true;
            try
            {
                AttackResult result;
                if (defender == null)
                {
                    result = AttackPlayer(attacker);
                }
                else
                {
                    result = AttackCreature(attacker, defender);
                }

                // RFOnline стиль: немедленное обновление после боя
                GameEngine.GameEngine.Instance.ForceImmediateUpdate($"Attack: {attacker.Name}");

                return result;
            }
            finally
            {
                _isProcessingAction = false;
            }

        }

        // Обрабатывает атаку существа по герою противника
        private AttackResult AttackPlayer(CreatureCard attacker)
        {
            // Получаем силу атаки существа
            var damage = attacker.Attack;

            // Проверка способности Lifesteal (Вампиризм)
            if (attacker.HasAbility(AbilityType.Lifesteal))
            {
                CurrentPlayer.Heal(damage);
                Log($"{attacker.Name} крадет {damage} здоровья");
            }

            // Нанесение урона герою противника
            OpponentPlayer.TakeDamage(damage, attacker);

            // Помечаем существо как "уставшее" после атаки
            attacker.Exhaust();

            // Запись в лог о совершенной атаке
            Log($"{attacker.Name} атакует героя {OpponentPlayer.Name} на {damage} урона");

            // Проверка не привела ли атака к победе
            CheckGameOver();

            return AttackResult.Successful($"Нанесено {damage} урона герою");
        }

        // Обрабатывает атаку одного существа по другому
        private AttackResult AttackCreature(CreatureCard attacker, CreatureCard defender)
        {
            // Проверка что цель находится на поле противника
            if (!OpponentPlayer.Board.Contains(defender))
                return AttackResult.Failed("Цель не на поле противника");

            // Сохраняем значения атаки для обоих существ
            var attackerDamage = attacker.Attack;
            var defenderDamage = defender.Attack;

            // Обработка способности Poison (Яд)
            if (attacker.HasAbility(AbilityType.Poison) && attackerDamage > 0)
            {
                defender.TakeDamage(defender.Health, true);
                Log($"{attacker.Name} отравляет {defender.Name}");
            }
            else
            {
                // Обычная атака
                defender.TakeDamage(attackerDamage);
            }

            // Контратака защитника
            if (defender.Health > 0 && defenderDamage > 0)
            {
                if (defender.HasAbility(AbilityType.Poison))
                {
                    attacker.TakeDamage(attacker.Health, true);
                    Log($"{defender.Name} отравляет {attacker.Name}");
                }
                else
                {
                    attacker.TakeDamage(defenderDamage);
                }
            }

            // Помечаем атакующее существо как уставшее
            attacker.Exhaust();

            // Детальное логирование боя между существами
            Log($"{attacker.Name} ({attacker.Attack}/{attacker.Health}) атакует {defender.Name} ({defender.Attack}/{defender.Health})");

            // Убираем мертвые существа с поля боя
            CleanDeadCreatures();

            // Проверка условий завершения игры
            CheckGameOver();

            return AttackResult.Successful();
        }

        // Удаляет мертвые существа с полей обоих игроков
        private void CleanDeadCreatures()
        {
            CleanPlayerDeadCreatures(CurrentPlayer);
            CleanPlayerDeadCreatures(OpponentPlayer);
        }

        // Удаляет мертвые существа с поля конкретного игрока
        private void CleanPlayerDeadCreatures(Player player)
        {
            // Находим все существа с нулевым или отрицательным здоровьем
            var deadCreatures = player.Board.Where(c => c.Health <= 0).ToList();

            // Обрабатываем каждое мертвое существо
            foreach (var creature in deadCreatures)
            {
                // Удаляем с поля и перемещаем в кладбище
                player.Board.Remove(creature);
                player.Graveyard.Add(creature);
                Log($"{creature.Name} погибает");

                // Активируем эффекты смерти
                creature.TriggerDeath();
            }
        }

        // Проверяет условия завершения игры и определяет победителя
        private void CheckGameOver()
        {
            bool stateChanged = false;

            // Проверка на ничью
            if (Player1.Health <= 0 && Player2.Health <= 0)
            {
                if (Status != GameStatus.Draw)
                {
                    Status = GameStatus.Draw;
                    Log("=== НИЧЬЯ ===");
                    stateChanged = true;
                }
            }
            // Победа второго игрока
            else if (Player1.Health <= 0)
            {
                if (Status != GameStatus.Player2Wins)
                {
                    Status = GameStatus.Player2Wins;
                    Log($"=== ПОБЕДА {Player2.Name.ToUpper()} ===");
                    stateChanged = true;
                }
            }
            // Победа первого игрока
            else if (Player2.Health <= 0)
            {
                if (Status != GameStatus.Player1Wins)
                {
                    Status = GameStatus.Player1Wins;
                    Log($"=== ПОБЕДА {Player1.Name.ToUpper()} ===");
                    stateChanged = true;
                }
            }

            // Если статус изменился - уведомляем
            if (stateChanged)
            {
                OnStateChanged?.Invoke(this);
            }
        }

        // Добавляет сообщение в лог игры с временной меткой
        public void Log(string message)
        {
            // Форматируем сообщение с временем
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var logMessage = $"[{timestamp}] {message}";

            // Добавляем в список логов
            GameLog.Add(logMessage);

            // Обновляем время последнего изменения
            LastUpdated = DateTime.Now;

            // Ограничиваем размер лога
            if (GameLog.Count > 100)
            {
                GameLog.RemoveAt(0);
            }
        }

        // Возвращает краткое текстовое описание текущего состояния игры
        public string GetGameSummary()
        {
            return $"{Player1.Name} ({Player1.Health}❤️) vs {Player2.Name} ({Player2.Health}❤️) - Ход {TurnNumber}";
        }

        // Метод для принудительного обновления состояния
        public void ForceStateUpdate()
        {
            OnStateChanged?.Invoke(this);
        }
    }

    // Класс для представления результата атаки
    public class AttackResult
    {
        // Свойства результата атаки
        public bool IsSuccessful { get; }  // Успешна ли атака
        public string Message { get; }     // Сообщение об успехе
        public string Error { get; }       // Сообщение об ошибке (если атака не удалась)

        // Приватный конструктор
        private AttackResult(bool success, string message, string error)
        {
            IsSuccessful = success;
            Message = message;
            Error = error;
        }

        // Создает успешный результат атаки
        public static AttackResult Successful(string message = null)
        {
            return new AttackResult(true, message, null);
        }

        // Создает неуспешный результат атаки с описанием ошибки
        public static AttackResult Failed(string error)
        {
            return new AttackResult(false, null, error);
        }
    }

    // Перечисление возможных статусов игры
    public enum GameStatus
    {
        Active,         // Игра активна, идет игровой процесс
        Player1Wins,    // Победа первого игрока
        Player2Wins,    // Победа второго игрока
        Draw,           // Ничья
        Saved           // Игра сохранена
    }

}