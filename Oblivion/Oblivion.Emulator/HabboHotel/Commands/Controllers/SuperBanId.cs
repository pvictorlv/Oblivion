﻿using System.Linq;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class SuperBan. This class cannot be inherited.
    /// </summary>
    internal sealed class SuperBanId : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SuperBan" /> class.
        /// </summary>
        public SuperBanId()
        {
            MinRank = 5;
            Description = "Super ban a user by id!";
            Usage = ":superbanid [USERNAME] [REASON]";
            MinParams = -1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            if (!uint.TryParse(pms[0], out var userId)) return true;

            var client = Oblivion.GetGame().GetClientManager().GetClientByUserId(userId);
            if (client == null)
            {
                session.SendNotif(Oblivion.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            
            if (client.GetHabbo().Rank >= session.GetHabbo().Rank)
            {
                session.SendNotif(Oblivion.GetLanguage().GetVar("user_is_higher_rank"));
                return true;
            }
            Oblivion.GetGame()
                .GetModerationTool()
                .LogStaffEntry(session.GetHabbo().UserName, client.GetHabbo().UserName, "Ban",
                    "User has received a Super ban.");
            Oblivion.GetGame()
                .GetBanManager()
                .BanUser(client, session.GetHabbo().UserName, 788922000.0, string.Join(" ", pms.Skip(1)),
                    false, false);
            return true;
        }
    }
}