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
            client.SendWhisper("Você recebeu 15 moedas!");
            client.GetHabbo().UpdateCreditsBalance();


            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET epoints = epoints + 1 WHERE id = @id");
                dbClient.AddParameter("id", client.GetHabbo().Id);
                dbClient.RunQuery();
            }

            Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(client, Oblivion.GetDbConfig().DbData["badge.code"], 1, true);
            
            Oblivion.GetGame()
                .GetModerationTool()
                .LogStaffEntry(session.GetHabbo().UserName, client.GetHabbo().UserName,
                    "EventPoints", $"Rewarded user [{pms[0]}] in event");


            var msg = new ServerMessage(LibraryParser.OutgoingRequest("RoomNotificationMessageComposer"));
          
            msg.AppendString("rank");
            msg.AppendInteger(5);
            msg.AppendString("title");
            msg.AppendString("Hotel");
            msg.AppendString("message");
            msg.AppendString($"O usuário {client.GetHabbo().UserName} ganhou o evento!");
            msg.AppendString("linkUrl");
            msg.AppendString("");
            msg.AppendString("linkTitle");
            msg.AppendString("");
            msg.AppendString("display");
            msg.AppendString("BUBBLE");
            Oblivion.GetGame().GetClientManager().SendMessage(msg);
            

            return true;
        }
    }
}