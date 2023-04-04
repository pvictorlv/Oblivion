using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.Configuration;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Groups.Interfaces;
using Oblivion.HabboHotel.Rooms.Chat;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Rooms.Data
{
    /// <summary>
    ///     Class RoomData.
    /// </summary>
    internal class RoomData
    {
        /// <summary>
        ///     The _model
        /// </summary>
        private RoomModel _model;

        /// <summary>
        ///     The allow pets
        /// </summary>
        internal bool AllowPets, AllowPetsEating, AllowWalkThrough, HideWall;

        /// <summary>
        ///     The allow rights override
        /// </summary>
        internal bool AllowRightsOverride;

        /// <summary>
        ///     The category
        /// </summary>
        internal int Category;

        /// <summary>
        ///  Room blocked cmd
        /// </summary>
        internal List<string> BlockedCommands;


        /// <summary>
        ///     The chat balloon
        /// </summary>
        internal uint ChatBalloon, ChatSpeed, ChatMaxDistance, ChatFloodProtection;

        /// <summary>
        ///     The chat type
        /// </summary>
        internal int ChatType;

        internal int CompetitionStatus, CompetitionVotes;

        /// <summary>
        /// Is room for sale?
        /// </summary>
        public bool RoomForSale = false;

        /// <summary>
        /// The room cost
        /// </summary>
        public int RoomSaleCost = 0;

        /// <summary>
        /// the currency cost type (d = diamond, c = credit)
        /// </summary>
        public string RoomSaleType = "";


        /// <summary>
        ///     The description
        /// </summary>
        internal string Description;

        internal bool DisablePull = false;

        internal bool DisablePush = false;


        /// <summary>
        ///     The event
        /// </summary>
        internal RoomEvent Event;

        /// <summary>
        ///     The floor
        /// </summary>
        internal string Floor;

        /// <summary>
        ///     The floor thickness
        /// </summary>
        internal int FloorThickness;

        /// <summary>
        ///     The group
        /// </summary>
        internal Guild Group;

        /// <summary>
        ///     The group identifier
        /// </summary>
        internal uint GroupId;

        /// <summary>
        ///     The identifier
        /// </summary>
        internal uint Id;

        /// <summary>
        ///     The land scape
        /// </summary>
        internal string LandScape;

        public DateTime LastUsed = DateTime.Now;

        /// <summary>
        ///     The model name
        /// </summary>
        internal string ModelName;

        /// <summary>
        ///     The name
        /// </summary>
        internal string Name;

        /// <summary>
        ///     The owner
        /// </summary>
        internal string Owner;

        /// <summary>
        ///     The owner identifier
        /// </summary>
        internal int OwnerId;

        /// <summary>
        ///     The pass word
        /// </summary>
        internal string PassWord;

        /// <summary>
        ///     The room chat
        /// </summary>
        internal List<Chatlog> RoomChat;

        /// <summary>
        ///     The score
        /// </summary>
        internal int Score;

        /// <summary>
        ///     The state
        /// </summary>
        internal int State;

        /// <summary>
        ///     The tags
        /// </summary>
        internal List<string> Tags;

        /// <summary>
        ///     The trade state
        /// </summary>
        internal int TradeState;

        /// <summary>
        ///     The type
        /// </summary>
        internal string Type;

        /// <summary>
        ///     The users now
        /// </summary>
        internal uint UsersNow, UsersMax;

        /// <summary>
        ///     The wall height
        /// </summary>
        internal int WallHeight;

        /// <summary>
        ///     The wall paper
        /// </summary>
        internal string WallPaper;

        /// <summary>
        ///     The wall thickness
        /// </summary>
        internal int WallThickness;

        /// <summary>
        ///     The who can ban
        /// </summary>
        internal int WhoCanBan;

        /// <summary>
        ///     The who can kick
        /// </summary>
        internal int WhoCanKick;

        /// <summary>
        ///     The who can mute
        /// </summary>
        internal int WhoCanMute;

        /// <summary>
        ///     The word filter
        /// </summary>
        internal List<string> WordFilter;

        /// <summary>
        ///     Gets the tag count.
        /// </summary>
        /// <value>The tag count.</value>
        internal int TagCount => Tags.Count;

        /// <summary>
        ///     Gets a value indicating whether this instance has event.
        /// </summary>
        /// <value><c>true</c> if this instance has event; otherwise, <c>false</c>.</value>
        internal bool HasEvent => false;

        /// <summary>
        ///     Gets the model.
        /// </summary>
        /// <value>The model.</value>
        internal RoomModel Model => _model ?? (_model = Oblivion.GetGame().GetRoomManager().GetModel(ModelName, Id));

        /// <summary>
        ///     Resets the model.
        /// </summary>
        internal Task ResetModel()
        {
            _model = Oblivion.GetGame().GetRoomManager().GetModel(ModelName, Id);
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Fills the null.
        /// </summary>
        /// <param name="id">The identifier.</param>
        internal Task FillNull(uint id)
        {
            Id = id;
            Name = "Unknown Room";
            Description = "-";
            Type = "private";
            Owner = "-";
            Category = 0;
            UsersNow = 0;
            UsersMax = 0;
            ModelName = "NO_MODEL";
            Score = 0;
            Tags = new List<string>();
            BlockedCommands = new List<string>();
            AllowPets = true;
            AllowPetsEating = false;
            AllowWalkThrough = true;
            HideWall = false;
            PassWord = string.Empty;
            WallPaper = "0.0";
            Floor = "0.0";
            LandScape = "0.0";
            WallThickness = 0;
            FloorThickness = 0;
            Group = null;
            AllowRightsOverride = false;
            Event = null;
            WhoCanBan = 0;
            WhoCanKick = 0;
            WhoCanMute = 0;
            TradeState = 2;
            State = 0;
            RoomChat = new List<Chatlog>();
            WordFilter = new List<string>();
            WallHeight = -1;
            _model = Oblivion.GetGame().GetRoomManager().GetModel(ModelName, Id);
            CompetitionStatus = 0;
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Fills the specified row.
        /// </summary>
        /// <param name="row">The row.</param>
        internal Task Fill(DataRow row, uint user = 0u)
        {
            try
            {
                Id = Convert.ToUInt32(row["id"]);
                Name = (string) row["caption"];
                PassWord = (string) row["password"];
                Description = (string) row["description"];
                Type = (string) row["roomtype"];
                Owner = (string) row["owner"];
                OwnerId = 0;
                RoomChat = new List<Chatlog>();
                WordFilter = new List<string>();
                BlockedCommands = new List<string>();

                using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    uint integer;
                    var client = Oblivion.GetGame().GetClientManager().GetClientByUserName(Owner);
                    if (client == null && user == 0)
                    {
                        queryReactor.SetQuery("SELECT id FROM users WHERE username = @name");
                        queryReactor.AddParameter("name", Owner);
                        integer = Convert.ToUInt32(queryReactor.GetInteger());
                    }
                    else
                    {
                        integer = client?.GetHabbo()?.Id ?? user;
                    }

                    OwnerId = integer != uint.MinValue ? Convert.ToInt32(integer) : 0;
                }
                var roomState = row["state"].ToString().ToLower();

                switch (roomState)
                {
                    case "locked":
                        State = 1;
                        break;
                    case "password":
                        State = 2;
                        break;
                    default:
                        State = 0;
                        break;
                }

                ModelName = (string) row["model_name"];
                WallPaper = (string) row["wallpaper"];
                Floor = (string) row["floor"];
                LandScape = (string) row["landscape"];

                int.TryParse(row["trade_state"].ToString(), out TradeState);
                int.TryParse(row["category"].ToString(), out Category);
                int.TryParse(row["walls_height"].ToString(), out WallHeight);
                int.TryParse(row["score"].ToString(), out Score);
                int.TryParse(row["floorthick"].ToString(), out FloorThickness);
                int.TryParse(row["wallthick"].ToString(), out WallThickness);
                int.TryParse(row["chat_type"].ToString(), out ChatType);
                int.TryParse(row["mute_settings"].ToString(), out WhoCanMute);
                int.TryParse(row["kick_settings"].ToString(), out WhoCanKick);
                int.TryParse(row["ban_settings"].ToString(), out WhoCanBan);

                uint.TryParse(row["users_max"].ToString(), out UsersMax);
                uint.TryParse(row["group_id"].ToString(), out GroupId);
                uint.TryParse(row["chat_balloon"].ToString(), out ChatBalloon);
                uint.TryParse(row["chat_speed"].ToString(), out ChatSpeed);
                uint.TryParse(row["chat_max_distance"].ToString(), out ChatMaxDistance);
                uint.TryParse(row["chat_flood_protection"].ToString(), out ChatFloodProtection);

                AllowPets = Oblivion.EnumToBool(row["allow_pets"].ToString());
                AllowPetsEating = Oblivion.EnumToBool(row["allow_pets_eat"].ToString());
                AllowWalkThrough = Oblivion.EnumToBool(row["allow_walkthrough"].ToString());
                HideWall = Oblivion.EnumToBool(row["hidewall"].ToString());

                AllowRightsOverride = false;

                if (GroupId > 0)
                    Group = Oblivion.GetGame().GetGroupManager().GetGroup(GroupId);
                if (Id > 0)
                    Event = Oblivion.GetGame().GetRoomEvents().GetEvent(Id);
                _model = Oblivion.GetGame().GetRoomManager().GetModel(ModelName, Id);

                CompetitionStatus = 0;


                Tags = new List<string>();

                if (row.IsNull("tags") || string.IsNullOrEmpty(row["tags"].ToString()))
                    return Task.CompletedTask;

                Tags = row["tags"].ToString().Split(',').ToList();
            }
            catch (Exception ex)
            {
                Logging.LogException("Exception on RoomData Loading (Fill Void): " + ex);
                Logging.HandleException(ex, "Oblivion.HabboHotel.Rooms.RoomData");
            }

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Serializes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="showEvents">if set to <c>true</c> [show events].</param>
        /// <param name="enterRoom"></param>
        internal async Task Serialize(ServerMessage message, bool showEvents = false, bool enterRoom = false)
        {
            if (message == null || Tags == null) return;

            await message.AppendIntegerAsync(Id);
            await message.AppendStringAsync(Name);
            await message.AppendIntegerAsync(OwnerId);
            await message.AppendStringAsync(Owner);
            await message.AppendIntegerAsync(State);
            await message.AppendIntegerAsync(UsersNow);
            await message.AppendIntegerAsync(UsersMax);
            await message.AppendStringAsync(Description);
            await message.AppendIntegerAsync(TradeState);
            await message.AppendIntegerAsync(Score);
            await message.AppendIntegerAsync(0); // Ranking
            await message.AppendIntegerAsync(Category);

            await message.AppendIntegerAsync(TagCount);
            /* TODO CHECK */ foreach (var current in Tags) await message.AppendStringAsync(current);

            var enumType = 32;
            

            if (Group != null) enumType += 2;
            if (showEvents && Event != null) enumType += 4;
            if (Type == "private") enumType += 8;
            if (AllowPets) enumType += 16;

            await message.AppendIntegerAsync(enumType);

            if (Group != null)
            {

                await message.AppendIntegerAsync(Group.Id);
                await message.AppendStringAsync(Group.Name);
                await message.AppendStringAsync(Group.Badge);
            }
            if (showEvents && Event != null)
            {
                await message.AppendStringAsync(Event.Name);
                await message.AppendStringAsync(Event.Description);
                await message.AppendIntegerAsync((int) Math.Floor((Event.Time - Oblivion.GetUnixTimeStamp()) / 60.0));
            }
        }

        /// <summary>
        ///     Serializes the room data.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="session">The session.</param>
        /// <param name="isNotReload">if set to <c>true</c> [from view].</param>
        /// <param name="sendRoom">if set to <c>true</c> [send room].</param>
        /// <param name="show">if set to <c>true</c> [show].</param>
        internal async Task SerializeRoomData(ServerMessage message, GameClient session, bool isNotReload,
            bool? sendRoom = false, bool show = true)
        {
            var room = session.GetHabbo().CurrentRoom;

            await message.InitAsync(LibraryParser.OutgoingRequest("RoomDataMessageComposer"));
            message.AppendBool(show); //flatId
            await Serialize(message, true, !isNotReload);
            message.AppendBool(isNotReload);
            message.AppendBool(Oblivion.GetGame().GetNavigator() != null &&
                               Oblivion.GetGame().GetNavigator().GetPublicItem(Id) != null); // staffPick
            message.AppendBool(!isNotReload || session.GetHabbo().HasFuse("fuse_mod")); // bypass bell, pass ...
            message.AppendBool(room != null && room.RoomMuted); //roomMuted
            await message.AppendIntegerAsync(WhoCanMute);
            await message.AppendIntegerAsync(WhoCanKick);
            await message.AppendIntegerAsync(WhoCanBan);
            message.AppendBool(room != null && room.CheckRights(session, true));
            await message.AppendIntegerAsync(ChatType);
            await message.AppendIntegerAsync(ChatBalloon);
            await message.AppendIntegerAsync(ChatSpeed);
            await message.AppendIntegerAsync(ChatMaxDistance);
            await message.AppendIntegerAsync(ChatFloodProtection);
            if (sendRoom == null) return;

            if (sendRoom.Value)
            {
                room = Oblivion.GetGame().GetRoomManager().GetRoom(Id);
                if (room != null)
                    await room.SendMessageAsync(message);
            }
            else await session.SendMessageAsync(message);
        }

        public void Dispose()
        {
            _model = null;
            Event = null;
            BlockedCommands?.Clear();
            BlockedCommands = null;
            Group = null;
            RoomChat?.Clear();
            RoomChat = null;
            WordFilter?.Clear();
            WordFilter = null;
        }
    }
}