using System.Threading.Tasks;
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

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            using (var adapter = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                Oblivion.GetGame().GetItemManager().LoadItems(adapter);
                await Oblivion.GetGame().GetCatalog().Initialize(adapter);
                await Oblivion.GetGame().ReloadItems();
            }
            var msg = new ServerMessage(LibraryParser.OutgoingRequest("PublishShopMessageComposer"));
            msg.AppendBool(false);
            await Oblivion.GetGame()
                .GetClientManager()
                .SendMessageAsync(msg);
            var message = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
            await message.AppendStringAsync("ninja_promo_LTD");
            await message.AppendIntegerAsync(4);
            await message.AppendStringAsync("title");
            await message.AppendStringAsync("Novo Raro Limitado!");
            await message.AppendStringAsync("message");
            await message.AppendStringAsync(
                "<i><h1>Como Assim?</h1>, Um Novo Raro Limitado foi Adicionado na Loja!<br> Descubra como ele é Abrindo a Loja!</br>");
            await message.AppendStringAsync("linkUrl");
            await message.AppendStringAsync("event:catalog/open/ultd_furni");
            await message.AppendStringAsync("linkTitle");
            await message.AppendStringAsync("Ver o Raro");

            await Oblivion.GetGame().GetClientManager().SendMessageAsync(message);
            return true;
        }
    }
}