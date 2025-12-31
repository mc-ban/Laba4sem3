using CardGame.Core.GameLogic;
using CardGame.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CardGame.Core.GameState
{
    [Serializable]
    public class Player : INotifyPropertyChanged
    {
        // Основная информация об игроке
        public string Id { get; set; }
        public string Name { get; set; }
        public Faction Faction { get; set; }
        public int MaxHealth { get; set; } = 30;

        // События для уведомления об изменении коллекций
        [field: NonSerialized]
        public event Action HandChanged;
        [field: NonSerialized]
        public event Action BoardChanged;

        public List<ICard> Hand
        {
            get => _hand;
            set
            {
                if (_hand != value)
                {
                    _hand = value;
                    OnPropertyChanged();
                    HandChanged?.Invoke(); // УВЕДОМЛЯЕМ ОБ ИЗМЕНЕНИИ
                }
            }
        }

        public List<CreatureCard> Board
        {
            get => _board;
            set
            {
                if (_board != value)
                {
                    _board = value;
                    OnPropertyChanged();
                    BoardChanged?.Invoke(); // УВЕДОМЛЯЕМ ОБ ИЗМЕНЕНИИ
                }
            }
        }

        // Приватные поля с уведомлениями
        private List<ICard> _hand = new List<ICard>();
        private List<CreatureCard> _board = new List<CreatureCard>();

        // Остальные коллекции
        public List<ICard> Deck { get; set; } = new List<ICard>();
        public List<ICard> Graveyard { get; set; } = new List<ICard>();

        // Дополнительные характеристики
        public int SpellDamageBonus { get; set; } = 0;
        public int FatigueDamage { get; set; } = 0;
        public bool HasUsedHeroPower { get; set; } = false;
        public int ManaCrystals { get; set; } = 0;

        // События для отслеживания изменений состояния
        [field: NonSerialized]
        public event Action<Player> OnHealthChanged;
        [field: NonSerialized]
        public event Action<Player> OnManaChanged;
        [field: NonSerialized]
        public event Action<CreatureCard> OnCreatureSummoned;
        [field: NonSerialized]
        public event Action<CreatureCard> OnCreatureDied;

        // Приватные поля с уведомлениями
        private int _health = 30;
        private int _mana = 0;
        private int _maxMana = 0;

        public int Health
        {
            get => _health;
            set
            {
                if (_health != value)
                {
                    _health = value;
                    OnPropertyChanged();
                    OnHealthChanged?.Invoke(this);
                }
            }
        }

        public int Mana
        {
            get => _mana;
            set
            {
                if (_mana != value)
                {
                    _mana = value;
                    OnPropertyChanged();
                    OnManaChanged?.Invoke(this);
                }
            }
        }

        public int MaxMana
        {
            get => _maxMana;
            set
            {
                if (_maxMana != value)
                {
                    _maxMana = value;
                    OnPropertyChanged();
                }
            }
        }

        public int DeckCount => Deck?.Count ?? 0;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Конструктор
        public Player(string name, Faction faction)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            Faction = faction;
            Health = 30;
            MaxHealth = 30;
            ManaCrystals = 0;
            MaxMana = 0;
            Mana = 0;
        }

        // Инициализация колоды
        public void InitializeDeck(List<ICard> deck, bool shuffle = true)
        {
            Deck = deck;
            if (shuffle)
            {
                ShuffleDeck();
            }
        }

        // Перемешивание колоды
        private void ShuffleDeck()
        {
            var rng = new Random();
            int n = Deck.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                var value = Deck[k];
                Deck[k] = Deck[n];
                Deck[n] = value;
            }
        }

        // Взятие карт
        public DrawResult DrawCard(int count = 1)
        {
            var result = new DrawResult();

            for (int i = 0; i < count; i++)
            {
                if (Deck.Count == 0)
                {
                    TakeFatigueDamage();
                    result.FatigueDamageTaken = FatigueDamage;
                    continue;
                }

                if (Hand.Count >= GameRules.MAX_HAND_SIZE)
                {
                    result.CardsBurned++;
                    Deck.RemoveAt(0);
                    continue;
                }

                var card = Deck[0];
                Deck.RemoveAt(0);
                Hand.Add(card);
                result.CardsDrawn++;

                // Уведомляем об изменении руки
                OnPropertyChanged(nameof(Hand));
                HandChanged?.Invoke();
            }

            return result;
        }

        // Урон от усталости
        private void TakeFatigueDamage()
        {
            FatigueDamage++;
            TakeDamage(FatigueDamage, null);
        }

        // Сброс маны для нового хода
        public void ResetMana()
        {
            try
            {
                if (ManaCrystals < 10)
                {
                    ManaCrystals++;
                }

                MaxMana = ManaCrystals;
                Mana = MaxMana;

                OnPropertyChanged(nameof(Mana));
                OnPropertyChanged(nameof(MaxMana));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ResetMana: {ex}");
            }
        }

        // Трата маны
        public void SpendMana(int amount)
        {
            try
            {
                if (amount < 0)
                    return;

                if (Mana < amount)
                {
                    Mana = 0;
                }
                else
                {
                    Mana -= amount;
                }

                OnPropertyChanged(nameof(Mana));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in SpendMana: {ex}");
            }
        }

        // Получение урона
        public void TakeDamage(int damage, CreatureCard source)
        {
            Health = Math.Max(0, Health - damage);
            OnPropertyChanged(nameof(Health));
            OnHealthChanged?.Invoke(this);
        }

        // Исцеление
        public void Heal(int amount)
        {
            Health = Math.Min(MaxHealth, Health + amount);
            OnPropertyChanged(nameof(Health));
            OnHealthChanged?.Invoke(this);
        }

        // Проверка возможности розыгрыша карты
        public bool CanPlayCard(ICard card)
        {
            if (Mana < card.ManaCost)
                return false;

            if (card.Type == CardType.Creature && Board.Count >= 7)
                return false;

            if (!Hand.Contains(card))
                return false;

            return true;
        }

        // Разыгрывание карты
        public PlayResult PlayCard(ICard card, CreatureCard target = null, GameState gameState = null)
        {
            try
            {
                Debug.WriteLine($"=== Player.PlayCard ===");
                Debug.WriteLine($"Player: {Name}, Card: {card?.Name}, ManaCost: {card?.ManaCost}");

                if (Mana < card.ManaCost)
                {
                    return PlayResult.FailedResult($"Недостаточно маны! Нужно {card.ManaCost}, у вас {Mana}");
                }

                var cardInHand = Hand.FirstOrDefault(c => c.Id == card.Id);
                if (cardInHand == null)
                {
                    return PlayResult.FailedResult("Карта не найдена в руке");
                }

                if (card is CreatureCard && Board.Count >= 7)
                {
                    return PlayResult.FailedResult("Нет места на поле!");
                }

                int oldMana = Mana;
                SpendMana(card.ManaCost);

                bool removed = Hand.Remove(cardInHand);
                if (removed)
                {
                    OnPropertyChanged(nameof(Hand));
                    HandChanged?.Invoke();
                }

                if (!removed)
                {
                    Mana = oldMana;
                    OnManaChanged?.Invoke(this);
                    return PlayResult.FailedResult("Не удалось удалить карту из руки");
                }

                if (card is CreatureCard creature)
                {
                    var result = SummonCreature(creature, gameState);

                    if (result.IsSuccess)
                    {
                        OnPropertyChanged(nameof(Board));
                        BoardChanged?.Invoke(); // Уведомляем ViewModel
                    }

                    return result;
                }
                else if (card is SpellCard spell)
                {
                    var result = CastSpell(spell, target, gameState);

                    if (result.IsSuccess)
                    {
                        OnPropertyChanged(nameof(Board));
                        BoardChanged?.Invoke();
                    }

                    return result;
                }

                return PlayResult.FailedResult("Неизвестный тип карты");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in Player.PlayCard: {ex}");
                return PlayResult.FailedResult($"Ошибка при розыгрыше карты: {ex.Message}");
            }
        }

        // Призыв существа
        private PlayResult SummonCreature(CreatureCard creature, GameState gameState)
        {
            try
            {
                if (Board.Count >= 7)
                {
                    return PlayResult.FailedResult("Нет места на поле боя");
                }

                var summonedCreature = (CreatureCard)creature.Clone();

                if (summonedCreature.IsCharge)
                {
                    summonedCreature.CanAttack = true;
                    summonedCreature.IsExhausted = false;
                }
                else
                {
                    summonedCreature.CanAttack = false;
                    summonedCreature.IsExhausted = true;
                }

                Board.Add(summonedCreature);
                Graveyard.Add(creature);

                OnPropertyChanged(nameof(Board));
                BoardChanged?.Invoke();

                if (gameState != null)
                {
                    summonedCreature.OnDeath += (deadCreature) => HandleCreatureDeath(deadCreature, gameState);
                }

                OnCreatureSummoned?.Invoke(summonedCreature);

                return PlayResult.SuccessResult($"Призван {summonedCreature.Name}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SummonCreature: {ex}");
                return PlayResult.FailedResult($"Ошибка при призыве существа: {ex.Message}");
            }
        }

        // Обработка смерти существа
        private void HandleCreatureDeath(CreatureCard creature, GameState gameState)
        {
            Board.Remove(creature);
            Graveyard.Add(creature);

            OnPropertyChanged(nameof(Board));
            BoardChanged?.Invoke();

            OnCreatureDied?.Invoke(creature);

            gameState?.Log($"{creature.Name} погибает");
        }

        // Применение заклинания
        private PlayResult CastSpell(SpellCard spell, CreatureCard target, GameState gameState)
        {
            Graveyard.Add(spell);

            switch (spell.Effect.Type)
            {
                case SpellEffectType.Damage:
                    if (target != null)
                    {
                        target.TakeDamage(spell.Effect.Value + SpellDamageBonus);
                        return PlayResult.SuccessResult($"{spell.Name} наносит {spell.Effect.Value} урона {target.Name}");
                    }
                    break;
                case SpellEffectType.Heal:
                    if (target != null)
                    {
                        target.Heal(spell.Effect.Value);
                        return PlayResult.SuccessResult($"{spell.Name} исцеляет {target.Name} на {spell.Effect.Value}");
                    }
                    break;
            }

            return PlayResult.SuccessResult($"Заклинание {spell.Name} применено");
        }

        // Подготовка к новому ходу
        public void StartTurn()
        {
            try
            {
                HasUsedHeroPower = false;

                foreach (var creature in Board)
                {
                    if (creature != null)
                    {
                        creature.ResetForNewTurn();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in Player.StartTurn: {ex}");
            }
        }

        // Проверка наличия существ с Taunt
        public bool HasTauntCreatures()
        {
            return Board.Any(c => c.IsTaunt);
        }

        // Получение существ с Taunt
        public List<CreatureCard> GetTauntCreatures()
        {
            return Board.Where(c => c.IsTaunt).ToList();
        }

        // Получение существ, готовых к атаке
        public List<CreatureCard> GetAttackReadyCreatures()
        {
            return Board.Where(c => c.CanAttack && !c.IsExhausted).ToList();
        }
    }

    // Результат взятия карт
    public class DrawResult
    {
        public int CardsDrawn { get; set; }
        public int CardsBurned { get; set; }
        public int FatigueDamageTaken { get; set; }
    }

    // Результат розыгрыша карты
    public class PlayResult
    {
        public bool IsSuccess { get; }
        public string Message { get; }
        public string Error { get; }

        private PlayResult(bool success, string message, string error)
        {
            IsSuccess = success;
            Message = message;
            Error = error;
        }

        public static PlayResult SuccessResult(string message = null)
        {
            return new PlayResult(true, message, null);
        }

        public static PlayResult FailedResult(string error)
        {
            return new PlayResult(false, null, error);
        }
    }
}