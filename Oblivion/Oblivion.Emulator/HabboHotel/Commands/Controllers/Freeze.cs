using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class Freeze. This class cannot be inherited.
    /// </summary>
    internal sealed class Freeze : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Freeze" /> class.
        /// </summary>
        public Freeze()
        {
            MinRank = 7;
            Description = "Makes the user can't walk. To let user can walk again, execute this command again.";
            Usage = ":freeze [USERNAME]";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var user = session.GetHabbo()
                .CurrentRoom.GetRoomUserManager()
                .GetRoomUserByHabbo(pms[0]);
            if (user == null) session.SendWhisper(Oblivion.GetLanguage().GetVar("user_not_found"));
            else user.Frozen = !user.Frozen;

            return true;
        }
    }
}