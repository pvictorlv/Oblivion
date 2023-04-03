using System;
using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class FloodUser. This class cannot be inherited.
    /// </summary>
    internal sealed class FloodUser : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FloodUser" /> class.
        /// </summary>
        public FloodUser()
        {
            MinRank = 5;
            Description = "Flood user.";
            Usage = ":flood [USERNAME] [TIME]";
            MinParams = 2;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var client = Oblivion.GetGame().GetClientManager().GetClientByUserName(pms[0]);
            if (client == null)
            {
                 await Session.SendWhisperAsync(Oblivion.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            if (client.GetHabbo().Rank >= session.GetHabbo().Rank)
            {
                 await Session.SendWhisperAsync(Oblivion.GetLanguage().GetVar("user_is_higher_rank"));
                return true;
            }
            if (!int.TryParse(pms[1], out _))
            {
                 await Session.SendWhisperAsync(Oblivion.GetLanguage().GetVar("enter_numbers"));
                return true;
            }

            client.GetHabbo().FloodTime = Oblivion.GetUnixTimeStamp() + Convert.ToInt32(pms[1]);
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("FloodFilterMessageComposer"));
            await serverMessage.AppendIntegerAsync(Convert.ToInt32(pms[1]));
            await client.SendMessage(serverMessage);
            return true;
        }
    }
}