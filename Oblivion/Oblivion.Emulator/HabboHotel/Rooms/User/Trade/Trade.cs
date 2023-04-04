using System;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.Configuration;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Rooms.User.Trade
{
    /// <summary>
    ///     Class Trade.
    /// </summary>
    internal class Trade
    {
        /// <summary>
        ///     The _one identifier
        /// </summary>
        private readonly uint _oneId;

        /// <summary>
        ///     The _room identifier
        /// </summary>
        private readonly uint _roomId;

        /// <summary>
        ///     The _two identifier
        /// </summary>
        private readonly uint _twoId;

        /// <summary>
        ///     The _users
        /// </summary>
        private readonly TradeUser[] _users;

        /// <summary>
        ///     The _trade stage
        /// </summary>
        private int _tradeStage;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Trade" /> class.
        /// </summary>
        /// <param name="userOneId">The user one identifier.</param>
        /// <param name="userTwoId">The user two identifier.</param>
        /// <param name="roomId">The room identifier.</param>
        internal Trade(uint userOneId, uint userTwoId, uint roomId)
        {
            _oneId = userOneId;
            _twoId = userTwoId;
            _users = new TradeUser[2];
            _users[0] = new TradeUser(userOneId, roomId);
            _users[1] = new TradeUser(userTwoId, roomId);
            _tradeStage = 1;
            _roomId = roomId;
            /* TODO CHECK */ 
        }

        public async Task Start()
        {
            var users = _users;
            foreach (var tradeUser in users)
            {
                if (!tradeUser.GetRoomUser().Statusses.ContainsKey("trd"))
                {
                    tradeUser.GetRoomUser().AddStatus("trd", "");
                    tradeUser.GetRoomUser().UpdateNeeded = true;
                }
            }

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("TradeStartMessageComposer"));
            serverMessage.AppendInteger(_oneId);
            serverMessage.AppendInteger(1);
            serverMessage.AppendInteger(_twoId);
            serverMessage.AppendInteger(1);
            await SendMessageToUsers(serverMessage);
        }

        /// <summary>
        ///     Gets a value indicating whether [all users accepted].
        /// </summary>
        /// <value><c>true</c> if [all users accepted]; otherwise, <c>false</c>.</value>
        internal bool AllUsersAccepted
        {
            get
            {
                {
                    return _users.All(t => t == null || t.HasAccepted);
                }
            }
        }

        /// <summary>
        ///     Determines whether the specified identifier contains user.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns><c>true</c> if the specified identifier contains user; otherwise, <c>false</c>.</returns>
        internal bool ContainsUser(uint id)
        {
            {
                return _users.Any(t => t != null && t.UserId == id);
            }
        }

        /// <summary>
        ///     Gets the trade user.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>TradeUser.</returns>
        internal TradeUser GetTradeUser(uint id)
        {
            return _users.FirstOrDefault(t => t != null && t.UserId == id);
        }

        /// <summary>
        ///     Offers the item.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="item">The item.</param>
        internal async Task OfferItem(uint userId, UserItem item)
        {
            var tradeUser = GetTradeUser(userId);
            if (tradeUser == null || item == null || !item.BaseItem.AllowTrade || tradeUser.HasAccepted ||
                _tradeStage != 1)
            {
                return;
            }
            ClearAccepted();
            if (!tradeUser.OfferedItems.Contains(item))
            {
                tradeUser.OfferedItems.Add(item);
            }
//            UpdateTradeWindow();
        }

        /// <summary>
        ///     Takes the back item.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="item">The item.</param>
        internal async Task TakeBackItem(uint userId, UserItem item)
        {
            var tradeUser = GetTradeUser(userId);
            if (tradeUser == null || item == null || tradeUser.HasAccepted || _tradeStage != 1)
            {
                return;
            }
            ClearAccepted();
            tradeUser.OfferedItems.Remove(item);
            await UpdateTradeWindow();
        }

        /// <summary>
        ///     Accepts the specified user identifier.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        internal async Task Accept(uint userId)
        {
            var tradeUser = GetTradeUser(userId);
            if (tradeUser == null || _tradeStage != 1)
            {
                return;
            }
            tradeUser.HasAccepted = true;
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("TradeAcceptMessageComposer"));
            await serverMessage.AppendIntegerAsync(userId);
            await serverMessage.AppendIntegerAsync(1);
            await SendMessageToUsers(serverMessage);
            if (!AllUsersAccepted)
            {
                return;
            }
            await SendMessageToUsers(new ServerMessage(LibraryParser.OutgoingRequest("TradeConfirmationMessageComposer")));
            _tradeStage++;
            ClearAccepted();
        }

        /// <summary>
        ///     Unaccepts the specified user identifier.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        internal async Task Unaccept(uint userId)
        {
            var tradeUser = GetTradeUser(userId);
            if (tradeUser == null || _tradeStage != 1 || AllUsersAccepted)
            {
                return;
            }
            tradeUser.HasAccepted = false;
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("TradeAcceptMessageComposer"));
            await serverMessage.AppendIntegerAsync(userId);
            await serverMessage.AppendIntegerAsync(0);
            await SendMessageToUsers(serverMessage);
        }

        /// <summary>
        ///     Completes the trade.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        internal async Task CompleteTrade(uint userId)
        {
            var tradeUser = GetTradeUser(userId);
            if (tradeUser == null || _tradeStage != 2)
            {
                return;
            }
            tradeUser.HasAccepted = true;
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("TradeAcceptMessageComposer"));
            await serverMessage.AppendIntegerAsync(userId);
            await serverMessage.AppendIntegerAsync(1);
            await SendMessageToUsers(serverMessage);
            if (!AllUsersAccepted)
            {
                return;
            }
            _tradeStage = 999;
            Finnito();
        }

        /// <summary>
        ///     Clears the accepted.
        /// </summary>
        internal void ClearAccepted()
        {
            var users = _users;
            /* TODO CHECK */ foreach (var tradeUser in users)
            {
                tradeUser.HasAccepted = false;
            }
        }

        /// <summary>
        ///     Updates the trade window.
        /// </summary>
        internal async Task UpdateTradeWindow()
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("TradeUpdateMessageComposer"));
            var firstUser = _users.First();
            var secondUser = _users.Last();

            await serverMessage.AppendIntegerAsync(firstUser.UserId);
            await serverMessage.AppendIntegerAsync(firstUser.OfferedItems.Count);
            /* TODO CHECK */ foreach (var current in firstUser.OfferedItems)
            {
                await serverMessage.AppendIntegerAsync(current.VirtualId);
                await serverMessage.AppendStringAsync(current.BaseItem.Type.ToString().ToLower());
                await serverMessage.AppendIntegerAsync(current.VirtualId);
                await serverMessage.AppendIntegerAsync(current.BaseItem.SpriteId);
                await serverMessage.AppendIntegerAsync(0);
                serverMessage.AppendBool(true);
                await serverMessage.AppendIntegerAsync(0);
                await serverMessage.AppendStringAsync("");
                await serverMessage.AppendIntegerAsync(0);
                await serverMessage.AppendIntegerAsync(0);
                await serverMessage.AppendIntegerAsync(0);
                if (current.BaseItem.Type == 's')
                {
                    await serverMessage.AppendIntegerAsync(0);
                }
            }

            await serverMessage.AppendIntegerAsync(0);
            await serverMessage.AppendIntegerAsync(0);
            await serverMessage.AppendIntegerAsync(1);
            await serverMessage.AppendIntegerAsync(secondUser.OfferedItems.Count);
            /* TODO CHECK */ foreach (var current in secondUser.OfferedItems)
            {
                await serverMessage.AppendIntegerAsync(current.VirtualId);
                await serverMessage.AppendStringAsync(current.BaseItem.Type.ToString().ToLower());
                await serverMessage.AppendIntegerAsync(current.VirtualId);
                await serverMessage.AppendIntegerAsync(current.BaseItem.SpriteId);
                await serverMessage.AppendIntegerAsync(0);
                serverMessage.AppendBool(true);

                await serverMessage.AppendIntegerAsync(0);
                await serverMessage.AppendStringAsync("");

                await serverMessage.AppendIntegerAsync(0);
                await serverMessage.AppendIntegerAsync(0);
                await serverMessage.AppendIntegerAsync(0);
                if (current.BaseItem.Type == 's')
                {
                    await serverMessage.AppendIntegerAsync(0);
                }
            }

            await serverMessage.AppendIntegerAsync(0);
            await serverMessage.AppendIntegerAsync(0);

            await SendMessageToUsers(serverMessage);
        }

        /// <summary>
        ///     Delivers the items.
        /// </summary>
        internal async Task DeliverItems()
        {
            var userOne = GetTradeUser(_oneId);
            var userTwo = GetTradeUser(_twoId);
            if (userOne?.GetClient()?.GetHabbo() == null || userTwo?.GetClient()?.GetHabbo() == null) return;
            var offeredItems = userOne.OfferedItems;
            var offeredItems2 = userTwo.OfferedItems;
            foreach (var current in offeredItems)
            {
                if (userOne.GetClient().GetHabbo().GetInventoryComponent().GetItem(current.Id) == null)
                {
                    await userOne.GetClient().SendNotif("El tradeo ha fallado.");
                    await userTwo.GetClient().SendNotif("El tradeo ha fallado.");
                    return;
                }
            }

            foreach (var current2 in offeredItems2)
            {
                if (userTwo.GetClient().GetHabbo().GetInventoryComponent().GetItem(current2.Id) == null)
                {
                    await userOne.GetClient().SendNotif("El tradeo ha fallado.");
                    await userTwo.GetClient().SendNotif("El tradeo ha fallado.");
                    return;
                }
            }

            await userTwo.GetClient().GetHabbo().GetInventoryComponent().RunDbUpdate();
            await userOne.GetClient().GetHabbo().GetInventoryComponent().RunDbUpdate();
            /* TODO CHECK */ foreach (var current3 in offeredItems)
            {
                await userOne.GetClient().GetHabbo().GetInventoryComponent().RemoveItem(current3.Id, false, 0);
               await userTwo
                    .GetClient()
                    .GetHabbo()
                    .GetInventoryComponent()
                    .AddNewItem(current3.Id, current3.BaseItem.ItemId, current3.ExtraData, current3.GroupId, false, false, 0,
                        0,
                        current3.SongCode);
            }

           foreach (var current4 in offeredItems2)
            {
                await userTwo.GetClient().GetHabbo().GetInventoryComponent().RemoveItem(current4.Id, false, 0);
                await userOne
                    .GetClient()
                    .GetHabbo()
                    .GetInventoryComponent()
                    .AddNewItem(current4.Id, current4.BaseItem.ItemId, current4.ExtraData, current4.GroupId, false, false, 0,
                        0,
                        current4.SongCode);
            }

            await userOne.GetClient().GetHabbo().GetInventoryComponent().RunDbUpdate();
            await userTwo.GetClient().GetHabbo().GetInventoryComponent().RunDbUpdate();
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("NewInventoryObjectMessageComposer"));
            await serverMessage.AppendIntegerAsync(1);
            var i = 1;
            foreach (var current5 in offeredItems)
            {
                if (current5.BaseItem.Type.ToString().ToLower() != "s")
                {
                    i = 2;
                    break;
                }
            }

            await serverMessage.AppendIntegerAsync(i);
            await serverMessage.AppendIntegerAsync(offeredItems.Count);
            /* TODO CHECK */ foreach (var current6 in offeredItems)
            {
                await serverMessage.AppendIntegerAsync(current6.VirtualId);
            }
            await userTwo.GetClient().SendMessageAsync(serverMessage);
            var serverMessage2 = new ServerMessage(LibraryParser.OutgoingRequest("NewInventoryObjectMessageComposer"));
            await serverMessage2.AppendIntegerAsync(1);
            i = 1;
            if (offeredItems2.Any(current7 => current7.BaseItem.Type.ToString().ToLower() != "s"))
            {
                i = 2;
            }
            await serverMessage2.AppendIntegerAsync(i);
            await serverMessage2.AppendIntegerAsync(offeredItems2.Count);
            /* TODO CHECK */ foreach (var current8 in offeredItems2)
            {
                await serverMessage2.AppendIntegerAsync(current8.VirtualId);
            }
            await userOne.GetClient().SendMessageAsync(serverMessage2);
            await userOne.GetClient().GetHabbo().GetInventoryComponent().UpdateItems(false);
            await userTwo.GetClient().GetHabbo().GetInventoryComponent().UpdateItems(false);
        }

        /// <summary>
        ///     Closes the trade clean.
        /// </summary>
        internal async Task CloseTradeClean()
        {
            {
                /* TODO CHECK */
                foreach (var tradeUser in _users)
                {
                    if (tradeUser?.GetRoomUser() != null)
                    {
                        tradeUser.GetRoomUser().RemoveStatus("trd");
                        tradeUser.GetRoomUser().UpdateNeeded = true;
                    }
                }

                await SendMessageToUsers(new ServerMessage(LibraryParser.OutgoingRequest("TradeCompletedMessageComposer")));
                GetRoom().ActiveTrades.Remove(this);
            }
        }

        /// <summary>
        ///     Closes the trade.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        internal async Task CloseTrade(uint userId)
        {
            {
                /* TODO CHECK */ foreach (
                    var tradeUser in _users.Where(tradeUser => tradeUser?.GetRoomUser() != null))
                {
                    tradeUser.GetRoomUser().RemoveStatus("trd");
                    tradeUser.GetRoomUser().UpdateNeeded = true;
                }
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("TradeCloseMessageComposer"));
                await serverMessage.AppendIntegerAsync(userId);
                await serverMessage.AppendIntegerAsync(0);
                await SendMessageToUsers(serverMessage);
            }
        }

        /// <summary>
        ///     Sends the message to users.
        /// </summary>
        /// <param name="message">The message.</param>
        internal async Task SendMessageToUsers(ServerMessage message)
        {
            if (_users == null)
            {
                return;
            }

            foreach (var tradeUser in _users)
            {
                if (tradeUser?.GetClient() != null)
                {
                    await tradeUser.GetClient().SendMessageAsync(message);
                }
            }
        }

        /// <summary>
        ///     Finnitoes this instance.
        /// </summary>
        private async void Finnito()
        {
            try
            {
                await Task.Run(async () =>
                {
                    await DeliverItems();
                });
                await CloseTradeClean();

            }
            catch (Exception ex)
            {
                Logging.LogThreadException(ex.ToString(), "Trade task");
            }
        }

        /// <summary>
        ///     Gets the room.
        /// </summary>
        /// <returns>Room.</returns>
        private Room GetRoom() => Oblivion.GetGame().GetRoomManager().GetRoom(_roomId);
    }
}