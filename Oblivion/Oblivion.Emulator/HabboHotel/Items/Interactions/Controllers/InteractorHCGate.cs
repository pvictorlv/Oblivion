using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorHcGate : FurniInteractorModel
    {
        public override void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            var user = session.GetHabbo();
            var ishc = user.Vip;

            if (!ishc)
            {
                var message = new ServerMessage(LibraryParser.OutgoingRequest("CustomUserNotificationMessageComposer"));
                message.AppendInteger(3);
                session.SendMessage(message);
                return;
            }

            if (item?.GetBaseItem() == null || item.GetBaseItem().InteractionType != Interaction.HcGate)
                return;

            var modes = item.GetBaseItem().Modes - 1;

            if (modes <= 0)
                item.UpdateState(false, true);

            int.TryParse(item.ExtraData, out var currentMode);
            int newMode;

            if (currentMode <= 0)
                newMode = 1;
            else if (currentMode >= modes)
                newMode = 0;
            else
                newMode = currentMode + 1;

            if (newMode == 0 && !item.GetRoom().GetGameMap().ItemCanBePlacedHere(item.X, item.Y))
                return;

            item.ExtraData = newMode.ToString();
            item.UpdateState();
            item.GetRoom().GetGameMap().UpdateMapForItem(item);
            if (item.GetRoom().GotWireds())
                item.GetRoom()
                    .GetWiredHandler()
                    .ExecuteWired(Interaction.TriggerStateChanged,
                        item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id), item);
        }

        public override void OnWiredTrigger(RoomItem item)
        {
            var num = item.GetBaseItem().Modes - 1;

            if (num <= 0)
                item.UpdateState(false, true);

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

            if (num3 == 0 && !item.GetRoom().GetGameMap().ItemCanBePlacedHere(item.X, item.Y))
                return;

            item.ExtraData = num3.ToString();
            item.UpdateState();
            item.GetRoom().GetGameMap().UpdateMapForItem(item);
        }
    }
}