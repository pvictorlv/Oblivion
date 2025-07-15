// EXEMPLO DE COMO INTEGRAR O SISTEMA DE IA DOS PETS
// Este arquivo mostra como inicializar a IA dos pets no código existente

// No método onde o pet é criado/carregado (provavelmente em Pet.cs ou similar):

/*
// Exemplo de inicialização da IA do pet
public void InitializePetAI()
{
    // Quando um pet é colocado no quarto
    if (this.PlacedInRoom && this.RoomUser != null)
    {
        var petBot = this.RoomUser.BotAi as PetBot;
        if (petBot != null)
        {
            // Inicializar o sistema de IA
            petBot.InitializeIntelligence(this);
        }
    }
}
*/

// No BotManager.cs ou onde os bots são gerenciados:

/*
// Exemplo de como garantir que a IA seja inicializada
public void LoadPetIntoRoom(Pet pet, Room room)
{
    // Código existente para carregar pet...
    
    // Após carregar o pet no quarto:
    var roomUser = room.GetRoomUserManager().GetRoomUserByVirtualId(pet.VirtualId);
    if (roomUser != null && roomUser.BotAi is PetBot petBot)
    {
        petBot.InitializeIntelligence(pet);
    }
}
*/

// Para registrar os novos comandos no CommandManager:

/*
// No método onde os comandos são registrados (provavelmente CommandManager.cs):
public void RegisterCommands()
{
    // Comandos existentes...
    
    // Novos comandos de IA
    Commands.Add("petstatus", new PetStatus());
    Commands.Add("botstats", new BotStats());
}
*/

// Exemplo de uso no RoomManager ou similar:

/*
// Quando um bot é criado no quarto
public void CreateBot(RoomBot botData, Room room)
{
    // Código existente...
    
    // O GenericBot já inicializa automaticamente os sistemas de IA
    // no construtor, então não precisa de código adicional
    
    var bot = new GenericBot(botData, virtualId, isBartender, speechInterval);
    // O bot já terá memória, personalidade e sistema de tarefas ativos
}
*/