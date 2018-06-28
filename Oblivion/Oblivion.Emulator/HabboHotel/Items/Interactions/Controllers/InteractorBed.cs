using System;
using System.Globalization;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.Util;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorBed : FurniInteractorModel
    {
        public override void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
            user.Statusses["lay"] = TextHandling.GetString(item.GetBaseItem().Height);

            user.Z = item.Z;
            user.RotHead = item.Rot;
            user.RotBody = item.Rot;
            user.UpdateNeeded = true;

            if (item.GetBaseItem().InteractionType == Interaction.PressurePadBed)
            {
                item.ExtraData = "1";
                item.UpdateState();
            }
        }
    }
}