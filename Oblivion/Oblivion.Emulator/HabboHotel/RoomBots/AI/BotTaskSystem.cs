using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.RoomBots.AI
{
    /// <summary>
    /// Sistema de tarefas para bots, permitindo que executem ações úteis
    /// </summary>
    internal class BotTaskSystem
    {
        private readonly Queue<BotTask> _taskQueue;
        private readonly List<BotTask> _completedTasks;
        private readonly Dictionary<TaskType, int> _taskPriorities;
        private BotTask _currentTask;
        private readonly int _maxQueueSize;

        public BotTaskSystem(int maxQueueSize = 10)
        {
            _taskQueue = new Queue<BotTask>();
            _completedTasks = new List<BotTask>();
            _taskPriorities = InitializeTaskPriorities();
            _maxQueueSize = maxQueueSize;
        }

        /// <summary>
        /// Adiciona uma nova tarefa à fila
        /// </summary>
        public bool AddTask(TaskType taskType, Dictionary<string, object> parameters = null, uint requesterId = 0)
        {
            if (_taskQueue.Count >= _maxQueueSize) return false;

            var task = new BotTask
            {
                Id = Guid.NewGuid(),
                Type = taskType,
                Parameters = parameters ?? new Dictionary<string, object>(),
                RequesterId = requesterId,
                CreatedAt = DateTime.Now,
                Status = TaskStatus.Pending,
                Priority = _taskPriorities.ContainsKey(taskType) ? _taskPriorities[taskType] : 5
            };

            // Inserir na posição correta baseado na prioridade
            var tempList = _taskQueue.ToList();
            tempList.Add(task);
            tempList = tempList.OrderBy(t => t.Priority).ToList();
            
            _taskQueue.Clear();
            foreach (var t in tempList)
                _taskQueue.Enqueue(t);

            return true;
        }

        /// <summary>
        /// Processa a próxima tarefa na fila
        /// </summary>
        public async Task<bool> ProcessNextTask(RoomUser botUser, Room room)
        {
            if (_currentTask != null && _currentTask.Status == TaskStatus.InProgress)
                return false;

            if (!_taskQueue.Any()) return false;

            _currentTask = _taskQueue.Dequeue();
            _currentTask.Status = TaskStatus.InProgress;
            _currentTask.StartedAt = DateTime.Now;

            try
            {
                var success = await ExecuteTask(_currentTask, botUser, room);
                _currentTask.Status = success ? TaskStatus.Completed : TaskStatus.Failed;
                _currentTask.CompletedAt = DateTime.Now;
                
                _completedTasks.Add(_currentTask);
                if (_completedTasks.Count > 50) // Manter apenas as últimas 50 tarefas
                    _completedTasks.RemoveAt(0);

                _currentTask = null;
                return success;
            }
            catch (Exception ex)
            {
                _currentTask.Status = TaskStatus.Failed;
                _currentTask.ErrorMessage = ex.Message;
                _currentTask.CompletedAt = DateTime.Now;
                _completedTasks.Add(_currentTask);
                _currentTask = null;
                return false;
            }
        }

        /// <summary>
        /// Obtém estatísticas das tarefas
        /// </summary>
        public TaskStatistics GetStatistics()
        {
            var completed = _completedTasks.Count(t => t.Status == TaskStatus.Completed);
            var failed = _completedTasks.Count(t => t.Status == TaskStatus.Failed);
            var pending = _taskQueue.Count;

            return new TaskStatistics
            {
                CompletedTasks = completed,
                FailedTasks = failed,
                PendingTasks = pending,
                TotalTasks = completed + failed + pending,
                SuccessRate = completed + failed > 0 ? (float)completed / (completed + failed) * 100 : 0,
                AverageExecutionTime = _completedTasks.Any() ? 
                    _completedTasks.Where(t => t.CompletedAt.HasValue && t.StartedAt.HasValue)
                                  .Average(t => (t.CompletedAt.Value - t.StartedAt.Value).TotalSeconds) : 0
            };
        }

        /// <summary>
        /// Obtém tarefas sugeridas baseadas no contexto do quarto
        /// </summary>
        public List<TaskType> GetSuggestedTasks(Room room, RoomUser botUser)
        {
            var suggestions = new List<TaskType>();

            // Analisar contexto do quarto
            var userCount = room.GetRoomUserManager().GetRoomUsers().Count;
            var items = room.GetRoomItemHandler().GetFloor.Values.ToList();

            // Sugerir tarefas baseadas no contexto
            if (userCount == 0)
            {
                suggestions.Add(TaskType.CleanRoom);
                suggestions.Add(TaskType.OrganizeItems);
            }
            else if (userCount > 5)
            {
                suggestions.Add(TaskType.WelcomeUsers);
                suggestions.Add(TaskType.ModerateChat);
            }

            // Verificar se há itens no chão que podem ser organizados
            if (items.Any())
            {
                suggestions.Add(TaskType.OrganizeItems);
            }

            // Tarefas de entretenimento
            if (userCount > 1)
            {
                suggestions.Add(TaskType.TellJoke);
                suggestions.Add(TaskType.StartGame);
                suggestions.Add(TaskType.ShareFact);
            }

            return suggestions.Distinct().ToList();
        }

        private async Task<bool> ExecuteTask(BotTask task, RoomUser botUser, Room room)
        {
            switch (task.Type)
            {
                case TaskType.WelcomeUsers:
                    return await ExecuteWelcomeUsers(task, botUser, room);
                
                case TaskType.CleanRoom:
                    return await ExecuteCleanRoom(task, botUser, room);
                
                case TaskType.OrganizeItems:
                    return await ExecuteOrganizeItems(task, botUser, room);
                
                case TaskType.TellJoke:
                    return await ExecuteTellJoke(task, botUser, room);
                
                case TaskType.ShareFact:
                    return await ExecuteShareFact(task, botUser, room);
                
                case TaskType.StartGame:
                    return await ExecuteStartGame(task, botUser, room);
                
                case TaskType.ModerateChat:
                    return await ExecuteModerateChat(task, botUser, room);
                
                case TaskType.HelpUser:
                    return await ExecuteHelpUser(task, botUser, room);
                
                case TaskType.GiveGift:
                    return await ExecuteGiveGift(task, botUser, room);
                
                case TaskType.PlayMusic:
                    return await ExecutePlayMusic(task, botUser, room);
                
                default:
                    return false;
            }
        }

        private async Task<bool> ExecuteWelcomeUsers(BotTask task, RoomUser botUser, Room room)
        {
            var users = room.GetRoomUserManager().GetRoomUsers().Where(u => u.GetClient() != null).ToList();
            if (!users.Any()) return false;

            var welcomeMessages = new[]
            {
                "Bem-vindos ao nosso quarto! Sintam-se em casa!",
                "Olá pessoal! Que bom ter vocês aqui!",
                "Sejam bem-vindos! Se precisarem de algo, me avisem!"
            };

            var message = welcomeMessages[new Random().Next(welcomeMessages.Length)];
            await botUser.Chat(null, message, false, 0);
            return true;
        }

        private async Task<bool> ExecuteCleanRoom(BotTask task, RoomUser botUser, Room room)
        {
            await botUser.Chat(null, "Vou dar uma organizada no quarto!", false, 0);
            
            // Simular limpeza movendo-se pelo quarto
            var gameMap = room.GetGameMap();
            var walkableSquares = gameMap.WalkableList.ToList();
            
            for (int i = 0; i < Math.Min(5, walkableSquares.Count); i++)
            {
                var square = walkableSquares[new Random().Next(walkableSquares.Count)];
                await botUser.MoveTo(square.X, square.Y);
                await Task.Delay(2000); // Simular tempo de limpeza
            }
            
            await botUser.Chat(null, "Pronto! Quarto limpo e organizado!", false, 0);
            return true;
        }

        private async Task<bool> ExecuteOrganizeItems(BotTask task, RoomUser botUser, Room room)
        {
            await botUser.Chat(null, "Vou organizar os itens do quarto!", false, 0);
            
            // Simular organização
            await Task.Delay(3000);
            
            await botUser.Chat(null, "Tudo organizado! Ficou muito melhor!", false, 0);
            return true;
        }

        private async Task<bool> ExecuteTellJoke(BotTask task, RoomUser botUser, Room room)
        {
            var jokes = new[]
            {
                "Por que os pássaros voam para o sul no inverno? Porque é longe demais para ir andando!",
                "O que o pato disse para a pata? Vem quá!",
                "Por que o livro de matemática estava triste? Porque tinha muitos problemas!",
                "O que a impressora falou para a outra impressora? Essa folha é sua ou é impressão minha?",
                "Por que o café foi ao médico? Porque estava passando mal!"
            };

            var joke = jokes[new Random().Next(jokes.Length)];
            await botUser.Chat(null, $"Querem ouvir uma piada? {joke}", false, 0);
            return true;
        }

        private async Task<bool> ExecuteShareFact(BotTask task, RoomUser botUser, Room room)
        {
            var facts = new[]
            {
                "Vocês sabiam que os polvos têm três corações?",
                "Fato curioso: As abelhas podem reconhecer rostos humanos!",
                "Sabia que um grupo de flamingos é chamado de 'flamboyance'?",
                "Curiosidade: Os golfinhos têm nomes únicos para cada indivíduo!",
                "Fato interessante: As formigas podem levantar 50 vezes seu próprio peso!"
            };

            var fact = facts[new Random().Next(facts.Length)];
            await botUser.Chat(null, fact, false, 0);
            return true;
        }

        private async Task<bool> ExecuteStartGame(BotTask task, RoomUser botUser, Room room)
        {
            var games = new[]
            {
                "Vamos jogar 'Verdade ou Desafio'? Quem topa?",
                "Que tal um jogo de adivinhação? Estou pensando em um número de 1 a 100!",
                "Vamos fazer uma competição de piadas? Quem conta a melhor?",
                "Que tal um quiz? Primeira pergunta: Qual é a capital do Brasil?"
            };

            var game = games[new Random().Next(games.Length)];
            await botUser.Chat(null, game, false, 0);
            return true;
        }

        private async Task<bool> ExecuteModerateChat(BotTask task, RoomUser botUser, Room room)
        {
            await botUser.Chat(null, "Pessoal, vamos manter um ambiente respeitoso para todos!", false, 0);
            return true;
        }

        private async Task<bool> ExecuteHelpUser(BotTask task, RoomUser botUser, Room room)
        {
            var userId = task.Parameters.ContainsKey("userId") ? (uint)task.Parameters["userId"] : 0;
            var user = room.GetRoomUserManager().GetRoomUserByHabbo(userId);
            
            if (user != null)
            {
                await botUser.Chat(null, $"Oi {user.GetUsername()}! Posso ajudar você com alguma coisa?", false, 0);
                return true;
            }
            
            return false;
        }

        private async Task<bool> ExecuteGiveGift(BotTask task, RoomUser botUser, Room room)
        {
            var userId = task.Parameters.ContainsKey("userId") ? (uint)task.Parameters["userId"] : 0;
            var user = room.GetRoomUserManager().GetRoomUserByHabbo(userId);
            
            if (user != null)
            {
                await botUser.Chat(null, $"Aqui está um presente para você, {user.GetUsername()}!", false, 0);
                // Aqui poderia implementar a lógica real de dar um item
                return true;
            }
            
            return false;
        }

        private async Task<bool> ExecutePlayMusic(BotTask task, RoomUser botUser, Room room)
        {
            await botUser.Chat(null, "🎵 Tocando uma música relaxante para vocês! 🎵", false, 0);
            return true;
        }

        private Dictionary<TaskType, int> InitializeTaskPriorities()
        {
            return new Dictionary<TaskType, int>
            {
                { TaskType.HelpUser, 1 },
                { TaskType.ModerateChat, 2 },
                { TaskType.WelcomeUsers, 3 },
                { TaskType.GiveGift, 4 },
                { TaskType.TellJoke, 5 },
                { TaskType.ShareFact, 5 },
                { TaskType.StartGame, 6 },
                { TaskType.PlayMusic, 7 },
                { TaskType.CleanRoom, 8 },
                { TaskType.OrganizeItems, 9 }
            };
        }
    }

    /// <summary>
    /// Representa uma tarefa do bot
    /// </summary>
    internal class BotTask
    {
        public Guid Id { get; set; }
        public TaskType Type { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public uint RequesterId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TaskStatus Status { get; set; }
        public int Priority { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Tipos de tarefas disponíveis
    /// </summary>
    internal enum TaskType
    {
        WelcomeUsers,
        CleanRoom,
        OrganizeItems,
        TellJoke,
        ShareFact,
        StartGame,
        ModerateChat,
        HelpUser,
        GiveGift,
        PlayMusic
    }

    /// <summary>
    /// Status da tarefa
    /// </summary>
    internal enum TaskStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed
    }

    /// <summary>
    /// Estatísticas das tarefas
    /// </summary>
    internal class TaskStatistics
    {
        public int CompletedTasks { get; set; }
        public int FailedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int TotalTasks { get; set; }
        public float SuccessRate { get; set; }
        public double AverageExecutionTime { get; set; }
    }
}