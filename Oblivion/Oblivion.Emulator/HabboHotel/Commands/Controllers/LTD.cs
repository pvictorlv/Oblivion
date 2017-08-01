using Oblivion.Configuration;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class LTD. This class cannot be inherited.
    /// </summary>
    internal sealed class Ltd : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Ltd" /> class.
        /// </summary>
        public Ltd()
        {
            MinRank = 7;
            Description = "Atualiza os LTDS";
            Usage = ":ltd";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            using (var adapter = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                Oblivion.GetGame().GetItemManager().LoadItems(adapter);
                Oblivion.GetGame().GetCatalog().Initialize(adapter);
                Oblivion.GetGame().ReloadItems();
            }
            var msg = new ServerMessage(LibraryParser.OutgoingRequest("PublishShopMessageComposer"));
            msg.AppendBool(false);
            Oblivion.GetGame()
                .GetClientManager()
                .QueueBroadcaseMessage(msg);
            var message = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
            message.AppendString("ninja_promo_LTD");
            message.AppendInteger(4);
            message.AppendString("title");
            message.AppendString("Novo Raro Limitado!");
            message.AppendString("message");
            message.AppendString(
                "<i><h1>Como Assim?</h1>, Um Novo Raro Limitado foi Adicionado na Loja!<br> Descubra como ele é Abrindo a Loja!</br>");
            message.AppendString("linkUrl");
            message.AppendString("event:catalog/open/ultd_furni");
            message.AppendString("linkTitle");
            message.AppendString("Ver o Raro");

            Oblivion.GetGame().GetClientManager().QueueBroadcaseMessage(message);
            return true;
        }
    }
}