using System.Globalization;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    internal sealed class FlagUser : Command
    {

        public FlagUser()
        {
            MinRank = 7;
            Description = "Enable name change for user";
            Usage = ":flaguser [name]";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var name = pms[0];
            var user = Oblivion.GetGame().GetClientManager().GetClientByUserName(name);
            if (user?.GetHabbo() == null) return false;

            var habbo = user.GetHabbo();
            var response = new ServerMessage();
            response.Init(LibraryParser.OutgoingRequest("UserObjectMessageComposer"));
            response.AppendInteger(Oblivion.GetGame().GetClientManager().GetVirtualId(habbo.Id));
            response.AppendString(habbo.UserName);
            response.AppendString(habbo.Look);
            response.AppendString(habbo.Gender.ToUpper());
            response.AppendString(habbo.Motto);
            response.AppendString("");
            response.AppendBool(false);
            response.AppendInteger(habbo.Respect);
            response.AppendInteger(habbo.DailyRespectPoints);
            response.AppendInteger(habbo.DailyPetRespectPoints);
            response.AppendBool(true);
            response.AppendString(habbo.LastOnline.ToString(CultureInfo.InvariantCulture));
            response.AppendBool(true);
            response.AppendBool(false);
            
            habbo.GetClient().SendMessage(response);

            user.GetHabbo().LastChange = 0;
            return true;
        }
    }
}