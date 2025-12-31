using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CardGame.Core.Models
{
    [Serializable]
    public class CreatureCard : ICard, INotifyPropertyChanged
    {
        private int _health;
        private int _attack;

        public int Health
        {
            get => _health;
            set
            {
                if (_health != value)
                {
                    _health = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsDead));
                }
            }
        }

        public int Attack
        {
            get => _attack;
            set
            {
                if (_attack != value)
                {
                    _attack = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsDead => Health <= 0;

        public event PropertyChangedEventHandler PropertyChanged;

        [field: NonSerialized]
        public event Action<CreatureCard> OnDeath;
        [field: NonSerialized]
        public event Action<CreatureCard> OnDamage;
        [field: NonSerialized]
        public event Action<CreatureCard> OnHeal;

        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ManaCost { get; set; }
        public CardRarity Rarity { get; set; }
        public CardType Type => CardType.Creature;
        public Faction Faction { get; set; }
        public int MaxHealth { get; set; }
        public string ImagePath { get; set; }

        public List<Ability> Abilities { get; set; } = new List<Ability>();
        public bool CanAttack { get; set; }
        public bool IsExhausted { get; set; }
        public bool IsTaunt => Abilities.Any(a => a.Type == AbilityType.Taunt && a.IsActive);
        public bool IsCharge => Abilities.Any(a => a.Type == AbilityType.Charge && a.IsActive);
        public bool HasDivineShield => Abilities.Any(a => a.Type == AbilityType.DivineShield && a.IsActive);
        public bool IsFrozen { get; set; }

        public CreatureCard()
        {
            Id = Guid.NewGuid().ToString();
            ImagePath = string.Empty;
        }

        public CreatureCard(string name, int manaCost, int attack, int health, Faction faction,
                          CardRarity rarity = CardRarity.Common) : this()
        {
            Name = name;
            ManaCost = manaCost;
            Attack = attack;
            Health = health;
            MaxHealth = health;
            Faction = faction;
            Rarity = rarity;
            ImagePath = GenerateImagePath(name, faction);
        }

        private string GenerateImagePath(string cardName, Faction faction)
        {
            string imageName = cardName.ToLower()
                .Replace(" ", "_")
                .Replace("'", "")
                .Replace("-", "_")
                .Replace("(", "")
                .Replace(")", "");
            string factionFolder = faction.ToString().ToLower();

            return $"pack://application:,,,/Resources/Cards/{factionFolder}/{imageName}.jpg";
        }

        public void TriggerDeath()
        {
            OnDeath?.Invoke(this);
        }

        public void TakeDamage(int damage, bool ignoreDivineShield = false)
        {
            if (HasDivineShield && !ignoreDivineShield)
            {
                RemoveAbility(AbilityType.DivineShield);
                OnDamage?.Invoke(this);
                return;
            }

            Health -= damage;
            OnDamage?.Invoke(this);

            if (Health <= 0)
            {
                TriggerDeath();
            }
        }

        public void Heal(int amount)
        {
            Health = Math.Min(Health + amount, MaxHealth);
            OnHeal?.Invoke(this);
        }

        public void AddAbility(Ability ability)
        {
            Abilities.Add(ability);
        }

        public void RemoveAbility(AbilityType abilityType)
        {
            Abilities.RemoveAll(a => a.Type == abilityType);
        }

        public bool HasAbility(AbilityType abilityType)
        {
            return Abilities.Any(a => a.Type == abilityType && a.IsActive);
        }

        public void ResetForNewTurn()
        {
            CanAttack = !IsFrozen;
            IsExhausted = false;

            if (IsFrozen)
            {
                IsFrozen = false;
            }
        }

        public void Exhaust()
        {
            CanAttack = false;
            IsExhausted = true;
        }

        public object Clone()
        {
            return new CreatureCard
            {
                Id = Guid.NewGuid().ToString(),
                Name = Name,
                Description = Description,
                ManaCost = ManaCost,
                Rarity = Rarity,
                Faction = Faction,
                Attack = Attack,
                Health = Health,
                MaxHealth = MaxHealth,
                ImagePath = ImagePath,
                Abilities = Abilities.Select(a => (Ability)a.Clone()).ToList(),
                CanAttack = CanAttack,
                IsExhausted = IsExhausted,
                IsFrozen = IsFrozen
            };
        }

        public override string ToString()
        {
            var abilities = Abilities.Count > 0 ?
                $" [{string.Join(",", Abilities.Where(a => a.IsActive).Select(a => a.Symbol))}]" : "";

            var status = CanAttack ? "Может атаковать" : "Не может атаковать";

            return $"{Name}  Редкость: {Rarity} (Атака: {Attack}/ ХП: {Health}) {abilities} [{status}]";
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}