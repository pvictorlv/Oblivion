using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.HabboHotel.Rooms;

namespace Oblivion.HabboHotel.Pets.AI
{
    /// <summary>
    /// Sistema de inteligência artificial para pets
    /// </summary>
    internal class PetIntelligence
    {
        private readonly Pet _pet;
        private readonly Dictionary<uint, PetUserRelationship> _relationships;
        private readonly List<PetMemory> _memories;
        private readonly PetEmotionalState _emotionalState;
        private readonly PetLearningSystem _learningSystem;
        private readonly Random _random;

        public PetIntelligence(Pet pet)
        {
            _pet = pet;
            _relationships = new Dictionary<uint, PetUserRelationship>();
            _memories = new List<PetMemory>();
            _emotionalState = new PetEmotionalState();
            _learningSystem = new PetLearningSystem();
            _random = new Random();
        }

        /// <summary>
        /// Processa interação com usuário
        /// </summary>
        public async Task<PetResponse> ProcessUserInteraction(RoomUser user, string action, string message = "")
        {
            var userId = user.GetClient()?.GetHabbo()?.Id ?? 0;
            if (userId == 0) return null;

            // Atualizar relacionamento
            UpdateRelationship(userId, user.GetUsername(), action);

            // Criar memória da interação
            CreateMemory(userId, action, message);

            // Processar emocionalmente
            _emotionalState.ProcessInteraction(action, GetRelationshipLevel(userId));

            // Gerar resposta baseada na inteligência
            return await GenerateResponse(user, action, message);
        }

        /// <summary>
        /// Comportamento autônomo do pet
        /// </summary>
        public async Task<PetAction> GetAutonomousBehavior(Room room, RoomUser petUser)
        {
            // Verificar necessidades básicas
            if (_pet.Energy < 30)
                return new PetAction { Type = PetActionType.Rest, Priority = 10 };

            if (_pet.Nutrition < 30)
                return new PetAction { Type = PetActionType.SeekFood, Priority = 9 };

            // Comportamento baseado no estado emocional
            switch (_emotionalState.CurrentEmotion)
            {
                case PetEmotion.Happy:
                    return GetHappyBehavior(room, petUser);
                case PetEmotion.Sad:
                    return GetSadBehavior(room, petUser);
                case PetEmotion.Excited:
                    return GetExcitedBehavior(room, petUser);
                case PetEmotion.Lonely:
                    return GetLonelyBehavior(room, petUser);
                case PetEmotion.Playful:
                    return GetPlayfulBehavior(room, petUser);
                default:
                    return GetNeutralBehavior(room, petUser);
            }
        }

        /// <summary>
        /// Aprende um novo comando
        /// </summary>
        public bool LearnCommand(string command, uint teacherId)
        {
            var relationship = GetRelationship(teacherId);
            var learningChance = CalculateLearningChance(relationship);

            if (_random.NextDouble() < learningChance)
            {
                _learningSystem.AddCommand(command, teacherId);
                _emotionalState.ProcessInteraction("learn_success", relationship.Level);
                return true;
            }

            _emotionalState.ProcessInteraction("learn_failure", relationship.Level);
            return false;
        }

        /// <summary>
        /// Executa um comando aprendido
        /// </summary>
        public async Task<bool> ExecuteCommand(string command, RoomUser petUser, RoomUser commander)
        {
            var commanderId = commander.GetClient()?.GetHabbo()?.Id ?? 0;
            if (commanderId == 0) return false;

            var relationship = GetRelationship(commanderId);
            var obedienceChance = CalculateObedienceChance(relationship, command);

            if (_random.NextDouble() < obedienceChance)
            {
                await ExecuteCommandAction(command, petUser, commander);
                _emotionalState.ProcessInteraction("obey_command", relationship.Level);
                return true;
            }

            // Pet se recusa a obedecer
            await ShowDisobedience(petUser, commander);
            _emotionalState.ProcessInteraction("disobey_command", relationship.Level);
            return false;
        }

        /// <summary>
        /// Obtém informações sobre o pet para o dono
        /// </summary>
        public PetStatus GetPetStatus()
        {
            return new PetStatus
            {
                Emotion = _emotionalState.CurrentEmotion,
                Happiness = _emotionalState.Happiness,
                Intelligence = _learningSystem.IntelligenceLevel,
                KnownCommands = _learningSystem.GetKnownCommands(),
                FavoriteUsers = GetFavoriteUsers(),
                RecentMemories = _memories.TakeLast(5).ToList(),
                Needs = GetCurrentNeeds()
            };
        }

        private void UpdateRelationship(uint userId, string userName, string action)
        {
            if (!_relationships.ContainsKey(userId))
            {
                _relationships[userId] = new PetUserRelationship
                {
                    UserId = userId,
                    UserName = userName,
                    Level = RelationshipLevel.Stranger,
                    Trust = 0,
                    Affection = 0,
                    InteractionCount = 0,
                    LastInteraction = DateTime.Now
                };
            }

            var relationship = _relationships[userId];
            relationship.InteractionCount++;
            relationship.LastInteraction = DateTime.Now;

            // Atualizar baseado na ação
            switch (action.ToLower())
            {
                case "pet":
                case "feed":
                case "play":
                    relationship.Affection += 5;
                    relationship.Trust += 2;
                    break;
                case "scold":
                case "ignore":
                    relationship.Affection -= 3;
                    relationship.Trust -= 1;
                    break;
                case "teach":
                    relationship.Trust += 3;
                    break;
            }

            // Atualizar nível do relacionamento
            UpdateRelationshipLevel(relationship);
        }

        private void UpdateRelationshipLevel(PetUserRelationship relationship)
        {
            if (relationship.Affection >= 80 && relationship.Trust >= 70)
                relationship.Level = RelationshipLevel.BestFriend;
            else if (relationship.Affection >= 50 && relationship.Trust >= 40)
                relationship.Level = RelationshipLevel.Friend;
            else if (relationship.Affection >= 20 && relationship.Trust >= 20)
                relationship.Level = RelationshipLevel.Acquaintance;
            else
                relationship.Level = RelationshipLevel.Stranger;
        }

        private void CreateMemory(uint userId, string action, string context)
        {
            var memory = new PetMemory
            {
                UserId = userId,
                Action = action,
                Context = context,
                Timestamp = DateTime.Now,
                EmotionalImpact = CalculateEmotionalImpact(action)
            };

            _memories.Add(memory);

            // Manter apenas as últimas 100 memórias
            if (_memories.Count > 100)
                _memories.RemoveAt(0);
        }

        private async Task<PetResponse> GenerateResponse(RoomUser user, string action, string message)
        {
            var relationship = GetRelationship(user.GetClient()?.GetHabbo()?.Id ?? 0);
            var responses = GetResponsesForAction(action, relationship.Level);

            if (!responses.Any())
                return null;

            var response = responses[_random.Next(responses.Count)];
            return new PetResponse
            {
                Message = response,
                Animation = GetAnimationForAction(action),
                SoundEffect = GetSoundForAction(action)
            };
        }

        private PetAction GetHappyBehavior(Room room, RoomUser petUser)
        {
            var actions = new[]
            {
                new PetAction { Type = PetActionType.PlayAround, Priority = 7 },
                new PetAction { Type = PetActionType.FollowOwner, Priority = 6 },
                new PetAction { Type = PetActionType.ShowTricks, Priority = 5 }
            };
            return actions[_random.Next(actions.Length)];
        }

        private PetAction GetSadBehavior(Room room, RoomUser petUser)
        {
            var actions = new[]
            {
                new PetAction { Type = PetActionType.Rest, Priority = 8 },
                new PetAction { Type = PetActionType.SeekAttention, Priority = 7 },
                new PetAction { Type = PetActionType.HideInCorner, Priority = 6 }
            };
            return actions[_random.Next(actions.Length)];
        }

        private PetAction GetExcitedBehavior(Room room, RoomUser petUser)
        {
            var actions = new[]
            {
                new PetAction { Type = PetActionType.RunAround, Priority = 9 },
                new PetAction { Type = PetActionType.Jump, Priority = 8 },
                new PetAction { Type = PetActionType.PlayWithToys, Priority = 7 }
            };
            return actions[_random.Next(actions.Length)];
        }

        private PetAction GetLonelyBehavior(Room room, RoomUser petUser)
        {
            var users = room.GetRoomUserManager().GetRoomUsers().Where(u => u.GetClient() != null).ToList();
            if (users.Any())
            {
                return new PetAction { Type = PetActionType.ApproachUser, Priority = 8, TargetUser = users[_random.Next(users.Count)] };
            }
            return new PetAction { Type = PetActionType.Rest, Priority = 5 };
        }

        private PetAction GetPlayfulBehavior(Room room, RoomUser petUser)
        {
            var actions = new[]
            {
                new PetAction { Type = PetActionType.PlayWithToys, Priority = 8 },
                new PetAction { Type = PetActionType.ChaseOwner, Priority = 7 },
                new PetAction { Type = PetActionType.ShowTricks, Priority = 6 }
            };
            return actions[_random.Next(actions.Length)];
        }

        private PetAction GetNeutralBehavior(Room room, RoomUser petUser)
        {
            var actions = new[]
            {
                new PetAction { Type = PetActionType.Wander, Priority = 4 },
                new PetAction { Type = PetActionType.Rest, Priority = 3 },
                new PetAction { Type = PetActionType.Observe, Priority = 2 }
            };
            return actions[_random.Next(actions.Length)];
        }

        private double CalculateLearningChance(PetUserRelationship relationship)
        {
            var baseChance = 0.3;
            var relationshipBonus = (int)relationship.Level * 0.15;
            var trustBonus = relationship.Trust / 100.0 * 0.2;
            var intelligenceBonus = _learningSystem.IntelligenceLevel / 100.0 * 0.15;

            return Math.Min(0.9, baseChance + relationshipBonus + trustBonus + intelligenceBonus);
        }

        private double CalculateObedienceChance(PetUserRelationship relationship, string command)
        {
            var baseChance = 0.5;
            var relationshipBonus = (int)relationship.Level * 0.2;
            var trustBonus = relationship.Trust / 100.0 * 0.15;
            var commandFamiliarity = _learningSystem.GetCommandFamiliarity(command) * 0.1;

            return Math.Min(0.95, baseChance + relationshipBonus + trustBonus + commandFamiliarity);
        }

        private async Task ExecuteCommandAction(string command, RoomUser petUser, RoomUser commander)
        {
            switch (command.ToLower())
            {
                case "sit":
                case "sentar":
                    await petUser.Chat(null, "*senta*", false, 0);
                    // Implementar animação de sentar
                    break;
                case "stay":
                case "ficar":
                    await petUser.Chat(null, "*fica parado*", false, 0);
                    break;
                case "come":
                case "vem":
                    await petUser.MoveTo(commander.SquareInFront);
                    await petUser.Chat(null, "*vem correndo*", false, 0);
                    break;
                case "play":
                case "brincar":
                    await petUser.Chat(null, "*brinca alegremente*", false, 0);
                    break;
            }
        }

        private async Task ShowDisobedience(RoomUser petUser, RoomUser commander)
        {
            var disobedienceActions = new[]
            {
                "*ignora o comando*",
                "*olha para outro lado*",
                "*faz cara de desentendido*",
                "*se distrai com algo*"
            };

            var action = disobedienceActions[_random.Next(disobedienceActions.Length)];
            await petUser.Chat(null, action, false, 0);
        }

        private PetUserRelationship GetRelationship(uint userId)
        {
            return _relationships.ContainsKey(userId) ? _relationships[userId] : new PetUserRelationship
            {
                UserId = userId,
                Level = RelationshipLevel.Stranger,
                Trust = 0,
                Affection = 0
            };
        }

        private RelationshipLevel GetRelationshipLevel(uint userId)
        {
            return GetRelationship(userId).Level;
        }

        private List<string> GetResponsesForAction(string action, RelationshipLevel relationshipLevel)
        {
            var responses = new List<string>();

            switch (action.ToLower())
            {
                case "pet":
                    responses.AddRange(relationshipLevel >= RelationshipLevel.Friend 
                        ? new[] { "*ronrona feliz*", "*se esfrega carinhosamente*", "*fica muito feliz*" }
                        : new[] { "*aceita o carinho*", "*fica quieto*" });
                    break;
                case "feed":
                    responses.AddRange(new[] { "*come com prazer*", "*agradece pela comida*", "*fica satisfeito*" });
                    break;
                case "play":
                    responses.AddRange(relationshipLevel >= RelationshipLevel.Acquaintance
                        ? new[] { "*brinca alegremente*", "*se diverte muito*", "*pula de alegria*" }
                        : new[] { "*brinca timidamente*", "*participa do jogo*" });
                    break;
            }

            return responses;
        }

        private string GetAnimationForAction(string action)
        {
            switch (action.ToLower())
            {
                case "pet": return "happy";
                case "feed": return "eat";
                case "play": return "play";
                default: return "idle";
            }
        }

        private string GetSoundForAction(string action)
        {
            switch (action.ToLower())
            {
                case "pet": return "purr";
                case "feed": return "eat";
                case "play": return "bark";
                default: return null;
            }
        }

        private int CalculateEmotionalImpact(string action)
        {
            switch (action.ToLower())
            {
                case "pet":
                case "feed":
                case "play": return 5;
                case "scold": return -3;
                case "ignore": return -1;
                default: return 0;
            }
        }

        private List<string> GetFavoriteUsers()
        {
            return _relationships.Values
                .Where(r => r.Level >= RelationshipLevel.Friend)
                .OrderByDescending(r => r.Affection)
                .Take(3)
                .Select(r => r.UserName)
                .ToList();
        }

        private Dictionary<string, int> GetCurrentNeeds()
        {
            return new Dictionary<string, int>
            {
                { "Energia", _pet.Energy },
                { "Nutrição", _pet.Nutrition },
                { "Felicidade", _emotionalState.Happiness },
                { "Atenção", Math.Max(0, 100 - (int)(DateTime.Now - _relationships.Values.LastOrDefault()?.LastInteraction ?? DateTime.Now.AddHours(-1)).TotalMinutes) }
            };
        }
    }

    // Classes de apoio para o sistema de IA dos pets
    internal class PetUserRelationship
    {
        public uint UserId { get; set; }
        public string UserName { get; set; }
        public RelationshipLevel Level { get; set; }
        public int Trust { get; set; }
        public int Affection { get; set; }
        public int InteractionCount { get; set; }
        public DateTime LastInteraction { get; set; }
    }

    internal class PetMemory
    {
        public uint UserId { get; set; }
        public string Action { get; set; }
        public string Context { get; set; }
        public DateTime Timestamp { get; set; }
        public int EmotionalImpact { get; set; }
    }

    internal class PetEmotionalState
    {
        public PetEmotion CurrentEmotion { get; set; } = PetEmotion.Neutral;
        public int Happiness { get; set; } = 50;
        public int Stress { get; set; } = 0;
        public DateTime LastEmotionChange { get; set; } = DateTime.Now;

        public void ProcessInteraction(string action, RelationshipLevel relationshipLevel)
        {
            switch (action.ToLower())
            {
                case "pet":
                case "feed":
                case "play":
                    Happiness += 10;
                    Stress = Math.Max(0, Stress - 5);
                    break;
                case "scold":
                    Happiness -= 5;
                    Stress += 10;
                    break;
                case "ignore":
                    Happiness -= 2;
                    Stress += 3;
                    break;
            }

            UpdateEmotion();
        }

        private void UpdateEmotion()
        {
            if (Happiness >= 80) CurrentEmotion = PetEmotion.Happy;
            else if (Happiness >= 60) CurrentEmotion = PetEmotion.Playful;
            else if (Happiness <= 20) CurrentEmotion = PetEmotion.Sad;
            else if (Stress >= 70) CurrentEmotion = PetEmotion.Stressed;
            else CurrentEmotion = PetEmotion.Neutral;

            LastEmotionChange = DateTime.Now;
        }
    }

    internal class PetLearningSystem
    {
        private readonly Dictionary<string, CommandKnowledge> _knownCommands;
        public int IntelligenceLevel { get; private set; } = 50;

        public PetLearningSystem()
        {
            _knownCommands = new Dictionary<string, CommandKnowledge>();
        }

        public void AddCommand(string command, uint teacherId)
        {
            if (!_knownCommands.ContainsKey(command))
            {
                _knownCommands[command] = new CommandKnowledge
                {
                    Command = command,
                    TeacherId = teacherId,
                    LearnedAt = DateTime.Now,
                    UsageCount = 0,
                    SuccessRate = 0.5f
                };
                IntelligenceLevel += 2;
            }
        }

        public List<string> GetKnownCommands()
        {
            return _knownCommands.Keys.ToList();
        }

        public float GetCommandFamiliarity(string command)
        {
            return _knownCommands.ContainsKey(command) ? _knownCommands[command].SuccessRate : 0f;
        }
    }

    internal class CommandKnowledge
    {
        public string Command { get; set; }
        public uint TeacherId { get; set; }
        public DateTime LearnedAt { get; set; }
        public int UsageCount { get; set; }
        public float SuccessRate { get; set; }
    }

    internal enum PetEmotion
    {
        Neutral,
        Happy,
        Sad,
        Excited,
        Lonely,
        Playful,
        Stressed
    }

    internal enum RelationshipLevel
    {
        Stranger,
        Acquaintance,
        Friend,
        BestFriend
    }

    internal class PetResponse
    {
        public string Message { get; set; }
        public string Animation { get; set; }
        public string SoundEffect { get; set; }
    }

    internal class PetAction
    {
        public PetActionType Type { get; set; }
        public int Priority { get; set; }
        public RoomUser TargetUser { get; set; }
    }

    internal enum PetActionType
    {
        Rest,
        SeekFood,
        PlayAround,
        FollowOwner,
        ShowTricks,
        SeekAttention,
        HideInCorner,
        RunAround,
        Jump,
        PlayWithToys,
        ApproachUser,
        ChaseOwner,
        Wander,
        Observe
    }

    internal class PetStatus
    {
        public PetEmotion Emotion { get; set; }
        public int Happiness { get; set; }
        public int Intelligence { get; set; }
        public List<string> KnownCommands { get; set; }
        public List<string> FavoriteUsers { get; set; }
        public List<PetMemory> RecentMemories { get; set; }
        public Dictionary<string, int> Needs { get; set; }
    }
}