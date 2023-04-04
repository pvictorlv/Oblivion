using System.Collections.Generic;
using System.Drawing;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Rooms.Items.Games.Teams.Enums;
using Oblivion.HabboHotel.Rooms.User;

namespace Oblivion.HabboHotel.Rooms.Items.Games.Teams
{
    public class TeamManager
    {
        public List<RoomUser> BlueTeam;
        public string Game;
        public List<RoomUser> GreenTeam;
        public List<RoomUser> RedTeam;
        public List<RoomUser> YellowTeam;

        public static TeamManager CreateTeamforGame(string game) => new TeamManager
        {
            Game = game,
            BlueTeam = new List<RoomUser>(),
            RedTeam = new List<RoomUser>(),
            GreenTeam = new List<RoomUser>(),
            YellowTeam = new List<RoomUser>()
        };

        public bool CanEnterOnTeam(Team t)
        {
            if (t.Equals(Team.Blue)) return BlueTeam.Count < 5;
            if (t.Equals(Team.Red)) return RedTeam.Count < 5;
            if (t.Equals(Team.Yellow)) return YellowTeam.Count < 5;
            return t.Equals(Team.Green) && GreenTeam.Count < 5;
        }

        public async void AddUser(RoomUser user)
        {
            if (user?.GetClient() == null) return;

            switch (user.Team)
            {
                case Team.Blue:
                    BlueTeam.Add(user);
                    break;
                case Team.Red:
                    RedTeam.Add(user);
                    break;
                case Team.Yellow:
                    YellowTeam.Add(user);
                    break;
                case Team.Green:
                    GreenTeam.Add(user);
                    break;
            }


            if (string.IsNullOrEmpty(Game)) return;
            if (Game.ToLower() == "banzai")
            {
                var currentRoom = user.GetClient().GetHabbo().CurrentRoom;
                using (var enumerator = currentRoom.GetRoomItemHandler().FloorItems.Values.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;
                        if (current == null) continue;

                        if (current.GetBaseItem().InteractionType.Equals(Interaction.BanzaiGateBlue))
                        {
                            current.ExtraData = BlueTeam.Count.ToString();
                            await current.UpdateState();
                            if (BlueTeam.Count != 5) continue;
                            /* TODO CHECK */
                            foreach (
                                var current2 in
                                currentRoom.GetGameMap().GetRoomUsers(new Point(current.X, current.Y)))
                                current2.SqState = 0;
                            currentRoom.GetGameMap().GameMap[current.X, current.Y] = 0;
                        }
                        else if (current.GetBaseItem().InteractionType.Equals(Interaction.BanzaiGateRed))
                        {
                            current.ExtraData = RedTeam.Count.ToString();
                            await current.UpdateState();
                            if (RedTeam.Count != 5) continue;
                            /* TODO CHECK */
                            foreach (
                                var current3 in
                                currentRoom.GetGameMap().GetRoomUsers(new Point(current.X, current.Y)))
                                current3.SqState = 0;
                            currentRoom.GetGameMap().GameMap[current.X, current.Y] = 0;
                        }
                        else if (current.GetBaseItem().InteractionType.Equals(Interaction.BanzaiGateGreen))
                        {
                            current.ExtraData = GreenTeam.Count.ToString();
                            await current.UpdateState();
                            if (GreenTeam.Count != 5) continue;
                            /* TODO CHECK */
                            foreach (
                                var current4 in
                                currentRoom.GetGameMap().GetRoomUsers(new Point(current.X, current.Y)))
                                current4.SqState = 0;
                            currentRoom.GetGameMap().GameMap[current.X, current.Y] = 0;
                        }
                        else if (current.GetBaseItem().InteractionType.Equals(Interaction.BanzaiGateYellow))
                        {
                            current.ExtraData = YellowTeam.Count.ToString();
                            await current.UpdateState();
                            if (YellowTeam.Count != 5) continue;
                            /* TODO CHECK */
                            foreach (
                                var current5 in
                                currentRoom.GetGameMap().GetRoomUsers(new Point(current.X, current.Y)))
                                current5.SqState = 0;
                            currentRoom.GetGameMap().GameMap[current.X, current.Y] = 0;
                        }
                    }
                }
            }
            else if (Game.ToLower() == "freeze")
            {
                var currentRoom2 = user.GetClient().GetHabbo().CurrentRoom;
                /* TODO CHECK */
                foreach (var current6 in currentRoom2.GetRoomItemHandler().FloorItems.Values)
                {
                    if (current6.GetBaseItem().InteractionType == Interaction.FreezeBlueGate)
                    {
                        current6.ExtraData = BlueTeam.Count.ToString();
                        await current6.UpdateState();
                    }
                    else if (current6.GetBaseItem().InteractionType == Interaction.FreezeRedGate)
                    {
                        current6.ExtraData = RedTeam.Count.ToString();
                        await current6.UpdateState();
                    }
                    else if (current6.GetBaseItem().InteractionType == Interaction.FreezeGreenGate)
                    {
                        current6.ExtraData = GreenTeam.Count.ToString();
                        await current6.UpdateState();
                    }
                    else if (current6.GetBaseItem().InteractionType == Interaction.FreezeYellowGate)
                    {
                        current6.ExtraData = YellowTeam.Count.ToString();
                        await current6.UpdateState();
                    }
                }
            }
        }

        public async void OnUserLeave(RoomUser user)
        {
            if (user == null) return;

            switch (user.Team)
            {
                case Team.Blue:
                    BlueTeam.Remove(user);
                    break;
                case Team.Red:
                    RedTeam.Remove(user);
                    break;
                case Team.Yellow:
                    YellowTeam.Remove(user);
                    break;
                case Team.Green:
                    GreenTeam.Remove(user);
                    break;
            }


            if (string.IsNullOrEmpty(Game)) return;

            var currentRoom = user.GetClient().GetHabbo().CurrentRoom;
            if (currentRoom == null) return;

            switch (Game.ToLower())
            {
                case "banzai":
                    using (var enumerator = currentRoom.GetRoomItemHandler().FloorItems.Values.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            var current = enumerator.Current;
                            if (current == null) continue;

                            if (current.GetBaseItem().InteractionType.Equals(Interaction.BanzaiGateBlue))
                            {
                                current.ExtraData = BlueTeam.Count.ToString();
                                await current.UpdateState();
                                if (currentRoom.GetGameMap().GameMap[current.X, current.Y] != 0) continue;
                                /* TODO CHECK */
                                foreach (
                                    var current2 in
                                    currentRoom.GetGameMap().GetRoomUsers(new Point(current.X, current.Y)))
                                    current2.SqState = 1;
                                currentRoom.GetGameMap().GameMap[current.X, current.Y] = 1;
                            }
                            else if (current.GetBaseItem().InteractionType.Equals(Interaction.BanzaiGateRed))
                            {
                                current.ExtraData = RedTeam.Count.ToString();
                                await current.UpdateState();
                                if (currentRoom.GetGameMap().GameMap[current.X, current.Y] != 0) continue;
                                /* TODO CHECK */
                                foreach (
                                    var current3 in
                                    currentRoom.GetGameMap().GetRoomUsers(new Point(current.X, current.Y)))
                                    current3.SqState = 1;
                                currentRoom.GetGameMap().GameMap[current.X, current.Y] = 1;
                            }
                            else if (current.GetBaseItem().InteractionType.Equals(Interaction.BanzaiGateGreen))
                            {
                                current.ExtraData = GreenTeam.Count.ToString();
                                await current.UpdateState();
                                if (currentRoom.GetGameMap().GameMap[current.X, current.Y] != 0) continue;
                                /* TODO CHECK */
                                foreach (
                                    var current4 in
                                    currentRoom.GetGameMap().GetRoomUsers(new Point(current.X, current.Y)))
                                    current4.SqState = 1;
                                currentRoom.GetGameMap().GameMap[current.X, current.Y] = 1;
                            }
                            else if (current.GetBaseItem().InteractionType.Equals(Interaction.BanzaiGateYellow))
                            {
                                current.ExtraData = YellowTeam.Count.ToString();
                                await current.UpdateState();
                                if (currentRoom.GetGameMap().GameMap[current.X, current.Y] != 0) continue;
                                /* TODO CHECK */
                                foreach (
                                    var current5 in
                                    currentRoom.GetGameMap().GetRoomUsers(new Point(current.X, current.Y)))
                                    current5.SqState = 1;
                                currentRoom.GetGameMap().GameMap[current.X, current.Y] = 1;
                            }
                        }
                    }

                    break;

                case "freeze":
                    /* TODO CHECK */
                    foreach (var current6 in currentRoom.GetRoomItemHandler().FloorItems.Values)
                    {
                        if (current6.GetBaseItem().InteractionType == Interaction.FreezeBlueGate)
                        {
                            current6.ExtraData = BlueTeam.Count.ToString();
                            await current6.UpdateState();
                        }
                        else if (current6.GetBaseItem().InteractionType == Interaction.FreezeRedGate)
                        {
                            current6.ExtraData = RedTeam.Count.ToString();
                            await current6.UpdateState();
                        }
                        else if (current6.GetBaseItem().InteractionType == Interaction.FreezeGreenGate)
                        {
                            current6.ExtraData = GreenTeam.Count.ToString();
                            await current6.UpdateState();
                        }
                        else if (current6.GetBaseItem().InteractionType == Interaction.FreezeYellowGate)
                        {
                            current6.ExtraData = YellowTeam.Count.ToString();
                            await current6.UpdateState();
                        }
                    }

                    break;
            }
        }

        public void Dispose()
        {
            BlueTeam.Clear();
            GreenTeam.Clear();
            RedTeam.Clear();
            YellowTeam.Clear();
            BlueTeam = null;
            GreenTeam = null;
            RedTeam = null;
            YellowTeam = null;
            Game = null;
        }
    }
}