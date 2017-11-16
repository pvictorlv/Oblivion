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

        public override bool Execute(GameClient session, string[] pms)
        {
            using (var adapter = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                Oblivion.GetGame().GetItemManager().LoadItems(adapter);
                Oblivion.GetGame().GetCatalog().Initialize(adapter);
            }
            var msg = new ServerMessage(LibraryParser.OutgoingRequest("PublishShopMessageComposer"));
            msg.AppendBool(false);
            Oblivion.GetGame()
                .GetClientManager()
                .SendMessage(msg);
            return true;
        }
    }
}