using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;
using System.Threading.Tasks;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    public class ResetPosition : IWiredItem
    {
        public ResetPosition(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            OtherString = string.Empty;
            OtherExtraString = string.Empty;
            OtherExtraString2 = string.Empty;
            Delay = 0;
            Items = new ConcurrentList<RoomItem>();
        }


        public void Dispose()
        {

        }

        public bool Disposed { get; set; }

        public Interaction Type => Interaction.ActionPosReset;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items { get; set; }

        public int Delay { get; set; }

        private long _mNext;

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }


        public async Task<bool> Execute(params object[] stuff)
        {
            if (Room == null)
                return false;

            if (Items?.Count == 0)
                return false;
            
            if (string.IsNullOrWhiteSpace(OtherString) || string.IsNullOrWhiteSpace(OtherExtraString))
                return false;

            var booleans = OtherString.Split(',');

            if (booleans.Length < 3)
                return false;

            var num = Oblivion.Now();

            if (_mNext > num)
                await Task.Delay((int)(_mNext - num));


            var extraData = booleans[0] == "true";
            var rot = booleans[1] == "true";
            var position = booleans[2] == "true";
            var arr = OtherExtraString.Split('/');
            if (arr.Length > 0)
            {
                foreach (var itemData in arr)
                {
                    if (string.IsNullOrWhiteSpace(itemData))
                        continue;

                    var innerData = itemData.Split('|');
                    if (innerData.Length < 4)
                        continue;

                    var itemId = uint.Parse(innerData[0]);

                    var fItem = Room.GetRoomItemHandler().GetItem(itemId);
                    if (Room.GetWiredHandler().OtherBoxHasItem(this, fItem))
                    {
                        continue;
                    }

                    if (fItem == null)
                        continue;

                    var extraDataToSet = extraData ? innerData[1] : fItem.ExtraData;
                    var rotationToSet = rot ? int.Parse(innerData[2]) : fItem.Rot;

                    var positions = innerData[3].Split(',');
                    if (positions.Length < 3)
                        continue;

                    int xToSet;
                    if (position) int.TryParse(positions[0], out xToSet);
                    else xToSet = fItem.X;
                    int yToSet;
                    if (position) int.TryParse(positions[1], out yToSet);
                    else yToSet = fItem.Y;
                    double zToSet;
                    if (position) double.TryParse(positions[2], out zToSet);
                    else zToSet = fItem.Z;


                    var serverMessage =
                        new ServerMessage(LibraryParser.OutgoingRequest("ItemAnimationMessageComposer"));
                    serverMessage.AppendInteger(fItem.X);
                    serverMessage.AppendInteger(fItem.Y);
                    serverMessage.AppendInteger(xToSet);
                    serverMessage.AppendInteger(yToSet);
                    serverMessage.AppendInteger(1);
                    serverMessage.AppendInteger(fItem.VirtualId);
                    serverMessage.AppendString(fItem.Z.ToString(Oblivion.CultureInfo));
                    serverMessage.AppendString(zToSet.ToString(Oblivion.CultureInfo));
                    serverMessage.AppendInteger(0);
                    Room.SendMessage(serverMessage);

                    Room.GetRoomItemHandler().SetFloorItem(null, fItem, xToSet, yToSet, rotationToSet, false, false,
                        false,
                        false, false);
                    fItem.ExtraData = extraDataToSet;
                    fItem.UpdateState();
                }
            }
            _mNext = Oblivion.Now() + Delay;
            return true;
        }
    }
}