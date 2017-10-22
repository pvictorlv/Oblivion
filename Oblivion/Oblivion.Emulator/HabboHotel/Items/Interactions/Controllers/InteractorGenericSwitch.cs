using System.Linq;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Quests;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorGenericSwitch : FurniInteractorModel
    {
        public override void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            var num = item.GetBaseItem().Modes - 1;

            if (session == null || !hasRights || num <= 0 || item.GetBaseItem().InteractionType == Interaction.Pinata)
                return;

            Oblivion.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.FurniSwitch);

            int.TryParse(item.ExtraData, out var num2);
            int num3;

            if (num2 <= 0)
                num3 = 1;
            else
            {
                if (num2 >= num)
                    num3 = 0;
                else
                    num3 = num2 + 1;
            }

            item.ExtraData = num3.ToString();
            item.UpdateState();
            if (item.GetRoom().GotWireds())
                item.GetRoom()
                    .GetWiredHandler()
                    .ExecuteWired(Interaction.TriggerStateChanged,
                        item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id), item);

            if (!item.GetBaseItem().StackMultipler)
                return;

            var room = item.GetRoom();

            /* TODO CHECK */ foreach (
                var current in
                room.GetRoomUserManager().UserList.Values.Where(current =>
                    current.Statusses.ContainsKey("sit") && current.X == item.X && current.Y == item.Y))
                room.GetRoomUserManager().UpdateUserStatus(current, true);
        }

        public override void OnWiredTrigger(RoomItem item)
        {
            var num = item.GetBaseItem().Modes - 1;

            if (num == 0)
                return;

            if (!int.TryParse(item.ExtraData, out var num2))
                return;

            int num3;

            if (num2 <= 0)
                num3 = 1;
            else
            {
                if (num2 >= num)
                    num3 = 0;
                else
                    num3 = num2 + 1;
            }

            item.ExtraData = num3.ToString();
            item.UpdateState();

            if (!item.GetBaseItem().StackMultipler)
                return;

            var room = item.GetRoom();

            /* TODO CHECK */ foreach (
                var current in
                room.GetRoomUserManager().UserList.Values.Where(current =>
                    current.Statusses.ContainsKey("sit") && (current.X == item.X && current.Y == item.Y)))
                room.GetRoomUserManager().UpdateUserStatus(current, true);
        }
    }
}