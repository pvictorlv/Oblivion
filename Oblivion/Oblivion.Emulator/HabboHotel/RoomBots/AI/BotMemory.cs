using System;
using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.RoomBots.AI
{
    /// <summary>
    /// Sistema de memória para bots, permitindo que lembrem de interações passadas
    /// </summary>
    internal class BotMemory
    {
        private readonly Dictionary<uint, UserInteraction> _userInteractions;
        private readonly List<string> _conversationHistory;
        private readonly Dictionary<string, int> _topicInterest;
        private readonly int _maxMemorySize;

        public BotMemory(int maxMemorySize = 100)
        {
            _userInteractions = new Dictionary<uint, UserInteraction>();
            _conversationHistory = new List<string>();
            _topicInterest = new Dictionary<string, int>();
            _maxMemorySize = maxMemorySize;
        }

        /// <summary>
        /// Registra uma interação com um usuário
        /// </summary>
        public void RecordUserInteraction(uint userId, string userName, string message, InteractionType type)
        {
            if (!_userInteractions.ContainsKey(userId))
            {
                _userInteractions[userId] = new UserInteraction
                {
                    UserId = userId,
                    UserName = userName,
                    FirstInteraction = DateTime.Now,
                    InteractionCount = 0,
                    LastMessages = new List<string>(),
                    Relationship = RelationshipLevel.Stranger,
                    PreferredTopics = new List<string>()
                };
            }

            var interaction = _userInteractions[userId];
            interaction.LastInteraction = DateTime.Now;
            interaction.InteractionCount++;
            interaction.LastMessages.Add($"{DateTime.Now:HH:mm}: {message}");

            // Manter apenas as últimas 10 mensagens
            if (interaction.LastMessages.Count > 10)
                interaction.LastMessages.RemoveAt(0);

            // Atualizar nível de relacionamento baseado na frequência de interação
            UpdateRelationshipLevel(interaction);

            // Analisar tópicos de interesse
            AnalyzeTopicInterest(message);

            // Adicionar ao histórico geral
            _conversationHistory.Add($"{userName}: {message}");
            if (_conversationHistory.Count > _maxMemorySize)
                _conversationHistory.RemoveAt(0);
        }

        /// <summary>
        /// Obtém informações sobre um usuário específico
        /// </summary>
        public UserInteraction GetUserInfo(uint userId)
        {
            return _userInteractions.ContainsKey(userId) ? _userInteractions[userId] : null;
        }

        /// <summary>
        /// Verifica se o bot já conhece o usuário
        /// </summary>
        public bool KnowsUser(uint userId)
        {
            return _userInteractions.ContainsKey(userId) && 
                   _userInteractions[userId].InteractionCount > 0;
        }

        /// <summary>
        /// Obtém uma resposta personalizada baseada no histórico
        /// </summary>
        public string GetPersonalizedResponse(uint userId, string currentMessage)
        {
            var userInfo = GetUserInfo(userId);
            if (userInfo == null) return null;

            // Resposta baseada no nível de relacionamento
            switch (userInfo.Relationship)
            {
                case RelationshipLevel.Stranger:
                    return GetStrangerResponse(currentMessage);
                case RelationshipLevel.Acquaintance:
                    return GetAcquaintanceResponse(userInfo, currentMessage);
                case RelationshipLevel.Friend:
                    return GetFriendResponse(userInfo, currentMessage);
                case RelationshipLevel.BestFriend:
                    return GetBestFriendResponse(userInfo, currentMessage);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Obtém tópicos de maior interesse
        /// </summary>
        public List<string> GetTopInterests(int count = 5)
        {
            return _topicInterest.OrderByDescending(x => x.Value)
                                .Take(count)
                                .Select(x => x.Key)
                                .ToList();
        }

        private void UpdateRelationshipLevel(UserInteraction interaction)
        {
            var daysSinceFirst = (DateTime.Now - interaction.FirstInteraction).Days;
            var interactionsPerDay = daysSinceFirst > 0 ? interaction.InteractionCount / (double)daysSinceFirst : interaction.InteractionCount;

            if (interaction.InteractionCount >= 50 && interactionsPerDay >= 5)
                interaction.Relationship = RelationshipLevel.BestFriend;
            else if (interaction.InteractionCount >= 20 && interactionsPerDay >= 2)
                interaction.Relationship = RelationshipLevel.Friend;
            else if (interaction.InteractionCount >= 5)
                interaction.Relationship = RelationshipLevel.Acquaintance;
            else
                interaction.Relationship = RelationshipLevel.Stranger;
        }

        private void AnalyzeTopicInterest(string message)
        {
            var words = message.ToLower().Split(' ');
            var topics = new[] { "jogo", "música", "filme", "comida", "esporte", "trabalho", "escola", "amor", "amizade", "diversão" };

            foreach (var topic in topics)
            {
                if (words.Any(w => w.Contains(topic)))
                {
                    if (!_topicInterest.ContainsKey(topic))
                        _topicInterest[topic] = 0;
                    _topicInterest[topic]++;
                }
            }
        }

        private string GetStrangerResponse(string message)
        {
            var responses = new[]
            {
                "Olá! É a primeira vez que conversamos. Como você está?",
                "Oi! Prazer em conhecê-lo!",
                "Olá! Sou novo aqui, mas adoraria conversar!"
            };
            return responses[new Random().Next(responses.Length)];
        }

        private string GetAcquaintanceResponse(UserInteraction userInfo, string message)
        {
            var responses = new[]
            {
                $"Oi {userInfo.UserName}! Como você tem passado?",
                $"Olá novamente, {userInfo.UserName}!",
                $"Que bom te ver de novo, {userInfo.UserName}!"
            };
            return responses[new Random().Next(responses.Length)];
        }

        private string GetFriendResponse(UserInteraction userInfo, string message)
        {
            var responses = new[]
            {
                $"Oi {userInfo.UserName}! Sempre um prazer conversar com você!",
                $"Olá meu amigo {userInfo.UserName}! Como estão as coisas?",
                $"Ei {userInfo.UserName}! Estava esperando você aparecer!"
            };
            return responses[new Random().Next(responses.Length)];
        }

        private string GetBestFriendResponse(UserInteraction userInfo, string message)
        {
            var responses = new[]
            {
                $"Oi {userInfo.UserName}! Meu melhor amigo! Como você está?",
                $"Olá {userInfo.UserName}! Sempre fico feliz quando você aparece!",
                $"Ei {userInfo.UserName}! Você é meu amigo favorito aqui!"
            };
            return responses[new Random().Next(responses.Length)];
        }
    }

    /// <summary>
    /// Informações sobre interação com usuário
    /// </summary>
    internal class UserInteraction
    {
        public uint UserId { get; set; }
        public string UserName { get; set; }
        public DateTime FirstInteraction { get; set; }
        public DateTime LastInteraction { get; set; }
        public int InteractionCount { get; set; }
        public List<string> LastMessages { get; set; }
        public RelationshipLevel Relationship { get; set; }
        public List<string> PreferredTopics { get; set; }
    }

    /// <summary>
    /// Tipos de interação
    /// </summary>
    internal enum InteractionType
    {
        Chat,
        Shout,
        Whisper,
        Command,
        Gift,
        Trade
    }

    /// <summary>
    /// Níveis de relacionamento
    /// </summary>
    internal enum RelationshipLevel
    {
        Stranger,
        Acquaintance,
        Friend,
        BestFriend
    }
}