using System.Threading.Tasks;
using Oblivion.Collections;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Effects
{
    public class GiveReward : IWiredItem
    {
        public void Dispose()
        {
        }

        public bool Disposed { get; set; }

        public GiveReward(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            OtherString = string.Empty;
            OtherExtraString = string.Empty;
            OtherExtraString2 = string.Empty;
        }

        public Interaction Type => Interaction.ActionGiveReward;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public ConcurrentList<RoomItem> Items
        {
            get { return new ConcurrentList<RoomItem>(); }
            set { }
        }

        public int Delay
        {
            get { return 0; }
            set { }
        }

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public Task<bool> Execute(params object[] stuff)
        {
            var user = (RoomUser) stuff?[0];
            if (user?.GetClient()?.GetHabbo()?.GetBadgeComponent() == null) return false;
            if (stuff[1] == null)
                return false;


            if (OtherExtraString2 == null)
                return false;

            if (!int.TryParse(OtherExtraString2, out var amountLeft))
                return false;

            var unique = OtherBool;

            var premied = false;

            if (amountLeft == 1)
            {
                var message = new ServerMessage(LibraryParser.OutgoingRequest("WiredRewardAlertMessageComposer"));

                message.AppendInteger(0);
                await user.GetClient().SendMessageAsync(message);

                return true;
            }

            /* TODO CHECK */
            foreach (var dataStr in OtherString.Split(';'))
            {
                var dataArray = dataStr.Split(',');

                if (dataArray.Length < 3) continue;

                var badge = dataArray[0];

                if (badge == null) continue;

                var isbadge = badge == "0";

                var code = dataArray[1];

                if (code == null) continue;

                if (!int.TryParse(dataArray[2], out var percentage)) continue;

                var random = Oblivion.GetRandomNumber(0, 100);

                var message = new ServerMessage(LibraryParser.OutgoingRequest("WiredRewardAlertMessageComposer"));

                if (!unique && percentage < random)
                    continue;

                premied = true;
                
                if (isbadge)
                {
                    if (user.GetClient().GetHabbo().GetBadgeComponent().HasBadge(code))
                    {
                        message.AppendInteger(1);
                        await user.GetClient().SendMessageAsync(message);
                    }
                    else
                    {
                        user.GetClient()
                            .GetHabbo()
                            .GetBadgeComponent()
                            .GiveBadge(code, true, user.GetClient(), true);

                        message.AppendInteger(7);
                        await user.GetClient().SendMessageAsync(message);
                    }
                }
                else //item or effect
                {
                    var roomItem = Oblivion.GetGame().GetItemManager().GetItem(uint.Parse(code));

                    if (roomItem == null)
                        continue;

                    if (roomItem.Type == 'e') // is effect
                    {
                        user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent()
                            .AddNewEffect(roomItem.SpriteId, 3600, 1);
                    }
                    else
                    {
                        user.GetClient().GetHabbo().GetInventoryComponent()
                            .AddNewItem("0u", roomItem.ItemId, "0", 0u, true, false, 0, 0);
                        user.GetClient()
                            .SendMessage(new ServerMessage(
                                LibraryParser.OutgoingRequest("UpdateInventoryMessageComposer")));
                    }

                    message.AppendInteger(6);
                    await user.GetClient().SendMessageAsync(message);
                }
            }

            if (!premied)
            {
                var message = new ServerMessage(LibraryParser.OutgoingRequest("WiredRewardAlertMessageComposer"));
                message.AppendInteger(4);
                await user.GetClient().SendMessageAsync(message);
            }
            else if (amountLeft > 1)
            {
                amountLeft--;
                OtherExtraString2 = amountLeft.ToString();
            }

            return true;
        }
    }
}