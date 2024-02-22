﻿using System.Linq;
using System.Threading.Tasks;
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

        public override async Task<bool> Execute(GameClient client, string[] pms)
        {
            var currentRoom = client.GetHabbo().CurrentRoom;
            var user = client.GetHabbo();
            var roomOwner = currentRoom.GetRoomUserManager().GetRoomUserByHabbo((uint) currentRoom.RoomData.OwnerId);

            if (user == null || roomOwner == null)
            {
                await client.SendWhisperAsync("O dono deve estar na sala para ela ser vendida!");
                return false;
            }

            if (!currentRoom.RoomData.RoomForSale)
            {
                await client.SendWhisperAsync("A sala não está à venda!");
                return false;
            }


            if (currentRoom.RoomData.OwnerId == user.Id)
            {
                await client.SendWhisperAsync("Não compre sua própria sala!");
                return false;
            }
            var cost = currentRoom.RoomData.RoomSaleCost;
            var type = currentRoom.RoomData.RoomSaleType;
            if ((type == "d" && cost <= user.Diamonds) || type == "c" && cost <= user.Credits)
            {
                using (var Adapter = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                {
                    Adapter.SetQuery("UPDATE rooms_data SET owner = @newowner WHERE id = @roomid");
                    Adapter.AddParameter("newowner", user.UserName);
                    Adapter.AddParameter("roomid", currentRoom.RoomId);
                    await Adapter.RunQueryAsync();

                    Adapter.SetNoLockQuery("UPDATE items_rooms SET user_id = @newowner WHERE room_id = @roomid;");
                    Adapter.AddParameter("newowner", user.Id);
                    Adapter.AddParameter("roomid", currentRoom.RoomId);
                    await Adapter.RunQueryAsync();

                    await Adapter.RunFastQueryAsync($"DELETE FROM rooms_rights WHERE room_id = '{currentRoom.RoomId}'");
                    await Adapter.RunFastQueryAsync($"UPDATE bots SET room_id = NULL WHERE room_id = '{currentRoom.RoomId}'");
                }

                if (currentRoom.RoomData.Group != null)
                {
                    await Oblivion.GetGame().GetGroupManager().DeleteGroup(currentRoom.RoomData.Group.Id);
                }
                //Change Room Owners
                currentRoom.RoomData.OwnerId = (int)user.Id;
                currentRoom.RoomData.Owner = user.UserName;

            

                //Take Credits or Diamonds from User
                if (type == "c")
                {
                    user.Credits -= cost;
                    await user.UpdateCreditsBalance();

                }
                else
                {
                    user.Diamonds -= cost;
                    await user.UpdateSeasonalCurrencyBalance();
                }


                //Give Credits or Diamonds to User
                if (type == "c")
                {
                    roomOwner.GetClient().GetHabbo().Credits += cost;
                    await roomOwner.GetClient().GetHabbo().UpdateCreditsBalance();
                }
                else 
                {
                    roomOwner.GetClient().GetHabbo().Diamonds += cost;
                    await roomOwner.GetClient().GetHabbo().UpdateSeasonalCurrencyBalance();
                }

                currentRoom.RoomData.RoomForSale = false;
                currentRoom.RoomData.RoomSaleCost = 0;
                currentRoom.RoomData.RoomSaleType = "";
                using (var dbClient = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                {
                    await currentRoom.GetRoomItemHandler().SaveFurniture(dbClient);

                }
                await Oblivion.GetGame().GetRoomManager().UnloadRoom(currentRoom, "purchase");
               
            }
            else
            {
                await client.SendWhisperAsync($"O preço é {cost}{type}");
            }
            return true;
        }

    }
}