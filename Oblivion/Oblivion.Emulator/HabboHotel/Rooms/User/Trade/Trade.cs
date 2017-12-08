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
            var users = _users;
            /* TODO CHECK */ foreach (var tradeUser in users.Where(tradeUser => !tradeUser.GetRoomUser().Statusses.ContainsKey("trd")))
            {
                tradeUser.GetRoomUser().AddStatus("trd", "");
                tradeUser.GetRoomUser().UpdateNeeded = true;
            }
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("TradeStartMessageComposer"));
            serverMessage.AppendInteger(userOneId);
            serverMessage.AppendInteger(1);
            serverMessage.AppendInteger(userTwoId);
            serverMessage.AppendInteger(1);
            SendMessageToUsers(serverMessage);
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
        internal void OfferItem(uint userId, UserItem item)
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
        internal void TakeBackItem(uint userId, UserItem item)
        {
            var tradeUser = GetTradeUser(userId);
            if (tradeUser == null || item == null || tradeUser.HasAccepted || _tradeStage != 1)
            {
                return;
            }
            ClearAccepted();
            tradeUser.OfferedItems.Remove(item);
            UpdateTradeWindow();
        }

        /// <summary>
        ///     Accepts the specified user identifier.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        internal void Accept(uint userId)
        {
            var tradeUser = GetTradeUser(userId);
            if (tradeUser == null || _tradeStage != 1)
            {
                return;
            }
            tradeUser.HasAccepted = true;
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("TradeAcceptMessageComposer"));
            serverMessage.AppendInteger(userId);
            serverMessage.AppendInteger(1);
            SendMessageToUsers(serverMessage);
            if (!AllUsersAccepted)
            {
                return;
            }
            SendMessageToUsers(new ServerMessage(LibraryParser.OutgoingRequest("TradeConfirmationMessageComposer")));
            _tradeStage++;
            ClearAccepted();
        }

        /// <summary>
        ///     Unaccepts the specified user identifier.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        internal void Unaccept(uint userId)
        {
            var tradeUser = GetTradeUser(userId);
            if (tradeUser == null || _tradeStage != 1 || AllUsersAccepted)
            {
                return;
            }
            tradeUser.HasAccepted = false;
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("TradeAcceptMessageComposer"));
            serverMessage.AppendInteger(userId);
            serverMessage.AppendInteger(0);
            SendMessageToUsers(serverMessage);
        }

        /// <summary>
        ///     Completes the trade.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        internal void CompleteTrade(uint userId)
        {
            var tradeUser = GetTradeUser(userId);
            if (tradeUser == null || _tradeStage != 2)
            {
                return;
            }
            tradeUser.HasAccepted = true;
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("TradeAcceptMessageComposer"));
            serverMessage.AppendInteger(userId);
            serverMessage.AppendInteger(1);
            SendMessageToUsers(serverMessage);
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
        internal void UpdateTradeWindow()
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("TradeUpdateMessageComposer"));
            var firstUser = _users.First();
            var secondUser = _users.Last();

            serverMessage.AppendInteger(firstUser.UserId);
            serverMessage.AppendInteger(firstUser.OfferedItems.Count);
            /* TODO CHECK */ foreach (var current in firstUser.OfferedItems)
            {
                serverMessage.AppendInteger(current.VirtualId);
                serverMessage.AppendString(current.BaseItem.Type.ToString().ToLower());
                serverMessage.AppendInteger(current.VirtualId);
                serverMessage.AppendInteger(current.BaseItem.SpriteId);
                serverMessage.AppendInteger(0);
                serverMessage.AppendBool(true);
                serverMessage.AppendInteger(0);
                serverMessage.AppendString("");
                serverMessage.AppendInteger(0);
                serverMessage.AppendInteger(0);
                serverMessage.AppendInteger(0);
                if (current.BaseItem.Type == 's')
                {
                    serverMessage.AppendInteger(0);
                }
            }

            serverMessage.AppendInteger(0);
            serverMessage.AppendInteger(0);
            serverMessage.AppendInteger(1);
            serverMessage.AppendInteger(secondUser.OfferedItems.Count);
            /* TODO CHECK */ foreach (var current in secondUser.OfferedItems)
            {
                serverMessage.AppendInteger(current.VirtualId);
                serverMessage.AppendString(current.BaseItem.Type.ToString().ToLower());
                serverMessage.AppendInteger(current.VirtualId);
                serverMessage.AppendInteger(current.BaseItem.SpriteId);
                serverMessage.AppendInteger(0);
                serverMessage.AppendBool(true);

                serverMessage.AppendInteger(0);
                serverMessage.AppendString("");

                serverMessage.AppendInteger(0);
                serverMessage.AppendInteger(0);
                serverMessage.AppendInteger(0);
                if (current.BaseItem.Type == 's')
                {
                    serverMessage.AppendInteger(0);
                }
            }

            serverMessage.AppendInteger(0);
            serverMessage.AppendInteger(0);

            SendMessageToUsers(serverMessage);
        }

        /// <summary>
        ///     Delivers the items.
        /// </summary>
        internal void DeliverItems()
        {
            var userOne = GetTradeUser(_oneId);
            var userTwo = GetTradeUser(_twoId);
            if (userOne?.GetClient()?.GetHabbo() == null || userTwo?.GetClient()?.GetHabbo() == null) return;
            var offeredItems = userOne.OfferedItems;
            var offeredItems2 = userTwo.OfferedItems;
            if (offeredItems.Any(current => userOne.GetClient().GetHabbo().GetInventoryComponent().GetItem(current.Id) == null))
            {
                userOne.GetClient().SendNotif("El tradeo ha fallado.");
                userTwo.GetClient().SendNotif("El tradeo ha fallado.");
                return;
            }
            if (
                offeredItems2.Any(
                    current2 =>
                        userTwo.GetClient().GetHabbo().GetInventoryComponent().GetItem(current2.Id) == null))
            {
                userOne.GetClient().SendNotif("El tradeo ha fallado.");
                userTwo.GetClient().SendNotif("El tradeo ha fallado.");
                return;
            }
            userTwo.GetClient().GetHabbo().GetInventoryComponent().RunDbUpdate();
            userOne.GetClient().GetHabbo().GetInventoryComponent().RunDbUpdate();
            /* TODO CHECK */ foreach (var current3 in offeredItems)
            {
                userOne.GetClient().GetHabbo().GetInventoryComponent().RemoveItem(current3.Id, false);
                userTwo
                    .GetClient()
                    .GetHabbo()
                    .GetInventoryComponent()
                    .AddNewItem(current3.Id, current3.BaseItemId, current3.ExtraData, current3.GroupId, false, false, 0,
                        0,
                        current3.SongCode);
                userOne.GetClient().GetHabbo().GetInventoryComponent().RunDbUpdate();
                userTwo.GetClient().GetHabbo().GetInventoryComponent().RunDbUpdate();
            }
           foreach (var current4 in offeredItems2)
            {
                userTwo.GetClient().GetHabbo().GetInventoryComponent().RemoveItem(current4.Id, false);
                userOne
                    .GetClient()
                    .GetHabbo()
                    .GetInventoryComponent()
                    .AddNewItem(current4.Id, current4.BaseItemId, current4.ExtraData, current4.GroupId, false, false, 0,
                        0,
                        current4.SongCode);
                userTwo.GetClient().GetHabbo().GetInventoryComponent().RunDbUpdate();
                userOne.GetClient().GetHabbo().GetInventoryComponent().RunDbUpdate();
            }
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("NewInventoryObjectMessageComposer"));
            serverMessage.AppendInteger(1);
            var i = 1;
            if (offeredItems.Any(current5 => current5.BaseItem.Type.ToString().ToLower() != "s"))
            {
                i = 2;
            }
            serverMessage.AppendInteger(i);
            serverMessage.AppendInteger(offeredItems.Count);
            /* TODO CHECK */ foreach (var current6 in offeredItems)
            {
                serverMessage.AppendInteger(current6.VirtualId);
            }
            userTwo.GetClient().SendMessage(serverMessage);
            var serverMessage2 = new ServerMessage(LibraryParser.OutgoingRequest("NewInventoryObjectMessageComposer"));
            serverMessage2.AppendInteger(1);
            i = 1;
            if (offeredItems2.Any(current7 => current7.BaseItem.Type.ToString().ToLower() != "s"))
            {
                i = 2;
            }
            serverMessage2.AppendInteger(i);
            serverMessage2.AppendInteger(offeredItems2.Count);
            /* TODO CHECK */ foreach (var current8 in offeredItems2)
            {
                serverMessage2.AppendInteger(current8.VirtualId);
            }
            userOne.GetClient().SendMessage(serverMessage2);
            userOne.GetClient().GetHabbo().GetInventoryComponent().UpdateItems(false);
            userTwo.GetClient().GetHabbo().GetInventoryComponent().UpdateItems(false);
        }

        /// <summary>
        ///     Closes the trade clean.
        /// </summary>
        internal void CloseTradeClean()
        {
            {
                /* TODO CHECK */ foreach (
                    var tradeUser in _users.Where(tradeUser => tradeUser?.GetRoomUser() != null))
                {
                    tradeUser.GetRoomUser().RemoveStatus("trd");
                    tradeUser.GetRoomUser().UpdateNeeded = true;
                }
                SendMessageToUsers(new ServerMessage(LibraryParser.OutgoingRequest("TradeCompletedMessageComposer")));
                GetRoom().ActiveTrades.Remove(this);
            }
        }

        /// <summary>
        ///     Closes the trade.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        internal void CloseTrade(uint userId)
        {
            {
                /* TODO CHECK */ foreach (
                    var tradeUser in _users.Where(tradeUser => tradeUser?.GetRoomUser() != null))
                {
                    tradeUser.GetRoomUser().RemoveStatus("trd");
                    tradeUser.GetRoomUser().UpdateNeeded = true;
                }
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("TradeCloseMessageComposer"));
                serverMessage.AppendInteger(userId);
                serverMessage.AppendInteger(0);
                SendMessageToUsers(serverMessage);
            }
        }

        /// <summary>
        ///     Sends the message to users.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SendMessageToUsers(ServerMessage message)
        {
            if (_users == null)
            {
                return;
            }

            {
                /* TODO CHECK */ foreach (var tradeUser in _users.Where(tradeUser => tradeUser?.GetClient() != null))
                {
                    tradeUser.GetClient().SendMessage(message);
                }
            }
        }

        /// <summary>
        ///     Finnitoes this instance.
        /// </summary>
        private void Finnito()
        {
            try
            {
                Task.Factory.StartNew(() =>
                {
                    DeliverItems();
                    CloseTradeClean();
                });
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