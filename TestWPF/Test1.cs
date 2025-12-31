

namespace TestWPF
{
    [TestClass]
    public class GameTests
    {
        #region Тесты Player

        [TestMethod]
        public void Player_Constructor_ShouldInitializeProperties()
        {
            // Arrange & Act
            var player = new Player("TestPlayer", Faction.Nature);

            // Assert
            Assert.AreEqual("TestPlayer", player.Name);
            Assert.AreEqual(Faction.Nature, player.Faction);
            Assert.AreEqual(30, player.Health);
            Assert.AreEqual(30, player.MaxHealth);
            Assert.AreEqual(0, player.Mana);
            Assert.AreEqual(0, player.MaxMana);
            Assert.IsNotNull(player.Hand);
            Assert.IsNotNull(player.Board);
            Assert.IsNotNull(player.Deck);
            Assert.IsNotNull(player.Graveyard);
        }

        [TestMethod]
        public void Player_DrawCard_ShouldAddCardToHand()
        {
            // Arrange
            var player = new Player("TestPlayer", Faction.Nature);
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
            var player = new Player("TestPlayer", Faction.Nature);
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
        public void Player_DrawCard_WhenHandFull_ShouldBurnCards()
        {
            // Arrange
            var player = new Player("TestPlayer", Faction.Nature);

            // Заполняем руку
            for (int i = 0; i < 10; i++)
            {
                player.Hand.Add(new CreatureCard { Name = $"Card{i}" });
            }

            // Добавляем карты в колоду
            player.Deck = new List<ICard>
            {
                new CreatureCard { Name = "BurnMe1" },
                new CreatureCard { Name = "BurnMe2" }
            };

            // Act
            var result = player.DrawCard();

            // Assert
            Assert.AreEqual(0, result.CardsDrawn);
            Assert.AreEqual(2, result.CardsBurned);
            Assert.AreEqual(10, player.Hand.Count); // Рука осталась полной
            Assert.AreEqual(0, player.Deck.Count); // Колода опустела
        }

        [TestMethod]
        public void Player_SpendMana_ShouldReduceMana()
        {
            // Arrange
            var player = new Player("TestPlayer", Faction.Nature);
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
            var player = new Player("TestPlayer", Faction.Nature);
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
            var player = new Player("TestPlayer", Faction.Nature);
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
            var player = new Player("TestPlayer", Faction.Nature);
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
            var player = new Player("TestPlayer", Faction.Nature);
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
            var player = new Player("TestPlayer", Faction.Nature);
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
            var player = new Player("TestPlayer", Faction.Nature);
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
            var player = new Player("TestPlayer", Faction.Nature);
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
        public void CreatureCard_TakeDamage_WithPoison_ShouldKill()
        {
            // Arrange
            var creature = new CreatureCard
            {
                Name = "Test Creature",
                Health = 10
            };

            // Act
            creature.TakeDamage(5, true); // Ядовитый урон

            // Assert
            Assert.AreEqual(0, creature.Health);
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

        [TestMethod]
        public void CreatureCard_ResetForNewTurn_ShouldResetExhaustion()
        {
            // Arrange
            var creature = new CreatureCard
            {
                Name = "Test Creature",
                CanAttack = false,
                IsExhausted = true,
                IsFrozen = true
            };

            // Act
            creature.ResetForNewTurn();

            // Assert
            Assert.IsFalse(creature.IsExhausted);
            Assert.IsTrue(creature.CanAttack);
            Assert.IsFalse(creature.IsFrozen); // Размораживается
        }

        [TestMethod]
        public void CreatureCard_HasAbility_ShouldReturnCorrectValue()
        {
            // Arrange
            var creature = new CreatureCard
            {
                Name = "Test Creature",
                Abilities = new List<Ability>
                {
                    new Ability { Type = AbilityType.Taunt },
                    new Ability { Type = AbilityType.Charge }
                }
            };

            // Act & Assert
            Assert.IsTrue(creature.HasAbility(AbilityType.Taunt));
            Assert.IsTrue(creature.HasAbility(AbilityType.Charge));
            Assert.IsFalse(creature.HasAbility(AbilityType.Lifesteal));
        }

        #endregion

        #region Тесты GameState

        [TestMethod]
        public void GameState_Constructor_ShouldInitializeGame()
        {
            // Arrange
            var player1 = new Player("Player1", Faction.Nature);
            var player2 = new Player("Player2", Faction.Fire);

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
        public void GameState_StartTurn_ShouldResetPlayerMana()
        {
            // Arrange
            var player1 = new Player("Player1", Faction.Nature);
            var player2 = new Player("Player2", Faction.Fire);
            var gameState = new GameState(player1, player2);

            // Игрок использовал всю ману
            player1.Mana = 0;
            player1.MaxMana = 3;

            // Act
            gameState.StartTurn();

            // Assert
            Assert.AreEqual(4, player1.MaxMana); // +1 кристалл маны
            Assert.AreEqual(4, player1.Mana); // Полная мана
        }

        [TestMethod]
        public void GameState_EndTurn_ShouldSwitchPlayers()
        {
            // Arrange
            var player1 = new Player("Player1", Faction.Nature);
            var player2 = new Player("Player2", Faction.Fire);
            var gameState = new GameState(player1, player2);

            // Act
            gameState.EndTurn();

            // Assert
            Assert.AreEqual(player2, gameState.CurrentPlayer);
            Assert.AreEqual(player1, gameState.OpponentPlayer);
            Assert.AreEqual(2, gameState.TurnNumber);
        }

        [TestMethod]
        public void GameState_AttackCreature_ShouldDamageBothCreatures()
        {
            // Arrange
            var player1 = new Player("Player1", Faction.Nature);
            var player2 = new Player("Player2", Faction.Fire);
            var gameState = new GameState(player1, player2);

            var attacker = new CreatureCard
            {
                Name = "Attacker",
                Attack = 3,
                Health = 5,
                CanAttack = true
            };

            var defender = new CreatureCard
            {
                Name = "Defender",
                Attack = 2,
                Health = 4
            };

            player1.Board.Add(attacker);
            player2.Board.Add(defender);

            // Act
            var result = gameState.Attack(attacker, defender);

            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual(2, attacker.Health); // 5 - 2 (атака защитника) = 3
            Assert.AreEqual(1, defender.Health); // 4 - 3 (атака атакующего) = 1
        }

        [TestMethod]
        public void GameState_AttackCreature_WithPoison_ShouldKill()
        {
            // Arrange
            var player1 = new Player("Player1", Faction.Nature);
            var player2 = new Player("Player2", Faction.Fire);
            var gameState = new GameState(player1, player2);

            var attacker = new CreatureCard
            {
                Name = "Poison Attacker",
                Attack = 1,
                Health = 5,
                CanAttack = true,
                Abilities = new List<Ability> { new Ability { Type = AbilityType.Poison } }
            };

            var defender = new CreatureCard
            {
                Name = "Defender",
                Attack = 2,
                Health = 10
            };

            player1.Board.Add(attacker);
            player2.Board.Add(defender);

            // Act
            var result = gameState.Attack(attacker, defender);

            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual(3, attacker.Health); // 5 - 2 = 3
            Assert.AreEqual(0, defender.Health); // Убит ядом
        }

        [TestMethod]
        public void GameState_AttackPlayer_ShouldDamageOpponent()
        {
            // Arrange
            var player1 = new Player("Player1", Faction.Nature);
            var player2 = new Player("Player2", Faction.Fire);
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

        [TestMethod]
        public void GameState_Attack_WithTaunt_ShouldFailWithoutTauntTarget()
        {
            // Arrange
            var player1 = new Player("Player1", Faction.Nature);
            var player2 = new Player("Player2", Faction.Fire);
            var gameState = new GameState(player1, player2);

            var attacker = new CreatureCard
            {
                Name = "Attacker",
                Attack = 3,
                Health = 5,
                CanAttack = true
            };

            var tauntCreature = new CreatureCard
            {
                Name = "Taunt Defender",
                Attack = 1,
                Health = 3,
                IsTaunt = true
            };

            var regularCreature = new CreatureCard
            {
                Name = "Regular Defender",
                Attack = 2,
                Health = 2
            };

            player1.Board.Add(attacker);
            player2.Board.Add(tauntCreature);
            player2.Board.Add(regularCreature);

            // Act - пытаемся атаковать обычное существо при наличии Taunt
            var result = gameState.Attack(attacker, regularCreature);

            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.IsTrue(result.Error.Contains("Taunt"));
        }

        [TestMethod]
        public void GameState_CheckGameOver_Player1Dead_ShouldSetPlayer2Wins()
        {
            // Arrange
            var player1 = new Player("Player1", Faction.Nature);
            var player2 = new Player("Player2", Faction.Fire);
            var gameState = new GameState(player1, player2);

            player1.Health = 0;

            // Act
            // Вызываем приватный метод через рефлексию или через публичный метод, который его вызывает
            var attackResult = gameState.Attack(new CreatureCard
            {
                Name = "Dummy",
                Attack = 0,
                Health = 1,
                CanAttack = true
            }, null);

            // Assert
            Assert.AreEqual(GameStatus.Player2Wins, gameState.Status);
        }

        [TestMethod]
        public void GameState_CheckGameOver_BothDead_ShouldSetDraw()
        {
            // Arrange
            var player1 = new Player("Player1", Faction.Nature);
            var player2 = new Player("Player2", Faction.Fire);
            var gameState = new GameState(player1, player2);

            player1.Health = 0;
            player2.Health = 0;

            // Act
            var attackResult = gameState.Attack(new CreatureCard
            {
                Name = "Dummy",
                Attack = 0,
                Health = 1,
                CanAttack = true
            }, null);

            // Assert
            Assert.AreEqual(GameStatus.Draw, gameState.Status);
        }

        #endregion

        #region Тесты SpellCard

        [TestMethod]
        public void SpellCard_Creation_ShouldInitializeProperties()
        {
            // Arrange & Act
            var spell = new SpellCard
            {
                Name = "Fireball",
                ManaCost = 4,
                Description = "Deals 6 damage",
                Faction = Faction.Fire,
                Rarity = Rarity.Common,
                Effect = new SpellEffect
                {
                    Type = SpellEffectType.Damage,
                    Value = 6
                }
            };

            // Assert
            Assert.AreEqual("Fireball", spell.Name);
            Assert.AreEqual(4, spell.ManaCost);
            Assert.AreEqual(CardType.Spell, spell.Type);
            Assert.AreEqual(SpellEffectType.Damage, spell.Effect.Type);
            Assert.AreEqual(6, spell.Effect.Value);
        }

        [TestMethod]
        public void Player_PlaySpellCard_Damage_ShouldDamageTarget()
        {
            // Arrange
            var player = new Player("TestPlayer", Faction.Nature);
            player.Mana = 10;
            player.MaxMana = 10;

            var spell = new SpellCard
            {
                Name = "Fireball",
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
            var gameState = new GameState(player, new Player("Opponent", Faction.Fire));

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
            var player = new Player("TestPlayer", Faction.Nature);
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
            var gameState = new GameState(player, new Player("Opponent", Faction.Fire));

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
            var player = new Player("TestPlayer", Faction.Nature);
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
            var player = new Player("TestPlayer", Faction.Nature);
            player.Mana = 10;

            var spell = new SpellCard
            {
                Name = "Fireball",
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

        [TestMethod]
        public void FullGameFlow_AttackSequence_ShouldWorkCorrectly()
        {
            // Arrange
            var player1 = new Player("Player1", Faction.Nature);
            var player2 = new Player("Player2", Faction.Fire);
            var gameState = new GameState(player1, player2);

            // Игрок 1 призывает существо
            var creature1 = new CreatureCard
            {
                Name = "Knight",
                ManaCost = 3,
                Attack = 3,
                Health = 3,
                CanAttack = false // Без Charge не может атаковать сразу
            };

            player1.Mana = 10;
            player1.Hand.Add(creature1);

            // Act 1: Игрок 1 разыгрывает существо
            var playResult = gameState.PlayCard(creature1, null);
            Assert.IsTrue(playResult.IsSuccess);

            // Act 2: Завершаем ход игрока 1
            gameState.EndTurn();

            // Act 3: Теперь существо должно быть готово к атаке
            var knight = player1.Board.First();
            knight.CanAttack = true;

            // Act 4: Атакуем героя противника
            var attackResult = gameState.Attack(knight, null);

            // Assert
            Assert.IsTrue(attackResult.IsSuccessful);
            Assert.AreEqual(27, player2.Health); // 30 - 3 = 27
            Assert.IsTrue(knight.IsExhausted);
        }

        [TestMethod]
        public void FullGameFlow_SpellAndAttack_ShouldWorkCorrectly()
        {
            // Arrange
            var player1 = new Player("Player1", Faction.Nature);
            var player2 = new Player("Player2", Faction.Fire);
            var gameState = new GameState(player1, player2);

            // Игрок 2 призывает существо с Taunt
            var tauntCreature = new CreatureCard
            {
                Name = "Taunt Guard",
                Health = 5,
                IsTaunt = true
            };

            player2.Board.Add(tauntCreature);

            // Игрок 1 имеет заклинание урона
            var fireball = new SpellCard
            {
                Name = "Fireball",
                ManaCost = 4,
                Effect = new SpellEffect
                {
                    Type = SpellEffectType.Damage,
                    Value = 6
                }
            };

            player1.Mana = 10;
            player1.Hand.Add(fireball);

            // Act 1: Игрок 1 использует огненный шар на существо с Taunt
            var spellResult = gameState.PlayCard(fireball, tauntCreature);
            Assert.IsTrue(spellResult.IsSuccess);

            // Assert 1: Существо должно умереть (6 урона > 5 здоровья)
            Assert.AreEqual(0, player2.Board.Count);
            Assert.AreEqual(1, player2.Graveyard.Count);

            // Теперь можно атаковать героя напрямую
            var attacker = new CreatureCard
            {
                Name = "Attacker",
                Attack = 4,
                Health = 4,
                CanAttack = true
            };

            player1.Board.Add(attacker);

            // Act 2: Атакуем героя
            var attackResult = gameState.Attack(attacker, null);

            // Assert 2
            Assert.IsTrue(attackResult.IsSuccessful);
            Assert.AreEqual(26, player2.Health); // 30 - 4 = 26
        }

        #endregion

        #region Тесты на граничные случаи

        [TestMethod]
        public void Player_CanPlayCard_CreatureWithFullBoard_ShouldReturnFalse()
        {
            // Arrange
            var player = new Player("TestPlayer", Faction.Nature);
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
                Type = CardType.Creature
            };

            player.Hand.Add(creature);

            // Act
            bool canPlay = player.CanPlayCard(creature);

            // Assert
            Assert.IsFalse(canPlay);
        }

        [TestMethod]
        public void CreatureCard_Death_ShouldTriggerDeathEffects()
        {
            // Arrange
            bool deathTriggered = false;
            var creature = new CreatureCard
            {
                Name = "Test Creature",
                Health = 5
            };

            creature.OnDeath += (deadCreature) => deathTriggered = true;

            // Act
            creature.TakeDamage(10);

            // Assert
            Assert.IsTrue(deathTriggered);
            Assert.AreEqual(0, creature.Health);
        }

        [TestMethod]
        public void GameState_Log_ShouldAddTimestampedMessages()
        {
            // Arrange
            var player1 = new Player("Player1", Faction.Nature);
            var player2 = new Player("Player2", Faction.Fire);
            var gameState = new GameState(player1, player2);

            // Act
            gameState.Log("Test message");

            // Assert
            Assert.AreEqual(1, gameState.GameLog.Count);
            Assert.IsTrue(gameState.GameLog[0].Contains("Test message"));
            Assert.IsTrue(gameState.GameLog[0].StartsWith("["));
        }

        #endregion
    }
}