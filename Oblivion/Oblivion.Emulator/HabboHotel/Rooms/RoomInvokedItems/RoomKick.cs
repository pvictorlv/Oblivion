namespace Oblivion.HabboHotel.Rooms.RoomInvokedItems
{
    internal struct RoomKick
    {
        internal string Alert;
        internal uint MinRank;

        public RoomKick(string alert, uint minRank)
        {
            Alert = alert;
            MinRank = minRank;
        }
    }
}