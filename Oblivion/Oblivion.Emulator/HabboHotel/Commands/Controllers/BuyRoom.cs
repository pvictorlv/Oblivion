using System.Linq;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class About. This class cannot be inherited.
    /// </summary>
    internal sealed class BuyRoom : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="About" /> class.
        /// </summary>
        public BuyRoom()
        {
            MinRank = 1;
            Description = "Compre essa sala";
            Usage = ":buyroom";
            MinParams = 0;
        }

        public override bool Execute(GameClient client, string[] pms)
        {
            var currentRoom = client.GetHabbo().CurrentRoom;
            var user = client.GetHabbo();
            var roomOwner = currentRoom.GetRoomUserManager().GetRoomUserByHabbo((uint) currentRoom.RoomData.OwnerId);

            if (user == null || roomOwner == null)
            {
                client.SendWhisper("O dono deve estar na sala para ela ser vendida!");
                return false;
            }

            if (!currentRoom.RoomData.RoomForSale)
            {
                client.SendWhisper("A sala não está à venda!");
                return false;
            }


            if (currentRoom.RoomData.OwnerId == user.Id)
            {
                client.SendWhisper("Não compre sua própria sala!");
                return false;
            }
            var cost = currentRoom.RoomData.RoomSaleCost;
            var type = currentRoom.RoomData.RoomSaleType;
            if ((type == "d" && cost <= user.Diamonds) || type == "c" && cost <= user.Credits)
            {
                using (var Adapter = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    Adapter.SetQuery("UPDATE rooms_data SET owner = @newowner WHERE id = @roomid");
                    Adapter.AddParameter("newowner", user.UserName);
                    Adapter.AddParameter("roomid", currentRoom.RoomId);
                    Adapter.RunQuery();

                    Adapter.SetNoLockQuery("UPDATE items_rooms SET user_id = @newowner WHERE room_id = @roomid");
                    Adapter.AddParameter("newowner", user.Id);
                    Adapter.AddParameter("roomid", currentRoom.RoomId);
                    Adapter.RunQuery();

                    Adapter.RunFastQuery($"DELETE FROM rooms_rights WHERE room_id = '{currentRoom.RoomId}'");
                    Adapter.RunFastQuery($"UPDATE bots SET room_id = '0' WHERE room_id = '{currentRoom.RoomId}'");
                }

                if (currentRoom.RoomData.Group != null)
                {
                    Oblivion.GetGame().GetGroupManager().DeleteGroup(currentRoom.RoomData.Group.Id);
                }
                //Change Room Owners
                currentRoom.RoomData.OwnerId = (int)user.Id;
                currentRoom.RoomData.Owner = user.UserName;

                //Change Item Owners
                /* TODO CHECK */
                foreach (var CurrentItem in currentRoom.GetRoomItemHandler().GetWallAndFloor)
                {
                    CurrentItem.UserId = user.Id;
                }

                //Take Credits or Diamonds from User
                if (type == "c")
                {
                    user.Credits -= cost;
                    user.UpdateCreditsBalance();

                }
                else
                {
                    user.Diamonds -= cost;
                    user.UpdateSeasonalCurrencyBalance();
                }


                //Give Credits or Diamonds to User
                if (type == "c")
                {
                    roomOwner.GetClient().GetHabbo().Credits += cost;
                    roomOwner.GetClient().GetHabbo().UpdateCreditsBalance();
                }
                else 
                {
                    roomOwner.GetClient().GetHabbo().Diamonds += cost;
                    roomOwner.GetClient().GetHabbo().UpdateSeasonalCurrencyBalance();
                }

                currentRoom.RoomData.RoomForSale = false;
                currentRoom.RoomData.RoomSaleCost = 0;
                currentRoom.RoomData.RoomSaleType = "";

              
                var UsersToReturn = currentRoom.GetRoomUserManager().GetRoomUsers();
                Oblivion.GetGame().GetRoomManager().UnloadRoom(currentRoom, "purchase");
                /* TODO CHECK */ foreach (var User in UsersToReturn.Where(User => User?.GetClient() != null))
                {
                    var forwardToRoom = new ServerMessage(LibraryParser.OutgoingRequest("RoomForwardMessageComposer"));
                    forwardToRoom.AppendInteger(currentRoom.RoomId);
                    User.GetClient().SendMessage(forwardToRoom);
                }
            }
            else
            {
                client.SendWhisper($"O preço é {cost}{type}");
            }
            return true;
        }

    }
}