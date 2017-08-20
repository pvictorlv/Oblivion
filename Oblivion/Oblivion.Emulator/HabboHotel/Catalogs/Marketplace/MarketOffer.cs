namespace Oblivion.HabboHotel.Catalogs.Marketplace
{
    public class MarketOffer
    {
        public MarketOffer(int offerId, int spriteId, int totalPrice, int itemType, int limitedNumber, int limitedStack)
        {
            OfferId = offerId;
            SpriteId = spriteId;
            ItemType = itemType;
            TotalPrice = totalPrice;
            LimitedNumber = limitedNumber;
            LimitedStack = limitedStack;
        }

        public int OfferId { get; set; }
        public int ItemType { get; set; }
        public int SpriteId { get; set; }
        public int TotalPrice { get; set; }
        public int LimitedNumber { get; set; }
        public int LimitedStack { get; set; }
    }
}