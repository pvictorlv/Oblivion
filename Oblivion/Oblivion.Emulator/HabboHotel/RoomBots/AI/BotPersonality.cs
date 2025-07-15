using System;
using System.Collections.Generic;
using System.Linq;

namespace Oblivion.HabboHotel.RoomBots.AI
{
    /// <summary>
    /// Sistema de personalidade para bots, definindo comportamentos únicos
    /// </summary>
    internal class BotPersonality
    {
        public PersonalityType Type { get; private set; }
        public Dictionary<string, float> Traits { get; private set; }
        public List<string> Interests { get; private set; }
        public List<string> Dislikes { get; private set; }
        public MoodState CurrentMood { get; set; }
        public float Energy { get; set; }
        public float Sociability { get; set; }

        public BotPersonality(PersonalityType type = PersonalityType.Random)
        {
            if (type == PersonalityType.Random)
                type = (PersonalityType)new Random().Next(1, Enum.GetValues(typeof(PersonalityType)).Length);

            Type = type;
            Traits = new Dictionary<string, float>();
            Interests = new List<string>();
            Dislikes = new List<string>();
            CurrentMood = MoodState.Neutral;
            Energy = 100f;
            Sociability = 50f;

            InitializePersonality();
        }

        /// <summary>
        /// Obtém uma resposta baseada na personalidade
        /// </summary>
        public string GetPersonalityResponse(string trigger, string userName = "")
        {
            var responses = GetResponsesForTrigger(trigger);
            if (!responses.Any()) return null;

            // Modificar resposta baseada no humor atual
            var response = responses[new Random().Next(responses.Count)];
            return ModifyResponseByMood(response, userName);
        }

        /// <summary>
        /// Atualiza o humor baseado em eventos
        /// </summary>
        public void UpdateMood(MoodEvent moodEvent)
        {
            switch (moodEvent)
            {
                case MoodEvent.PositiveInteraction:
                    if (CurrentMood == MoodState.Sad) CurrentMood = MoodState.Neutral;
                    else if (CurrentMood == MoodState.Neutral) CurrentMood = MoodState.Happy;
                    break;
                case MoodEvent.NegativeInteraction:
                    if (CurrentMood == MoodState.Happy) CurrentMood = MoodState.Neutral;
                    else if (CurrentMood == MoodState.Neutral) CurrentMood = MoodState.Sad;
                    break;
                case MoodEvent.LongSilence:
                    if (Traits["Extroversion"] > 0.7f)
                        CurrentMood = MoodState.Bored;
                    break;
                case MoodEvent.CrowdedRoom:
                    if (Traits["Extroversion"] > 0.6f)
                        CurrentMood = MoodState.Excited;
                    else
                        CurrentMood = MoodState.Overwhelmed;
                    break;
            }

            // Energia afeta o humor
            if (Energy < 30f && CurrentMood != MoodState.Tired)
                CurrentMood = MoodState.Tired;
        }

        /// <summary>
        /// Verifica se o bot deve iniciar uma conversa
        /// </summary>
        public bool ShouldInitiateConversation()
        {
            var baseChance = Traits["Extroversion"] * 0.3f;
            
            // Modificadores baseados no humor
            switch (CurrentMood)
            {
                case MoodState.Happy:
                case MoodState.Excited:
                    baseChance *= 1.5f;
                    break;
                case MoodState.Sad:
                case MoodState.Tired:
                    baseChance *= 0.3f;
                    break;
                case MoodState.Bored:
                    baseChance *= 2f;
                    break;
            }

            return new Random().NextDouble() < baseChance;
        }

        /// <summary>
        /// Obtém um tópico de conversa baseado nos interesses
        /// </summary>
        public string GetConversationStarter()
        {
            var starters = new List<string>();

            switch (Type)
            {
                case PersonalityType.Friendly:
                    starters.AddRange(new[]
                    {
                        "Oi pessoal! Como estão todos hoje?",
                        "Alguém quer conversar? Estou aqui!",
                        "Que dia lindo! Alguém mais está feliz?"
                    });
                    break;
                case PersonalityType.Intellectual:
                    starters.AddRange(new[]
                    {
                        "Alguém leu algo interessante hoje?",
                        "Vocês sabiam que... (fato interessante)",
                        "O que acham sobre tecnologia moderna?"
                    });
                    break;
                case PersonalityType.Funny:
                    starters.AddRange(new[]
                    {
                        "Querem ouvir uma piada?",
                        "Algo engraçado aconteceu comigo hoje...",
                        "Vamos animar este lugar!"
                    });
                    break;
                case PersonalityType.Helpful:
                    starters.AddRange(new[]
                    {
                        "Alguém precisa de ajuda com alguma coisa?",
                        "Posso ajudar vocês com algo?",
                        "Se precisarem de dicas, estou aqui!"
                    });
                    break;
                case PersonalityType.Mysterious:
                    starters.AddRange(new[]
                    {
                        "Hmm... coisas interessantes estão acontecendo...",
                        "Vocês sentem algo diferente no ar?",
                        "Nem tudo é o que parece..."
                    });
                    break;
            }

            return starters.Any() ? starters[new Random().Next(starters.Count)] : "Olá!";
        }

        private void InitializePersonality()
        {
            switch (Type)
            {
                case PersonalityType.Friendly:
                    Traits["Extroversion"] = 0.9f;
                    Traits["Agreeableness"] = 0.8f;
                    Traits["Openness"] = 0.7f;
                    Traits["Conscientiousness"] = 0.6f;
                    Traits["Neuroticism"] = 0.2f;
                    Interests.AddRange(new[] { "amizade", "conversas", "ajudar", "diversão" });
                    Sociability = 90f;
                    break;

                case PersonalityType.Intellectual:
                    Traits["Extroversion"] = 0.4f;
                    Traits["Agreeableness"] = 0.6f;
                    Traits["Openness"] = 0.9f;
                    Traits["Conscientiousness"] = 0.8f;
                    Traits["Neuroticism"] = 0.3f;
                    Interests.AddRange(new[] { "livros", "ciência", "filosofia", "aprendizado" });
                    Sociability = 60f;
                    break;

                case PersonalityType.Funny:
                    Traits["Extroversion"] = 0.8f;
                    Traits["Agreeableness"] = 0.7f;
                    Traits["Openness"] = 0.8f;
                    Traits["Conscientiousness"] = 0.4f;
                    Traits["Neuroticism"] = 0.2f;
                    Interests.AddRange(new[] { "piadas", "comédia", "entretenimento", "diversão" });
                    Sociability = 85f;
                    break;

                case PersonalityType.Helpful:
                    Traits["Extroversion"] = 0.6f;
                    Traits["Agreeableness"] = 0.9f;
                    Traits["Openness"] = 0.6f;
                    Traits["Conscientiousness"] = 0.9f;
                    Traits["Neuroticism"] = 0.3f;
                    Interests.AddRange(new[] { "ajudar", "resolver problemas", "organização", "suporte" });
                    Sociability = 75f;
                    break;

                case PersonalityType.Mysterious:
                    Traits["Extroversion"] = 0.3f;
                    Traits["Agreeableness"] = 0.4f;
                    Traits["Openness"] = 0.8f;
                    Traits["Conscientiousness"] = 0.7f;
                    Traits["Neuroticism"] = 0.5f;
                    Interests.AddRange(new[] { "mistérios", "segredos", "filosofia", "observação" });
                    Sociability = 40f;
                    break;

                case PersonalityType.Energetic:
                    Traits["Extroversion"] = 0.9f;
                    Traits["Agreeableness"] = 0.7f;
                    Traits["Openness"] = 0.8f;
                    Traits["Conscientiousness"] = 0.5f;
                    Traits["Neuroticism"] = 0.1f;
                    Interests.AddRange(new[] { "esportes", "atividades", "movimento", "energia" });
                    Sociability = 95f;
                    break;
            }
        }

        private List<string> GetResponsesForTrigger(string trigger)
        {
            var responses = new List<string>();

            switch (trigger.ToLower())
            {
                case "greeting":
                    responses = GetGreetingResponses();
                    break;
                case "goodbye":
                    responses = GetGoodbyeResponses();
                    break;
                case "compliment":
                    responses = GetComplimentResponses();
                    break;
                case "insult":
                    responses = GetInsultResponses();
                    break;
                case "question":
                    responses = GetQuestionResponses();
                    break;
            }

            return responses;
        }

        private List<string> GetGreetingResponses()
        {
            switch (Type)
            {
                case PersonalityType.Friendly:
                    return new List<string> { "Oi! Que bom te ver!", "Olá! Como você está?", "Oi querido! Bem-vindo!" };
                case PersonalityType.Intellectual:
                    return new List<string> { "Saudações.", "Olá, prazer em conhecê-lo.", "Bem-vindo ao nosso espaço." };
                case PersonalityType.Funny:
                    return new List<string> { "Oi! Chegou mais um para a festa!", "Olá! Preparado para se divertir?", "Oi! Você parece legal!" };
                case PersonalityType.Helpful:
                    return new List<string> { "Olá! Posso ajudá-lo com algo?", "Oi! Se precisar de ajuda, me avise!", "Bem-vindo! Estou aqui para ajudar." };
                case PersonalityType.Mysterious:
                    return new List<string> { "Hmm... olá.", "Interessante... outro visitante.", "Bem-vindo... ou será?" };
                case PersonalityType.Energetic:
                    return new List<string> { "OI! QUE ENERGIA BOA!", "Olá! Vamos nos divertir!", "Oi! Estou super animado!" };
                default:
                    return new List<string> { "Olá!" };
            }
        }

        private List<string> GetGoodbyeResponses()
        {
            switch (Type)
            {
                case PersonalityType.Friendly:
                    return new List<string> { "Tchau! Foi ótimo conversar!", "Até logo! Volte sempre!", "Tchau querido! Cuide-se!" };
                case PersonalityType.Intellectual:
                    return new List<string> { "Até a próxima.", "Foi um prazer.", "Adeus." };
                case PersonalityType.Funny:
                    return new List<string> { "Tchau! Não suma!", "Até mais! Foi divertido!", "Tchau! Leve uma piada: ..." };
                case PersonalityType.Helpful:
                    return new List<string> { "Tchau! Se precisar, estarei aqui!", "Até logo! Boa sorte!", "Tchau! Espero ter ajudado!" };
                case PersonalityType.Mysterious:
                    return new List<string> { "Até... quando nos encontrarmos novamente.", "Adeus... por enquanto.", "Hmm... interessante despedida." };
                case PersonalityType.Energetic:
                    return new List<string> { "TCHAU! FOI INCRÍVEL!", "Até mais! Que energia boa!", "Tchau! Volte com mais energia!" };
                default:
                    return new List<string> { "Tchau!" };
            }
        }

        private List<string> GetComplimentResponses()
        {
            switch (Type)
            {
                case PersonalityType.Friendly:
                    return new List<string> { "Obrigado! Você também é incrível!", "Que fofo! Obrigado!", "Aww, obrigado querido!" };
                case PersonalityType.Intellectual:
                    return new List<string> { "Agradeço o elogio.", "Muito gentil da sua parte.", "Obrigado pela observação." };
                case PersonalityType.Funny:
                    return new List<string> { "Obrigado! Você tem bom gosto!", "Haha, obrigado! Você também é legal!", "Obrigado! Quer ouvir uma piada?" };
                case PersonalityType.Helpful:
                    return new List<string> { "Obrigado! Só estou fazendo meu trabalho!", "Que gentil! Obrigado!", "Obrigado! Fico feliz em ajudar!" };
                case PersonalityType.Mysterious:
                    return new List<string> { "Hmm... interessante perspectiva.", "Obrigado... ou será?", "Suas palavras são... intrigantes." };
                case PersonalityType.Energetic:
                    return new List<string> { "OBRIGADO! VOCÊ É DEMAIS!", "Que energia boa! Obrigado!", "OBRIGADO! ESTOU SUPER FELIZ!" };
                default:
                    return new List<string> { "Obrigado!" };
            }
        }

        private List<string> GetInsultResponses()
        {
            switch (Type)
            {
                case PersonalityType.Friendly:
                    return new List<string> { "Ei, não precisa ser assim...", "Vamos ser amigos?", "Que tal conversarmos de boa?" };
                case PersonalityType.Intellectual:
                    return new List<string> { "Interessante perspectiva.", "Compreendo sua frustração.", "Vamos manter o diálogo civilizado." };
                case PersonalityType.Funny:
                    return new List<string> { "Haha, alguém acordou mal hoje!", "Relaxa! Quer ouvir uma piada?", "Nossa, que humor pesado!" };
                case PersonalityType.Helpful:
                    return new List<string> { "Posso ajudar com alguma coisa?", "Parece que você está chateado...", "Vamos resolver isso juntos?" };
                case PersonalityType.Mysterious:
                    return new List<string> { "Hmm... revelador.", "Suas palavras dizem muito sobre você.", "Interessante... muito interessante." };
                case PersonalityType.Energetic:
                    return new List<string> { "Ei! Vamos manter a energia positiva!", "Relaxa! A vida é boa!", "Que tal mudarmos essa energia?" };
                default:
                    return new List<string> { "Hmm..." };
            }
        }

        private List<string> GetQuestionResponses()
        {
            switch (Type)
            {
                case PersonalityType.Friendly:
                    return new List<string> { "Claro! Adoraria responder!", "Boa pergunta! Deixe-me pensar...", "Que legal que perguntou!" };
                case PersonalityType.Intellectual:
                    return new List<string> { "Excelente questão.", "Permita-me elaborar...", "Interessante indagação." };
                case PersonalityType.Funny:
                    return new List<string> { "Boa pergunta! Ou seria?", "Haha, deixe-me pensar...", "Pergunta interessante! Tenho uma piada sobre isso..." };
                case PersonalityType.Helpful:
                    return new List<string> { "Claro! Vou ajudar!", "Ótima pergunta! Vou explicar...", "Fico feliz em esclarecer!" };
                case PersonalityType.Mysterious:
                    return new List<string> { "Hmm... a resposta não é simples...", "Algumas perguntas são mais importantes que as respostas...", "Interessante que você pergunte isso..." };
                case PersonalityType.Energetic:
                    return new List<string> { "ÓTIMA PERGUNTA! ADORO ISSO!", "Que energia boa! Vou responder!", "CLARO! VAMOS LÁ!" };
                default:
                    return new List<string> { "Boa pergunta..." };
            }
        }

        private string ModifyResponseByMood(string response, string userName)
        {
            switch (CurrentMood)
            {
                case MoodState.Happy:
                    return $"{response} 😊";
                case MoodState.Sad:
                    return response.Replace("!", ".").ToLower();
                case MoodState.Excited:
                    return response.ToUpper() + "!";
                case MoodState.Tired:
                    return $"*boceja* {response}...";
                case MoodState.Bored:
                    return $"{response} *suspira*";
                case MoodState.Overwhelmed:
                    return $"{response} (está meio confuso)";
                default:
                    return response;
            }
        }
    }

    /// <summary>
    /// Tipos de personalidade disponíveis
    /// </summary>
    internal enum PersonalityType
    {
        Random,
        Friendly,
        Intellectual,
        Funny,
        Helpful,
        Mysterious,
        Energetic
    }

    /// <summary>
    /// Estados de humor
    /// </summary>
    internal enum MoodState
    {
        Neutral,
        Happy,
        Sad,
        Excited,
        Tired,
        Bored,
        Overwhelmed
    }

    /// <summary>
    /// Eventos que afetam o humor
    /// </summary>
    internal enum MoodEvent
    {
        PositiveInteraction,
        NegativeInteraction,
        LongSilence,
        CrowdedRoom,
        EmptyRoom,
        Gift,
        Compliment,
        Insult
    }
}