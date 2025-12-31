using System;

namespace CardGame.Core.Models
{
        public interface ICard : ICloneable
        {
            string Id { get; set; }
            string Name { get; set; }
            string Description { get; set; }
            int ManaCost { get; set; }
            CardRarity Rarity { get; set; }
            CardType Type { get; }
            Faction Faction { get; set; }

            // Измените с { get; } на { get; set; }
            string ImagePath { get; set; }
        }

        public enum CardRarity { Common, Rare, Epic }
        public enum CardType { Creature, Spell }
        public enum Faction { Humans, Beasts, Mythical, Elements }
    }     // Наследует ICloneable для возможности копирования карт