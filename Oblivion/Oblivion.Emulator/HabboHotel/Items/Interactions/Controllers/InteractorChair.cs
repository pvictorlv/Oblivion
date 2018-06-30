using System;
using System.Globalization;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorChair : FurniInteractorModel
    {
        public override void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
            if (!user.Statusses.ContainsKey("sit"))
            {
                if (item.GetBaseItem().StackMultipler && !string.IsNullOrWhiteSpace(item.ExtraData))
                    if (item.ExtraData != "0")
                    {
                        var num2 = Convert.ToInt32(item.ExtraData);
                        user.Statusses.TryAdd("sit",
                            item.GetBaseItem().ToggleHeight[num2].ToString(CultureInfo.InvariantCulture)
                                .Replace(',', '.'));
                    }
                    else
                    {
                        user.Statusses.TryAdd("sit",
                            Convert.ToString(item.GetBaseItem().Height, CultureInfo.InvariantCulture));
                    }
                else
                {
                    user.Statusses.TryAdd("sit",
                        Convert.ToString(item.GetBaseItem().Height, CultureInfo.InvariantCulture));
                }
            }

            if (Math.Abs(user.Z - item.Z) > 0 || user.RotBody != item.Rot)
            {
                user.Z = item.Z;
                user.RotHead = item.Rot;
                user.RotBody = item.Rot;
                user.UpdateNeeded = true;
            }
        }
    }
}