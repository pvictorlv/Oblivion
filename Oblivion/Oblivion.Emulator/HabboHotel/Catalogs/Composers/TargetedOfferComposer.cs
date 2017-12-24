using Oblivion.HabboHotel.Catalogs.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Catalogs.Composers
{
    internal class TargetedOfferComposer
    {
        internal static void GenerateMessage(ServerMessage message, TargetedOffer offer)
        {
            message.Init(LibraryParser.OutgoingRequest("TargetedOfferMessageComposer"));
            message.AppendInteger(1);
            message.AppendInteger(offer.Id);
            message.AppendString(offer.Identifier);
            message.AppendString(offer.Identifier);
            message.AppendInteger(offer.CostCredits);

            if (offer.CostDiamonds > 0)
            {
                message.AppendInteger(offer.CostDiamonds);
                message.AppendInteger(5);
            }
            else
            {
                message.AppendInteger(offer.CostDuckets);
                message.AppendInteger(0);
            }

            message.AppendInteger(offer.PurchaseLimit);

            var timeLeft = offer.ExpirationTime - Oblivion.GetUnixTimeStamp();

            message.AppendInteger(timeLeft);
            message.AppendString(offer.Title);
            message.AppendString(offer.Description);
            message.AppendString(offer.Image);
            message.AppendString(string.Empty);
            message.StartArray();

            /* TODO CHECK */ foreach (var product in offer.Products)
            {
                message.AppendString(product);
                message.SaveArray();
            }

            message.EndArray();
        }
    }
}