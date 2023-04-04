using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.Util;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class Alert. This class cannot be inherited.
    /// </summary>
    internal sealed class Developer : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Developer" /> class.
        /// </summary>
        public Developer()
        {
            MinRank = 8;
            Description = "Developer command";
            Usage = ":developer [info/set/copy/paste/delete]";
            MinParams = -1;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var mode = pms[0];
            pms = pms.Skip(1).ToArray();

            switch (mode.ToLower())
            {
                case "info":
                    {
                        if (pms.Length == 0)  await session.SendWhisperAsync("Usage :developer info [items/user/users/cache]");
                        else return await GetInfo(session, pms);

                        break;
                    }
                case "set":
                    {
                        if (pms.Length < 2)  await session.SendWhisperAsync("Usage :developer set [item/baseItem] id");
                        else return await Set(session, pms);

                        break;
                    }
                case "copy":
                    {
                        return Copy(session);
                    }
                case "paste":
                    {
                        return await Paste(session);
                    }
                case "delete":
                    {
                        return await Delete(session);
                    }
                default:
                    {
                         await session.SendWhisperAsync("Usage :developer [info/set/copy/paste/delete]");
                        break;
                    }
            }

            return true;
        }

        private static async Task<bool> Delete(GameClient session)
        {
            var room = session.GetHabbo().CurrentRoom;

            var user = room.GetRoomUserManager()
                .GetRoomUserByHabbo(session.GetHabbo().Id);

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                /* TODO CHECK */ foreach (
                    var item in
                        room.GetGameMap()
                            .GetAllRoomItemForSquare(user.LastSelectedX, user.LastSelectedY))
                {
                    await queryReactor.RunNoLockFastQueryAsync("DELETE FROM items_rooms WHERE id = '" + item.Id + "';");

                    await room.GetRoomItemHandler().RemoveRoomItem(item, false);
                    item.Dispose(true);
                }
            }

            return true;
        }

        private static async Task<bool> Paste(GameClient session)
        {
            var room = session.GetHabbo().CurrentRoom;

            var user = room.GetRoomUserManager()
                .GetRoomUserByVirtualId(session.CurrentRoomUserId);

            if (user.CopyX == 0 || user.CopyY == 0)
            {
                 await session.SendWhisperAsync("First usage :developer copy");
                return true;
            }
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
              foreach (
                    var item in
                        room.GetGameMap()
                            .GetAllRoomItemForSquare(user.CopyX, user.CopyY))
              {
                  var itemId = Guid.NewGuid();
                  ShortGuid insertId = itemId;

                    queryReactor.SetNoLockQuery(
                        "INSERT INTO items_rooms (base_item, user_id, room_id, extra_data, x, y, z, rot, group_id) VALUES (" +
                        item.GetBaseItem().ItemId + ", " + user.UserId + ", " + user.RoomId + ", @extraData, " +
                        user.LastSelectedX + ", " + user.LastSelectedY + ", @height, " + item.Rot + ", " + item.GroupId +
                        ");");
                    queryReactor.AddParameter("extraData", item.ExtraData);
                    queryReactor.AddParameter("height", TextHandling.GetString(item.Z));

//                    var insertId = (uint)queryReactor.InsertQuery();

                    var roomItem = new RoomItem(insertId, user.RoomId, item.GetBaseItem().ItemId, item.ExtraData,
                        user.LastSelectedX, user.LastSelectedY, item.Z, item.Rot, session.GetHabbo().CurrentRoom,
                        user.UserId, item.GroupId, item.SongCode,
                        item.IsBuilder, item.LimitedNo, item.LimitedTot);
                    await room.GetRoomItemHandler().DeveloperSetFloorItem(session, roomItem);
                }
            }

            return true;
        }

        private static bool Copy(GameClient session)
        {
            var user =
                session.GetHabbo()
                    .CurrentRoom.GetRoomUserManager()
                    .GetRoomUserByHabbo(session.GetHabbo().UserName);

            user.CopyX = user.LastSelectedX;
            user.CopyY = user.LastSelectedY;

            return true;
        }

        private static async Task<bool> Set(GameClient session, IReadOnlyList<string> pms)
        {
            var type = pms[0];
            var id = uint.Parse(pms[1]);

            var itemId = Oblivion.GetGame().GetItemManager().GetRealId(id);
            switch (type.ToLower())
            {
                case "item":
                    {
                        if (pms.Count == 2)
                        {
                             await session.SendWhisperAsync("Usage :developer set item id [x/y/z] value");
                            break;
                        }

                        var item = session.GetHabbo().CurrentRoom.GetRoomItemHandler().GetItem(itemId);
                        if (item == null)
                        {
                             await session.SendWhisperAsync("Item no encontrado");
                            return false;
                        }

                        int x = item.X, y = item.Y;
                        var z = item.Z;

                        var i = 2;
                        while (pms.Count >= i + 2)
                        {
                            switch (pms[i].ToLower())
                            {
                                case "x":
                                    {
                                        x = int.Parse(pms[i + 1]);
                                        break;
                                    }
                                case "y":
                                    {
                                        y = int.Parse(pms[i + 1]);
                                        break;
                                    }
                                case "z":
                                    {
                                        z = double.Parse(pms[i + 1]);
                                        break;
                                    }
                                case "rot":
                                    {
                                        item.Rot = int.Parse(pms[i + 1]);
                                        break;
                                    }
                            }
                            i += 2;
                        }

                        if (item.IsWallItem) await session.GetHabbo().CurrentRoom.GetRoomItemHandler().SetWallItem(session, item);
                        else
                            await session.GetHabbo().CurrentRoom.GetRoomItemHandler().SetFloorItem(item, x, y, z, item.Rot, true);
                        break;
                    }

                case "baseitem":
                    {
                        if (pms.Count == 2)
                        {
                             await session.SendWhisperAsync("Usage :developer set baseItem baseId [stack,trade,modes,height] value");
                            break;
                        }

                        var item = Oblivion.GetGame().GetItemManager().GetItem(id);
                        if (item == null)
                        {
                             await session.SendWhisperAsync("Item no encontrado");
                            return false;
                        }

                        var i = 2;
                        while (pms.Count >= i + 2)
                        {
                            switch (pms[i].ToLower())
                            {
                                case "stack":
                                    {
                                        item.Stackable = pms[i + 1] == "1" || pms[i + 1] == "true";
                                        break;
                                    }
                                case "trade":
                                    {
                                        item.AllowTrade = pms[i + 1] == "1" || pms[i + 1] == "true";
                                        break;
                                    }
                                case "modes":
                                    {
                                        item.Modes = uint.Parse(pms[i + 1]);
                                        break;
                                    }
                                case "height":
                                    {
                                        var stackHeightStr = pms[i + 1].Replace(',', '.');
                                        if (stackHeightStr.Contains(';'))
                                        {
                                            var heightsStr = stackHeightStr.Split(';');
                                            item.ToggleHeight =
                                                heightsStr.Select(
                                                    heightStr => double.Parse(heightStr, CultureInfo.InvariantCulture))
                                                    .ToArray();
                                            item.Height = item.ToggleHeight[0];
                                            item.StackMultipler = true;
                                        }
                                        else
                                        {
                                            item.Height = double.Parse(stackHeightStr, CultureInfo.InvariantCulture);
                                            item.StackMultipler = false;
                                        }

                                        break;
                                    }
                            }
                            i += 2;
                        }

                        Item.Save(item.ItemId, item.Stackable, item.AllowTrade,
                            item.StackMultipler ? item.ToggleHeight : new[] { item.Height }, item.Modes);
                        break;
                    }
            }
            return true;
        }

        private static async Task<bool> GetInfo(GameClient session, IReadOnlyList<string> pms)
        {
            var type = pms[0];

            var user =
                session.GetHabbo()
                    .CurrentRoom.GetRoomUserManager()
                    .GetRoomUserByVirtualId(session.CurrentRoomUserId);
            var text = new StringBuilder();
            switch (type)
            {
                case "cache":
                    {
                        text.AppendLine("Displaying info of all cached data avaible");
                        text.Append("Users: " + Oblivion.UsersCached.Count + '\r');
                        text.Append("Rooms: " + Oblivion.GetGame().GetRoomManager().LoadedRooms.Count + '\r');
                        text.Append("Rooms Data: " + Oblivion.GetGame().GetRoomManager().LoadedRoomData.Count + '\r');
                        text.Append("Groups: " + Oblivion.GetGame().GetGroupManager().Groups.Count + '\r');
                        text.Append("Items: " + Oblivion.GetGame().GetItemManager().CountItems() + '\r');
                        text.Append("Catalog Items: " + Oblivion.GetGame().GetCatalog().Offers.Count + '\r');

                        await session.SendNotifWithScroll(text.ToString());
                        break;
                    }
                case "users":
                    {
                        text.AppendLine("Displaying info of all users of this room");

                        /* TODO CHECK */ foreach (var roomUser in session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers())
                            AppendUserInfo(roomUser, text);

                        await session.SendNotifWithScroll(text.ToString());
                        break;
                    }
                case "user":
                    {
                        var roomUser =
                            session.GetHabbo()
                                .CurrentRoom.GetRoomUserManager()
                                .GetRoomUserByHabbo(session.GetHabbo().LastSelectedUser);
                        if (roomUser == null || roomUser.IsBot || roomUser.GetClient() == null)
                            text.Append("User not found");
                        else AppendUserInfo(roomUser, text);

                        await session.SendNotifWithScroll(text.ToString());
                        break;
                    }
                case "items":
                    {
                        text.AppendLine(
                            $"Displaying info of coordinates: (X/Y)  {user.LastSelectedX}/{user.LastSelectedY}");

                        /* TODO CHECK */ foreach (
                            var item in
                                session.GetHabbo()
                                    .CurrentRoom.GetGameMap()
                                    .GetAllRoomItemForSquare(user.LastSelectedX, user.LastSelectedY))
                        {
                            text.Append($"## itemId: {item.Id}  itemBaseId: {item.GetBaseItem().ItemId} \r");
                            text.Append(
                                $"itemName: {item.GetBaseItem().Name}  itemSpriteId: {item.GetBaseItem().SpriteId} \r");
                            text.Append($"itemInteraction: {item.GetBaseItem().InteractionType} \r");
                            text.Append($"itemInteractionCount: {item.GetBaseItem().Modes} \r");
                            text.Append($"itemPublicName: {item.GetBaseItem().PublicName} \r");
                            text.Append($"X/Y/Z/Rot:  {item.X}/{item.Y}/{item.Z}/{item.Rot}  Height: {item.Height} \r");
                            if (item.GetBaseItem().StackMultipler)
                                text.Append("Heights: " + string.Join("  -  ", item.GetBaseItem().ToggleHeight) + '\r');
                            text.AppendLine(
                                $"Can: {(item.GetBaseItem().Walkable ? "walk" : string.Empty)}  {(item.GetBaseItem().IsSeat ? "sit" : string.Empty)}  {(item.GetBaseItem().Stackable ? "stack" : string.Empty)}");
                        }

                        await session.SendNotifWithScroll(text.ToString());
                        break;
                    }
            }

            return true;
        }

        private static void AppendUserInfo(RoomUser user, StringBuilder text)
        {
            text.Append(
                $"## userId: {user.UserId}  name: {user.GetUserName()} rank: {user.GetClient().GetHabbo().Rank} \r");
            if (user.IsDancing) text.Append("actions: dancing \r");
            if (user.IsLyingDown) text.Append("actions: lying \r");
            if (user.IsSitting) text.Append("actions: sitting \r");
            if (user.CurrentEffect > 0) text.Append("actions: effect." + user.CurrentEffect);
            if (user.IsWalking) text.Append($" walking.To(X/Y  {user.GoalX}/{user.GoalY})");
            text.Append("\r");

            text.Append("room rights: ");
            if (user.GetClient().GetHabbo().HasFuse("fuse_mod")) text.Append(" staff");
            if (user.GetClient().GetHabbo().HasFuse("fuse_any_room_controller")) text.Append(" controlAnyRoom");
            if (user.GetClient().GetHabbo().CurrentRoom.CheckRights(user.GetClient(), true)) text.Append(" owner");
            if (user.GetClient().GetHabbo().CurrentRoom.CheckRights(user.GetClient(), false, true))
                text.Append(" groupAdmin");
            else if (user.GetClient().GetHabbo().CurrentRoom.CheckRights(user.GetClient(), false, false, true))
                text.Append(" groupMember");
            text.Append("\r");

            text.Append("prohibitions: ");
            if (!user.CanWalk) text.Append(" walk");
            if (user.GetClient().GetHabbo().Muted) text.Append(" chat");
            text.Append("\r");

            text.AppendLine($"X/Y/Z/Rot:  {user.X}/{user.Y}/{user.Z}/{user.RotBody}");
        }
    }
}