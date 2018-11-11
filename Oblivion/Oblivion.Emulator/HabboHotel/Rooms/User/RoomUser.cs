using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Oblivion.Configuration;
using Oblivion.HabboHotel.Camera;
using Oblivion.HabboHotel.Commands;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.PathFinding;
using Oblivion.HabboHotel.Pets;
using Oblivion.HabboHotel.RoomBots;
using Oblivion.HabboHotel.Rooms.Items.Games.Teams.Enums;
using Oblivion.HabboHotel.Rooms.Items.Games.Types.Freeze.Enum;
using Oblivion.HabboHotel.Rooms.User.Path;
using Oblivion.HabboHotel.Users;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;
using Oblivion.Security;
using Oblivion.Util;

namespace Oblivion.HabboHotel.Rooms.User
{
    public class RoomUser
    {
        /// <summary>
        ///     The _m client
        /// </summary>
        private GameClient _mClient;

        /// <summary>
        /// the last purchased photo
        /// </summary>
        public CameraPhotoPreview LastPhotoPreview;

        /// <summary>
        ///     The _m room
        /// </summary>
        private Room _mRoom;

        /// <summary>
        ///     The allow override
        /// </summary>
        internal bool AllowOverride;

        /// <summary>
        /// The interacting user id used in custom commands like :sex
        /// </summary>
        internal uint InteractingUser;

        /// <summary>
        ///     The banzai power up
        /// </summary>
        internal FreezePowerUp BanzaiPowerUp;

        /// <summary>
        ///     The bot ai
        /// </summary>
        internal BotAi BotAi;

        /// <summary>
        ///     The bot data
        /// </summary>
        internal RoomBot BotData;

        /// <summary>
        ///     The can walk
        /// </summary>
        internal bool CanWalk;

        /// <summary>
        ///     The carry item identifier
        /// </summary>
        internal int CarryItemId;

        /// <summary>
        ///     The carry timer
        /// </summary>
        internal int CarryTimer;

        /// <summary>
        ///     The current item effect
        /// </summary>
        internal int CurrentItemEffect;

        /// <summary>
        ///     The dance identifier
        /// </summary>
        internal int DanceId;

        /// <summary>
        ///     The fast walking
        /// </summary>
        internal bool FastWalking;


        /// <summary>
        ///     The following owner
        /// </summary>
        internal RoomUser FollowingOwner;

        /// <summary>
        ///     The freeze counter
        /// </summary>
        internal int FreezeCounter;

        /// <summary>
        ///     The freezed
        /// </summary>
        internal bool Freezed; //En el freeze

        /// <summary>
        ///     The freeze lives
        /// </summary>
        internal int FreezeLives;

        /// <summary>
        ///     The frozen
        /// </summary>
        internal bool Frozen; //por comando

        internal int FrozenTick;

        /// <summary>
        ///     The gate identifier
        /// </summary>
        internal string GateId;

        /// <summary>
        ///     The goal x
        /// </summary>
        internal int GoalX;

        internal int LastSelectedX, CopyX;
        internal int LastSelectedY, CopyY;

        /// <summary>
        ///     The goal y
        /// </summary>
        internal int GoalY;

        /// <summary>
        ///     The habbo identifier
        /// </summary>
        internal uint HabboId;

        /// <summary>
        ///     The handeling ball status
        /// </summary>
        internal int HandelingBallStatus = 0;

        /// <summary>
        /// This is the last time who ACH_RoomDecoHosting was progressed
        /// </summary>
        internal long LastHostingDate;


        /// <summary>
        /// This is the last time who ACH_RbTagB
        /// </summary>
        internal long LastRollerDate;


        /// <summary>
        ///     The horse identifier
        /// </summary>
        internal uint HorseId;

        /// <summary>
        ///     The idle time
        /// </summary>
        internal int IdleTime;

        /// <summary>
        ///     The interacting gate
        /// </summary>
        internal bool InteractingGate;

        /// <summary>
        ///     The internal room identifier
        /// </summary>
        internal int InternalRoomId;

        /// <summary>
        ///     The is asleep
        /// </summary>
        internal bool IsAsleep;


        /// <summary>
        ///     The is lying down
        /// </summary>
        internal bool IsLyingDown;

        /// <summary>
        ///     The is moonwalking
        /// </summary>
        internal bool IsMoonwalking;

        /// <summary>
        ///     The is sitting
        /// </summary>
        internal bool IsSitting;

        /// <summary>
        ///     The is spectator
        /// </summary>
        internal bool IsSpectator;

        /// <summary>
        ///     The is walking
        /// </summary>
        internal bool IsWalking;

        /// <summary>
        ///     The last bubble
        /// </summary>
        internal int LastBubble = 0;

        /// <summary>
        ///     The last interaction
        /// </summary>
        internal int LastInteraction;

        /// <summary>
        ///     The last item
        /// </summary>
        internal string LastItem = "0";

        /// <summary>
        ///     The locked tiles count
        /// </summary>
        internal int LockedTilesCount;

        /// <summary>
        ///     The love lock partner
        /// </summary>
        internal uint LoveLockPartner;

        /// <summary>
        ///     The on camping tent
        /// </summary>
        internal bool OnCampingTent;

        /// <summary>
        ///     The path
        /// </summary>
        internal List<Vector2D> Path = new List<Vector2D>();

        /// <summary>
        ///     The path recalc needed
        /// </summary>
        internal bool PathRecalcNeeded;

        /// <summary>
        ///     The path step
        /// </summary>
        internal int PathStep = 1;

        /// <summary>
        ///     The pet data
        /// </summary>
        internal Pet PetData;

        /// <summary>
        ///     The riding horse
        /// </summary>
        internal bool RidingHorse;

        /// <summary>
        ///     The room identifier
        /// </summary>
        internal uint RoomId;

        /// <summary>
        ///     The rot body
        /// </summary>
        internal int RotBody;

        /// <summary>
        ///     The rot head
        /// </summary>
        internal int RotHead;

        /// <summary>
        ///     The set step
        /// </summary>
        internal bool SetStep;

        /// <summary>
        ///     The set x
        /// </summary>
        internal int SetX;

        /// <summary>
        ///     The set y
        /// </summary>
        internal int SetY;

        /// <summary>
        ///     The set z
        /// </summary>
        internal double SetZ;

        /// <summary>
        ///     The shield active
        /// </summary>
        internal bool ShieldActive;

        /// <summary>
        ///     The shield counter
        /// </summary>
        internal int ShieldCounter;

        /// <summary>
        ///     The sign time
        /// </summary>
        internal int SignTime;

        /// <summary>
        ///     The sq state
        /// </summary>
        internal byte SqState;

        /// <summary>
        ///     The statusses
        /// </summary>
        internal ConcurrentDictionary<string, string> Statusses;

        /// <summary>
        ///     The team
        /// </summary>
        internal Team Team;

        /// <summary>
        ///     The tele delay
        /// </summary>
        internal int TeleDelay;

        /// <summary>
        ///     The teleport enabled
        /// </summary>
        internal bool TeleportEnabled;

        /// <summary>
        ///     The throw ball at goal
        /// </summary>
        internal bool ThrowBallAtGoal;

        /// <summary>
        ///     The update needed
        /// </summary>
        internal bool UpdateNeeded;

        internal int UpdateNeededCounter;

        /// <summary>
        ///     The user identifier
        /// </summary>
        internal uint UserId;

        internal int UserTimeInCurrentRoom;

        /// <summary>
        ///     The virtual identifier
        /// </summary>
        internal int VirtualId;

        /// <summary>
        ///     The x
        /// </summary>
        internal int X;

        /// <summary>
        ///     The y
        /// </summary>
        internal int Y;

        /// <summary>
        ///     The z
        /// </summary>
        internal double Z;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RoomUser" /> class.
        /// </summary>
        /// <param name="habboId">The habbo identifier.</param>
        /// <param name="roomId">The room identifier.</param>
        /// <param name="virtualId">The virtual identifier.</param>
        /// <param name="room">The room.</param>
        /// <param name="isSpectator">if set to <c>true</c> [is spectator].</param>
        internal RoomUser(uint habboId, uint roomId, int virtualId, Room room, bool isSpectator)
        {
            Freezed = false;
            HabboId = habboId;
            RoomId = roomId;
            VirtualId = virtualId;
            IdleTime = 0;
            X = 0;
            Y = 0;
            Z = 0.0;
            UserTimeInCurrentRoom = 0;
            RotHead = 0;
            RotBody = 0;
            UpdateNeeded = true;
            Statusses = new ConcurrentDictionary<string, string>();
            TeleDelay = -1;
            _mRoom = room;
            AllowOverride = false;
            CanWalk = true;
            IsSpectator = isSpectator;
            SqState = 3;
            InternalRoomId = 0;
            CurrentItemEffect = 0;
            FreezeLives = 0;
            InteractingGate = false;
            GateId = "0";
            LastInteraction = 0;
            LockedTilesCount = 0;
        }

        /// <summary>
        ///     Gets the coordinate.
        /// </summary>
        /// <value>The coordinate.</value>
        internal Point Coordinate => new Point(X, Y);

        /// <summary>
        ///     Gets the square behind.
        /// </summary>
        /// <value>The square behind.</value>
        internal Point SquareBehind
        {
            get
            {
                var x = X;
                var y = Y;

                switch (RotBody)
                {
                    case 0:
                        y++;
                        break;

                    case 1:
                        x--;
                        y++;
                        break;

                    case 2:
                        x--;
                        break;

                    case 3:
                        x--;
                        y--;
                        break;

                    case 4:
                        y--;
                        break;

                    case 5:
                        x++;
                        y--;
                        break;

                    case 6:
                        x++;
                        break;

                    case 7:
                        x++;
                        y++;
                        break;
                }

                return new Point(x, y);
            }
        }

        /// <summary>
        ///     Gets the square in front.
        /// </summary>
        /// <value>The square in front.</value>
        internal Point SquareInFront
        {
            get
            {
                var x = X + 1;
                var y = 0;
                switch (RotBody)
                {
                    case 0:
                        x = X;
                        y = Y - 1;
                        break;

                    case 1:
                        x = X + 1;
                        y = Y - 1;
                        break;

                    case 2:
                        x = X + 1;
                        y = Y;
                        break;

                    case 3:
                        x = X + 1;
                        y = Y + 1;
                        break;

                    case 4:
                        x = X;
                        y = Y + 1;
                        break;

                    case 5:
                        x = X - 1;
                        y = Y + 1;
                        break;

                    case 6:
                        x = X - 1;
                        y = Y;
                        break;

                    case 7:
                        x = X - 1;
                        y = Y - 1;
                        break;
                }

                return new Point(x, y);
            }
        }

        /// <summary>
        ///     Gets the square in front.
        /// </summary>
        /// <value>The square in front.</value>
        internal Point SquaresInFront(int count)
        {
            var x = X + count;
            var y = 0;
            switch (RotBody)
            {
                case 0:
                    x = X;
                    y = Y - count;
                    break;

                case 1:
                    x = X + count;
                    y = Y - count;
                    break;

                case 2:
                    x = X + count;
                    y = Y;
                    break;

                case 3:
                    x = X + count;
                    y = Y + count;
                    break;

                case 4:
                    x = X;
                    y = Y + count;
                    break;

                case 5:
                    x = X - count;
                    y = Y + count;
                    break;

                case 6:
                    x = X - count;
                    y = Y;
                    break;

                case 7:
                    x = X - count;
                    y = Y - count;
                    break;
            }

            return new Point(x, y);
        }


        /// <summary>
        ///     Gets a value indicating whether this instance is pet.
        /// </summary>
        /// <value><c>true</c> if this instance is pet; otherwise, <c>false</c>.</value>
        internal bool IsPet => IsBot && BotData.IsPet;

        /// <summary>
        ///     Gets the current effect.
        /// </summary>
        /// <value>The current effect.</value>
        internal int CurrentEffect
        {
            get
            {
                if (GetClient() == null || GetClient().GetHabbo() == null)
                    return 0;

                return GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().CurrentEffect;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is dancing.
        /// </summary>
        /// <value><c>true</c> if this instance is dancing; otherwise, <c>false</c>.</value>
        internal bool IsDancing => DanceId >= 1;

        /// <summary>
        ///     Gets a value indicating whether [needs autokick].
        /// </summary>
        /// <value><c>true</c> if [needs autokick]; otherwise, <c>false</c>.</value>
        internal bool NeedsAutokick => !IsBot &&
                                       (GetClient() == null || GetClient().GetHabbo() == null ||
                                        GetClient().GetHabbo().Rank <= 6u && IdleTime >= 1800);

        /// <summary>
        ///     Gets a value indicating whether this instance is trading.
        /// </summary>
        /// <value><c>true</c> if this instance is trading; otherwise, <c>false</c>.</value>
        internal bool IsTrading => !IsBot && Statusses.ContainsKey("trd");

        /// <summary>
        ///     Gets a value indicating whether this instance is bot.
        /// </summary>
        /// <value><c>true</c> if this instance is bot; otherwise, <c>false</c>.</value>
        internal bool IsBot => BotData != null;


        /// <summary>
        ///     Gets the name of the user.
        /// </summary>
        /// <returns>System.String.</returns>
        internal string GetUserName()
        {
            if (!IsBot)
                return GetClient()?.GetHabbo() != null ? GetClient().GetHabbo().UserName : string.Empty;
            if (!IsPet)
                return BotData == null ? string.Empty : BotData.Name;
            return PetData.Name;
        }

        /// <summary>
        ///     Determines whether this instance is owner.
        /// </summary>
        /// <returns><c>true</c> if this instance is owner; otherwise, <c>false</c>.</returns>
        internal bool IsOwner()
        {
            try
            {
                var currentRoom = GetRoom();
                return !IsBot && currentRoom != null && GetUserName() == currentRoom.RoomData.Owner;
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "IsOwner");
                return false;
            }
        }

        /// <summary>
        ///     Uns the idle.
        /// </summary>
        internal void UnIdle()
        {
            IdleTime = 0;
            if (!IsAsleep)
                return;
            IsAsleep = false;
            ApplyEffect(0);
            using (var sleep = new ServerMessage(LibraryParser.OutgoingRequest("RoomUserIdleMessageComposer")))
            {
                sleep.AppendInteger(VirtualId);
                sleep.AppendBool(false);
                GetRoom().SendMessage(sleep);
            }
        }

        private bool _disposed;

        /// <summary>
        ///     Disposes this instance.
        /// </summary>
        internal void Dispose()
        {
            if (_disposed) return;
            Statusses?.Clear();
            Path?.Clear();

            BotAi?.Dispose();
            _mRoom = null;
            _mClient = null;
            Statusses = null;
            Path = null;
            LastPhotoPreview = null;
            FollowingOwner = null;
            LastPhotoPreview = null;
            BotAi = null;
            BotData = null;
            PetData = null;
            _disposed = true;
        }

        /// <summary>
        ///     Chats the specified session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="msg">The MSG.</param>
        /// <param name="shout">if set to <c>true</c> [shout].</param>
        /// <param name="count">The count.</param>
        /// <param name="textColor">Color of the text.</param>
        internal void Chat(GameClient session, string msg, bool shout, int count, int textColor = 0)
        {
            if (IsPet || IsBot)
            {
                if (!IsPet)
                    textColor = 2;
                using (var botChatmsg = new ServerMessage())
                {
                    botChatmsg.Init(shout
                        ? LibraryParser.OutgoingRequest("ShoutMessageComposer")
                        : LibraryParser.OutgoingRequest("ChatMessageComposer"));
                    botChatmsg.AppendInteger(VirtualId);
                    botChatmsg.AppendString(msg);
                    botChatmsg.AppendInteger(0);
                    botChatmsg.AppendInteger(textColor);
                    botChatmsg.AppendInteger(0);
                    botChatmsg.AppendInteger(count);

                    var location = new Vector2D(X, Y);
                    GetRoom().SendMessageWithRange(location, botChatmsg);
                    return;
                }
            }

            if (session?.GetHabbo() == null)
                return;

            if (msg.Length > 100)
                return;

            if (!((msg.StartsWith(":deleteblackword ") || msg.StartsWith("ban")) && session.GetHabbo().Rank > 4) &&
                !BobbaFilter.CanTalk(session, msg))
                return;


            if (session.GetHabbo().Rank < 4 && GetRoom().CheckMute(session))
                return;


            UnIdle();
            if (!IsPet && !IsBot)
            {
                if (msg.StartsWith(":") && CommandsManager.TryExecute(msg.Substring(1), session))
                {
                    if (GetRoom() != null && GetRoom().GotWireds())
                        GetRoom().GetWiredHandler().ExecuteWired(Interaction.TriggerOnUserSayCommand, this, msg);

                    return;
                }

                if (GetRoom() == null || GetRoom().Disposed) return;

                var habbo = GetClient().GetHabbo();


                if (!habbo.CanTalk(true)) return;


                if (GetRoom().GotWireds())
                    if (GetRoom().GetWiredHandler().ExecuteWired(Interaction.TriggerOnUserSay, this, msg))
                        return;

                GetRoom().AddChatlog(session.GetHabbo().Id, msg, true);

                if (GetRoom()?
                        .RoomData?.WordFilter != null && GetRoom()
                        .RoomData.WordFilter.Count > 0)
                    msg = GetRoom()
                        .RoomData.WordFilter.Aggregate(msg,
                            (current, s) => Regex.Replace(current, Regex.Escape(s), "bobba", RegexOptions.IgnoreCase));


                habbo.Preferences.ChatColor = textColor;
            }
            else if (!IsPet)
                textColor = 2;

            var needReChange = false;
            var colorPrefix = session.GetHabbo().Prefixes[0];
            var prefix = session.GetHabbo().Prefixes[1];
            var colorBubble = session.GetHabbo().Prefixes[2];
            var name = session.GetHabbo().UserName;

            if (colorPrefix != "#000000" || !string.IsNullOrWhiteSpace(prefix) || colorBubble != "#000000")
            {
                name = $"<font color='#{colorPrefix}'>{prefix}</font> <font color='#{colorBubble}'>{name}</font>";
                ChangeName(name);
                needReChange = true;
            }

            using (var chatMsg = new ServerMessage())
            {
                chatMsg.Init(shout
                    ? LibraryParser.OutgoingRequest("ShoutMessageComposer")
                    : LibraryParser.OutgoingRequest("ChatMessageComposer"));
                chatMsg.AppendInteger(VirtualId);
                chatMsg.AppendString(msg);
                chatMsg.AppendInteger(ChatEmotions.GetEmotionsForText(msg));
                chatMsg.AppendInteger(textColor);
                chatMsg.AppendInteger(0);
                chatMsg.AppendInteger(count);
                GetRoom().BroadcastChatMessageWithRange(chatMsg, this, session.GetHabbo().Id);
                if (needReChange)
                {
                    ChangeName(GetUserName());
                }

                GetRoom().OnUserSay(this, msg, shout);
            }
        }

        private void ChangeName(string name)
        {
            using (var message =
                new ServerMessage(LibraryParser.OutgoingRequest("UserUpdateNameInRoomMessageComposer")))
            {
                message.AppendInteger(RoomId);
                message.AppendInteger(VirtualId);
                message.AppendString(name);
                GetRoom().SendMessage(message);
            }
        }

        /// <summary>
        ///     Clears the movement.
        /// </summary>
        internal void ClearMovement(bool everything = false)
        {
            IsWalking = false;
            GoalX = 0;
            GoalY = 0;
            SetStep = false;

            SetX = 0;
            SetY = 0;
            SetZ = 0.0;


            if (everything)
            {
                Path.Clear();
                PathRecalcNeeded = false;
                PathStep = 1;
                UpdateNeeded = true;
            }

            if (Statusses == null)
            {
                return;
            }

            if (Statusses.TryRemove("mv", out _))
                UpdateNeeded = true;
        }


        /// <summary>
        ///     Moves to.
        /// </summary>
        /// <param name="c">The c.</param>
        internal void MoveTo(Point c)
        {
            MoveTo(c.X, c.Y);
        }

        /// <summary>
        ///     Moves to.
        /// </summary>
        /// <param name="x">The p x.</param>
        /// <param name="y">The p y.</param>
        /// <param name="pOverride">if set to <c>true</c> [p override].</param>
        internal void MoveTo(int x, int y, bool pOverride)
        {
            if (x >= GetRoom().GetGameMap().GameMap.GetLength(0) ||
                y >= GetRoom().GetGameMap().GameMap.GetLength(1))
                return;

            if (TeleportEnabled)
            {
                UnIdle();
                using (var msg = _mRoom
                    .GetRoomItemHandler()
                    .UpdateUserOnRoller(this, new Point(x, y), 0u,
                        GetRoom().GetGameMap().SqAbsoluteHeight(GoalX, GoalY)))
                {
                    if (msg == null) return;
                    _mRoom?
                        .SendMessage(msg);
                }


                if (Statusses.ContainsKey("sit")) Z -= 0.35;
                UpdateNeeded = true;
                GetRoom().GetRoomUserManager().UpdateUserStatus(this, false);
                return;
            }

            if (!Gamemap.CanWalk(GetRoom().GetGameMap().GameMap[x, y], pOverride))
                return;
            if (Frozen)
            {
                return;
            }

            var coord = new Point(x, y);
            if (RidingHorse && !IsBot || IsPet)
            {
                var allRoomItemForSquare = GetRoom().GetGameMap().GetCoordinatedHeighestItems(coord);
                if (allRoomItemForSquare.Any(current =>
                    current.GetBaseItem().IsSeat ||
                    current.GetBaseItem().InteractionType == Interaction.LowPool ||
                    current.GetBaseItem().InteractionType == Interaction.Pool ||
                    current.GetBaseItem().InteractionType == Interaction.HaloweenPool ||
                    current.GetBaseItem().InteractionType == Interaction.Bed ||
                    current.GetBaseItem().InteractionType == Interaction.PressurePadBed ||
                    current.GetBaseItem().InteractionType == Interaction.Guillotine))
                    return;
            }

            UnIdle();
            GoalX = x;
            GoalY = y;
            LastSelectedX = x;
            LastSelectedY = y;

            PathRecalcNeeded = true;
            ThrowBallAtGoal = false;
        }

        /// <summary>
        ///     Moves to.
        /// </summary>
        /// <param name="pX">The p x.</param>
        /// <param name="pY">The p y.</param>
        internal void MoveTo(int pX, int pY)
        {
            MoveTo(pX, pY, false);
        }

        /// <summary>
        ///     Unlocks the walking.
        /// </summary>
        internal void UnlockWalking()
        {
            AllowOverride = false;
            CanWalk = true;
        }

        /// <summary>
        ///     Sets the position.
        /// </summary>
        /// <param name="pX">The p x.</param>
        /// <param name="pY">The p y.</param>
        /// <param name="pZ">The p z.</param>
        internal void SetPos(int pX, int pY, double pZ)
        {
            X = pX;
            Y = pY;
            Z = pZ;
        }

        /// <summary>
        ///     Carries the item.
        /// </summary>
        /// <param name="item">The item.</param>
        internal void CarryItem(int item)
        {
            CarryItemId = item;
            CarryTimer = item > 0 ? 240 : 0;
            using (var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("ApplyHanditemMessageComposer")))
            {
                serverMessage.AppendInteger(VirtualId);
                serverMessage.AppendInteger(item);
                _mRoom?.SendMessage(serverMessage);
            }
        }

        /// <summary>
        ///     Sets the rot.
        /// </summary>
        /// <param name="rotation">The rotation.</param>
        internal void SetRot(int rotation)
        {
            SetRot(rotation, false);
        }

        /// <summary>
        ///     Sets the rot.
        /// </summary>
        /// <param name="rotation">The rotation.</param>
        /// <param name="headOnly">if set to <c>true</c> [head only].</param>
        internal void SetRot(int rotation, bool headOnly)
        {
            if (Statusses == null) return;
            if (Statusses.ContainsKey("lay") || IsWalking) return;
            var num = RotBody - rotation;
            RotHead = RotBody;
            if (Statusses.ContainsKey("sit") || headOnly)
                switch (RotBody)
                {
                    case 4:
                    case 2:
                        if (num > 0) RotHead = RotBody - 1;
                        else if (num < 0) RotHead = RotBody + 1;
                        break;

                    case 6:
                    case 0:
                        if (num > 0) RotHead = RotBody - 1;
                        else if (num < 0) RotHead = RotBody + 1;
                        break;
                }
            else if (num <= -2 || num >= 2)
            {
                RotHead = rotation;
                RotBody = rotation;
            }
            else RotHead = rotation;

            UpdateNeeded = true;
        }


        /// <summary>
        ///     Adds the status.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        internal void AddStatus(string key, string value)
        {
            Statusses[key] = value;
        }

        /// <summary>
        ///     Removes the status.
        /// </summary>
        /// <param name="key">The key.</param>
        internal void RemoveStatus(string key)
        {
            Statusses.TryRemove(key, out _);
        }

        /// <summary>
        ///     Applies the effect.
        /// </summary>
        /// <param name="effectId">The effect identifier.</param>
        internal void ApplyEffect(int effectId)
        {
            if (IsBot || GetClient() == null || GetClient().GetHabbo() == null ||
                GetClient().GetHabbo().GetAvatarEffectsInventoryComponent() == null)
                return;
            GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(effectId);
        }

        /// <summary>
        ///     Serializes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        internal bool Serialize(ServerMessage message)
        {
            if (message == null)
                return false;
            if (IsSpectator)
                return false;
            if (!IsBot)
            {
                if (GetClient() == null || GetClient().GetHabbo() == null)
                    return false;
                var group = Oblivion.GetGame().GetGroupManager().GetGroup(GetClient().GetHabbo().FavouriteGroup);
                if (GetClient() == null || GetClient().GetHabbo() == null)
                    return false;
                var habbo = GetClient().GetHabbo();

                if (habbo == null)
                    return false;

                message.AppendInteger(habbo.Id);
                message.AppendString(habbo.UserName);
                message.AppendString(habbo.Motto);
                message.AppendString(habbo.Look);
                message.AppendInteger(VirtualId);
                message.AppendInteger(X);
                message.AppendInteger(Y);
                message.AppendString(TextHandling.GetString(Z));
                message.AppendInteger(0);
                message.AppendInteger(1);
                message.AppendString(habbo.Gender.ToLower());
                if (group != null)
                {
                    message.AppendInteger(group.Id);
                    message.AppendInteger(0);
                    message.AppendString(group.Name);
                }
                else
                {
                    message.AppendInteger(0);
                    message.AppendInteger(0);
                    message.AppendString("");
                }

                message.AppendString("");
                message.AppendInteger(habbo.AchievementPoints);
                message.AppendBool(false);
                return true;
            }

            if (BotAi == null || BotData == null)
                return false;

            message.AppendInteger(BotAi.BaseId);
            message.AppendString(BotData.Name);
            message.AppendString(BotData.Motto);
            if (BotData.AiType == AiType.Pet)
                if (PetData.Type == 16u)
                    message.AppendString(PetData.MoplaBreed.PlantData);
                else if (PetData.HaveSaddle == Convert.ToBoolean(2))
                    message.AppendString(string.Concat(BotData.Look.ToLower(), " 3 4 10 0 2 ", PetData.PetHair, " ",
                        PetData.HairDye, " 3 ", PetData.PetHair, " ", PetData.HairDye));
                else if (PetData.HaveSaddle == Convert.ToBoolean(1))
                    message.AppendString(string.Concat(BotData.Look.ToLower(), " 3 2 ", PetData.PetHair, " ",
                        PetData.HairDye, " 3 ", PetData.PetHair, " ", PetData.HairDye, " 4 9 0"));
                else
                    message.AppendString(string.Concat(BotData.Look.ToLower(), " 2 2 ", PetData.PetHair, " ",
                        PetData.HairDye, " 3 ", PetData.PetHair, " ", PetData.HairDye));
            else
                message.AppendString(BotData.Look.ToLower());
            message.AppendInteger(VirtualId);
            message.AppendInteger(X);
            message.AppendInteger(Y);
            message.AppendString(TextHandling.GetString(Z));
            message.AppendInteger(0);
            message.AppendInteger(BotData.AiType == AiType.Generic ? 4 : 2);
            if (BotData.AiType == AiType.Pet)
            {
                message.AppendInteger(PetData.Type);
                message.AppendInteger(PetData.OwnerId);
                message.AppendString(PetData.OwnerName);
                message.AppendInteger(PetData.Type == 16u ? 0 : 1);
                message.AppendBool(PetData.HaveSaddle);
                message.AppendBool(RidingHorse);
                message.AppendInteger(0);
                message.AppendInteger(PetData.Type == 16u ? 1 : 0);
                message.AppendString(PetData.Type == 16u ? PetData.MoplaBreed.GrowStatus : "");
                return true;
            }

            message.AppendString(BotData.Gender.ToLower());
            message.AppendInteger(BotData.OwnerId);
            message.AppendString(Oblivion.GetGame().GetClientManager().GetNameById(BotData.OwnerId));
            message.AppendInteger(5);
            message.AppendShort(1);
            message.AppendShort(2);
            message.AppendShort(3);
            message.AppendShort(4);
            message.AppendShort(5);
            return true;
        }

        /// <summary>
        ///     Serializes the status.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SerializeStatus(ServerMessage message)
        {
            if (Statusses == null) return;
            message.AppendInteger(VirtualId);
            message.AppendInteger(X);
            message.AppendInteger(Y);
            message.AppendString(TextHandling.GetString(Z));
            message.AppendInteger(RotHead);
            message.AppendInteger(RotBody);
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("/");
            if (IsPet && PetData.Type == 16u)
                stringBuilder.AppendFormat("/{0}{1}", PetData.MoplaBreed.GrowStatus, Statusses.Count >= 1 ? "/" : "");

            foreach (var current in Statusses)
            {
                stringBuilder.Append(current.Key);
                if (!string.IsNullOrEmpty(current.Value))
                {
                    stringBuilder.Append(" ");
                    stringBuilder.Append(current.Value);
                }

                stringBuilder.Append("/");
            }

            stringBuilder.Append("/");
            message.AppendString(stringBuilder.ToString());

            if (Statusses.TryRemove("sign", out _))
            {
                UpdateNeeded = true;
            }
        }

        /// <summary>
        ///     Serializes the status.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="status">The status.</param>
        internal void SerializeStatus(ServerMessage message, string status)
        {
            if (IsSpectator)
                return;
            message.AppendInteger(VirtualId);
            message.AppendInteger(X);
            message.AppendInteger(Y);
            message.AppendString(TextHandling.GetString(SetZ));
            message.AppendInteger(RotHead);
            message.AppendInteger(RotBody);
            message.AppendString(status);
        }

        /// <summary>
        ///     Gets the client.
        /// </summary>
        /// <returns>GameClient.</returns>
        internal GameClient GetClient()
        {
            if (IsBot)
                return null;

            if (_mClient != null)
                return _mClient;

            return _mClient = Oblivion.GetGame().GetClientManager().GetClientByUserId(HabboId);
        }

        /// <summary>
        ///     Gets the room.
        /// </summary>
        /// <returns>Room.</returns>
        internal Room GetRoom() => _mRoom;
    }
}