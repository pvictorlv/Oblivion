using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class HotelAlert. This class cannot be inherited.
    /// </summary>
    internal sealed class StaffAlert : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="StaffAlert" /> class.
        /// </summary>
        public StaffAlert()
        {
            MinRank = 5;
            Description = "Alerts to all connected staffs.";
            Usage = ":sa [MESSAGE]";
            MinParams = -1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var msg = string.Join(" ", pms);

            var message =
                new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
            message.AppendString("staffcloud");
            message.AppendInteger(2);
            message.AppendString("title");
            message.AppendString("Staff Internal Alert");
            message.AppendString("message");
            message.AppendString(
                string.Format(
                    "{0}\r\n- <i>Sender: {1}</i>",
                    msg, session.GetHabbo().UserName));
            Oblivion.GetGame().GetClientManager().StaffAlert(message);
            Oblivion.GetGame()
                .GetModerationTool()
                .LogStaffEntry(session.GetHabbo().UserName, string.Empty, "StaffAlert",
                    string.Format("Staff alert [{0}]", msg));

            return true;
        }
    }
}