using System;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;
using System.Threading.Tasks;
using Oblivion.Configuration;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    public class ResetPosition : IWiredItem, IWiredCycler
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


        private int _delay;

        public double TickCount { get; set; }

        public int Delay
        {
            get => _delay;
            set
            {
                _delay = value;
                TickCount = value / 1000;
            }
        }

        public bool Requested;
        private long _mNext;

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public async Task<bool> OnCycle()
        {
            try
            {
                if (!Requested) return false;

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
                    return false;



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

                        var itemId = innerData[0];

                        var fItem = Room.GetRoomItemHandler().GetItem(itemId);


                        if (fItem == null)
                            continue;

                        var extraDataToSet = extraData ? innerData[1] : fItem.ExtraData;
                        var rotationToSet = rot ? int.Parse(innerData[2]) : fItem.Rot;

                        var positions = innerData[3].Split(',');
                        if (positions.Length < 3)
                            continue;

                        int xToSet, yToSet;
                        double zToSet;

                        if (position)
                        {
                            int.TryParse(positions[0], out xToSet);
                            int.TryParse(positions[1], out yToSet);
                            double.TryParse(positions[2], out zToSet);
                        }
                        else
                        {
                            xToSet = fItem.X;
                            yToSet = fItem.Y;
                            zToSet = fItem.Z;
                        }


                        var serverMessage =
                            new ServerMessage(LibraryParser.OutgoingRequest("ItemAnimationMessageComposer"));
                        await serverMessage.AppendIntegerAsync(fItem.X);
                        await serverMessage.AppendIntegerAsync(fItem.Y);
                        await serverMessage.AppendIntegerAsync(xToSet);
                        await serverMessage.AppendIntegerAsync(yToSet);
                        await serverMessage.AppendIntegerAsync(1);
                        await serverMessage.AppendIntegerAsync(fItem.VirtualId);
                        await serverMessage.AppendStringAsync(fItem.Z.ToString(Oblivion.CultureInfo));
                        await serverMessage.AppendStringAsync(zToSet.ToString(Oblivion.CultureInfo));
                        await serverMessage.AppendIntegerAsync(0);
                        Room.SendMessage(serverMessage);

                        Room.GetRoomItemHandler().SetFloorItem(null, fItem, xToSet, yToSet, rotationToSet, false, false,
                            true,
                            false, true);

                        fItem.ExtraData = extraDataToSet;
                        fItem.UpdateState();
                        fItem.GetRoom().GetGameMap().UpdateMapForItem(fItem);

                    }

                    Room.GetGameMap().GenerateMaps();
                }

                Requested = false;
                _mNext = Oblivion.Now() + Delay;
                return true;
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "ResetPostion.cs");
                return false;
            }

        }
        public async Task<bool> Execute(params object[] stuff)
        {
            _mNext = Oblivion.Now() + Delay;
            Requested = true;
            return true;
        }
    }
}