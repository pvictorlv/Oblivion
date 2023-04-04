using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class GiveBadge. This class cannot be inherited.
    /// </summary>
    internal sealed class GivePoints : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GiveBadge" /> class.
        /// </summary>
        public GivePoints()
        {
            MinRank = 6;
            Description = "Give points for user";
            Usage = ":reward [username]";
            MinParams = 1;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var client = Oblivion.GetGame().GetClientManager().GetClientByUserName(pms[0]);
            if (client == null)
            {
                await session.SendNotif(Oblivion.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            
            client.GetHabbo().Credits += 15;
            await client.SendWhisperAsync("Você recebeu 15 moedas!");
            await client.GetHabbo().UpdateCreditsBalance();


            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET epoints = epoints + 1 WHERE id = @id");
                dbClient.AddParameter("id", client.GetHabbo().Id);
                await dbClient.RunQueryAsync();
            }

            await Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(client, Oblivion.GetDbConfig().DbData["badge.code"], 1, true);

            await Oblivion.GetGame()
                .GetModerationTool()
                .LogStaffEntry(session.GetHabbo().UserName, client.GetHabbo().UserName,
                    "EventPoints", $"Rewarded user [{pms[0]}] in event");


            var msg = new ServerMessage(LibraryParser.OutgoingRequest("RoomNotificationMessageComposer"));
          
            await msg.AppendStringAsync("rank");
            await msg.AppendIntegerAsync(5);
            await msg.AppendStringAsync("title");
            await msg.AppendStringAsync("Hotel");
            await msg.AppendStringAsync("message");
            await msg.AppendStringAsync($"O usuário {client.GetHabbo().UserName} ganhou o evento!");
            await msg.AppendStringAsync("linkUrl");
            await msg.AppendStringAsync("");
            await msg.AppendStringAsync("linkTitle");
            await msg.AppendStringAsync("");
            await msg.AppendStringAsync("display");
            await msg.AppendStringAsync("BUBBLE");
            await Oblivion.GetGame().GetClientManager().SendMessageAsync(msg);
            

            return true;
        }
    }
}