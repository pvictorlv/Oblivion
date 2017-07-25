using System.Linq;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Support;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class BanUser. This class cannot be inherited.
    /// </summary>
    internal sealed class BanUser : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BanUser" /> class.
        /// </summary>
        public BanUser()
        {
            MinRank = 4;
            Description = "Ban a user!";
            Usage = ":ban [USERNAME] [TIME] [REASON]";
            MinParams = -2;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            {
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
                try
                {
                    var length = int.Parse(pms[1]);

                    var message = pms.Length < 3 ? string.Empty : string.Join(" ", pms.Skip(2));
                    if (string.IsNullOrWhiteSpace(message))
                        message = Oblivion.GetLanguage().GetVar("command_ban_user_no_reason");

                    ModerationTool.BanUser(session, user.GetHabbo().Id, length, message);
                    Oblivion.GetGame()
                        .GetModerationTool()
                        .LogStaffEntry(session.GetHabbo().UserName, user.GetHabbo().UserName, "Ban",
                            string.Format("USER:{0} TIME:{1} REASON:{2}", pms[0], pms[1], pms[2]));
                }
                catch
                {
                    // error handle
                }

                return true;
            }
        }
    }
}