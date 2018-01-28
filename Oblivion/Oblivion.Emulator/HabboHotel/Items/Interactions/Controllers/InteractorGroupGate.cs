﻿using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.PathFinding;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorGroupGate : FurniInteractorModel
    {
        public override void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
            if (session == null || item == null || user == null)
                return;

            var distance = PathFinder.GetDistance(user.X, user.Y, item.X, item.Y);

            if (distance > 0 || user.GoalX == 0 && user.GoalY == 0)
                return;
            var gp = Oblivion.GetGame().GetGroupManager().GetGroup(item.GroupId);
            if (gp == null)
            {
                return;
            }

            if (!gp.Members.ContainsKey(session.GetHabbo().Id))
            {
                user.ClearMovement();
                return;
            }

            item.ExtraData = "0";
            item.UpdateState(false, true);
            item.InteractingUser = 1;

            if (user.GoalX != item.X || user.GoalY != item.Y)
                return;

            switch (user.RotBody)
            {
                case 3:
                case 4:
                case 5:
                    user.MoveTo(item.GetRoom()
                        .GetGameMap()
                        .CanWalk(item.SquareBehind.X, item.SquareBehind.Y, user.AllowOverride)
                        ? item.SquareBehind
                        : item.SquareInFront);
                    break;

                default:
                    user.MoveTo(item.GetRoom()
                        .GetGameMap()
                        .CanWalk(item.SquareInFront.X, item.SquareInFront.Y, user.AllowOverride)
                        ? item.SquareInFront
                        : item.SquareBehind);
                    break;
            }
        }
    }
}