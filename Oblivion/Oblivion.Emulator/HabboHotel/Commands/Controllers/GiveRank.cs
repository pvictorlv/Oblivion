using System;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class GiveCredits. This class cannot be inherited.
    /// </summary>
    internal sealed class GiveRank : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GiveCredits" /> class.
        /// </summary>
        public GiveRank()
        {
            MinRank = 11;
            Description = "Dar Rango al Usuario.";
            Usage = ":giverank [USERNAME] [RANK]";
            MinParams = 2;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var staff = session.GetHabbo().UserName.ToLower();
            if (staff != "dark" && staff != "roberta" && staff != "crazzyflos")
            {
                return false;
            }

            var user = Oblivion.GetGame().GetClientManager().GetClientByUserName(pms[0]);

            if (user == null)
            {
                session.SendWhisper(Oblivion.GetLanguage().GetVar("user_not_found"));
                return true;
            }

            if (user.GetHabbo().Rank >= session.GetHabbo().Rank)
            {
                session.SendWhisper(Oblivion.GetLanguage().GetVar("user_is_higher_rank"));
                return true;
            }

            var userName = pms[0];
            var rank = pms[1];
            using (var adapter = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                adapter.SetQuery("UPDATE users SET rank=@rank WHERE username=@user LIMIT 1");
                adapter.AddParameter("user", userName);
                adapter.AddParameter("rank", rank);
                adapter.RunQuery();
            }

            user.GetHabbo().Rank = Convert.ToUInt32(pms[1]);
            session.SendWhisper(Oblivion.GetLanguage().GetVar("user_rank_update"));
            return true;
        }
    }
}