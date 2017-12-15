using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Oblivion.Configuration;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Datas;
using Oblivion.HabboHotel.Items.Handlers;
using Oblivion.HabboHotel.Items.Interactions;
using Oblivion.HabboHotel.Items.Interactions.Controllers;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.HabboHotel.Items.Interactions.Interfaces;
using Oblivion.HabboHotel.Items.Wired.Handlers;
using Oblivion.HabboHotel.Pathfinding;
using Oblivion.HabboHotel.Pets;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.Items.Games.Teams.Enums;
using Oblivion.HabboHotel.Rooms.Items.Games.Types.Freeze.Enum;
using Oblivion.HabboHotel.Rooms.Items.Games.Types.Soccer.Enums;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.HabboHotel.Rooms.User.Path;
using Oblivion.HabboHotel.SoundMachine;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;
using Oblivion.Util;

namespace Oblivion.HabboHotel.Items.Interfaces
{
    /// <summary>
    ///     Class RoomItem.
    /// </summary>
    public class RoomItem
    {
        /// <summary>
        ///     The _m base item
        /// </summary>
        private Item _mBaseItem;

        /// <summary>
        ///     The _m room
        /// </summary>
        private Room _mRoom;

        /// <summary>
        ///     The _update needed
        /// </summary>
        private bool _updateNeeded;

        /// <summary>
        ///     The ball is moving
        /// </summary>
        internal bool BallIsMoving;

        /// <summary>
        ///     The ball value
        /// </summary>
        internal int BallValue;

        /// <summary>
        ///     The base item
        /// </summary>
        internal uint BaseItem;

        /// <summary>
        ///     The come direction
        /// </summary>
        internal IComeDirection ComeDirection;

        /// <summary>
        ///     The extra data
        /// </summary>
        internal string ExtraData;

        /// <summary>
        ///     The freeze power up
        /// </summary>
        internal FreezePowerUp FreezePowerUp;

        /// <summary>
        ///     The group data
        /// </summary>
        internal string GroupData;

        /// <summary>
        ///     The group identifier
        /// </summary>
        internal uint GroupId;

        /// <summary>
        ///     The highscore data
        /// </summary>
        internal HighscoreData HighscoreData;

        /// <summary>
        ///     The identifier
        /// </summary>
        internal long Id;

        /// <summary>
        /// the virtual id
        /// </summary>
        internal uint VirtualId;

        /// <summary>
        ///     The interacting ball user
        /// </summary>
        internal GameClient InteractingBallUser;

        /// <summary>
        ///     The interacting user
        /// </summary>
        internal uint InteractingUser;

        /// <summary>
        ///     The interacting user2
        /// </summary>
        internal uint InteractingUser2;

        /// <summary>
        ///     The interaction count
        /// </summary>
        public byte InteractionCount;

        /// <summary>
        ///     The interaction count helper
        /// </summary>
        internal byte InteractionCountHelper;

        /// <summary>
        ///     The is builder
        /// </summary>
        internal bool IsBuilder;

        /// <summary>
        ///     The is trans
        /// </summary>
        internal bool IsTrans;

        /// <summary>
        ///     The limited no
        /// </summary>
        internal int LimitedNo;

        /// <summary>
        ///     The limited tot
        /// </summary>
        internal int LimitedTot;

        /// <summary>
        ///     The magic remove
        /// </summary>
        internal bool MagicRemove;

        internal MovementDirection MoveToDirMovement = MovementDirection.None;

        /// <summary>
        ///     The on cannon acting
        /// </summary>
        internal bool OnCannonActing = false;

        /// <summary>
        ///     The pending reset
        /// </summary>
        internal bool PendingReset;

        /// <summary>
        ///     The pets list
        /// </summary>
        internal List<Pet> PetsList = new List<Pet>(2);

        /// <summary>
        ///     The room identifier
        /// </summary>
        internal uint RoomId;

        /// <summary>
        ///     The rot
        /// </summary>
        internal int Rot;

        /// <summary>
        ///     The song code
        /// </summary>
        internal string SongCode;

        /// <summary>
        ///     The team
        /// </summary>
        internal Team Team;

        /// <summary>
        ///     The update counter
        /// </summary>
        internal int UpdateCounter;

        /// <summary>
        ///     The user identifier
        /// </summary>
        internal uint UserId;

        /// <summary>
        ///     The value
        /// </summary>
        internal int Value;

        /// <summary>
        ///     The viking cotie burning
        /// </summary>
        internal bool VikingCotieBurning;

        /// <summary>
        ///     The wall coord
        /// </summary>
        internal WallCoordinate WallCoord;


        /// <summary>
        ///     Initializes a new instance of the <see cref="RoomItem" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="roomId">The room identifier.</param>
        /// <param name="baseItem">The base item.</param>
        /// <param name="extraData">The extra data.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        /// <param name="rot">The rot.</param>
        /// <param name="pRoom">The p room.</param>
        /// <param name="userid">The userid.</param>
        /// <param name="eGroup">The group.</param>
        /// <param name="songCode">The song code.</param>
        /// <param name="isBuilder">if set to <c>true</c> [is builder].</param>
        internal RoomItem(long id, uint roomId, uint baseItem, string extraData, int x, int y, double z, int rot,
            Room pRoom, uint userid, uint eGroup, string songCode, bool isBuilder, int limNo, int limStack)
        {
            Id = id;

            VirtualId = Oblivion.GetGame().GetItemManager().GetVirtualId(id);
            RoomId = roomId;
            BaseItem = baseItem;
            ExtraData = extraData;
            GroupId = eGroup;
            X = x;
            Y = y;
            if (!double.IsInfinity(z)) Z = z;
            Rot = rot;
            UpdateNeeded = false;
            UpdateCounter = 0;
            InteractingUser = 0u;
            InteractingUser2 = 0u;
            IsTrans = false;
            InteractingBallUser = null;
            InteractionCount = 0;
            Value = 0;
            UserId = userid;
            SongCode = songCode;
            IsBuilder = isBuilder;

            _mBaseItem = Oblivion.GetGame().GetItemManager().GetItem(baseItem);
            _mRoom = pRoom;

            if (_mBaseItem == null) return;

            LimitedNo = limNo;
            LimitedTot = limStack;
            if (_mBaseItem.Name.ContainsAny("guild_", "grp", "gld_"))
            {
                GroupData = extraData;
                ExtraData = GroupData.Split(';')[0];
                if (GroupData.Contains(";;"))
                {
                    GroupData = GroupData.Replace(";;", ";");
                    _mRoom.GetRoomItemHandler().AddOrUpdateItem(id);
                }
            }

            switch (_mBaseItem.InteractionType)
            {
                case Interaction.PressurePadBed:
                case Interaction.Bed:
                    pRoom.ContainsBeds++;
                    break;

                case Interaction.Hopper:
                    IsTrans = true;
                    ReqUpdate(0, true);
                    break;

                case Interaction.Teleport:
                case Interaction.QuickTeleport:
                    IsTrans = true;
                    ReqUpdate(0, true);
                    break;

                case Interaction.Roller:
                    IsRoller = true;
                    pRoom.GetRoomItemHandler().GotRollers = true;
                    break;

                case Interaction.FootballCounterGreen:
                case Interaction.BanzaiGateGreen:
                case Interaction.BanzaiScoreGreen:
                case Interaction.FreezeGreenCounter:
                case Interaction.FreezeGreenGate:
                    Team = Team.Green;
                    break;

                case Interaction.FootballCounterYellow:
                case Interaction.BanzaiGateYellow:
                case Interaction.BanzaiScoreYellow:
                case Interaction.FreezeYellowCounter:
                case Interaction.FreezeYellowGate:
                    Team = Team.Yellow;
                    break;

                case Interaction.FootballCounterBlue:
                case Interaction.BanzaiGateBlue:
                case Interaction.BanzaiScoreBlue:
                case Interaction.FreezeBlueCounter:
                case Interaction.FreezeBlueGate:
                    Team = Team.Blue;
                    break;

                case Interaction.FootballCounterRed:
                case Interaction.BanzaiGateRed:
                case Interaction.BanzaiScoreRed:
                case Interaction.FreezeRedCounter:
                case Interaction.FreezeRedGate:
                    Team = Team.Red;
                    break;

                case Interaction.BanzaiFloor:
                case Interaction.BanzaiCounter:
                case Interaction.BanzaiPuck:
                case Interaction.BanzaiPyramid:
                case Interaction.FreezeTimer:
                case Interaction.FreezeExit:
                    break;

                case Interaction.BanzaiTele:
                    ExtraData = string.Empty;
                    break;

                case Interaction.BreedingTerrier:
                    if (!pRoom.GetRoomItemHandler().BreedingTerrier.ContainsKey(Id))
                        pRoom.GetRoomItemHandler().BreedingTerrier.Add(Id, this);
                    break;

                case Interaction.BreedingBear:
                    if (!pRoom.GetRoomItemHandler().BreedingBear.ContainsKey(Id))
                        pRoom.GetRoomItemHandler().BreedingBear.Add(Id, this);
                    break;

                case Interaction.VikingCotie:
                    if (int.TryParse(extraData, out var num) && num >= 1 && num < 5) VikingCotieBurning = true;
                    break;
            }

            IsWallItem = (_mBaseItem.Type.ToString().ToLower() == "i");
            IsFloorItem = (_mBaseItem.Type.ToString().ToLower() == "s");
            AffectedTiles = Gamemap.GetAffectedTiles(_mBaseItem.Length, _mBaseItem.Width, X, Y, rot);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RoomItem" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="roomId">The room identifier.</param>
        /// <param name="baseItem">The base item.</param>
        /// <param name="extraData">The extra data.</param>
        /// <param name="wallCoord">The wall coord.</param>
        /// <param name="pRoom">The p room.</param>
        /// <param name="userid">The userid.</param>
        /// <param name="eGroup">The group.</param>
        /// <param name="isBuilder">if set to <c>true</c> [is builder].</param>
        internal RoomItem(long id, uint roomId, uint baseItem, string extraData, WallCoordinate wallCoord, Room pRoom,
            uint userid, uint eGroup, bool isBuilder, uint addedVirtual)
        {
            BaseItem = baseItem;

            _mBaseItem = Oblivion.GetGame().GetItemManager().GetItem(baseItem);
            _mRoom = pRoom;

            if (_mBaseItem == null) Logging.LogException($"Unknown baseID: {baseItem}");

            Id = id;

            VirtualId = addedVirtual <= 0 ? Oblivion.GetGame().GetItemManager().GetVirtualId(id) : addedVirtual;

            RoomId = roomId;
            ExtraData = extraData;
            GroupId = eGroup;
            X = 0;
            Y = 0;
            Z = 0.0;
            UpdateNeeded = false;
            UpdateCounter = 0;
            InteractingUser = 0u;
            InteractingUser2 = 0u;
            IsTrans = false;
            InteractingBallUser = null;
            InteractionCount = 0;
            Value = 0;
            WallCoord = wallCoord;
            UserId = userid;
            IsBuilder = isBuilder;
            IsWallItem = true;
            IsFloorItem = false;
            AffectedTiles = new Dictionary<int, ThreeDCoord>();
            SongCode = string.Empty;
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is wired.
        /// </summary>
        /// <value><c>true</c> if this instance is wired; otherwise, <c>false</c>.</value>
        public bool IsWired => InteractionTypes.AreFamiliar(GlobalInteractions.Wired, GetBaseItem().InteractionType);

        /// <summary>
        ///     Gets the affected tiles.
        /// </summary>
        /// <value>The affected tiles.</value>
        internal Dictionary<int, ThreeDCoord> AffectedTiles { get; private set; }

        /// <summary>
        ///     Gets the x.
        /// </summary>
        /// <value>The x.</value>
        internal int X { get; private set; }
        

        /// <summary>
        ///     Gets the y.
        /// </summary>
        /// <value>The y.</value>
        internal int Y { get; private set; }

        /// <summary>
        ///     Gets the z.
        /// </summary>
        /// <value>The z.</value>
        internal double Z { get; private set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [update needed].
        /// </summary>
        /// <value><c>true</c> if [update needed]; otherwise, <c>false</c>.</value>
        internal bool UpdateNeeded
        {
            get => _updateNeeded;
            set
            {
                if (value) GetRoom().GetRoomItemHandler().QueueRoomItemUpdate(this);
                _updateNeeded = value;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is roller.
        /// </summary>
        /// <value><c>true</c> if this instance is roller; otherwise, <c>false</c>.</value>
        internal bool IsRoller { get; }

        /// <summary>
        ///     Gets the coordinate.
        /// </summary>
        /// <value>The coordinate.</value>
        internal Point Coordinate => new Point(X, Y);

        /// <summary>
        ///     Gets the get coords.
        /// </summary>
        /// <value>The get coords.</value>
        internal List<Point> GetCoords()
        {
            
                var list = new List<Point> {Coordinate};
                list.AddRange(AffectedTiles.Values.Select(current => new Point(current.X, current.Y)));
                return list;
            
        }

        internal double Height
        {
            get
            {
                try
                {
                    if (GetBaseItem() == null) return 0;
                    if (!GetBaseItem().StackMultipler) return GetBaseItem().Height;
                    if (string.IsNullOrEmpty(ExtraData)) ExtraData = "0";
                    return !int.TryParse(ExtraData, out int data) ? Z : GetBaseItem().ToggleHeight[data];
                }
                catch (Exception e)
                {
                    Writer.Writer.LogException("TotalHeight with furni BaseId: " + BaseItem + " in RoomId:" + RoomId +
                                               Environment.NewLine + e);
                    return 0;
                }
            }
        }

        /// <summary>
        ///     Gets the total height.
        /// </summary>
        /// <value>The total height.</value>
        internal double TotalHeight
        {
            get
            {
                try
                {
                    var curHeight = 0.0;

                    if (GetBaseItem() == null) return Z;
                    if (!GetBaseItem().StackMultipler) return Z + GetBaseItem().Height;
                    if (string.IsNullOrEmpty(ExtraData)) ExtraData = "0";
                    if (GetBaseItem().ToggleHeight.Length > 1)
                        if (int.TryParse(ExtraData, out int num2) && GetBaseItem().ToggleHeight.Length - 1 >= num2)
                            curHeight = Z + GetBaseItem().ToggleHeight[num2];

                    if (curHeight <= 0.0)
                        curHeight = Z + GetBaseItem().Height;

                    return curHeight;
                }
                catch (Exception e)
                {
                    Writer.Writer.LogException("TotalHeight with furni BaseId: " + BaseItem + " in RoomId:" + RoomId +
                                               Environment.NewLine + e);
                    return 1;
                }
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is wall item.
        /// </summary>
        /// <value><c>true</c> if this instance is wall item; otherwise, <c>false</c>.</value>
        internal bool IsWallItem { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this instance is floor item.
        /// </summary>
        /// <value><c>true</c> if this instance is floor item; otherwise, <c>false</c>.</value>
        internal bool IsFloorItem { get; set; }

        /// <summary>
        ///     Gets the square in front.
        /// </summary>
        /// <value>The square in front.</value>
        internal Point SquareInFront
        {
            get
            {
                var result = new Point(X, Y);
                {
                    switch (Rot)
                    {
                        case 0:
                            result.Y--;
                            break;

                        case 2:
                            result.X++;
                            break;

                        case 4:
                            result.Y++;
                            break;

                        case 6:
                            result.X--;
                            break;
                    }
                    return result;
                }
            }
        }

        /// <summary>
        ///     Gets the square behind.
        /// </summary>
        /// <value>The square behind.</value>
        internal Point SquareBehind
        {
            get
            {
                var result = new Point(X, Y);
                {
                    switch (Rot)
                    {
                        case 0:
                            result.Y++;
                            break;

                        case 2:
                            result.X--;
                            break;

                        case 4:
                            result.Y--;
                            break;

                        case 6:
                            result.X++;
                            break;
                    }
                    return result;
                }
            }
        }


        public Point SquareLeft
        {
            get
            {
                var Sq = new Point(X, Y);

                switch (Rot)
                {
                    case 0:
                        Sq.X++;
                        break;
                    case 2:
                        Sq.Y--;
                        break;
                    case 4:
                        Sq.X--;
                        break;
                    case 6:
                        Sq.Y++;
                        break;
                }

                return Sq;
            }
        }

        public Point SquareRight
        {
            get
            {
                var Sq = new Point(X, Y);

                switch (Rot)
                {
                    case 0:
                        Sq.X--;
                        break;
                    case 2:
                        Sq.Y++;
                        break;
                    case 4:
                        Sq.X++;
                        break;
                    case 6:
                        Sq.Y--;
                        break;
                }
                return Sq;
            }
        }

        /// <summary>
        ///     Gets the interactor.
        /// </summary>
        /// <value>The interactor.</value>
        internal IFurniInteractor Interactor
        {
            get
            {
                if (IsWired) return new InteractorWired();
                var interactionType = GetBaseItem().InteractionType;
                switch (interactionType)
                {
                    case Interaction.Roller:
                        return new InteractorRoller();

                    case Interaction.Gate:
                        return new InteractorGate();

                    case Interaction.GuildGate:
                        return new InteractorGroupGate();

//                    case Interaction.ScoreBoard:
//                        return new InteractorScoreboard();

                    case Interaction.VendingMachine:
                        return new InteractorVendor();

                    case Interaction.Alert:
                        return new InteractorAlert();

                    case Interaction.OneWayGate:
                        return new InteractorOneWayGate();

                    case Interaction.LoveShuffler:
                        return new InteractorLoveShuffler();

                    case Interaction.HabboWheel:
                        return new InteractorHabboWheel();

                    case Interaction.Dice:
                        return new InteractorDice();

                    case Interaction.Bottle:
                        return new InteractorSpinningBottle();

                    case Interaction.Hopper:
                        return new InteractorHopper();

                    case Interaction.Teleport:
                        return new InteractorTeleport();

                    case Interaction.Football:
                        return new InteractorFootball();

                    case Interaction.FootballCounterGreen:
                    case Interaction.FootballCounterYellow:
                    case Interaction.FootballCounterBlue:
                    case Interaction.FootballCounterRed:
                    case Interaction.ScoreBoard:
                        return new InteractorScoreCounter();

                    case Interaction.BanzaiScoreBlue:
                    case Interaction.BanzaiScoreRed:
                    case Interaction.BanzaiScoreYellow:
                    case Interaction.BanzaiScoreGreen:
                        return new InteractorBanzaiScoreCounter();

                    case Interaction.BanzaiCounter:
                        return new InteractorBanzaiTimer();

                    case Interaction.FreezeTimer:
                        return new InteractorFreezeTimer();

                    case Interaction.FreezeYellowCounter:
                    case Interaction.FreezeRedCounter:
                    case Interaction.FreezeBlueCounter:
                    case Interaction.FreezeGreenCounter:
                        return new InteractorFreezeScoreCounter();

                    case Interaction.FreezeTileBlock:
                    case Interaction.FreezeTile:
                        return new InteractorFreezeTile();

                    case Interaction.JukeBox:
                        return new InteractorJukebox();

                    case Interaction.PuzzleBox:
                        return new InteractorPuzzleBox();

                    case Interaction.Mannequin:
                        return new InteractorMannequin();

                    case Interaction.Fireworks:
                        return new InteractorFireworks();

                    case Interaction.GroupForumTerminal:
                        return new InteractorGroupForumTerminal();

                    case Interaction.VikingCotie:
                        return new InteractorVikingCotie();

                    case Interaction.Cannon:
                        return new InteractorCannon();

                    case Interaction.FxBox:
                        return new InteractorFxBox();

                    case Interaction.HcGate:
                        return new InteractorHcGate();

                    case Interaction.QuickTeleport:
                        return new InteractorQuickTeleport();

                    case Interaction.CrackableEgg:
                        return new InteractorCrackableEgg();

                    case Interaction.FloorSwitch1:
                    case Interaction.FloorSwitch2:
                        return new InteractorSwitch();

                    case Interaction.WalkInternalLink:
                        return new InteractorWalkInternalLink();

                    case Interaction.FloorSwitch:
                        return new InteractorWalkSwitch();
                    case Interaction.Totem:
                        return new InteractorTotem();

                    default:
                        return new InteractorGenericSwitch();
                }
            }
        }

        /// <summary>
        ///     Equalses the specified compared item.
        /// </summary>
        /// <param name="comparedItem">The compared item.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Equals(RoomItem comparedItem) => comparedItem?.Id == Id;


        internal void SetState(int x, int y, double z)
        {
            SetState(x, y, z, Gamemap.GetAffectedTiles(GetBaseItem().Length,
                GetBaseItem().Width, x, y, Rot));
        }

        /// <summary>
        ///     Sets the state.
        /// </summary>
        /// <param name="x">The p x.</param>
        /// <param name="y">The p y.</param>
        /// <param name="z">The p z.</param>
        /// <param name="tiles">The tiles.</param>
        internal void SetState(int x, int y, double z, Dictionary<int, ThreeDCoord> tiles)
        {
            X = x;
            Y = y;

            if (!double.IsInfinity(z)) Z = z;
            AffectedTiles = tiles;
        }


        /// <summary>
        ///     Destroys this instance.
        /// </summary>
        internal void Destroy()
        {
            Oblivion.GetGame().GetItemManager().RemoveVirtualItem(Id);
            _mBaseItem = null;
            _mRoom = null;
            AffectedTiles.Clear();
            HighscoreData = null;
            PetsList?.Clear();
            PetsList = null;
            WallCoord = null;
            InteractingBallUser = null;
        }

        /// <summary>
        ///     Processes the updates.
        /// </summary>
        internal void ProcessUpdates()
        {
            UpdateCounter--;
            if (UpdateCounter <= 0 || IsTrans)
            {
                UpdateNeeded = false;
                UpdateCounter = 0;

                var interactionType = GetBaseItem().InteractionType;

                switch (interactionType)
                {
                    case Interaction.ScoreBoard:
                    {
                        if (string.IsNullOrEmpty(ExtraData)) break;
                        int.TryParse(ExtraData, out var num);
                        if (num > 0)
                        {
                            if (InteractionCountHelper == 1)
                            {
                                num--;
                                InteractionCountHelper = 0;
                                ExtraData = num.ToString();
                                UpdateState();
                            }
                            else InteractionCountHelper++;

                            UpdateCounter = 1;
                        }
                        else
                        {
                            UpdateCounter = 0;
                        }
                        break;
                    }
                    case Interaction.VendingMachine:

                        if (ExtraData == "1")
                        {
                            var user = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);

                            if (user != null)
                            {
                                user.UnlockWalking();

                                var drink =
                                    GetBaseItem().VendingIds[
                                        Oblivion.GetRandomNumber(0, (GetBaseItem().VendingIds.Count - 1))];
                                user.CarryItem(drink);

                                /*if (GetBaseItem().VendingIds.Count > 0)
                                {
                                    var item =
                                        GetBaseItem().VendingIds[RandomNumber.Get(0, GetBaseItem().VendingIds.Count - 1)];
                                    roomUser2.CarryItem(item);
                                }
                                */
                            }
                            InteractingUser = 0u;
                            ExtraData = "0";
                            UpdateState(false, true);
                        }
                        break;

                    case Interaction.Alert:
                        if (ExtraData == "1")
                        {
                            ExtraData = "0";
                            UpdateState(false, true);
                        }
                        break;

                    case Interaction.OneWayGate:
                    {
                        RoomUser roomUser3 = null;
                        if (InteractingUser > 0u)
                            roomUser3 = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);
                        if (roomUser3 != null && roomUser3.X == X && roomUser3.Y == Y)
                        {
                            ExtraData = "1";
                            roomUser3.MoveTo(SquareBehind);
                            roomUser3.InteractingGate = false;
                            roomUser3.GateId = 0u;
                            ReqUpdate(1, false);
                            UpdateState(false, true);
                        }
                        else if (roomUser3 != null && roomUser3.Coordinate == SquareBehind)
                        {
                            roomUser3.UnlockWalking();
                            ExtraData = "0";
                            InteractingUser = 0u;
                            roomUser3.InteractingGate = false;
                            roomUser3.GateId = 0u;
                            UpdateState(false, true);
                        }
                        else if (ExtraData == "1")
                        {
                            ExtraData = "0";
                            UpdateState(false, true);
                        }
                        if (roomUser3 == null) InteractingUser = 0u;
                        break;
                    }
                    case Interaction.LoveShuffler:
                        if (ExtraData == "0")
                        {
                            ExtraData = RandomNumber.Get(1, 4).ToString();
                            ReqUpdate(20, false);
                        }
                        else ExtraData = "-1";
                        UpdateState(false, true);
                        return;

                    case Interaction.HabboWheel:
                        ExtraData = RandomNumber.Get(1, 10).ToString();
                        UpdateState();
                        return;

                    case Interaction.Dice:
                        ExtraData = RandomNumber.Get(1, 7).ToString();
                        UpdateState();
                        return;

                    case Interaction.Bottle:
                        ExtraData = RandomNumber.Get(0, 7).ToString();
                        UpdateState();
                        return;

                    case Interaction.Hopper:
                    {
                        bool flag = false, flag2 = false;
                        var num2 = 0;
                        if (InteractingUser > 0u)
                        {
                            var roomUser4 = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);
                            if (roomUser4 != null)
                            {
                                if (roomUser4.Coordinate == Coordinate)
                                {
                                    roomUser4.AllowOverride = false;
                                    if (roomUser4.TeleDelay == 0)
                                    {
                                        var aHopper = HopperHandler.GetAHopper(roomUser4.RoomId);
                                        var hopperId = HopperHandler.GetHopperId(aHopper);
                                        if (!roomUser4.IsBot && roomUser4.GetClient() != null &&
                                            roomUser4.GetClient().GetHabbo() != null &&
                                            roomUser4.GetClient().GetMessageHandler() != null)
                                        {
                                            roomUser4.GetClient().GetHabbo().IsHopping = true;
                                            roomUser4.GetClient().GetHabbo().HopperId = hopperId;
                                            var roomFwd =
                                                new ServerMessage(
                                                    LibraryParser.OutgoingRequest("RoomForwardMessageComposer"));
                                            roomFwd.AppendInteger(aHopper);
                                            roomUser4.GetClient().SendMessage(roomFwd);
                                            InteractingUser = 0u;
                                        }
                                    }
                                    else
                                    {
                                        roomUser4.TeleDelay--;
                                        flag = true;
                                    }
                                }
                                else if (roomUser4.Coordinate == SquareInFront)
                                {
                                    roomUser4.AllowOverride = true;
                                    flag2 = true;
                                    if (roomUser4.IsWalking && (roomUser4.GoalX != X || roomUser4.GoalY != Y))
                                        roomUser4.ClearMovement();
                                    roomUser4.CanWalk = false;
                                    roomUser4.AllowOverride = true;
                                    roomUser4.MoveTo(Coordinate.X, Coordinate.Y, true);
                                }
                                else InteractingUser = 0u;
                            }
                            else InteractingUser = 0u;
                        }
                        if (InteractingUser2 > 0u)
                        {
                            var roomUserByHabbo = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser2);
                            if (roomUserByHabbo != null)
                            {
                                flag2 = true;
                                roomUserByHabbo.UnlockWalking();
                                roomUserByHabbo.MoveTo(SquareInFront);
                            }
                            InteractingUser2 = 0u;
                        }
                        if (flag2)
                        {
                            if (ExtraData != "1")
                            {
                                ExtraData = "1";
                                UpdateState(false, true);
                            }
                        }
                        else if (flag)
                        {
                            if (ExtraData != "2")
                            {
                                ExtraData = "2";
                                UpdateState(false, true);
                            }
                        }
                        else if (ExtraData != "0")
                        {
                            if (num2 == 0)
                            {
                                ExtraData = "0";
                                UpdateState(false, true);
                            }
                            else num2--;
                        }
                        ReqUpdate(1, false);
                        return;
                    }
                    case Interaction.Teleport:
                    case Interaction.QuickTeleport:
                    {
                        bool keepDoorOpen = false, showTeleEffect = false;
                        if (InteractingUser > 0)
                        {
                            var user = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);
                            if (user != null)
                            {
                                if (user.Coordinate == Coordinate)
                                {
                                    user.AllowOverride = false;

                                    if (TeleHandler.IsTeleLinked(Id, _mRoom))
                                    {
                                        showTeleEffect = true;
                                        var linkedTele = TeleHandler.GetLinkedTele(Id, _mRoom);
                                        var teleRoomId = TeleHandler.GetTeleRoomId(linkedTele, _mRoom);
                                        if (teleRoomId == RoomId)
                                        {
                                            var item = GetRoom().GetRoomItemHandler().GetItem(linkedTele);
                                            if (item == null)
                                            {
                                                user.UnlockWalking();
                                            }
                                            else
                                            {
                                                user.SetPos(item.X, item.Y, item.Z);
                                                user.SetRot(item.Rot, false);
                                                item.ExtraData = "2";
                                                item.UpdateState(false, true);
                                                item.InteractingUser2 = InteractingUser;
                                            }
                                        }
                                        else
                                        {
                                            if (!user.IsBot && user.GetClient()?.GetHabbo() != null &&
                                                user.GetClient().GetMessageHandler() != null)
                                            {
                                                user.GetClient().GetHabbo().IsTeleporting = true;
                                                user.GetClient().GetHabbo().TeleportingRoomId = teleRoomId;
                                                user.GetClient().GetHabbo().TeleporterId = linkedTele;
                                                user.GetClient()
                                                    .GetMessageHandler()
                                                    .PrepareRoomForUser(teleRoomId, string.Empty);
                                            }
                                            InteractingUser = 0u;
                                        }
                                    }
                                    else
                                    {
                                        user.UnlockWalking();
                                        InteractingUser = 0;
                                    }
                                }
                                else if (user.Coordinate == SquareInFront)
                                {
                                    user.AllowOverride = true;
                                    keepDoorOpen = true;
                                    if (user.IsWalking && (user.GoalX != X || user.GoalY != Y)) user.ClearMovement();

                                    user.CanWalk = false;
                                    user.AllowOverride = true;

                                    user.MoveTo(Coordinate.X, Coordinate.Y, true);
                                }
                                else
                                {
                                    InteractingUser = 0;
                                }
                            }
                            else
                            {
                                InteractingUser = 0;
                            }
                        }
                        if (InteractingUser2 > 0)
                        {
                            var user2 = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser2);
                            if (user2 != null)
                            {
                                keepDoorOpen = true;
                                user2.UnlockWalking();
                                user2.MoveTo(SquareInFront);
                            }
                            InteractingUser2 = 0;
                        }
                        if (keepDoorOpen)
                        {
                            if (ExtraData != "1")
                            {
                                ExtraData = "1";
                                UpdateState(false, true);
                            }
                        }
                        else if (showTeleEffect)
                        {
                            if (ExtraData != "2")
                            {
                                ExtraData = "2";
                                UpdateState(false, true);
                            }
                        }
                        else if (ExtraData != "0")
                        {
                            ExtraData = "0";
                            UpdateState(false, true);
                        }
                        ReqUpdate(1, false);
                    }
                        break;

                    case Interaction.BanzaiFloor:
                        if (Value == 3)
                        {
                            if (InteractionCountHelper == 1)
                            {
                                InteractionCountHelper = 0;
                                switch (Team)
                                {
                                    case Team.Red:
                                        ExtraData = "5";
                                        break;

                                    case Team.Green:
                                        ExtraData = "8";
                                        break;

                                    case Team.Blue:
                                        ExtraData = "11";
                                        break;

                                    case Team.Yellow:
                                        ExtraData = "14";
                                        break;
                                }
                            }
                            else
                            {
                                ExtraData = string.Empty;
                                InteractionCountHelper += 1;
                            }
                            UpdateState();
                            InteractionCount += 1;
                            if (InteractionCount < 16)
                            {
                                UpdateCounter = 1;
                                return;
                            }
                            UpdateCounter = 0;
                        }
                        break;

                    case Interaction.BanzaiScoreBlue:
                    case Interaction.BanzaiScoreRed:
                    case Interaction.BanzaiScoreYellow:
                    case Interaction.BanzaiScoreGreen:
                    case Interaction.BanzaiPyramid:
                    case Interaction.FreezeExit:
                    case Interaction.FreezeRedCounter:
                    case Interaction.FreezeBlueCounter:
                    case Interaction.FreezeYellowCounter:
                    case Interaction.FreezeGreenCounter:
                    case Interaction.FreezeYellowGate:
                    case Interaction.FreezeRedGate:
                    case Interaction.FreezeGreenGate:
                    case Interaction.FreezeBlueGate:
                    case Interaction.FreezeTileBlock:
                    case Interaction.JukeBox:
                    case Interaction.MusicDisc:
                    case Interaction.PuzzleBox:
                    case Interaction.RoomBg:
                    case Interaction.ActionKickUser:
                    case Interaction.ActionGiveReward:
                    case Interaction.ArrowPlate:
                        break;

                    case Interaction.BanzaiCounter:
                    {
                        if (string.IsNullOrEmpty(ExtraData)) return;
                        int.TryParse(ExtraData, out var num4);
                        if (num4 > 0)
                        {
                            if (InteractionCountHelper == 1)
                            {
                                num4--;
                                InteractionCountHelper = 0;
                                if (!GetRoom().GetBanzai().IsBanzaiActive) break;
                                ExtraData = num4.ToString();
                                UpdateState();
                            }
                            else InteractionCountHelper += 1;
                            UpdateCounter = 1;
                            return;
                        }
                        UpdateCounter = 0;
                        GetRoom().GetBanzai().BanzaiEnd();
                        return;
                    }
                    case Interaction.BanzaiTele:
                        ExtraData = string.Empty;
                        UpdateState();
                        return;

                    case Interaction.BanzaiPuck:
                        if (InteractionCount > 4)
                        {
                            InteractionCount += 1;
                            UpdateCounter = 1;
                            return;
                        }
                        InteractionCount = 0;
                        UpdateCounter = 0;
                        return;

                    case Interaction.FreezeTimer:
                    {
                        if (string.IsNullOrEmpty(ExtraData)) return;
                        var num5 = 0;
                        int.TryParse(ExtraData, out num5);
                        if (num5 > 0)
                        {
                            if (InteractionCountHelper == 1)
                            {
                                num5--;
                                InteractionCountHelper = 0;
                                if (!GetRoom().GetFreeze().GameStarted) break;
                                ExtraData = num5.ToString();
                                UpdateState();
                            }
                            else InteractionCountHelper += 1;
                            UpdateCounter = 1;
                            return;
                        }
                        UpdateNeeded = false;
                        GetRoom().GetFreeze().StopGame();
                        return;
                    }
                    case Interaction.FreezeTile:
                        if (InteractingUser > 0u)
                        {
                            ExtraData = "11000";
                            UpdateState(false, true);
                            GetRoom().GetFreeze().OnFreezeTiles(this, FreezePowerUp, InteractingUser);
                            InteractingUser = 0u;
                            InteractionCountHelper = 0;
                        }
                        break;

                    case Interaction.WearItem:
                    {
                        ExtraData = "1";
                        UpdateState();
                        var text = string.Empty;
                        var clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(InteractingUser);
                        {
                            if (!clientByUserId.GetHabbo().Look.Contains("ha"))
                                text = $"{clientByUserId.GetHabbo().Look}.ha-1006-1326";
                            else
                            {
                                var array = clientByUserId.GetHabbo().Look.Split('.');
                                var array2 = array;
                                /* TODO CHECK */
                                foreach (var text2 in array2)
                                {
                                    var str = text2;
                                    if (text2.Contains("ha")) str = "ha-1006-1326";
                                    text = $"{text}{str}.";
                                }
                            }
                            if (text.EndsWith(".")) text = text.TrimEnd('.');
                            clientByUserId.GetHabbo().Look = text;
                            clientByUserId.GetMessageHandler()
                                .GetResponse()
                                .Init(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
                            clientByUserId.GetMessageHandler().GetResponse().AppendInteger(-1);
                            clientByUserId.GetMessageHandler()
                                .GetResponse()
                                .AppendString(clientByUserId.GetHabbo().Look);
                            clientByUserId.GetMessageHandler()
                                .GetResponse()
                                .AppendString(clientByUserId.GetHabbo().Gender.ToLower());
                            clientByUserId.GetMessageHandler()
                                .GetResponse()
                                .AppendString(clientByUserId.GetHabbo().Motto);
                            clientByUserId.GetMessageHandler()
                                .GetResponse()
                                .AppendInteger(clientByUserId.GetHabbo().AchievementPoints);
                            clientByUserId.GetMessageHandler().SendResponse();
                            var serverMessage = new ServerMessage();
                            serverMessage.Init(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
                            serverMessage.AppendInteger(InteractingUser2);
                            serverMessage.AppendString(clientByUserId.GetHabbo().Look);
                            serverMessage.AppendString(clientByUserId.GetHabbo().Gender.ToLower());
                            serverMessage.AppendString(clientByUserId.GetHabbo().Motto);
                            serverMessage.AppendInteger(clientByUserId.GetHabbo().AchievementPoints);
                            GetRoom().SendMessage(serverMessage);
                            return;
                        }
                    }
                    case Interaction.TriggerTimer:
                    case Interaction.TriggerRoomEnter:
                    case Interaction.TriggerGameEnd:
                    case Interaction.TriggerGameStart:
                    case Interaction.TriggerRepeater:
                    case Interaction.TriggerLongRepeater:
                    case Interaction.TriggerOnUserSay:
                    case Interaction.TriggerScoreAchieved:
                    case Interaction.TriggerStateChanged:
                    case Interaction.TriggerWalkOnFurni:
                    case Interaction.TriggerWalkOffFurni:
                    case Interaction.TriggerCollision:
                    case Interaction.ActionGiveScore:
                    case Interaction.ActionPosReset:
                    case Interaction.ActionMoveRotate:
                    case Interaction.ActionMoveToDir:
                    case Interaction.ActionResetTimer:
                    case Interaction.ActionShowMessage:
                    case Interaction.ActionEffectUser:
                    case Interaction.ActionHandItem:
                    case Interaction.ActionRollerSpeed:
                    case Interaction.ActionTeleportTo:
                    case Interaction.ActionToggleState:
                    case Interaction.ActionInverseChase:
                    case Interaction.ActionJoinTeam:
                    case Interaction.ActionLeaveTeam:
                    case Interaction.ActionChase:
                    case Interaction.ConditionFurnisHaveUsers:
                    case Interaction.ConditionStatePos:
                    case Interaction.ConditionTimeLessThan:
                    case Interaction.ConditionTimeMoreThan:
                    case Interaction.ConditionTriggerOnFurni:
                    case Interaction.ConditionFurniHasFurni:
                    case Interaction.ConditionItemsMatches:
                    case Interaction.ConditionGroupMember:
                    case Interaction.ConditionFurniTypeMatches:
                    case Interaction.ConditionHowManyUsersInRoom:
                    case Interaction.ConditionTriggererNotOnFurni:
                    case Interaction.ConditionFurniHasNotFurni:
                    case Interaction.ConditionFurnisHaveNotUsers:
                    case Interaction.ConditionItemsDontMatch:
                    case Interaction.ConditionUserIsNotInTeam:
                    case Interaction.ConditionUserIsInTeam:
                    case Interaction.ConditionFurniTypeDontMatch:
                    case Interaction.ConditionNotGroupMember:
                    case Interaction.ConditionUserWearingEffect:
                    case Interaction.ConditionUserWearingBadge:
                    case Interaction.ConditionUserNotWearingEffect:
                    case Interaction.ConditionUserNotWearingBadge:
                    case Interaction.ConditionDateRangeActive:
                    case Interaction.ConditionUserHasFurni:
                        ExtraData = "0";
                        UpdateState(false, true);
                        break;

                    case Interaction.PressurePadBed:
                    case Interaction.PressurePad:
                        ExtraData = "1";
                        UpdateState();
                        return;

                    case Interaction.Gift:
                        return;

                    default:
                        return;
                }
            }
        }

        /// <summary>
        ///     Reqs the update.
        /// </summary>
        /// <param name="cycles">The cycles.</param>
        /// <param name="setUpdate">if set to <c>true</c> [set update].</param>
        internal void ReqUpdate(int cycles, bool setUpdate)
        {
            UpdateCounter = cycles;
            if (setUpdate) UpdateNeeded = true;
        }


        public List<Point> GetSides()
        {
            var toReturn = new List<Point> {SquareBehind, SquareInFront, SquareLeft, SquareRight, Coordinate};
            return toReturn;
        }

        public void UserFurniCollision(RoomUser user)
        {
            if (user?.GetClient() == null || user.GetClient().GetHabbo() == null)
                return;

            GetRoom().GetWiredHandler().ExecuteWired(Interaction.TriggerCollision, user.GetClient().GetHabbo(), this);
        }

        /// <summary>
        ///     Updates the state.
        /// </summary>
        internal void UpdateState()
        {
            UpdateState(true, true);
        }

        /// <summary>
        ///     Updates the state.
        /// </summary>
        /// <param name="inDb">if set to <c>true</c> [in database].</param>
        /// <param name="inRoom">if set to <c>true</c> [in room].</param>
        internal void UpdateState(bool inDb, bool inRoom)
        {
            //todo: recode to new inventory system :)
            if (GetRoom() == null) return;
            var s = ExtraData;
            if (GetBaseItem().InteractionType == Interaction.MysteryBox)
            {
                using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.SetQuery($"SELECT extra_data FROM items_rooms WHERE id={Id} LIMIT 1");
                    ExtraData = queryReactor.GetString();
                }
                if (ExtraData.Contains(Convert.ToChar(5).ToString()))
                {
                    int num = int.Parse(ExtraData.Split(Convert.ToChar(5))[0]),
                        num2 = int.Parse(ExtraData.Split(Convert.ToChar(5))[1]);
                    s = (3 * num - num2).ToString();
                }
            }
            if (inDb)
                GetRoom().GetRoomItemHandler().AddOrUpdateItem(Id);
            if (!inRoom) return;
            var serverMessage = new ServerMessage(0);
            if (IsFloorItem)
            {
                serverMessage.Init(LibraryParser.OutgoingRequest("UpdateFloorItemExtraDataMessageComposer"));
                serverMessage.AppendString(VirtualId.ToString());
                switch (GetBaseItem().InteractionType)
                {
                    case Interaction.Mannequin:
                        serverMessage.AppendInteger(1);
                        serverMessage.AppendInteger(3);
                        if (ExtraData.Contains(Convert.ToChar(5).ToString()))
                        {
                            var array = ExtraData.Split(Convert.ToChar(5));
                            serverMessage.AppendString("GENDER");
                            serverMessage.AppendString(array[0]);
                            serverMessage.AppendString("FIGURE");
                            serverMessage.AppendString(array[1]);
                            serverMessage.AppendString("OUTFIT_NAME");
                            serverMessage.AppendString(array[2]);
                        }
                        else
                        {
                            serverMessage.AppendString("GENDER");
                            serverMessage.AppendString("");
                            serverMessage.AppendString("FIGURE");
                            serverMessage.AppendString("");
                            serverMessage.AppendString("OUTFIT_NAME");
                            serverMessage.AppendString("");
                        }
                        break;

                    case Interaction.Pinata:
                        serverMessage.AppendInteger(7);
                        if (ExtraData.Length <= 0)
                        {
                            serverMessage.AppendString("6");
                            serverMessage.AppendInteger(0);
                            serverMessage.AppendInteger(100);
                        }
                        else
                        {
                            serverMessage.AppendString((int.Parse(ExtraData) == 100) ? "8" : "6");
                            serverMessage.AppendInteger(int.Parse(ExtraData));
                            serverMessage.AppendInteger(100);
                        }
                        break;

                    case Interaction.WiredHighscore:
                        if (HighscoreData == null) HighscoreData = new HighscoreData(this);
                        HighscoreData.GenerateExtraData(this, serverMessage);
                        break;

                    case Interaction.CrackableEgg:
                        Oblivion.GetGame().GetRandomRewardFurniHandler().GetEggServerMessage(serverMessage, this);
                        break;

                    case Interaction.YoutubeTv:
                        serverMessage.AppendInteger(1);
                        serverMessage.AppendInteger(1);
                        serverMessage.AppendString("THUMBNAIL_URL");
                        serverMessage.AppendString(ExtraSettings.YoutubeThumbnailSuburl + s);
                        break;

                    default:
                        serverMessage.AppendInteger(0);
                        serverMessage.AppendString(s);
                        break;
                }
            }
            else
            {
                serverMessage.Init(LibraryParser.OutgoingRequest("UpdateRoomWallItemMessageComposer"));
                Serialize(serverMessage);
            }
            GetRoom().SendMessage(serverMessage);
        }

        /// <summary>
        ///     Serializes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void Serialize(ServerMessage message)
        {
            if (IsFloorItem)
            {
                message.AppendInteger(VirtualId);
                message.AppendInteger(GetBaseItem().SpriteId);
                message.AppendInteger(X);
                message.AppendInteger(Y);
                message.AppendInteger(Rot);
                message.AppendString(TextHandling.GetString(Z));
                message.AppendString(TextHandling.GetString(Z));

                switch (GetBaseItem().InteractionType)
                {
                    case Interaction.GuildGate:
                    case Interaction.GuildItem:
                    case Interaction.GroupForumTerminal:
                    case Interaction.GuildForum:
                    {
                        var group2 = Oblivion.GetGame().GetGroupManager().GetGroup(GroupId);
                        if (group2 == null)
                        {
                            message.AppendInteger(1);
                            message.AppendInteger(0);
                            message.AppendString(ExtraData);
                        }
                        else
                        {
                            message.AppendInteger(0);
                            message.AppendInteger(2);
                            message.AppendInteger(5);
                            message.AppendString(ExtraData);
                            message.AppendString(GroupId.ToString());
                            message.AppendString(group2.Badge);
                            message.AppendString(Oblivion.GetGame()
                                .GetGroupManager()
                                .GetGroupColour(group2.Colour1, true));
                            message.AppendString(Oblivion.GetGame()
                                .GetGroupManager()
                                .GetGroupColour(group2.Colour2, false));
                        }
                    }
                        break;

                    case Interaction.YoutubeTv:
                        message.AppendInteger(0);
                        if (ExtraData == "")
                        {
                            message.AppendInteger(0);
                            message.AppendString("");
                        }
                        else
                        {
                            message.AppendInteger(1);
                            message.AppendInteger(1);
                            message.AppendString("THUMBNAIL_URL");
                            message.AppendString(ExtraSettings.YoutubeThumbnailSuburl + ExtraData);
                        }
                        break;

                    case Interaction.MusicDisc:
                        message.AppendInteger(SoundMachineSongManager.GetSongId(SongCode));
                        message.AppendInteger(0);
                        message.AppendString(ExtraData);
                        break;

                    case Interaction.Background:
                    case Interaction.WalkInternalLink:
                        message.AppendInteger(0);
                        message.AppendInteger(1);
                        if (ExtraData != "")
                        {
                            message.AppendInteger(ExtraData.Split(Convert.ToChar(9)).Length / 2);
                            for (var i = 0; i <= ExtraData.Split(Convert.ToChar(9)).Length - 1; i++)
                                message.AppendString(ExtraData.Split(Convert.ToChar(9))[i]);
                        }
                        else message.AppendInteger(0);
                        break;

                    case Interaction.Gift:
                    {
                        var split = ExtraData.Split((char) 9);
                        var giftMessage = string.Empty;
                        var giftRibbon = 1;
                        var giftColor = 2;
                        var showGiver = false;
                        var giverName = string.Empty;
                        var giverLook = string.Empty;
                        var product = "A1 PIZ";
                        try
                        {
                            giftMessage = split[1];
                            giftRibbon = int.Parse(split[2]);
                            giftColor = int.Parse(split[3]);
                            showGiver = Oblivion.EnumToBool(split[4]);
                            giverName = split[5];
                            giverLook = split[6];
                            product = split[7];
                        }
                        catch
                        {
                        }
                        var ribbonAndColor = (giftRibbon * 1000) + giftColor;
                        message.AppendInteger(ribbonAndColor);
                        message.AppendInteger(1);
                        message.AppendInteger((showGiver) ? 6 : 4);
                        message.AppendString("EXTRA_PARAM");
                        message.AppendString("");
                        message.AppendString("MESSAGE");
                        message.AppendString(giftMessage);
                        if (showGiver)
                        {
                            message.AppendString("PURCHASER_NAME");
                            message.AppendString(giverName);
                            message.AppendString("PURCHASER_FIGURE");
                            message.AppendString(giverLook);
                        }
                        message.AppendString("PRODUCT_CODE");
                        message.AppendString(product);
                        message.AppendString("state");
                        message.AppendString(MagicRemove ? "1" : "0");
                    }
                        break;

                    case Interaction.Pinata:
                        message.AppendInteger(0);
                        message.AppendInteger(7);
                        message.AppendString((ExtraData == "100") ? "8" : "6");
                        if (ExtraData.Length <= 0)
                        {
                            message.AppendInteger(0);
                            message.AppendInteger(100);
                        }
                        else
                        {
                            message.AppendInteger(int.Parse(ExtraData));
                            message.AppendInteger(100);
                        }
                        break;

                    case Interaction.Mannequin:
                        message.AppendInteger(0);
                        message.AppendInteger(1);
                        message.AppendInteger(3);
                        if (ExtraData.Contains(Convert.ToChar(5).ToString()))
                        {
                            var array = ExtraData.Split((char) 5);
                            message.AppendString("GENDER");
                            message.AppendString(array[0]);
                            message.AppendString("FIGURE");
                            message.AppendString(array[1]);
                            message.AppendString("OUTFIT_NAME");
                            message.AppendString(array[2]);
                        }
                        else
                        {
                            message.AppendString("GENDER");
                            message.AppendString("");
                            message.AppendString("FIGURE");
                            message.AppendString("");
                            message.AppendString("OUTFIT_NAME");
                            message.AppendString("");
                        }
                        break;

                    case Interaction.BadgeDisplay:
                        var extra = ExtraData.Split('|');
                        message.AppendInteger(0);
                        message.AppendInteger(2);
                        message.AppendInteger(4);
                        message.AppendString("0");
                        message.AppendString(extra[0]);
                        message.AppendString(extra.Length > 1 ? extra[1] : "");
                        message.AppendString(extra.Length > 1 ? extra[2] : "");
                        break;

                    case Interaction.LoveLock:
                        var data = ExtraData.Split((char) 5);
                        message.AppendInteger(0);
                        message.AppendInteger(2);
                        message.AppendInteger(data.Length);
                        /* TODO CHECK */
                        foreach (var datak in data) message.AppendString(datak);
                        break;

                    case Interaction.Moplaseed:
                    case Interaction.RareMoplaSeed:
                        message.AppendInteger(0);
                        message.AppendInteger(1);
                        message.AppendInteger(1);
                        message.AppendString("rarity");
                        message.AppendString(ExtraData);
                        break;

                    case Interaction.RoomBg:
                        if (_mRoom.TonerData == null) _mRoom.TonerData = new TonerData(Id);
                        _mRoom.TonerData.GenerateExtraData(message);
                        break;

                    case Interaction.AdsMpu:
                        message.AppendInteger(0);
                        message.AppendInteger(1);
                        if (!string.IsNullOrEmpty(ExtraData) && ExtraData.Contains((char) 9))
                        {
                            var arrayData = ExtraData.Split((char) 9);
                            message.AppendInteger(arrayData.Length / 2);
                            /* TODO CHECK */
                            foreach (var dataStr in arrayData) message.AppendString(dataStr);
                        }
                        else message.AppendInteger(0);
                        break;

                    case Interaction.MysteryBox:
                        message.AppendInteger(0);
                        message.AppendInteger(0);
                        if (ExtraData.Contains(Convert.ToChar(5).ToString()))
                        {
                            var num3 = int.Parse(ExtraData.Split((char) 5)[0]);
                            var num4 = int.Parse(ExtraData.Split((char) 5)[1]);
                            message.AppendString((3 * num3 - num4).ToString());
                        }
                        else
                        {
                            ExtraData = $"0{Convert.ToChar(5)}0";
                            message.AppendString("0");
                        }
                        break;

                    case Interaction.WiredHighscore:
                        if (HighscoreData == null) HighscoreData = new HighscoreData(this);
                        message.AppendInteger(0);
                        HighscoreData.GenerateExtraData(this, message);
                        break;

                    case Interaction.CrackableEgg:
                        message.AppendInteger(0);
                        Oblivion.GetGame().GetRandomRewardFurniHandler().GetEggServerMessage(message, this);
                        break;

                    default:
                        if (LimitedNo > 0)
                        {
                            message.AppendInteger(1);
                            message.AppendInteger(256);
                            message.AppendString(ExtraData);
                            message.AppendInteger(LimitedNo);
                            message.AppendInteger(LimitedTot);
                        }
                        else
                        {
                            message.AppendInteger((GetBaseItem().InteractionType == Interaction.TileStackMagic)
                                ? 0
                                : 1);
                            message.AppendInteger(0);
                            message.AppendString(ExtraData);
                        }
                        break;
                }

                message.AppendInteger(-1);
                message.AppendInteger((GetBaseItem().InteractionType == Interaction.MysteryBox ||
                                       GetBaseItem().InteractionType == Interaction.YoutubeTv ||
                                       GetBaseItem().InteractionType == Interaction.Background)
                    ? 2
                    : (((GetBaseItem().InteractionType == Interaction.Moplaseed ||
                         GetBaseItem().InteractionType == Interaction.RareMoplaSeed) || GetBaseItem().Modes > 1)
                        ? 1
                        : 0));
                message.AppendInteger(IsBuilder ? -12345678 : Convert.ToInt32(UserId)); //-12345678 for bc

                return;
            }

            if (!IsWallItem)
                return;

            message.AppendString($"{VirtualId}{string.Empty}");
            message.AppendInteger(GetBaseItem().SpriteId);
            message.AppendString(WallCoord?.ToString() ?? string.Empty);

            var interactionType = GetBaseItem().InteractionType;

            message.AppendString(interactionType == Interaction.PostIt ? ExtraData.Split(' ')[0] : ExtraData);
            message.AppendInteger(-1);
            message.AppendInteger(GetBaseItem().Modes > 1 ? 1 : 0);
            message.AppendInteger(IsBuilder ? -12345678 : Convert.ToInt32(UserId));
        }

        /// <summary>
        ///     Refreshes the item.
        /// </summary>
        internal void RefreshItem()
        {
            _mBaseItem = null;
        }

        /// <summary>
        ///     Gets the base item.
        /// </summary>
        /// <returns>Item.</returns>
        internal Item GetBaseItem() =>
            _mBaseItem ?? (_mBaseItem = Oblivion.GetGame().GetItemManager().GetItem(BaseItem));

        /// <summary>
        ///     Gets the room.
        /// </summary>
        /// <returns>Room.</returns>
        internal Room GetRoom() => _mRoom ?? (_mRoom = Oblivion.GetGame().GetRoomManager().GetRoom(RoomId));

        /// <summary>
        ///     Users the walks on furni.
        /// </summary>
        /// <param name="user">The user.</param>
        internal void UserWalksOnFurni(RoomUser user)
        {
            if (GetRoom().GotWireds())
                GetRoom().GetWiredHandler().ExecuteWired(Interaction.TriggerWalkOnFurni, user, this);
            user.LastItem = Id;
        }

        /// <summary>
        ///     Users the walks off furni.
        /// </summary>
        /// <param name="user">The user.</param>
        internal void UserWalksOffFurni(RoomUser user)
        {
            if (GetRoom().GotWireds())
                GetRoom().GetWiredHandler().ExecuteWired(Interaction.TriggerWalkOffFurni, user, this);
        }
    }
}