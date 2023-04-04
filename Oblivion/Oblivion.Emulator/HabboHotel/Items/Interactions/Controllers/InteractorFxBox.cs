using System;
using System.Threading;
using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.PathFinding;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorFxBox : FurniInteractorModel
    {
        public override async Task OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (!hasRights)
                return ;

            RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);

            if (user == null)
                return ;

            Room room = session.GetHabbo().CurrentRoom;

            if (room == null)
                return ;

            int effectId = Convert.ToInt32(item.GetBaseItem().Name.Replace("fxbox_fx", ""));

            try
            {
                while (PathFinder.GetDistance(user.X, user.Y, item.X, item.Y) > 1)
                {
                    if (user.RotBody == 0)
                        await user.MoveTo(item.X, item.Y + 1);
                    else if (user.RotBody == 2)
                        await user.MoveTo(item.X - 1, item.Y);
                    else if (user.RotBody == 4)
                        await user.MoveTo(item.X, item.Y - 1);
                    else if(user.RotBody == 6)
                        await user.MoveTo(item.X + 1, item.Y);
                    else
                        await user.MoveTo(item.X, item.Y + 1); // Diagonal user...
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                if (PathFinder.GetDistance(user.X, user.Y, item.X, item.Y) == 1)
                {
                    await session.GetHabbo().GetAvatarEffectsInventoryComponent().AddNewEffect(effectId, -1, 0);
                    await session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(effectId);

                    await Task.Delay(500);
                    
                    await room.GetRoomItemHandler().RemoveFurniture(session, item.Id, false);

                    using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                        await queryReactor.RunNoLockFastQueryAsync("DELETE FROM items_rooms WHERE id = '" + item.Id + "';");
                }
            }

            return ;
        }
    }
}