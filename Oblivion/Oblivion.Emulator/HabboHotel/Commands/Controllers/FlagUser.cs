using System.Globalization;
using System.Threading.Tasks;
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

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var name = pms[0];
            var user = Oblivion.GetGame().GetClientManager().GetClientByUserName(name);
            if (user?.GetHabbo() == null) return false;

            var habbo = user.GetHabbo();
            var response = new ServerMessage();
            await response.InitAsync(LibraryParser.OutgoingRequest("UserObjectMessageComposer"));
            await response.AppendIntegerAsync(habbo.Id);
            await response.AppendStringAsync(habbo.UserName);
            await response.AppendStringAsync(habbo.Look);
            await response.AppendStringAsync(habbo.Gender.ToUpper());
            await response.AppendStringAsync(habbo.Motto);
            await response.AppendStringAsync("");
            response.AppendBool(false);
            await response.AppendIntegerAsync(habbo.Respect);
            await response.AppendIntegerAsync(habbo.DailyRespectPoints);
            await response.AppendIntegerAsync(habbo.DailyPetRespectPoints);
            response.AppendBool(true);
            await response.AppendStringAsync(habbo.LastOnline.ToString(CultureInfo.InvariantCulture));
            response.AppendBool(true);
            response.AppendBool(false);

            await habbo.GetClient().SendMessageAsync(response);

            user.GetHabbo().LastChange = 0;
            return true;
        }
    }
}