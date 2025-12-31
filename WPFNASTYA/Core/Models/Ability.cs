using System;

namespace CardGame.Core.Models
{
    // Класс, представляющий способность карты или существа
    // Реализует интерфейс ICloneable для поддержки глубокого копирования
    [Serializable]
    public class Ability : ICloneable
    {
        // Основные свойства способности
        public string Name { get; set; }              // Название способности (обычно равно имени типа)
        public string Description { get; set; }       // Описание способности для игрока
        public AbilityType Type { get; set; }         // Тип способности из перечисления AbilityType
        public Trigger Trigger { get; set; }          // Условие активации способности из перечисления Trigger
        public int Value { get; set; }                // Числовое значение способности (например, бонус к урону)
        public bool IsActive { get; set; } = true;    // Флаг активности способности (неактивные не работают)

        // Вычисляемое свойство, возвращающее символьное представление способности на русском
        public string Symbol => GetSymbol(Type);

        // Конструктор способности с параметрами
        // Если описание не указано, используется описание по умолчанию для данного типа
        public Ability(AbilityType type, Trigger trigger, int value = 0, string description = null)
        {
            Type = type;
            Trigger = trigger;
            Value = value;
            Description = description ?? GetDefaultDescription(type, value);  // Используем переданное или дефолтное описание
            Name = type.ToString();  // Имя равно строковому представлению типа
        }

        // Возвращает описание по умолчанию для указанного типа способности
        // Использует switch-выражение для выбора подходящего описания
        private string GetDefaultDescription(AbilityType type, int value)
        {
            return type switch
            {
                AbilityType.Taunt => "Противник должен атаковать это существо первым",
                AbilityType.Charge => "Может атаковать в тот же ход",
                AbilityType.DivineShield => "Игнорирует первый полученный урон",
                AbilityType.Windfury => "Может атаковать дважды за ход",
                AbilityType.Poison => "Убивает любое раненное им существо",
                AbilityType.Lifesteal => "Исцеляет вашего героя на нанесённый урон",
                AbilityType.Reborn => "Возрождается с 1 здоровьем после смерти",
                AbilityType.Rush => "Может атаковать других существ в тот же ход",
                AbilityType.Stealth => "Не может быть атакован или выбран целью в первый ход",
                AbilityType.SpellDamage => $"Ваши заклинания наносят на {value} урона больше",  // Использует значение value
                _ => type.ToString()  // Для необработанных типов возвращает имя типа
            };
        }

        // Возвращает символьное представление (русское название) для типа способности
        // Используется для отображения в пользовательском интерфейсе
        private string GetSymbol(AbilityType type)
        {
            return type switch
            {
                AbilityType.Taunt => "Провокация",
                AbilityType.Charge => "Заряд",
                AbilityType.DivineShield => "Божественный щит",
                AbilityType.Windfury => "Ярость ветра",
                AbilityType.Poison => "Яд",
                AbilityType.Lifesteal => "Вампиризм",
                AbilityType.Reborn => "Возрождение",
                AbilityType.Rush => "Спешка",
                AbilityType.Stealth => "Скрытность",
                AbilityType.SpellDamage => "Урон заклинания",
                AbilityType.Battlecry => "Боевой клич",
                AbilityType.Deathrattle => "Смертельный грохот",
                _ => "Неизвестно"  // Запасной вариант для необработанных типов
            };
        }

        // Реализация метода Clone из интерфейса ICloneable
        // Создает глубокую копию объекта Ability
        public object Clone()
        {
            // Создаем новую способность с теми же параметрами
            return new Ability(Type, Trigger, Value, Description)
            {
                // Копируем значение свойства IsActive
                IsActive = IsActive
            };
        }
    }

    // Перечисление типов способностей
    // Определяет все возможные виды способностей в игре
    public enum AbilityType
    {
        Taunt,          // Провокация: противник должен атаковать это существо первым
        Charge,         // Заряд: может атаковать в тот же ход
        DivineShield,   // Божественный щит: игнорирует первый полученный урон
        Windfury,       // Ярость ветра: может атаковать дважды за ход
        Poison,         // Яд: убивает любое раненное им существо
        Lifesteal,      // Вампиризм: исцеляет героя на нанесенный урон
        Reborn,         // Возрождение: возрождается с 1 здоровьем после смерти
        Rush,           // Спешка: может атаковать других существ в тот же ход
        Stealth,        // Скрытность: не может быть атакован в первый ход
        SpellDamage,    // Урон заклинаний: увеличивает урон заклинаний
        Battlecry,      // Боевой клич: эффект при розыгрыше карты
        Deathrattle,    // Смертельный грохот: эффект при смерти существа
        Freeze,         // Заморозка: замораживает цель
        Immune          // Неуязвимость: не может получить урон
    }

    // Перечисление триггеров (условий активации) способностей
    // Определяет, когда именно активируется способность
    public enum Trigger
    {
        OnPlay,          // При розыгрыше карты (для Battlecry)
        OnAttack,        // При атаке существа
        OnDamage,        // При получении урона (для DivineShield)
        OnHeal,          // При исцелении
        OnDeath,         // При смерти существа (для Deathrattle)
        OnTurnStart,     // В начале хода
        OnTurnEnd,       // В конце хода
        OnMinionSummon,  // При призыве существа
        OnSpellCast,     // При применении заклинания
        Permanent        // Постоянный эффект (для Taunt, Charge и др.)
    }
}