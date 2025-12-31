using System;
using System.Collections.Generic;
using CardGame.Core.Models;

namespace CardGame.GUI.Extensions
{
    public static class CardExtensions
    {
        private static readonly Dictionary<string, string> _imageCache = new Dictionary<string, string>();

        public static string ImagePath(this ICard card)
        {
            string key = card.Id;

            if (_imageCache.ContainsKey(key))
                return _imageCache[key];

            string imageName = GetImageName(card);
            string path = $"/Resources/Cards/{imageName}.png";

            _imageCache[key] = path;
            return path;
        }

        private static string GetImageName(ICard card)
        {
            if (card is CreatureCard creature)
            {
                return $"{creature.Faction}_{creature.Name.Replace(" ", "_")}";
            }

            return $"spell_{card.Name.Replace(" ", "_")}";
        }
    }
}