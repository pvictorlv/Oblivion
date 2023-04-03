using System;
using System.Threading.Tasks;
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

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
         

            var user = Oblivion.GetGame().GetClientManager().GetClientByUserName(pms[0]);

            if (user == null)
            {
                 await Session.SendWhisperAsync(Oblivion.GetLanguage().GetVar("user_not_found"));
                return true;
            }

            if (user.GetHabbo().Rank >= session.GetHabbo().Rank)
            {
                 await Session.SendWhisperAsync(Oblivion.GetLanguage().GetVar("user_is_higher_rank"));
                return true;
            }

            var userName = pms[0];
            var rank = pms[1];
            using (var adapter = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                adapter.SetQuery("UPDATE users SET rank=@rank WHERE username=@user LIMIT 1");
                adapter.AddParameter("user", userName);
                adapter.AddParameter("rank", rank);
                await adapter.RunQueryAsync();
            }

            user.GetHabbo().Rank = Convert.ToUInt32(pms[1]);
             await Session.SendWhisperAsync(Oblivion.GetLanguage().GetVar("user_rank_update"));
            return true;
        }
    }
}