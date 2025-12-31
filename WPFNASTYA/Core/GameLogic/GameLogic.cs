using System;
using System.Linq;
using CardGame.Core.GameState;
using CardGame.Core.Models;

namespace CardGame.Core.GameLogic
{
    // Статический класс, содержащий основные правила игры и константы для баланса
    public static class GameRules
    {
        // Константы для балансировки игрового процесса

        // Максимальное количество существ, которое может одновременно находиться на поле у одного игрока
        public const int MAX_BOARD_SIZE = 7;

        // Максимальное количество карт, которое может одновременно находиться в руке у одного игрока
        public const int MAX_HAND_SIZE = 10;

        // Начальное и максимальное значение здоровья героя (одинаковое значение)
        public const int STARTING_HEALTH = 30;

        // Максимальное количество мана-кристаллов, которое может иметь игрок
        public const int MAX_MANA = 10;

        // Проверяет, может ли игрок призвать новое существо на поле
        // Возвращает true, если на поле есть свободное место (меньше MAX_BOARD_SIZE существ)
        public static bool CanSummonCreature(Player player)
        {
            // Сравниваем текущее количество существ на поле с максимально допустимым
            return player.Board.Count < MAX_BOARD_SIZE;
        }

        // Проверяет, может ли игрок взять карту из колоды
        // Возвращает true, если в руке есть свободное место (меньше MAX_HAND_SIZE карт)
        public static bool CanDrawCard(Player player)
        {
            // Сравниваем текущее количество карт в руке с максимально допустимым
            return player.Hand.Count < MAX_HAND_SIZE;
        }

        // Проверяет, является ли выбранная цель допустимой для атаки
        // Учитывает правило Taunt: если у противника есть существа с Taunt, можно атаковать только их
        public static bool IsValidAttackTarget(CreatureCard attacker, CreatureCard defender, Player opponent)
        {
            // Если у противника есть существа с Taunt, но выбранная цель не имеет Taunt,
            // атака запрещена - нужно сначала атаковать существо с Taunt
            if (opponent.HasTauntCreatures() && !defender.IsTaunt)
                return false;

            // Во всех остальных случаях атака разрешена
            return true;
        }

        // Проверяет, может ли конкретное существо атаковать в текущий момент
        // Учитывает несколько условий: способность к атаке, усталость, заморозку и наличие силы атаки
        public static bool CanAttack(CreatureCard creature)
        {
            // Существо может атаковать, если:
            // 1. Оно имеет право атаковать (CanAttack = true)
            // 2. Оно не уставшее (не атаковало в этом ходу)
            // 3. Оно не заморожено
            // 4. Оно имеет положительную силу атаки (больше 0)
            return creature.CanAttack &&
                   !creature.IsExhausted &&
                   !creature.IsFrozen &&
                   creature.Attack > 0;
        }

        // Применяет урон от заклинания к цели с учетом бонуса к урону заклинаний
        // Учитывает SpellDamageBonus игрока, который применяет заклинание
        public static void ApplySpellDamage(SpellCard spell, Player caster, CreatureCard target)
        {
            // Общий урон = базовый урон заклинания + бонус игрока к урону заклинаний
            var totalDamage = spell.Effect.Value + caster.SpellDamageBonus;

            // Наносим рассчитанный урон цели
            target.TakeDamage(totalDamage);
        }

        // Обрабатывает эффекты, которые активируются при смерти существа
        // В текущей реализации обрабатывает только способность Reborn (Возрождение)
        public static void ProcessDeathrattle(CreatureCard creature, GameState.GameState gameState)
        {
            // Проверяем, имеет ли умершее существо способность Reborn
            if (creature.HasAbility(AbilityType.Reborn))
            {
                // Создаем копию умершего существа для возрождения
                var rebornCreature = (CreatureCard)creature.Clone();

                // Устанавливаем здоровье возрожденного существа в 1
                rebornCreature.Health = 1;

                // Помечаем существо как уставшее (не может атаковать в этот ход)
                rebornCreature.IsExhausted = true;

                // Запрещаем атаку в этот ход
                rebornCreature.CanAttack = false;

                // Удаляем способность Reborn, чтобы избежать бесконечной рекурсии
                // при повторной смерти возрожденного существа
                rebornCreature.RemoveAbility(AbilityType.Reborn);

                // Добавляем возрожденное существо на поле текущего игрока
                gameState.CurrentPlayer.Board.Add(rebornCreature);

                // Записываем сообщение в лог игры
                gameState.Log($"{creature.Name} возрождается с 1 здоровьем!");
            }
        }

        // Рассчитывает урон от усталости (Fatigue) для игрока
        // Урон от усталости увеличивается с каждым ходом: 1, 2, 3, 4, ...
        public static int CalculateFatigueDamage(int fatigueCount)
        {
            // Возвращаем текущее значение счетчика усталости как урон
            return fatigueCount;
        }

        // Проверяет, достигнуты ли условия для завершения игры
        // Игра завершается, когда здоровье хотя бы одного из игроков достигает 0 или меньше
        public static bool IsGameOver(Player player1, Player player2)
        {
            // Игра завершена, если здоровье первого ИЛИ второго игрока <= 0
            return player1.Health <= 0 || player2.Health <= 0;
        }

        // Определяет победителя игры на основе здоровья игроков
        // Возвращает победителя или null в случае ничьи
        public static Player GetWinner(Player player1, Player player2)
        {
            // Если здоровье обоих игроков <= 0 - ничья
            if (player1.Health <= 0 && player2.Health <= 0)
                return null; // Возвращаем null для обозначения ничьи

            // Если здоровье первого игрока <= 0 - победа второго игрока
            if (player1.Health <= 0)
                return player2;

            // В остальных случаях - победа первого игрока
            return player1;
        }
    }
}