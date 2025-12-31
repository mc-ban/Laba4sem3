using System;
using System.Collections.Generic;
using System.Text;

namespace CardGame.Core.Models
{
    public class CardFactory
    {
        // Хелпер-метод для генерации пути к изображению
        private string GenerateImagePath(string cardName, Faction faction)
        {
            // Словарь соответствий русских названий английским
            var cardMap = new Dictionary<string, string>
    {
        // Humans
        {"Молодой дракон", "young_dragon"},
        {"Берсерк", "berserker"},
        {"Лекарь", "healer"},
        {"Горный Стражник", "mountain_guard"},
        {"Пламенный Щит", "flame_shield"},
        {"Серебряная Лилия", "silver_lily"},
        {"Кровавый Капитан Аргус", "blood_captain_argus"},
        {"Стрелок Вечного Рассвета", "eternal_dawn_rifleman"},
        {"Повелитель фениксов", "phoenix_lord"},
        
        // Beasts
        {"Громовой Кабан", "thunder_boar"},
        {"Лунная Рысь", "moon_lynx"},
        {"Теневой Волчонок", "shadow_wolf_cub"},
        {"Старый Медвежий Властелин", "old_bear_lord"},
        {"Ворон-Падальщик Клык", "carrion_raven_fang"},
        {"Громовое Крыло", "thunder_wing"},
        
        // Mythical
        {"Огненный Саламандр", "fire_salamander"},
        {"Лесной Фейлик", "forest_fae"},
        {"Снежная Сирена", "snow_siren"},
        {"Грифон Степей", "steppe_griffin"},
        {"Лунный Единорог", "moon_unicorn"},
        {"Феникс Огненных Пеплов", "phoenix_fire_ash"},
        
        // Elements
        {"Искристый Шепот", "sparkling_whisper"},
        {"Песчаный Прыгун", "sand_jumper"},
        {"Каменный Бастион", "stone_bastion"},
        {"Ледяной Страж Мороза", "frost_guardian"},
        {"Огненный Вихрь", "fire_vortex"},
        {"Приливный Колосс", "tidal_colossus"},
        
        // Spells
        {"Огненный шар", "fireball"},
        {"Исцеление", "healing"},
        {"Щит", "shield"}
    };

            if (cardMap.TryGetValue(cardName, out string englishName))
            {
                string factionFolder = faction.ToString().ToLower();
                return $"{factionFolder}/{englishName}.jpg";
            }

            // Если карта не найдена в словаре, используем дефолтное изображение
            return $"{faction.ToString().ToLower()}/default.jpg";
        }

        // Создает стартовую колоду из 30 карт для указанной фракции
        public List<ICard> CreateStarterDeck(Faction faction)
        {
            var deck = new List<ICard>();

            // Базовые карты для всех фракций (нейтральные)
            deck.AddRange(CreateBasicNeutralCards());

            // Карты выбранной фракции
            deck.AddRange(GetFactionCards(faction));

            // Добавляем случайные карты для набора до 30 карт в колоде
            var random = new Random();
            while (deck.Count < 30)
            {
                deck.Add(CreateRandomCard(faction, random));
            }

            return deck;
        }

        private List<ICard> GetFactionCards(Faction faction)
        {
            return faction switch
            {
                Faction.Humans => CreateHumanCards(),
                Faction.Beasts => CreateBeastCards(),
                Faction.Mythical => CreateMythicalCards(),
                Faction.Elements => CreateElementCards(),
                _ => CreateHumanCards()
            };
        }

        private ICard CreateRandomCard(Faction faction, Random random)
        {
            var cards = GetFactionCards(faction);
            return cards[random.Next(cards.Count)];
        }

        private List<ICard> CreateBasicNeutralCards()
        {
            var cards = new List<ICard>
            {
                // Нейтральные существа
                new CreatureCard("Молодой дракон", 4, 3, 5, Faction.Humans)
                {
                    Description = "Молодой, но уже опасный дракон",
                    Abilities = { new Ability(AbilityType.Taunt, Trigger.Permanent) },
                    ImagePath = GenerateImagePath("Молодой дракон", Faction.Humans)
                },

                new CreatureCard("Берсерк", 3, 4, 2, Faction.Humans)
                {
                    Description = "Агрессивный воин",
                    Abilities = { new Ability(AbilityType.Charge, Trigger.Permanent) },
                    ImagePath = GenerateImagePath("Берсерк", Faction.Humans)
                },

                new CreatureCard("Лекарь", 2, 1, 3, Faction.Humans)
                {
                    Description = "Исцеляет союзников",
                    Abilities = { new Ability(AbilityType.Battlecry, Trigger.OnPlay) },
                    ImagePath = GenerateImagePath("Лекарь", Faction.Humans)
                },
                
                // Нейтральные заклинания
                new SpellCard("Огненный шар", 4, new SpellEffect
                {
                    Type = SpellEffectType.Damage,
                    Value = 6
                }, Faction.Humans, TargetType.AnyCreature)
                {
                    Description = "Наносит 6 урона цели",
                    ImagePath = GenerateImagePath("Огненный шар", Faction.Humans)
                },

                new SpellCard("Исцеление", 2, new SpellEffect
                {
                    Type = SpellEffectType.Heal,
                    Value = 5
                }, Faction.Humans, TargetType.AnyCreature)
                {
                    Description = "Восстанавливает 5 здоровья цели",
                    ImagePath = GenerateImagePath("Исцеление", Faction.Humans)
                },

                new SpellCard("Щит", 1, new SpellEffect
                {
                    Type = SpellEffectType.Buff,
                    Value = 2
                }, Faction.Humans, TargetType.FriendlyCreature)
                {
                    Description = "Даёт +0/+2 существу",
                    ImagePath = GenerateImagePath("Щит", Faction.Humans)
                }
            };

            return cards;
        }

        private List<ICard> CreateHumanCards()
        {
            var cards = new List<ICard>
            {
                new CreatureCard("Горный Стражник", 2, 2, 3, Faction.Humans, CardRarity.Common)
                {
                    Description = "Стойкий защитник горных перевалов",
                    Abilities = { new Ability(AbilityType.Taunt, Trigger.Permanent) },
                    ImagePath = GenerateImagePath("Горный Стражник", Faction.Humans)
                },

                new CreatureCard("Пламенный Щит", 1, 1, 2, Faction.Humans, CardRarity.Common)
                {
                    Description = "Щит, пылающий священным огнём",
                    Abilities = { new Ability(AbilityType.DivineShield, Trigger.Permanent) },
                    ImagePath = GenerateImagePath("Пламенный Щит", Faction.Humans)
                },

                new CreatureCard("Серебряная Лилия", 3, 2, 4, Faction.Humans, CardRarity.Common)
                {
                    Description = "Маг, специализирующийся на защитных чарах",
                    ImagePath = GenerateImagePath("Серебряная Лилия", Faction.Humans)
                },

                new CreatureCard("Кровавый Капитан Аргус", 4, 4, 5, Faction.Humans, CardRarity.Rare)
                {
                    Description = "Ветеран множества битв",
                    Abilities = {
                        new Ability(AbilityType.Battlecry, Trigger.OnPlay)
                        {
                            Description = "Даёт соседним существам +1/+1 и Taunt"
                        }
                    },
                    ImagePath = GenerateImagePath("Кровавый Капитан Аргус", Faction.Humans)
                },

                new CreatureCard("Стрелок Вечного Рассвета", 3, 3, 2, Faction.Humans, CardRarity.Rare)
                {
                    Description = "Снайпер, чьи стрелы никогда не промахиваются",
                    Abilities = {
                        new Ability(AbilityType.Battlecry, Trigger.OnPlay)
                        {
                            Description = "Наносит 2 урона любому существу"
                        }
                    },
                    ImagePath = GenerateImagePath("Стрелок Вечного Рассвета", Faction.Humans)
                },

                new CreatureCard("Повелитель фениксов", 6, 5, 6, Faction.Humans, CardRarity.Epic)
                {
                    Description = "Мастер огненных искусств",
                    Abilities = {
                        new Ability(AbilityType.SpellDamage, Trigger.Permanent) { Value = 2 },
                        new Ability(AbilityType.Battlecry, Trigger.OnPlay)
                        {
                            Description = "Наносит 3 урона двум случайным вражеским существам"
                        }
                    },
                    ImagePath = GenerateImagePath("Повелитель фениксов", Faction.Humans)
                }
            };

            return cards;
        }

        private List<ICard> CreateBeastCards()
        {
            var cards = new List<ICard>
            {
                new CreatureCard("Громовой Кабан", 3, 4, 3, Faction.Beasts, CardRarity.Common)
                {
                    Description = "Мощный зверь с электрическими клыками",
                    Abilities = { new Ability(AbilityType.Charge, Trigger.Permanent) },
                    ImagePath = GenerateImagePath("Громовой Кабан", Faction.Beasts)
                },

                new CreatureCard("Лунная Рысь", 2, 3, 2, Faction.Beasts, CardRarity.Common)
                {
                    Description = "Быстрый и незаметный хищник",
                    Abilities = { new Ability(AbilityType.Stealth, Trigger.Permanent) },
                    ImagePath = GenerateImagePath("Лунная Рысь", Faction.Beasts)
                },

                new CreatureCard("Теневой Волчонок", 1, 2, 1, Faction.Beasts, CardRarity.Common)
                {
                    Description = "Молодой волк, сливающийся с тенями",
                    ImagePath = GenerateImagePath("Теневой Волчонок", Faction.Beasts)
                },

                new CreatureCard("Старый Медвежий Властелин", 5, 4, 7, Faction.Beasts, CardRarity.Rare)
                {
                    Description = "Древний медведь, повелевающий лесом",
                    Abilities = {
                        new Ability(AbilityType.Taunt, Trigger.Permanent),
                        new Ability(AbilityType.Deathrattle, Trigger.OnDeath)
                        {
                            Description = "Призывает двух Волчонков (2/1)"
                        }
                    },
                    ImagePath = GenerateImagePath("Старый Медвежий Властелин", Faction.Beasts)
                },

                new CreatureCard("Ворон-Падальщик Клык", 4, 3, 3, Faction.Beasts, CardRarity.Rare)
                {
                    Description = "Хищная птица, питающаяся падалью",
                    Abilities = {
                        new Ability(AbilityType.Deathrattle, Trigger.OnDeath)
                        {
                            Description = "Восстанавливает 3 здоровья вашему герою"
                        }
                    },
                    ImagePath = GenerateImagePath("Ворон-Падальщик Клык", Faction.Beasts)
                },

                new CreatureCard("Громовое Крыло", 7, 6, 8, Faction.Beasts, CardRarity.Epic)
                {
                    Description = "Гигантская птица, вызывающая бури",
                    Abilities = {
                        new Ability(AbilityType.Windfury, Trigger.Permanent),
                        new Ability(AbilityType.Battlecry, Trigger.OnPlay)
                        {
                            Description = "Даёт другим вашим существам +1 атаки"
                        }
                    },
                    ImagePath = GenerateImagePath("Громовое Крыло", Faction.Beasts)
                }
            };

            return cards;
        }

        private List<ICard> CreateMythicalCards()
        {
            var cards = new List<ICard>
            {
                new CreatureCard("Огненный Саламандр", 1, 2, 1, Faction.Mythical, CardRarity.Common)
                {
                    Description = "Маленький, быстрый огненный элементаль",
                    Abilities = { new Ability(AbilityType.Charge, Trigger.Permanent) },
                    ImagePath = GenerateImagePath("Огненный Саламандр", Faction.Mythical)
                },

                new CreatureCard("Лесной Фейлик", 2, 1, 2, Faction.Mythical, CardRarity.Common)
                {
                    Description = "Крошечное существо, усиливает союзников",
                    Abilities = {
                        new Ability(AbilityType.Battlecry, Trigger.OnPlay)
                        {
                            Description = "Даёт случайному союзному существу +1/+1"
                        }
                    },
                    ImagePath = GenerateImagePath("Лесной Фейлик", Faction.Mythical)
                },

                new CreatureCard("Снежная Сирена", 2, 1, 3, Faction.Mythical, CardRarity.Common)
                {
                    Description = "Слабое ледяное существо с эффектом замедления",
                    Abilities = {
                        new Ability(AbilityType.Freeze, Trigger.OnAttack)
                        {
                            Description = "Замораживает атакованное существо"
                        }
                    },
                    ImagePath = GenerateImagePath("Снежная Сирена", Faction.Mythical)
                },

                new CreatureCard("Грифон Степей", 4, 4, 3, Faction.Mythical, CardRarity.Rare)
                {
                    Description = "Может атаковать сверху, игнорируя слабых существ",
                    Abilities = {
                        new Ability(AbilityType.Battlecry, Trigger.OnPlay)
                        {
                            Description = "Наносит 2 урона случайному вражескому существу"
                        }
                    },
                    ImagePath = GenerateImagePath("Грифон Степей", Faction.Mythical)
                },

                new CreatureCard("Лунный Единорог", 5, 3, 5, Faction.Mythical, CardRarity.Rare)
                {
                    Description = "Лечит союзников или даёт бафф при появлении",
                    Abilities = {
                        new Ability(AbilityType.Battlecry, Trigger.OnPlay)
                        {
                            Description = "Восстанавливает 3 здоровья всем союзным существам"
                        },
                        new Ability(AbilityType.Lifesteal, Trigger.Permanent)
                    },
                    ImagePath = GenerateImagePath("Лунный Единорог", Faction.Mythical)
                },

                new CreatureCard("Феникс Огненных Пеплов", 8, 7, 7, Faction.Mythical, CardRarity.Epic)
                {
                    Description = "Возрождается с частью здоровья после смерти",
                    Abilities = {
                        new Ability(AbilityType.Reborn, Trigger.OnDeath),
                        new Ability(AbilityType.Battlecry, Trigger.OnPlay)
                        {
                            Description = "Наносит 2 урона всем вражеским существам"
                        },
                        new Ability(AbilityType.DivineShield, Trigger.Permanent)
                    },
                    ImagePath = GenerateImagePath("Феникс Огненных Пеплов", Faction.Mythical)
                }
            };

            return cards;
        }

        private List<ICard> CreateElementCards()
        {
            var cards = new List<ICard>
            {
                new CreatureCard("Искристый Шепот", 1, 2, 1, Faction.Elements, CardRarity.Common)
                {
                    Description = "Элементаль электрических искр",
                    ImagePath = GenerateImagePath("Искристый Шепот", Faction.Elements)
                },

                new CreatureCard("Песчаный Прыгун", 2, 3, 2, Faction.Elements, CardRarity.Common)
                {
                    Description = "Быстрый элементаль песка",
                    ImagePath = GenerateImagePath("Песчаный Прыгун", Faction.Elements)
                },

                new CreatureCard("Каменный Бастион", 3, 1, 6, Faction.Elements, CardRarity.Common)
                {
                    Description = "Защитник из камня",
                    Abilities = { new Ability(AbilityType.Taunt, Trigger.Permanent) },
                    ImagePath = GenerateImagePath("Каменный Бастион", Faction.Elements)
                },

                new CreatureCard("Ледяной Страж Мороза", 4, 3, 5, Faction.Elements, CardRarity.Rare)
                {
                    Description = "Элементаль вечной мерзлоты",
                    Abilities = {
                        new Ability(AbilityType.Freeze, Trigger.OnDamage)
                        {
                            Description = "Замораживает атакующее существо"
                        }
                    },
                    ImagePath = GenerateImagePath("Ледяной Страж Мороза", Faction.Elements)
                },

                new CreatureCard("Огненный Вихрь", 5, 4, 4, Faction.Elements, CardRarity.Rare)
                {
                    Description = "Вращающийся элементаль огня",
                    Abilities = {
                        new Ability(AbilityType.Deathrattle, Trigger.OnDeath)
                        {
                            Description = "Наносит 2 урона всем существам"
                        }
                    },
                    ImagePath = GenerateImagePath("Огненный Вихрь", Faction.Elements)
                },

                new CreatureCard("Приливный Колосс", 9, 8, 10, Faction.Elements, CardRarity.Epic)
                {
                    Description = "Гигантский элементаль воды",
                    Abilities = {
                        new Ability(AbilityType.Taunt, Trigger.Permanent),
                        new Ability(AbilityType.DivineShield, Trigger.Permanent),
                        new Ability(AbilityType.Battlecry, Trigger.OnPlay)
                        {
                            Description = "Замораживает всех вражеских существ"
                        }
                    },
                    ImagePath = GenerateImagePath("Приливный Колосс", Faction.Elements)
                }
            };

            return cards;
        }
    }
}