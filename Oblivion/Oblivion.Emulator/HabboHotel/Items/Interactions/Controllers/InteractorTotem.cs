using System;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Quests;
using Oblivion.Util;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorTotem : FurniInteractorModel
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


            if (!item.GetRoom().CheckRights(session, true)) return;

            var items = item.GetRoom().GetGameMap().GetRoomItemForSquare(item.X, item.Y);

            int currentHead = -1, currentLeg = -1, currentPlanet = -1;
            foreach (var squareItem in items)
            {
                if (squareItem.GetBaseItem().InteractionType != Interaction.Totem) continue;
                if (squareItem.ExtraData == null)
                {
                    squareItem.ExtraData = "0";
                }
                var currentState = Convert.ToInt32(squareItem.ExtraData);
                if (squareItem.GetBaseItem().Name == "totem_head")
                {
                    currentHead = currentState;
                }

                if (squareItem.GetBaseItem().Name == "totem_leg")
                {
                    currentLeg = currentState;
                }

                if (squareItem.GetBaseItem().Name == "totem_planet")
                {
                    currentPlanet = currentState;
                }
            }

            if (currentHead < 0 || currentLeg < 0 || currentPlanet < 0)
                return;
            if (Oblivion.GetUnixTimeStamp() - session.GetHabbo().LastTotem <= 1800)
            {
                session.SendWhisper("Hey, você só pode receber um efeito a cada meia hora!");
                return;
            }
            if (currentPlanet == 0 && (currentLeg == 3 || currentLeg == 7 || currentLeg == 11) &&
                (currentHead == 6 || currentHead == 10 || currentHead == 14))
            {
                session.GetHabbo().GetAvatarEffectsInventoryComponent().AddNewEffect(24, 86400, 0);
            }
            else if (currentPlanet == 1 && (currentLeg == 1 || currentLeg == 5 || currentLeg == 9) &&
                     (currentHead == 4 || currentHead == 8 || currentHead == 12))
            {
                session.GetHabbo().GetAvatarEffectsInventoryComponent().AddNewEffect(25, 86400, 0);
                session.GetHabbo().GetAvatarEffectsInventoryComponent().AddNewEffect(26, 86400, 0);
            }
            else if (currentPlanet == 2 && (currentLeg == 2 || currentLeg == 6 || currentLeg == 10) &&
                     (currentHead == 5 || currentHead == 9 || currentHead == 13))
            {
                session.GetHabbo().GetAvatarEffectsInventoryComponent().AddNewEffect(23, 86400, 0);
            }
            else
            {
                return;
            }
            session.SendWhisper("Você recebeu 1 efeito!");
            session.GetHabbo().SaveLastTotem();
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
        }
    }
}