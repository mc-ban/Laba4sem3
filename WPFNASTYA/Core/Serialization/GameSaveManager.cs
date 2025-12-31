using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CardGame.Core;
using CardGame.Core.GameState;
using CardGame.Core.Models;

namespace CardGame.Core.Serialization
{
    // Интерфейс менеджера сохранений игр
    public interface IGameSaveManager
    {
        void SaveGame(GameState.GameState game, string saveName); // Сохранить игру
        GameState.GameState LoadGame(string saveName); // Загрузить игру
        List<SaveGameInfo> GetSaveFiles(); // Получить список сохранений
        bool DeleteSave(string saveName); // Удалить сохранение
        bool SaveExists(string saveName); // Проверить существование сохранения
    }

    // Информация о сохраненной игре
    public class SaveGameInfo
    {
        public string SaveName { get; set; } // Оригинальное имя (без метки)
        public string FileName { get; set; } // Полное имя файла (с меткой)
        public string FilePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModified { get; set; }
        public string GameId { get; set; }
        public string Player1Name { get; set; }
        public string Player2Name { get; set; }
        public int TurnNumber { get; set; }
        public GameStatus Status { get; set; }

        public string StatusDisplay => Status == GameStatus.Active ? "В процессе" :
                                      Status == GameStatus.Saved ? "Сохранено" :
                                      Status == GameStatus.Player1Wins ? $"Победил: {Player1Name}" :
                                      Status == GameStatus.Player2Wins ? $"Победил: {Player2Name}" :
                                      "Ничья";
    }
    public class GameSaveManager : IGameSaveManager
    {
        private readonly string _saveDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Saves");
        private readonly JsonSerializerOptions _jsonOptions;

        public GameSaveManager()
        {
            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
            }

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() },
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        // Сохранение игры в файл
        public void SaveGame(GameState.GameState game, string saveName)
        {
            try
            {
                // Очищаем имя файла от недопустимых символов
                var cleanName = CleanFileName(saveName);
                
                // Добавляем временную метку для уникальности
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"{cleanName}_{timestamp}.json";
                var savePath = Path.Combine(_saveDirectory, fileName);

                var saveData = new FullGameSaveData(game);
                var json = JsonSerializer.Serialize(saveData, _jsonOptions);
                File.WriteAllText(savePath, json, Encoding.UTF8);

                game.Status = GameStatus.Saved;
                game.Log($"Игра сохранена: {cleanName}");

                Debug.WriteLine($"Игра сохранена: {savePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при сохранении игры: {ex}");
                throw new Exception($"Не удалось сохранить игру: {ex.Message}", ex);
            }
        }

        // Загрузка игры из файла
        public GameState.GameState LoadGame(string saveName)
        {
            try
            {
                // Получаем список всех файлов сохранений
                var allFiles = Directory.GetFiles(_saveDirectory, "*.json");

                // Ищем файл, который начинается с указанного имени
                string targetFile = null;

                foreach (var file in allFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);

                    // Проверяем, начинается ли имя файла с saveName
                    if (fileName.StartsWith(saveName + "_"))
                    {
                        targetFile = file;
                        break;
                    }

                    // Дополнительная проверка: возможно saveName уже содержит временную метку
                    if (fileName == saveName)
                    {
                        targetFile = file;
                        break;
                    }
                }

                if (targetFile == null)
                {
                    throw new FileNotFoundException($"Сохранение '{saveName}' не найдено. Файлы в папке: {string.Join(", ", allFiles.Select(f => Path.GetFileName(f)))}");
                }

                // Загружаем игру
                return LoadFromFile(targetFile);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке игры: {ex}");
                throw new Exception($"Не удалось загрузить игру: {ex.Message}", ex);
            }
        }
        // Загрузка из конкретного файла
        private GameState.GameState LoadFromFile(string filePath)
        {
            var json = File.ReadAllText(filePath, Encoding.UTF8);
            var saveData = JsonSerializer.Deserialize<FullGameSaveData>(json, _jsonOptions);
            
            if (saveData == null)
                throw new InvalidOperationException("Не удалось загрузить сохранение");
            
            return saveData.RestoreGameState();
        }

        // Получение списка всех сохранений с информацией
        public List<SaveGameInfo> GetSaveFiles()
        {
            var saves = new List<SaveGameInfo>();

            try
            {
                if (!Directory.Exists(_saveDirectory))
                {
                    Directory.CreateDirectory(_saveDirectory);
                    return saves;
                }

                var files = Directory.GetFiles(_saveDirectory, "*.json");

                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        var json = File.ReadAllText(file, Encoding.UTF8);
                        var saveData = JsonSerializer.Deserialize<FullGameSaveData>(json, _jsonOptions);

                        if (saveData != null)
                        {
                            var fileName = Path.GetFileNameWithoutExtension(file);

                            // Извлекаем оригинальное имя (убираем временную метку)
                            var originalName = fileName;
                            var lastUnderscore = fileName.LastIndexOf('_');
                            if (lastUnderscore > 0)
                            {
                                // Проверяем, есть ли после последнего подчеркивания дата в формате ГГГГММДД_ЧЧММСС
                                var afterUnderscore = fileName.Substring(lastUnderscore + 1);
                                if (afterUnderscore.Length >= 8 && afterUnderscore.All(char.IsDigit))
                                {
                                    originalName = fileName.Substring(0, lastUnderscore);
                                }
                            }

                            saves.Add(new SaveGameInfo
                            {
                                SaveName = originalName,           // Имя для показа пользователю
                                FileName = fileName,               // Полное имя файла
                                FilePath = file,
                                CreatedAt = saveData.CreatedAt,
                                LastModified = fileInfo.LastWriteTime,
                                GameId = saveData.GameId,
                                Player1Name = saveData.Player1?.Name ?? "Игрок 1",
                                Player2Name = saveData.Player2?.Name ?? "Игрок 2",
                                TurnNumber = saveData.TurnNumber,
                                Status = saveData.Status
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Ошибка при чтении файла сохранения {file}: {ex}");
                    }
                }

                saves = saves.OrderByDescending(s => s.LastModified).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при получении списка сохранений: {ex}");
            }

            return saves;
        }
        // Извлечение оригинального имени из имени файла с временной меткой
        private string ExtractOriginalName(string fileName)
        {
            // Ищем последнее подчеркивание с датой (формат _20251231_143850)
            var lastUnderscore = fileName.LastIndexOf('_');
            if (lastUnderscore > 0)
            {
                var beforeLastUnderscore = fileName.LastIndexOf('_', lastUnderscore - 1);
                if (beforeLastUnderscore > 0)
                {
                    // Проверяем, является ли часть после последнего подчеркивания датой
                    var datePart = fileName.Substring(lastUnderscore + 1);
                    if (datePart.Length == 6 && int.TryParse(datePart, out _))
                    {
                        return fileName.Substring(0, beforeLastUnderscore);
                    }
                }
            }
            
            // Если не нашли шаблон даты, возвращаем как есть
            return fileName;
        }

        // Удаление сохранения
        public bool DeleteSave(string saveName)
        {
            try
            {
                // Ищем все файлы, начинающиеся с этого имени
                var files = Directory.GetFiles(_saveDirectory, $"{saveName}_*.json");
                if (files.Length == 0)
                    return false;
                
                // Удаляем самый новый файл
                var newestFile = files
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.LastWriteTime)
                    .First()
                    .FullName;
                
                File.Delete(newestFile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Проверка существования сохранения
        public bool SaveExists(string saveName)
        {
            var files = Directory.GetFiles(_saveDirectory, $"{saveName}_*.json");
            return files.Length > 0;
        }

        // Очистка имени файла
        private string CleanFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "autosave";
            
            var invalidChars = Path.GetInvalidFileNameChars();
            var cleanName = new string(fileName
                .Where(c => !invalidChars.Contains(c))
                .ToArray());
            
            return string.IsNullOrWhiteSpace(cleanName) ? "autosave" : cleanName;
        }

        // Автосохранение
        public void Autosave(GameState.GameState game)
        {
            try
            {
                var cleanName = "autosave";
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"{cleanName}_{timestamp}.json";
                var savePath = Path.Combine(_saveDirectory, fileName);
                
                // Удаляем старые автосохранения (оставляем только 3 последних)
                var oldAutosaves = Directory.GetFiles(_saveDirectory, "autosave_*.json")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.LastWriteTime)
                    .Skip(3)
                    .ToList();
                
                foreach (var oldFile in oldAutosaves)
                {
                    try { File.Delete(oldFile.FullName); }
                    catch { /* игнорируем ошибки удаления */ }
                }
                
                var saveData = new FullGameSaveData(game);
                var json = JsonSerializer.Serialize(saveData, _jsonOptions);
                File.WriteAllText(savePath, json, Encoding.UTF8);
                
                game.Log("Автосохранение выполнено");
                Debug.WriteLine("Автосохранение выполнено");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при автосохранении: {ex}");
            }
        }

        // Загрузка последнего автосохранения
        public GameState.GameState LoadLastAutosave()
        {
            try
            {
                var autosaves = Directory.GetFiles(_saveDirectory, "autosave_*.json");
                if (autosaves.Length == 0)
                    return null;
                
                // Берем самый новый автосейв
                var newestAutosave = autosaves
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.LastWriteTime)
                    .First()
                    .FullName;
                
                return LoadFromFile(newestAutosave);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке автосохранения: {ex}");
                return null;
            }
        }
        // Получение полного пути к файлу сохранения
        private string GetSavePath(string saveName)
        {
            // Убираем недопустимые символы из имени файла
            var invalidChars = Path.GetInvalidFileNameChars();
            var validName = new string(saveName.Where(c => !invalidChars.Contains(c)).ToArray());

            // Если имя стало пустым, используем дефолтное
            if (string.IsNullOrWhiteSpace(validName))
                validName = "autosave";

            // Добавляем временную метку для уникальности
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"{validName}_{timestamp}.json";

            return Path.Combine(_saveDirectory, fileName);
        }

        // Получение пути для автосохранения
        public string GetAutosavePath()
        {
            return Path.Combine(_saveDirectory, "autosave.json");
        }

        // Автосохранение
 
        // Загрузка автосохранения
        public GameState.GameState LoadAutosave()
        {
            var autosavePath = GetAutosavePath();
            if (File.Exists(autosavePath))
            {
                return LoadGame("autosave");
            }
            return null;
        }
    }

    // Полная структура данных для сохранения
    public class FullGameSaveData
    {
        public string GameId { get; set; }
        public FullPlayerSaveData Player1 { get; set; }
        public FullPlayerSaveData Player2 { get; set; }
        public string CurrentPlayerId { get; set; }
        public int TurnNumber { get; set; }
        public GameStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<string> GameLog { get; set; }

        public FullGameSaveData() { }

        public FullGameSaveData(GameState.GameState game)
        {
            GameId = game.GameId;
            Player1 = new FullPlayerSaveData(game.Player1);
            Player2 = new FullPlayerSaveData(game.Player2);
            CurrentPlayerId = game.CurrentPlayer.Id;
            TurnNumber = game.TurnNumber;
            Status = game.Status;
            CreatedAt = game.CreatedAt;
            LastUpdated = game.LastUpdated;
            GameLog = game.GameLog;
        }

        public GameState.GameState RestoreGameState()
        {
            // Восстанавливаем игроков
            var player1 = Player1.RestorePlayer();
            var player2 = Player2.RestorePlayer();

            // Создаем состояние игры с флагом загрузки
            var game = new GameState.GameState(player1, player2, true) // true = это загрузка
            {
                GameId = GameId,
                TurnNumber = TurnNumber,
                Status = Status,
                CreatedAt = CreatedAt,
                LastUpdated = LastUpdated,
                GameLog = GameLog ?? new List<string>()
            };

            // Восстанавливаем текущего игрока
            game.CurrentPlayer = game.Player1.Id == CurrentPlayerId ? game.Player1 : game.Player2;
            game.OpponentPlayer = game.CurrentPlayer == game.Player1 ? game.Player2 : game.Player1;

            // Обновляем время
            game.LastUpdated = DateTime.Now;

            return game;
        }
    }

    // Полные данные игрока
    public class FullPlayerSaveData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Faction Faction { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Mana { get; set; }
        public int MaxMana { get; set; }
        public int ManaCrystals { get; set; }
        public List<CardSaveData> Hand { get; set; }
        public List<CreatureSaveData> Board { get; set; }
        public List<CardSaveData> Deck { get; set; }
        public List<CardSaveData> Graveyard { get; set; }
        public bool HasUsedHeroPower { get; set; }
        public int SpellDamageBonus { get; set; }
        public int FatigueDamage { get; set; }

        public FullPlayerSaveData() { }

        public FullPlayerSaveData(Player player)
        {
            Id = player.Id;
            Name = player.Name;
            Faction = player.Faction;
            Health = player.Health;
            MaxHealth = player.MaxHealth;
            Mana = player.Mana;
            MaxMana = player.MaxMana;
            ManaCrystals = player.ManaCrystals;
            HasUsedHeroPower = player.HasUsedHeroPower;
            SpellDamageBonus = player.SpellDamageBonus;
            FatigueDamage = player.FatigueDamage;

            // Сохраняем все карты в руке
            Hand = player.Hand.Select(c => new CardSaveData(c)).ToList();

            // Сохраняем существ на поле
            Board = player.Board.Select(c => new CreatureSaveData(c)).ToList();

            // Сохраняем колоду
            Deck = player.Deck.Select(c => new CardSaveData(c)).ToList();

            // Сохраняем кладбище
            Graveyard = player.Graveyard.Select(c => new CardSaveData(c)).ToList();
        }

        public Player RestorePlayer()
        {
            var player = new Player(Name, Faction)
            {
                Id = Id,
                Health = Health,
                MaxHealth = MaxHealth,
                Mana = Mana,
                MaxMana = MaxMana,
                ManaCrystals = ManaCrystals,
                HasUsedHeroPower = HasUsedHeroPower,
                SpellDamageBonus = SpellDamageBonus,
                FatigueDamage = FatigueDamage
            };

            // ВАЖНО: не создаем колоду заново, а используем сохраненную
            // Восстанавливаем колоду из сохраненных данных
            if (Deck != null)
            {
                player.Deck = Deck.Select(c => c.RestoreCard(Faction)).ToList();
            }
            else
            {
                // Если Deck пустой, оставляем пустую колоду
                player.Deck = new List<ICard>();
            }

            // Восстанавливаем руку из сохраненных данных
            if (Hand != null)
            {
                player.Hand = Hand.Select(c => c.RestoreCard(Faction)).ToList();
            }
            else
            {
                player.Hand = new List<ICard>();
            }

            // Восстанавливаем поле из сохраненных данных
            if (Board != null)
            {
                player.Board = Board.Select(c => c.RestoreCreature(Faction)).ToList();
            }
            else
            {
                player.Board = new List<CreatureCard>();
            }

            // Восстанавливаем кладбище из сохраненных данных
            if (Graveyard != null)
            {
                player.Graveyard = Graveyard.Select(c => c.RestoreCard(Faction)).ToList();
            }
            else
            {
                player.Graveyard = new List<ICard>();
            }

            return player;
        }
    }

    // Данные карты
    public class CardSaveData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ManaCost { get; set; }
        public CardRarity Rarity { get; set; }
        public CardType Type { get; set; }
        public string ImagePath { get; set; }

        // Для существ
        public int? Attack { get; set; }
        public int? Health { get; set; }
        public int? MaxHealth { get; set; }
        public List<AbilitySaveData> Abilities { get; set; }

        // Для заклинаний
        public SpellEffectSaveData SpellEffect { get; set; }
        public TargetType? TargetType { get; set; }

        public CardSaveData() { }

        public CardSaveData(ICard card)
        {
            Id = card.Id;
            Name = card.Name;
            Description = card.Description;
            ManaCost = card.ManaCost;
            Rarity = card.Rarity;
            Type = card.Type;
            ImagePath = card.ImagePath;

            if (card is CreatureCard creature)
            {
                Attack = creature.Attack;
                Health = creature.Health;
                MaxHealth = creature.MaxHealth;
                Abilities = creature.Abilities.Select(a => new AbilitySaveData(a)).ToList();
            }
            else if (card is SpellCard spell)
            {
                SpellEffect = new SpellEffectSaveData(spell.Effect);
                TargetType = spell.TargetType;
            }
        }

        public ICard RestoreCard(Faction defaultFaction)
        {
            try
            {
                if (Type == CardType.Creature)
                {
                    var creature = new CreatureCard
                    {
                        Id = string.IsNullOrEmpty(Id) ? Guid.NewGuid().ToString() : Id, // Сохраняем оригинальный ID
                        Name = Name ?? "Неизвестное существо",
                        Description = Description ?? string.Empty,
                        ManaCost = ManaCost,
                        Rarity = Rarity,
                        Faction = defaultFaction,
                        ImagePath = ImagePath ?? GenerateDefaultImagePath(Name, defaultFaction),
                        Attack = Attack ?? 0,
                        Health = Health ?? 1,
                        MaxHealth = MaxHealth ?? 1
                    };

                    // Восстанавливаем способности
                    if (Abilities != null && Abilities.Count > 0)
                    {
                        foreach (var abilityData in Abilities)
                        {
                            var ability = abilityData.RestoreAbility();
                            creature.Abilities.Add(ability);
                        }
                    }

                    return creature;
                }
                else // Spell
                {
                    var spell = new SpellCard
                    {
                        Id = string.IsNullOrEmpty(Id) ? Guid.NewGuid().ToString() : Id, // Сохраняем оригинальный ID
                        Name = Name ?? "Неизвестное заклинание",
                        Description = Description ?? string.Empty,
                        ManaCost = ManaCost,
                        Rarity = Rarity,
                        Faction = defaultFaction,
                        ImagePath = ImagePath ?? GenerateDefaultImagePath(Name, defaultFaction),
                        Effect = SpellEffect?.RestoreEffect() ?? new SpellEffect { Type = SpellEffectType.Damage, Value = 0 },
                        TargetType = TargetType ?? Core.Models.TargetType.AnyCreature
                    };

                    return spell;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при восстановлении карты {Name}: {ex}");
                // Возвращаем карту-заглушку
                return new CreatureCard("Ошибка загрузки", 1, 1, 1, defaultFaction);
            }
        }

        private string GenerateDefaultImagePath(string cardName, Faction faction)
        {
            // Генерируем путь к изображению по умолчанию
            string factionFolder = faction.ToString().ToLower();
            return $"pack://application:,,,/Resources/Cards/{factionFolder}/default.jpg";
        }
    }

    // Данные существа
    public class CreatureSaveData : CardSaveData
    {
        public bool CanAttack { get; set; }
        public bool IsExhausted { get; set; }
        public bool IsFrozen { get; set; }

        public CreatureSaveData() { }

        public CreatureSaveData(CreatureCard creature) : base(creature)
        {
            CanAttack = creature.CanAttack;
            IsExhausted = creature.IsExhausted;
            IsFrozen = creature.IsFrozen;
        }

        public new CreatureCard RestoreCreature(Faction faction)
        {
            var creature = (CreatureCard)RestoreCard(faction);
            creature.CanAttack = CanAttack;
            creature.IsExhausted = IsExhausted;
            creature.IsFrozen = IsFrozen;
            return creature;
        }
    }

    // Данные способности
    public class AbilitySaveData
    {
        public AbilityType Type { get; set; }
        public Trigger Trigger { get; set; }
        public int Value { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }

        public AbilitySaveData() { }

        public AbilitySaveData(Ability ability)
        {
            Type = ability.Type;
            Trigger = ability.Trigger;
            Value = ability.Value;
            Description = ability.Description;
            IsActive = ability.IsActive;
        }

        public Ability RestoreAbility()
        {
            return new Ability(Type, Trigger, Value, Description)
            {
                IsActive = IsActive
            };
        }
    }

    // Данные эффекта заклинания
    public class SpellEffectSaveData
    {
        public SpellEffectType Type { get; set; }
        public int Value { get; set; }
        public string AdditionalData { get; set; }

        public SpellEffectSaveData() { }

        public SpellEffectSaveData(SpellEffect effect)
        {
            Type = effect.Type;
            Value = effect.Value;
            AdditionalData = effect.AdditionalData;
        }

        public SpellEffect RestoreEffect()
        {
            return new SpellEffect
            {
                Type = Type,
                Value = Value,
                AdditionalData = AdditionalData
            };
        }
    }
}