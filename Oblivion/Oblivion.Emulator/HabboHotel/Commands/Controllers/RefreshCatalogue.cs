using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class RefreshCatalogue. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshCatalogue : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshCatalogue" /> class.
        /// </summary>
        public RefreshCatalogue()
        {
            MinRank = 9;
            Description = "Refreshes Catalogue from Database.";
            Usage = ":refresh_catalogue";
            MinParams = 0;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            using (var adapter = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
            {
                Oblivion.GetGame().GetItemManager().LoadItems(adapter);
                Oblivion.GetGame().GetCatalog().Initialize(adapter);
            }
            var msg = new ServerMessage(LibraryParser.OutgoingRequest("PublishShopMessageComposer"));
            msg.AppendBool(false);
            await Oblivion.GetGame()
                .GetClientManager()
                .SendMessageAsync(msg);
            return true;
        }
    }
}