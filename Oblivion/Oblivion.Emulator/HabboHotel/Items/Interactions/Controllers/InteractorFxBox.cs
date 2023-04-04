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
        public override Task OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (!hasRights)
                return Task.CompletedTask;

            RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);

            if (user == null)
                return Task.CompletedTask;

            Room room = session.GetHabbo().CurrentRoom;

            if (room == null)
                return Task.CompletedTask;

            int effectId = Convert.ToInt32(item.GetBaseItem().Name.Replace("fxbox_fx", ""));

            try
            {
                while (PathFinder.GetDistance(user.X, user.Y, item.X, item.Y) > 1)
                {
                    if (user.RotBody == 0)
                        user.MoveTo(item.X, item.Y + 1);
                    else if (user.RotBody == 2)
                        user.MoveTo(item.X - 1, item.Y);
                    else if (user.RotBody == 4)
                        user.MoveTo(item.X, item.Y - 1);
                    else if(user.RotBody == 6)
                        user.MoveTo(item.X + 1, item.Y);
                    else
                        user.MoveTo(item.X, item.Y + 1); // Diagonal user...
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                if (PathFinder.GetDistance(user.X, user.Y, item.X, item.Y) == 1)
                {
                    session.GetHabbo().GetAvatarEffectsInventoryComponent().AddNewEffect(effectId, -1, 0);
                    session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(effectId);

                    Thread.Sleep(500); //Wait 0.5 second until remove furniture. (Delay)

                    room.GetRoomItemHandler().RemoveFurniture(session, item.Id, false);

                    using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                        queryReactor.RunNoLockFastQuery("DELETE FROM items_rooms WHERE id = '" + item.Id + "';");
                }
            }

            return Task.CompletedTask;
        }
    }
}