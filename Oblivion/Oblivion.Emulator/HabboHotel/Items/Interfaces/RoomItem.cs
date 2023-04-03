using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
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
    public class RoomItem : UserItem
    {
       
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
        ///     The come direction
        /// </summary>
        internal IComeDirection ComeDirection;

        
        /// <summary>
        ///     The freeze power up
        /// </summary>
        internal FreezePowerUp FreezePowerUp;

        /// <summary>
        ///     The group data
        /// </summary>
        internal string GroupData;
        

        /// <summary>
        ///     The highscore data
        /// </summary>
        internal HighscoreData HighscoreData;
        
       

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
        ///     The interaction count tempvar2
        /// </summary>
        public byte InteractionCount;

        /// <summary>
        ///     The interaction count helper tempvar1
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
        /// The tele link
        /// </summary>
        internal string TeleporterId = "0";

        /// <summary>
        ///     The rot
        /// </summary>
        internal int Rot;
        

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
        internal RoomItem(string id, uint roomId, uint baseItem, string extraData, int x, int y, double z, int rot,
            Room pRoom, uint userid, uint eGroup, string songCode, bool isBuilder, int limNo, int limStack) : base(id,baseItem,extraData,eGroup,songCode,limNo,limStack)
        {
            
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
            RoomId = roomId;
            _mRoom = pRoom;


            LimitedNo = limNo;
            LimitedTot = limStack;
            if (BaseItem.Name.ContainsAny("guild_", "grp", "gld_"))
            {
                GroupData = extraData;
                ExtraData = GroupData.Split(';')[0];
                if (GroupData.Contains(";;"))
                {
                    GroupData = GroupData.Replace(";;", ";");
                    _mRoom.GetRoomItemHandler().AddOrUpdateItem(Id);
                }
            }


            AffectedTiles = Gamemap.GetAffectedTiles(BaseItem.Length, BaseItem.Width, X, Y, rot);

            Interactor = GetInteractor();
            switch (GetBaseItem().InteractionType)
            {
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
            }
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
        internal RoomItem(string id, uint roomId, uint baseItem, string extraData, WallCoordinate wallCoord, Room pRoom,
            uint userid, uint eGroup, bool isBuilder) : base(id,baseItem,extraData,eGroup,"",0,0)
        {

            _mRoom = pRoom;


            Id = id;

            VirtualId = Oblivion.GetGame().GetItemManager().GetVirtualId(id);

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
            AffectedTiles = new Dictionary<int, ThreeDCoord>();
            SongCode = string.Empty;
            Interactor = GetInteractor();
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
        internal bool IsRoller { get; private set; }

        /// <summary>
        ///     Gets the coordinate.
        /// </summary>
        /// <value>The coordinate.</value>
        internal Point Coordinate => new(X, Y);

        /// <summary>
        ///     Gets the get coords.
        /// </summary>
        /// <value>The get coords.</value>
        internal List<Point> GetCoords()
        {
            var list = new List<Point>
            {
                Coordinate
            };

            if (AffectedTiles == null)
            {
                return list;
            }

            foreach (var tile in AffectedTiles.Values)
            {
                list.Add(new Point(tile.X, tile.Y));
            }

            
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


        private IFurniInteractor GetInteractor()
        {
            if (IsWired) return new InteractorWired();

            if (GetBaseItem().IsSeat) return new InteractorChair();

            var interactionType = GetBaseItem().InteractionType;
            switch (interactionType)
            {
                case Interaction.Roller:
                    IsRoller = true;
                    _mRoom.GetRoomItemHandler().GotRollers = true;
                    return new InteractorRoller();


                case Interaction.Normslaskates:
                    return new InteractorRollerSkates();
                case Interaction.IceSkates:
                    return new InteractorIceSkates();

                case Interaction.Gate:
                    return new InteractorGate();

                case Interaction.GuildGate:
                    return new InteractorGroupGate();

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
                    IsTrans = true;
                    ReqUpdate(0, true);
                    return new InteractorHopper();

                case Interaction.Teleport:
                    TeleporterId = TeleHandler.GetLinkedTele(Id);
                    IsTrans = true;
                    ReqUpdate(0, true);
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

                case Interaction.BanzaiTele:
                    ExtraData = string.Empty;
                    return new InteractorBanzaiTele();

                case Interaction.BanzaiGateBlue:
                case Interaction.BanzaiGateRed:
                case Interaction.BanzaiGateYellow:
                case Interaction.BanzaiGateGreen:
                    return new InteractorBanzaiGate();
                case Interaction.BreedingTerrier:
                    if (!_mRoom.GetRoomItemHandler().BreedingTerrier.ContainsKey(VirtualId))
                        _mRoom.GetRoomItemHandler().BreedingTerrier.Add(VirtualId, this);
                    break;
                case Interaction.BreedingBear:
                    if (!_mRoom.GetRoomItemHandler().BreedingBear.ContainsKey(VirtualId))
                        _mRoom.GetRoomItemHandler().BreedingBear.Add(VirtualId, this);
                    break;


                case Interaction.FreezeYellowGate:
                case Interaction.FreezeRedGate:
                case Interaction.FreezeGreenGate:
                case Interaction.FreezeBlueGate:
                    return new InteractorFreezeGate();

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
                    if (int.TryParse(ExtraData, out var num) && num >= 1 && num < 5) VikingCotieBurning = true;

                    return new InteractorVikingCotie();

                case Interaction.Cannon:
                    return new InteractorCannon();

                case Interaction.FxBox:
                    return new InteractorFxBox();

                case Interaction.HcGate:
                    return new InteractorHcGate();

                case Interaction.QuickTeleport:
                    TeleporterId = TeleHandler.GetLinkedTele(Id);
                    IsTrans = true;
                    ReqUpdate(0, true);
                    return new InteractorQuickTeleport();

                case Interaction.CrackableEgg:
                    return new InteractorCrackableEgg();

                case Interaction.FloorSwitch1:
                case Interaction.FloorSwitch2:
                    return new InteractorSwitch();

                case Interaction.PressurePadBed:
                case Interaction.Bed:
                    return new InteractorBed();


                case Interaction.Tent:
                case Interaction.BedTent:
                    return new InteractorTent();

                case Interaction.Pinata:
                    return new InteractorCrackableEgg();

                case Interaction.WalkInternalLink:
                    return new InteractorWalkInternalLink();

                case Interaction.Shower:
                case Interaction.ChairState:
                case Interaction.PressurePad:
                case Interaction.FloorSwitch:
                    return new InteractorWalkSwitch();

                case Interaction.Totem:
                    return new InteractorTotem();


                case Interaction.NearSwitch:
                    return new InteractorNearSwitch();

                default:
                    return new InteractorGenericSwitch();
            }

            return new InteractorGenericSwitch();
        }

        /// <summary>
        ///     Gets the interactor.
        /// </summary>
        /// <value>The interactor.</value>
        internal IFurniInteractor Interactor;


        internal async Task SetState(int x, int y, double z)
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
        internal void Dispose(bool removeVirtual)
        {
           // base.Dispose(removeVirtual);

            if (_mRoom != null)
            {
                if (_mRoom.GotWireds() && IsWired)
                {
                    _mRoom.GetWiredHandler().RemoveWired(this);
                }
            }

            if (removeVirtual)
                Oblivion.GetGame().GetItemManager().RemoveVirtualItem(Id);

            _mRoom = null;
            AffectedTiles?.Clear();
            HighscoreData?.Dispose();
            HighscoreData = null;
            PetsList?.Clear();
            PetsList = null;
            WallCoord = null;
            AffectedTiles = null;
            InteractingBallUser = null;
            BaseItem = null;
        }

        /// <summary>
        ///     Processes the updates.
        /// </summary>
        internal async Task ProcessUpdates()
        {
            UpdateCounter--;
            if (UpdateCounter <= 0 || IsTrans)
            {
                UpdateNeeded = false;
                UpdateCounter = 0;

                if (GetBaseItem() == null)
                {
                    return;
                }
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
                                await UpdateState();
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
                            }

                            InteractingUser = 0u;
                            ExtraData = "0";
                            await UpdateState(false, true);
                        }

                        break;

                    case Interaction.Alert:
                        if (ExtraData == "1")
                        {
                            ExtraData = "0";
                            await UpdateState(false, true);
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
                            await roomUser3.MoveTo(SquareBehind);
                            roomUser3.InteractingGate = false;
                            roomUser3.GateId = "0u";
                            ReqUpdate(1, false);
                            await UpdateState(false, true);
                        }
                        else if (roomUser3 != null && roomUser3.Coordinate == SquareBehind)
                        {
                            roomUser3.UnlockWalking();
                            ExtraData = "0";
                            InteractingUser = 0u;
                            roomUser3.InteractingGate = false;
                            roomUser3.GateId = "0u";
                            await UpdateState(false, true);
                        }
                        else if (ExtraData == "1")
                        {
                            ExtraData = "0";
                            await UpdateState(false, true);
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

                        await UpdateState(false, true);
                        return;

                    case Interaction.HabboWheel:
                        ExtraData = RandomNumber.Get(1, 10).ToString();
                        await UpdateState();
                        return;

                    case Interaction.Dice:
                        ExtraData = RandomNumber.Get(1, 7).ToString();
                        await UpdateState();
                        return;

                    case Interaction.Bottle:
                        ExtraData = RandomNumber.Get(0, 7).ToString();
                        await UpdateState();
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

                                            using (var roomFwd =
                                                new ServerMessage(
                                                    LibraryParser.OutgoingRequest("RoomForwardMessageComposer")))
                                            {
                                                await roomFwd.AppendIntegerAsync(aHopper);
                                                await roomUser4.GetClient().SendMessageAsync(roomFwd);
                                            }

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
                                    await roomUser4.MoveTo(Coordinate.X, Coordinate.Y, true);
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
                                    await roomUserByHabbo.MoveTo(SquareInFront);
                            }

                            InteractingUser2 = 0u;
                        }

                        if (flag2)
                        {
                            if (ExtraData != "1")
                            {
                                ExtraData = "1";
                                await UpdateState(false, true);
                            }
                        }
                        else if (flag)
                        {
                            if (ExtraData != "2")
                            {
                                ExtraData = "2";
                                await UpdateState(false, true);
                            }
                        }
                        else if (ExtraData != "0")
                        {
                            if (num2 == 0)
                            {
                                ExtraData = "0";
                                await UpdateState(false, true);
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

                                    if (TeleHandler.IsTeleLinked(Id, _mRoom, this))
                                    {
                                        showTeleEffect = true;
//                                        var linkedTele = TeleHandler.GetLinkedTele(Id, _mRoom);
                                        var teleRoomId = TeleHandler.GetTeleRoomId(TeleporterId, _mRoom);
                                        if (teleRoomId == RoomId)
                                        {
                                            var item = GetRoom().GetRoomItemHandler().GetItem(TeleporterId);
                                            if (item == null)
                                            {
                                                user.UnlockWalking();
                                                _mRoom.GetGameMap().GameMap[X, Y] = 1;
                                            }
                                            else
                                            {
                                                user.SetPos(item.X, item.Y, item.Z);
                                                user.SetRot(item.Rot, false);
                                                item.ExtraData = "2";
                                                await  item.UpdateState(false, true);
                                                item.InteractingUser2 = InteractingUser;
                                                _mRoom.GetGameMap().GameMap[X, Y] = 1;
                                            }
                                        }
                                        else
                                        {
                                            if (!user.IsBot && user.GetClient()?.GetHabbo() != null &&
                                                user.GetClient().GetMessageHandler() != null)
                                            {
                                                user.GetClient().GetHabbo().IsTeleporting = true;
                                                user.GetClient().GetHabbo().TeleportingRoomId = teleRoomId;
                                                user.GetClient().GetHabbo().TeleporterId = TeleporterId;
                                                await user.GetClient()
                                                    .GetMessageHandler()
                                                    .PrepareRoomForUser(teleRoomId, string.Empty);
                                                _mRoom.GetGameMap().GameMap[X, Y] = 1;
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
                                    if (user.IsWalking && (user.GoalX != X || user.GoalY != Y))
                                        user.ClearMovement();

                                    user.CanWalk = false;
                                    user.AllowOverride = true;
                                    _mRoom.GetGameMap().GameMap[X, Y] = 1;

                                    await user.MoveTo(X, Y, true);
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
                                await UpdateState(false, true);
                            }
                        }
                        else if (showTeleEffect)
                        {
                            if (ExtraData != "2")
                            {
                                ExtraData = "2";
                                await UpdateState(false, true);
                            }
                        }
                        else if (ExtraData != "0")
                        {
                            ExtraData = "0";
                            await UpdateState(false, true);
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

                            await UpdateState();
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
                                await UpdateState();
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
                        await UpdateState();
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
                        int.TryParse(ExtraData, out var num5);
                        if (num5 > 0)
                        {
                            if (InteractionCountHelper == 1)
                            {
                                num5--;
                                InteractionCountHelper = 0;
                                if (!GetRoom().GetFreeze().GameStarted) break;
                                ExtraData = num5.ToString();
                                await UpdateState();
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
                            await UpdateState(false, true);
                            GetRoom().GetFreeze().OnFreezeTiles(this, FreezePowerUp, InteractingUser);
                            InteractingUser = 0u;
                            InteractionCountHelper = 0;
                        }

                        break;

                    case Interaction.WearItem:
                    {
                        ExtraData = "1";
                        await UpdateState();
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
                            await clientByUserId.GetMessageHandler()
                                .GetResponse()
                                .InitAsync(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
                            await clientByUserId.GetMessageHandler().GetResponse().AppendIntegerAsync(-1);
                            await clientByUserId.GetMessageHandler()
                                .GetResponse()
                                .AppendStringAsync(clientByUserId.GetHabbo().Look);
                            await clientByUserId.GetMessageHandler()
                                .GetResponse()
                                .AppendStringAsync(clientByUserId.GetHabbo().Gender.ToLower());
                            await clientByUserId.GetMessageHandler()
                                .GetResponse()
                                .AppendStringAsync(clientByUserId.GetHabbo().Motto);
                            await clientByUserId.GetMessageHandler()
                                .GetResponse()
                                .AppendIntegerAsync(clientByUserId.GetHabbo().AchievementPoints);
                            await clientByUserId.GetMessageHandler().SendResponse();

                            using (var serverMessage = new ServerMessage())
                            {
                                await serverMessage.InitAsync(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
                                await serverMessage.AppendIntegerAsync(InteractingUser2);
                                await serverMessage.AppendStringAsync(clientByUserId.GetHabbo().Look);
                                await serverMessage.AppendStringAsync(clientByUserId.GetHabbo().Gender.ToLower());
                                await serverMessage.AppendStringAsync(clientByUserId.GetHabbo().Motto);
                                await serverMessage.AppendIntegerAsync(clientByUserId.GetHabbo().AchievementPoints);
                                await GetRoom().SendMessageAsync(serverMessage);
                            }

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
                        await UpdateState(false, true);
                        break;

                    case Interaction.PressurePadBed:
                    case Interaction.PressurePad:
                        ExtraData = "1";
                        await UpdateState();
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

        /// <summary>
        ///     Updates the state.
        /// </summary>
        internal async Task UpdateState()
        {
            await UpdateState(true, true);
        }

        /// <summary>
        ///     Updates the state.
        /// </summary>
        /// <param name="inDb">if set to <c>true</c> [in database].</param>
        /// <param name="inRoom">if set to <c>true</c> [in room].</param>
        internal async Task UpdateState(bool inDb, bool inRoom)
        {
            //todo: recode to new inventory system :)
            if (_mRoom == null) return;
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
            ServerMessage serverMessage;
            if (!IsWallItem)
            {
                serverMessage =
                    new ServerMessage(LibraryParser.OutgoingRequest("UpdateFloorItemExtraDataMessageComposer"));
                await serverMessage.AppendStringAsync(VirtualId.ToString());
                switch (GetBaseItem().InteractionType)
                {
                    case Interaction.Mannequin:
                        await serverMessage.AppendIntegerAsync(1);
                        await serverMessage.AppendIntegerAsync(3);
                        if (ExtraData.Contains(Convert.ToChar(5).ToString()))
                        {
                            var array = ExtraData.Split(Convert.ToChar(5));
                            await serverMessage.AppendStringAsync("GENDER");
                            await serverMessage.AppendStringAsync(array[0]);
                            await serverMessage.AppendStringAsync("FIGURE");
                            await serverMessage.AppendStringAsync(array[1]);
                            await serverMessage.AppendStringAsync("OUTFIT_NAME");
                            await serverMessage.AppendStringAsync(array[2]);
                        }
                        else
                        {
                            await serverMessage.AppendStringAsync("GENDER");
                            await serverMessage.AppendStringAsync("");
                            await serverMessage.AppendStringAsync("FIGURE");
                            await serverMessage.AppendStringAsync("");
                            await serverMessage.AppendStringAsync("OUTFIT_NAME");
                            await serverMessage.AppendStringAsync("");
                        }

                        break;

                    case Interaction.Pinata:
                        await serverMessage.AppendIntegerAsync(7);
                        if (ExtraData.Length <= 0)
                        {
                            await serverMessage.AppendStringAsync("6");
                            await serverMessage.AppendIntegerAsync(0);
                            await serverMessage.AppendIntegerAsync(100);
                        }
                        else
                        {
                            await serverMessage.AppendStringAsync((int.Parse(ExtraData) == 100) ? "8" : "6");
                            await serverMessage.AppendIntegerAsync(int.Parse(ExtraData));
                            await serverMessage.AppendIntegerAsync(100);
                        }

                        break;

                    case Interaction.WiredHighscore:
                        if (HighscoreData == null)
                            HighscoreData = new HighscoreData(this);
                        HighscoreData.GenerateExtraData(this, serverMessage);
                        break;

                    case Interaction.CrackableEgg:
                        Oblivion.GetGame().GetRandomRewardFurniHandler().GetEggServerMessage(serverMessage, this);
                        break;

                    case Interaction.YoutubeTv:
                        await serverMessage.AppendIntegerAsync(1);
                        await serverMessage.AppendIntegerAsync(1);
                        await serverMessage.AppendStringAsync("THUMBNAIL_URL");
                        await serverMessage.AppendStringAsync(ExtraSettings.WebSocketAddr + s);
                        break;

                    default:
                        await serverMessage.AppendIntegerAsync(0);
                        await serverMessage.AppendStringAsync(s);
                        break;
                }
            }
            else
            {
                serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UpdateRoomWallItemMessageComposer"));
                Serialize(serverMessage);
            }

            await GetRoom().SendMessage(serverMessage);
        }

        /// <summary>
        ///     Serializes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void Serialize(ServerMessage message)
        {
           //todo: serialize in interactors
            if (!IsWallItem)
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
                            message.AppendString(ExtraSettings.WebSocketAddr + ExtraData);
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
                        if (_mRoom.TonerData == null)
                            _mRoom.TonerData = new TonerData(Id);
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
                            foreach (var dataStr in arrayData)
                            {
                                message.AppendString(dataStr);
                            }
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
        ///     Gets the base item.
        /// </summary>
        /// <returns>Item.</returns>
        internal Item GetBaseItem() => BaseItem;

        /// <summary>
        ///     Gets the room.
        /// </summary>
        /// <returns>Room.</returns>
        internal Room GetRoom() => _mRoom ?? (_mRoom = Oblivion.GetGame().GetRoomManager().GetRoom(RoomId));

        /// <summary>
        ///     Users the walks on furni.
        /// </summary>
        /// <param name="user">The user.</param>
        internal async Task UserWalksOnFurni(RoomUser user, bool fromWired = false)
        {
            if (!fromWired && GetRoom().GotWireds())
            {
                await GetRoom().GetWiredHandler().ExecuteWired(Interaction.TriggerWalkOnFurni, user, this);
                await GetRoom().GetWiredHandler().ExecuteWired(Interaction.TriggerBotReachedStuff, this);
            }

            user.LastItem = Id;
        }

        /// <summary>
        ///     Users the walks off furni.
        /// </summary>
        /// <param name="user">The user.</param>
        internal async Task UserWalksOffFurni(RoomUser user)
        {
            Interactor.OnUserWalkOff(user.GetClient(), this, user);

            if (GetRoom().GotWireds())
                await GetRoom().GetWiredHandler().ExecuteWired(Interaction.TriggerWalkOffFurni, user, this);
        }
    }
}