using System;
using System.Collections.Generic;

namespace CardGame.Core.Models
{
    [Serializable]
    public class SpellCard : ICard
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ManaCost { get; set; }
        public CardRarity Rarity { get; set; }
        public CardType Type => CardType.Spell;
        public Faction Faction { get; set; }

        // Добавляем setter
        public string ImagePath { get; set; }

        public SpellEffect Effect { get; set; }
        public TargetType TargetType { get; set; }

        public SpellCard()
        {
            Id = Guid.NewGuid().ToString();
            ImagePath = string.Empty; // Инициализируем!
        }

        // Обновленный конструктор
        public SpellCard(string name, int manaCost, SpellEffect effect, Faction faction,
                        TargetType targetType = TargetType.AnyCreature) : this()
        {
            Name = name;
            ManaCost = manaCost;
            Effect = effect;
            Faction = faction;
            TargetType = targetType;

            // Генерируем ImagePath автоматически
            ImagePath = GenerateImagePath(name, faction);
        }

        // Новый метод для генерации пути к изображению
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

        public object Clone()
        {
            return new SpellCard
            {
                Id = Guid.NewGuid().ToString(),
                Name = Name,
                Description = Description,
                ManaCost = ManaCost,
                Rarity = Rarity,
                Faction = Faction,
                // Копируем ImagePath!
                ImagePath = ImagePath,
                Effect = (SpellEffect)Effect?.Clone(),
                TargetType = TargetType
            };
        }

        public override string ToString()
        {
            return $"{Name} - {Description} (Мана: {ManaCost})";
        }
    }

    [Serializable]
    public class SpellEffect : ICloneable
    {
        public SpellEffectType Type { get; set; }
        public int Value { get; set; }
        public string AdditionalData { get; set; }

        public object Clone()
        {
            return new SpellEffect
            {
                Type = Type,
                Value = Value,
                AdditionalData = AdditionalData
            };
        }
    }

    public enum SpellEffectType
    {
        Damage,
        Heal,
        Buff,
        Summon,
        Draw,
        Freeze,
        Silence,
        ReturnToHand,
        Destroy
    }

    public enum TargetType
    {
        AnyCreature,
        FriendlyCreature,
        EnemyCreature,
        AllCreatures,
        AllEnemyCreatures,
        AllFriendlyCreatures,
        EnemyPlayer,
        FriendlyPlayer,
        NoTarget
    }
}