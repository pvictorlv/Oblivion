﻿using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Triggers
{
    public class ScoreAchieved : IWiredItem
    {
        public ScoreAchieved(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            OtherString = string.Empty;
            Delay = 0;
            OtherBool = true;
            OtherExtraString = string.Empty;
            OtherExtraString2 = string.Empty;
        }

        public Interaction Type => Interaction.TriggerScoreAchieved;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items
        {
            get { return new List<RoomItem>(); }
            set { }
        }

        public int Delay { get; set; }

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public bool Execute(params object[] stuff)
        {
            var roomUser = (RoomUser) stuff[0];

            if (roomUser == null)
                return false;

            int scoreToGet;
            int.TryParse(OtherString, out scoreToGet);

            if (Room.GetGameManager().TeamPoints[(int) roomUser.Team] < scoreToGet)
                return false;

            var conditions = Room.GetWiredHandler().GetConditions(this);
            var effects = Room.GetWiredHandler().GetEffects(this);

            if (conditions.Any())
                /* TODO CHECK */ foreach (var current in conditions)
                {
                    if (!current.Execute(roomUser))
                        return false;

                    WiredHandler.OnEvent(current);
                }

            if (effects.Any(x => x.Type == Interaction.SpecialRandom))
            {
                var randomBox = effects.FirstOrDefault(x => x.Type == Interaction.SpecialRandom);
                if (randomBox != null && !randomBox.Execute())
                    return false;

                var selectedBox = Room.GetWiredHandler().GetRandomEffect(effects);
                if (!selectedBox.Execute())
                    return false;

                WiredHandler.OnEvent(randomBox);
                WiredHandler.OnEvent(selectedBox);
            }
            else if (effects.Any())
            {
                /* TODO CHECK */ foreach (var current2 in effects.Where(current2 => current2.Execute(roomUser, Type)))
                    WiredHandler.OnEvent(current2);
            }

            WiredHandler.OnEvent(this);
            return true;
        }
    }
}