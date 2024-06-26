using System;
using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.HabboHotel.Rooms.User.Path;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorCrackableEgg : FurniInteractorModel
    {
        public override async Task OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            RoomUser roomUser = null;
            if (session?.GetHabbo() != null)
                roomUser = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (roomUser == null)
                return;
            if (!roomUser.GetRoom().CheckRights(session, true))
                return;
            if (Gamemap.TilesTouching(item.X, item.Y, roomUser.X, roomUser.Y))
            {
                var cracks = 0;

                if (Oblivion.IsNum(item.ExtraData))
                    cracks = Convert.ToInt16(item.ExtraData);

                cracks++;
                item.ExtraData = Convert.ToString(cracks);
                await item.UpdateState(false, true);
            }
            else
            {
                await roomUser.MoveTo(item.SquareInFront);
            }
            var room = item.GetRoom();

            var crackableHandler = Oblivion.GetGame().GetRandomRewardFurniHandler();
            var maxCracks = crackableHandler.MaxCracks(item.GetBaseItem().Name);
            if (!short.TryParse(item.ExtraData, out var itemData))
            {
                item.ExtraData = "0";
                return;
            }


            await Oblivion.GetGame().GetAchievementManager()
                .ProgressUserAchievement(session, "ACH_PinataWhacker", 1, true);

            if (itemData >= maxCracks)
            {
                await Oblivion.GetGame().GetAchievementManager()
                    .ProgressUserAchievement(session, "ACH_PinataBreaker", 1, true);
                var prize = crackableHandler.GetRandomPrize(0, maxCracks);
                if (prize == 0) return;
                await room.GetRoomItemHandler().DeleteRoomItem(item);
                await session.GetHabbo().GetInventoryComponent().AddNewItem("0", prize, "", 0, true, false, 0, 0);
                await session.GetHabbo().GetInventoryComponent().UpdateItems(true);
            }
        }

        public override async Task OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
            if (item.GetBaseItem().InteractionType == Interaction.Pinata)
            {
                if (!user.IsWalking || item.ExtraData.Length <= 0) return;
                var num5 = int.Parse(item.ExtraData);
                if (num5 >= 100 || user.CurrentEffect != 158) return;
                var num6 = num5 + 1;
                item.ExtraData = num6.ToString();
                await  item.UpdateState();
                await Oblivion.GetGame()
                    .GetAchievementManager()
                    .ProgressUserAchievement(user.GetClient(), "ACH_PinataWhacker", 1);
                if (num6 == 100)
                {
                    await Oblivion.GetGame().GetPinataHandler().DeliverRandomPinataItem(user, item.GetRoom(), item);
                    await Oblivion.GetGame()
                        .GetAchievementManager()
                        .ProgressUserAchievement(user.GetClient(), "ACH_PinataBreaker", 1);
                }
            }
        }
    }
}