
using CardGame.Core.GameState;
using CardGame.Core.Models;

namespace TestWPF
{
    [TestClass]
    public class GameTests
    {
        #region Тесты Player

        
        [TestMethod]
        public void Player_DrawCard_ShouldAddCardToHand()
        {
            // Arrange
            var player = new Player("TestPlayer", Faction.Humans);
            var card = new CreatureCard
            {
                Name = "Test Creature",
                ManaCost = 3,
                Attack = 3,
                Health = 3
            };
            player.Deck = new List<ICard> { card };

            // Act
            var result = player.DrawCard();

            // Assert
            Assert.AreEqual(1, result.CardsDrawn);
            Assert.AreEqual(0, result.CardsBurned);
            Assert.AreEqual(1, player.Hand.Count);
            Assert.AreEqual(0, player.Deck.Count);
            Assert.AreEqual(card.Name, player.Hand[0].Name);
        }

        [TestMethod]
        public void Player_DrawCard_WhenDeckEmpty_ShouldCauseFatigue()
        {
            // Arrange
            var player = new Player("TestPlayer", Faction.Humans);
            player.Deck = new List<ICard>();
            int initialHealth = player.Health;

            // Act
            var result = player.DrawCard();

            // Assert
            Assert.AreEqual(0, result.CardsDrawn);
            Assert.AreEqual(1, result.FatigueDamageTaken);
            Assert.IsTrue(player.Health < initialHealth);
        }

        [TestMethod]
        public void Player_SpendMana_ShouldReduceMana()
        {
            // Arrange
            var player = new Player("TestPlayer", Faction.Humans);
            player.Mana = 5;
            player.MaxMana = 10;

            // Act
            player.SpendMana(3);

            // Assert
            Assert.AreEqual(2, player.Mana);
            Assert.AreEqual(10, player.MaxMana);
        }

        [TestMethod]
        public void Player_SpendMana_MoreThanAvailable_ShouldSetToZero()
        {
            // Arrange
            var player = new Player("TestPlayer", Faction.Humans);
            player.Mana = 3;
            player.MaxMana = 10;

            // Act
            player.SpendMana(5);

            // Assert
            Assert.AreEqual(0, player.Mana);
        }

        [TestMethod]
        public void Player_TakeDamage_ShouldReduceHealth()
        {
            // Arrange
            var player = new Player("TestPlayer", Faction.Humans);
            int initialHealth = player.Health;

            // Act
            player.TakeDamage(5, null);

            // Assert
            Assert.AreEqual(initialHealth - 5, player.Health);
        }

        [TestMethod]
        public void Player_TakeDamage_ShouldNotGoBelowZero()
        {
            // Arrange
            var player = new Player("TestPlayer", Faction.Humans);
            player.Health = 3;

            // Act
            player.TakeDamage(10, null);

            // Assert
            Assert.AreEqual(0, player.Health);
        }

        [TestMethod]
        public void Player_Heal_ShouldIncreaseHealth()
        {
            // Arrange
            var player = new Player("TestPlayer", Faction.Humans);
            player.Health = 20;

            // Act
            player.Heal(5);

            // Assert
            Assert.AreEqual(25, player.Health);
        }

        [TestMethod]
        public void Player_Heal_ShouldNotExceedMaxHealth()
        {
            // Arrange
            var player = new Player("TestPlayer", Faction.Humans);
            player.Health = 28;
            player.MaxHealth = 30;

            // Act
            player.Heal(5);

            // Assert
            Assert.AreEqual(30, player.Health);
        }

        [TestMethod]
        public void Player_ResetMana_ShouldIncreaseManaCrystals()
        {
            // Arrange
            var player = new Player("TestPlayer", Faction.Humans);
            player.ManaCrystals = 3;
            player.Mana = 0;
            player.MaxMana = 3;

            // Act
            player.ResetMana();

            // Assert
            Assert.AreEqual(4, player.ManaCrystals);
            Assert.AreEqual(4, player.MaxMana);
            Assert.AreEqual(4, player.Mana);
        }

        [TestMethod]
        public void Player_ResetMana_ShouldNotExceedTenCrystals()
        {
            // Arrange
            var player = new Player("TestPlayer", Faction.Humans);
            player.ManaCrystals = 10;

            // Act
            player.ResetMana();

            // Assert
            Assert.AreEqual(10, player.ManaCrystals); // Не должно превышать 10
            Assert.AreEqual(10, player.MaxMana);
            Assert.AreEqual(10, player.Mana);
        }

        #endregion

        #region Тесты CreatureCard

        [TestMethod]
        public void CreatureCard_TakeDamage_ShouldReduceHealth()
        {
            // Arrange
            var creature = new CreatureCard
            {
                Name = "Test Creature",
                Health = 10,
                MaxHealth = 10
            };

            // Act
            creature.TakeDamage(3);

            // Assert
            Assert.AreEqual(7, creature.Health);
        }

        [TestMethod]
        public void CreatureCard_Heal_ShouldIncreaseHealth()
        {
            // Arrange
            var creature = new CreatureCard
            {
                Name = "Test Creature",
                Health = 5,
                MaxHealth = 10
            };

            // Act
            creature.Heal(3);

            // Assert
            Assert.AreEqual(8, creature.Health);
        }

        [TestMethod]
        public void CreatureCard_Heal_ShouldNotExceedMaxHealth()
        {
            // Arrange
            var creature = new CreatureCard
            {
                Name = "Test Creature",
                Health = 8,
                MaxHealth = 10
            };

            // Act
            creature.Heal(5);

            // Assert
            Assert.AreEqual(10, creature.Health);
        }

        [TestMethod]
        public void CreatureCard_Exhaust_ShouldPreventAttack()
        {
            // Arrange
            var creature = new CreatureCard
            {
                Name = "Test Creature",
                CanAttack = true,
                IsExhausted = false
            };

            // Act
            creature.Exhaust();

            // Assert
            Assert.IsTrue(creature.IsExhausted);
            Assert.IsFalse(creature.CanAttack);
        }

        
    
        #endregion

        #region Тесты GameState

        [TestMethod]
        public void GameState_Constructor_ShouldInitializeGame()
        {
            // Arrange
            var player1 = new Player("Player1", Faction.Humans);
            var player2 = new Player("Player2", Faction.Beasts);

            // Act
            var gameState = new GameState(player1, player2);

            // Assert
            Assert.AreEqual(player1, gameState.Player1);
            Assert.AreEqual(player2, gameState.Player2);
            Assert.AreEqual(player1, gameState.CurrentPlayer);
            Assert.AreEqual(player2, gameState.OpponentPlayer);
            Assert.AreEqual(1, gameState.TurnNumber);
            Assert.AreEqual(GameStatus.Active, gameState.Status);
            Assert.IsNotNull(gameState.GameLog);
        }

        [TestMethod]
        public void GameState_EndTurn_ShouldSwitchPlayers()
        {
            // Arrange
            var player1 = new Player("Player1", Faction.Humans);
            var player2 = new Player("Player2", Faction.Beasts);
            var gameState = new GameState(player1, player2);

            // Act
            gameState.EndTurn();

            // Assert
            Assert.AreEqual(player2, gameState.CurrentPlayer);
            Assert.AreEqual(player1, gameState.OpponentPlayer);
            Assert.AreEqual(2, gameState.TurnNumber);
        }


        [TestMethod]
        public void GameState_AttackPlayer_ShouldDamageOpponent()
        {
            // Arrange
            var player1 = new Player("Player1", Faction.Humans);
            var player2 = new Player("Player2", Faction.Beasts);
            var gameState = new GameState(player1, player2);

            var attacker = new CreatureCard
            {
                Name = "Attacker",
                Attack = 5,
                Health = 5,
                CanAttack = true
            };

            player1.Board.Add(attacker);
            int initialHealth = player2.Health;

            // Act
            var result = gameState.Attack(attacker, null);

            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual(initialHealth - 5, player2.Health);
            Assert.IsTrue(attacker.IsExhausted);
        }


   
 
        #endregion

        #region Тесты SpellCard

        [TestMethod]
        public void SpellCard_Creation_ShouldInitializeProperties()
        {
            // Arrange & Act
            var spell = new SpellCard
            {
                Name = "Beastsball",
                ManaCost = 4,
                Description = "Deals 6 damage",
                Faction = Faction.Beasts,
                Effect = new SpellEffect
                {
                    Type = SpellEffectType.Damage,
                    Value = 6
                }
            };

            // Assert
            Assert.AreEqual("Beastsball", spell.Name);
            Assert.AreEqual(4, spell.ManaCost);
            Assert.AreEqual(CardType.Spell, spell.Type);
            Assert.AreEqual(SpellEffectType.Damage, spell.Effect.Type);
            Assert.AreEqual(6, spell.Effect.Value);
        }

        [TestMethod]
        public void Player_PlaySpellCard_Damage_ShouldDamageTarget()
        {
            // Arrange
            var player = new Player("TestPlayer", Faction.Humans);
            player.Mana = 10;
            player.MaxMana = 10;

            var spell = new SpellCard
            {
                Name = "Beastsball",
                ManaCost = 4,
                Effect = new SpellEffect
                {
                    Type = SpellEffectType.Damage,
                    Value = 6
                }
            };

            var target = new CreatureCard
            {
                Name = "Target Creature",
                Health = 10
            };

            player.Hand.Add(spell);
            var gameState = new GameState(player, new Player("Opponent", Faction.Beasts));

            // Act
            var result = player.PlayCard(spell, target, gameState);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(4, target.Health); // 10 - 6 = 4
            Assert.AreEqual(6, player.Mana); // 10 - 4 = 6
            Assert.IsFalse(player.Hand.Contains(spell)); // Карта ушла из руки
        }

        [TestMethod]
        public void Player_PlaySpellCard_Heal_ShouldHealTarget()
        {
            // Arrange
            var player = new Player("TestPlayer", Faction.Humans);
            player.Mana = 10;

            var spell = new SpellCard
            {
                Name = "Healing Touch",
                ManaCost = 3,
                Effect = new SpellEffect
                {
                    Type = SpellEffectType.Heal,
                    Value = 8
                }
            };

            var target = new CreatureCard
            {
                Name = "Wounded Creature",
                Health = 5,
                MaxHealth = 10
            };

            player.Hand.Add(spell);
            var gameState = new GameState(player, new Player("Opponent", Faction.Beasts));

            // Act
            var result = player.PlayCard(spell, target, gameState);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(10, target.Health); // 5 + 8 = 13, но максимум 10
            Assert.AreEqual(7, player.Mana); // 10 - 3 = 7
        }

        [TestMethod]
        public void Player_PlaySpellCard_NotEnoughMana_ShouldFail()
        {
            // Arrange
            var player = new Player("TestPlayer", Faction.Humans);
            player.Mana = 2;

            var spell = new SpellCard
            {
                Name = "Expensive Spell",
                ManaCost = 5,
                Effect = new SpellEffect
                {
                    Type = SpellEffectType.Damage,
                    Value = 1
                }
            };

            player.Hand.Add(spell);

            // Act
            var result = player.PlayCard(spell, null, null);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Error.Contains("Недостаточно маны"));
            Assert.AreEqual(2, player.Mana); // Мана не изменилась
            Assert.IsTrue(player.Hand.Contains(spell)); // Карта осталась в руке
        }

        [TestMethod]
        public void Player_PlaySpellCard_NotInHand_ShouldFail()
        {
            // Arrange
            var player = new Player("TestPlayer", Faction.Humans);
            player.Mana = 10;

            var spell = new SpellCard
            {
                Name = "Beastsball",
                ManaCost = 4,
                Effect = new SpellEffect
                {
                    Type = SpellEffectType.Damage,
                    Value = 6
                }
            };

            // Карта НЕ добавлена в руку!

            // Act
            var result = player.PlayCard(spell, null, null);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Error.Contains("не найдена в руке"));
        }

        #endregion

        #region Интеграционные тесты

  
        #endregion

        #region Тесты на граничные случаи

        [TestMethod]
        public void Player_CanPlayCard_CreatureWithFullBoard_ShouldReturnFalse()
        {
            // Arrange
            var player = new Player("TestPlayer", Faction.Humans);
            player.Mana = 10;

            // Заполняем поле
            for (int i = 0; i < 7; i++)
            {
                player.Board.Add(new CreatureCard { Name = $"Creature{i}" });
            }

            var creature = new CreatureCard
            {
                Name = "Extra Creature",
                ManaCost = 3,
            };

            player.Hand.Add(creature);

            // Act
            bool canPlay = player.CanPlayCard(creature);

            // Assert
            Assert.IsFalse(canPlay);
        }
        #endregion
    }
}