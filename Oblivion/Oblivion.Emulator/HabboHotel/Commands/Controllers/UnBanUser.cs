using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class UnBanUser. This class cannot be inherited.
    /// </summary>
    internal sealed class UnBanUser : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UnBanUser" /> class.
        /// </summary>
        public UnBanUser()
        {
            MinRank = 4;
            Description = "Unban a user!";
            Usage = ":unban [USERNAME]";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var user = Oblivion.GetHabboForName(pms[0]);

            if (user == null)
            {
                session.SendWhisper(Oblivion.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            if (user.Rank >= session.GetHabbo().Rank)
            {
                session.SendWhisper(Oblivion.GetLanguage().GetVar("user_is_higher_rank"));
                return true;
            }
            using (var adapter = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                adapter.SetQuery("DELETE FROM users_bans WHERE value = @name");
                adapter.AddParameter("name", user.UserName);
                adapter.RunQuery();
                Oblivion.GetGame()
                    .GetModerationTool()
                    .LogStaffEntry(session.GetHabbo().UserName, user.UserName, "Unban",
                        $"User has been Unbanned [{pms[0]}]");
                return true;
            }
        }
    }
}