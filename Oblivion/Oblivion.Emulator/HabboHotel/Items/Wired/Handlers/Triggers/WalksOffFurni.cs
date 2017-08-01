using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Items.Wired.Handlers.Triggers
{
    internal class WalksOffFurni : IWiredItem, IWiredCycler
    {
        private long _mNext;

        public WalksOffFurni(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            ToWork = new Queue();
            Items = new List<RoomItem>();
        }

        public Queue ToWork { get; set; }

        public ConcurrentQueue<RoomUser> ToWorkConcurrentQueue { get; set; }

        public bool OnCycle()
        {
            var num = Oblivion.Now();
            if (num <= _mNext)

                return false;

            lock (ToWork.SyncRoot)
            {
                while (ToWork.Count > 0)
                {
                    var roomUser = (RoomUser)ToWork.Dequeue();

                    var conditions = Room.GetWiredHandler().GetConditions(this);
                    var effects = Room.GetWiredHandler().GetEffects(this);

                    if (conditions.Any())
                    {
                        foreach (var current in conditions)
                        {
                            if (!current.Execute(roomUser))
                                return false;

                            WiredHandler.OnEvent(current);
                        }
                    }

                    if (!effects.Any())
                        continue;
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
                    else
                        foreach (var current2 in effects.Where(current2 => current2.Execute(roomUser, Type)))
                        WiredHandler.OnEvent(current2);
                }
            }

            _mNext = 0L;
            WiredHandler.OnEvent(this);
            return true;
        }

        public Interaction Type => Interaction.TriggerWalkOffFurni;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items { get; set; }

        public int Delay { get; set; }

        public string OtherString
        {
            get { return ""; }
            set { }
        }

        public string OtherExtraString
        {
            get { return ""; }
            set { }
        }

        public string OtherExtraString2
        {
            get { return ""; }
            set { }
        }

        public bool OtherBool
        {
            get { return true; }
            set { }
        }

        public bool Execute(params object[] stuff)
        {
            var roomUser = (RoomUser)stuff[0];
            var roomItem = (RoomItem)stuff[1];

            if (!Items.Contains(roomItem) || roomUser.LastItem != roomItem.Id)
                return false;

            if (roomItem.AffectedTiles.Values.Any(current => (current.X == roomUser.X && current.Y == roomUser.Y) || (roomUser.X == roomItem.X && roomUser.Y == roomItem.Y)))
                return false;

            ToWork.Enqueue(roomUser);

            if (Delay == 0)
                OnCycle();
            else
            {
                _mNext = (Oblivion.Now() + (Delay));

                Room.GetWiredHandler().EnqueueCycle(this);
            }

            return true;
        }
    }
}