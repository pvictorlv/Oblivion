using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class SetSpeed. This class cannot be inherited.
    /// </summary>
    internal sealed class SetSpeed : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SetSpeed" /> class.
        /// </summary>
        public SetSpeed()
        {
            MinRank = -1;
            Description = "Set speed of rollers.";
            Usage = ":setspeed [NUMBER]";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            double speed;
            if (double.TryParse(pms[0], out speed)) session.GetHabbo().CurrentRoom.GetRoomItemHandler().SetSpeed(speed);
            else session.SendWhisper(Oblivion.GetLanguage().GetVar("command_setspeed_error_numbers"));

            return true;
        }
    }
}