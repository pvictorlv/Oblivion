using System;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Handlers;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.HabboHotel.Rooms.User.Path;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorCrackableEgg : FurniInteractorModel
    {
        public override void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            RoomUser roomUser = null;
            if (session != null)
                roomUser = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);

            if (roomUser == null)
                return;

            if (Gamemap.TilesTouching(item.X, item.Y, roomUser.X, roomUser.Y))
            {
                var cracks = 0;

                if (Oblivion.IsNum(item.ExtraData))
                    cracks = Convert.ToInt16(item.ExtraData);

                cracks++;
                item.ExtraData = Convert.ToString(cracks);
                item.UpdateState(false, true);

            }
            else
            {

                roomUser.MoveTo(item.SquareInFront);

            }
            var room = item.GetRoom();

            var crackableHandler = Oblivion.GetGame().GetCrackableEggHandler();
            var maxCracks = crackableHandler.MaxCracks(item.GetBaseItem().Name);
            var itemData = Convert.ToInt16(item.ExtraData);

            if (itemData >= maxCracks)
            {
                var prize = crackableHandler.GetRandomPrize(maxCracks);
                if (prize == 0) return;
                room.GetRoomItemHandler().DeleteRoomItem(item);
                session.GetHabbo().GetInventoryComponent().AddNewItem(0, prize, "", 0, true, false, 0, 0);
                session.GetHabbo().GetInventoryComponent().UpdateItems(true);
            }
        }
    }
}