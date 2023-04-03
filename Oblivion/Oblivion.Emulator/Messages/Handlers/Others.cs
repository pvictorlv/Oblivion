using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.Configuration;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Quests.Composer;
using Oblivion.HabboHotel.Rooms;
using Oblivion.Messages.Parsers;
using Oblivion.Encryption.Encryption;
using Oblivion.Encryption.Encryption.Hurlant.Crypto.Prng;
using Oblivion.HabboHotel.Users;

namespace Oblivion.Messages.Handlers
{
    /// <summary>
    ///     Class GameClientMessageHandler.
    /// </summary>
    internal partial class GameClientMessageHandler
    {
        /// <summary>
        ///     The current loading room
        /// </summary>
        internal Room CurrentLoadingRoom;

        /// <summary>
        ///     The request
        /// </summary>
        protected ClientMessage Request;

        /// <summary>
        ///     The response
        /// </summary>
        protected ServerMessage Response;

        /// <summary>
        ///     The session
        /// </summary>
        protected GameClient Session;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameClientMessageHandler" /> class.
        /// </summary>
        /// <param name="session">The session.</param>
        internal GameClientMessageHandler(GameClient session)
        {
            Session = session;
            Response = new ServerMessage();
        }


        /// <summary>
        ///     Gets the response.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage GetResponse() => Response;

        /// <summary>
        ///     Destroys this instance.
        /// </summary>
        internal async Task Destroy()
        {
            Session = null;
        }

        /// <summary>
        ///     Handles the request.
        /// </summary>
        /// <param name="request">The request.</param>
        internal void HandleRequest(ClientMessage request)
        {
            Request = request;
            LibraryParser.HandlePacket(this, request);
        }

        /// <summary>
        ///     Sends the response.
        /// </summary>
        internal async Task SendResponse()
        {
            if (Response != null && Response.Id > 0 && Session?.GetConnection() != null)
                await Session.SendMessageAsync(Response);
        }


        /// <summary>
        ///     Gets the client version message event.
        /// </summary>
        internal async Task ReleaseVersion()
        {
            var release = Request.GetString();

            if (release.StartsWith("AIR63"))
                Session.IsAir = true;
        }

        /// <summary>
        ///     Pongs this instance.
        /// </summary>
        internal async Task Pong()
        {
            if (Session == null) return;

            Session.TimePingedReceived = DateTime.Now;
        }

        /// <summary>
        ///     Disconnects the event.
        /// </summary>
        internal async Task DisconnectEvent()
        {
            Session.Dispose();
        }

        /// <summary>
        ///     Latencies the test.
        /// </summary>
        internal async Task LatencyTest()
        {
            if (Session == null)
                return;
            Oblivion.GetGame().GetAchievementManager()
                .ProgressUserAchievement(Session, "ACH_AllTimeHotelPresence", 1, true);

            Session.TimePingedReceived = DateTime.Now;

            await Response.InitAsync(LibraryParser.OutgoingRequest("LatencyTestResponseMessageComposer"));
            await Response.AppendIntegerAsync(Request.GetIntegerFromString());
            await SendResponse();
        }


        /// <summary>
        ///     Initializes the crypto.
        /// </summary>
        internal async Task InitCrypto()
        {
            if (Session.IsAir)
            {
                await Response.InitAsync(LibraryParser.OutgoingRequest("InitCryptoMessageComposer"));
                await Response.AppendStringAsync(Handler.GetRsaDiffieHellmanPrimeKey().ToLower());
                await Response.AppendStringAsync(Handler.GetRsaDiffieHellmanGeneratorKey().ToLower());
                await SendResponse();
            }
        }


        /// <summary>
        ///     Secrets the key.
        /// </summary>
        internal async Task SecretKey()
        {
            var cipherKey = Request.GetString();
            var sharedKey = Handler.CalculateDiffieHellmanSharedKey(cipherKey);
            /*
            if (Session.IsAir)
            {
                Response.Init(LibraryParser.OutgoingRequest("SecretKeyMessageComposer"));
                Response.AppendString(Handler.GetRsaDiffieHellmanPublicKey().ToLower());
                Response.AppendBool(false);
                await SendResponse();

                var data = sharedKey.ToByteArray();

                if (data[data.Length - 1] == 0)
                    Array.Resize(ref data, data.Length - 1);

                Array.Reverse(data, 0, data.Length);

                Session.ServerRc4 = new ARC4(data);
                Session.GetConnection().ActivateRc4Filter();
            }
            */
        }

        internal async Task InitConsole()
        {
            Session.GetHabbo().InitMessenger();
        }

        /// <summary>
        ///     Machines the identifier.
        /// </summary>
        internal async Task MachineId()
        {
            Request.GetString();
            var machineId = Request.GetString();
            Session.MachineId = machineId;

            await Response.InitAsync(LibraryParser.OutgoingRequest("UniqueMachineIDMessageComposer"));
            await Response.AppendStringAsync(machineId);
            await SendResponse();
        }

        /// <summary>
        ///     Logins the with ticket.
        /// </summary>
        internal async Task LoginWithTicket()
        {
            if (Session == null || Session.GetHabbo() != null)
                return;

            var sso = Request.GetString();

            if (string.IsNullOrEmpty(sso) || string.IsNullOrWhiteSpace(sso) || sso.Length < 5 || !Session.TryLogin(sso))
            {
                Session?.Disconnect("Invalid sso or banned");
                return;
            }


            if (Session == null) return;
            Session.TimePingedReceived = DateTime.Now;
        }

        /// <summary>
        ///     Informations the retrieve.
        /// </summary>
        internal async Task InfoRetrieve()
        {
            if (Session?.GetHabbo() == null)
                return;
            
            var habbo = Session.GetHabbo();
            await Response.InitAsync(LibraryParser.OutgoingRequest("UserObjectMessageComposer"));
            await Response.AppendIntegerAsync(habbo.Id);
            await Response.AppendStringAsync(habbo.UserName);
            await Response.AppendStringAsync(habbo.Look);
            await Response.AppendStringAsync(habbo.Gender.ToUpper());
            await Response.AppendStringAsync(habbo.Motto);
            await Response.AppendStringAsync("");
            Response.AppendBool(false);
            await Response.AppendIntegerAsync(habbo.Respect);
            await Response.AppendIntegerAsync(habbo.DailyRespectPoints);
            await Response.AppendIntegerAsync(habbo.DailyPetRespectPoints);
            Response.AppendBool(false);
            await Response.AppendStringAsync(habbo.LastOnline.ToString(CultureInfo.InvariantCulture));
            Response.AppendBool(habbo.CanChangeName());
            Response.AppendBool(false);
            await SendResponse();

            await Response.InitAsync(LibraryParser.OutgoingRequest("BuildersClubMembershipMessageComposer"));
            await Response.AppendIntegerAsync(Session.GetHabbo().BuildersExpire);
            await Response.AppendIntegerAsync(Session.GetHabbo().BuildersItemsMax);
            await Response.AppendIntegerAsync(2);
            await SendResponse();
            var tradeLocked = Session.GetHabbo().CheckTrading();

            await Response.InitAsync(LibraryParser.OutgoingRequest("SendPerkAllowancesMessageComposer"));

            await Response.AppendIntegerAsync(16); // Count
             await Response.AppendStringAsync("USE_GUIDE_TOOL");
             await Response.AppendStringAsync("");
             Response.AppendBool(habbo.Rank >= 3);

             await Response.AppendStringAsync("GIVE_GUIDE_TOURS");
             await Response.AppendStringAsync("requirement.unfulfilled.helper_le");
             Response.AppendBool(habbo.Rank >= 3);

             await Response.AppendStringAsync("JUDGE_CHAT_REVIEWS");
             await Response.AppendStringAsync(""); // ??
             Response.AppendBool(true);

             await Response.AppendStringAsync("VOTE_IN_COMPETITIONS");
             await Response.AppendStringAsync(""); // ??
             Response.AppendBool(true);

             await Response.AppendStringAsync("CALL_ON_HELPERS");
             await Response.AppendStringAsync(""); // ??
             Response.AppendBool(true);

             await Response.AppendStringAsync("CITIZEN");
             await Response.AppendStringAsync(""); // ??
             Response.AppendBool(true);

             await Response.AppendStringAsync("TRADE");
             await Response.AppendStringAsync(""); // ??
             Response.AppendBool(tradeLocked);

             await Response.AppendStringAsync("HEIGHTMAP_EDITOR_BETA");
             await Response.AppendStringAsync(""); // ??
             Response.AppendBool(false);

             await Response.AppendStringAsync("EXPERIMENTAL_CHAT_BETA");
             await Response.AppendStringAsync("requirement.unfulfilled.helper_level_2");
             Response.AppendBool(true);

             await Response.AppendStringAsync("EXPERIMENTAL_TOOLBAR");
             await Response.AppendStringAsync(""); // ??
             Response.AppendBool(true);

             await Response.AppendStringAsync("BUILDER_AT_WORK");
             await Response.AppendStringAsync(""); // ??
             Response.AppendBool(true);

             await Response.AppendStringAsync("NAVIGATOR_PHASE_ONE_2014");
             await Response.AppendStringAsync(""); // ??
             Response.AppendBool(false);

             await Response.AppendStringAsync("CAMERA");
             await Response.AppendStringAsync(""); // ??
             Response.AppendBool(true);

             await Response.AppendStringAsync("NAVIGATOR_PHASE_TWO_2014");
             await Response.AppendStringAsync(""); // ??
             Response.AppendBool(true);

             await Response.AppendStringAsync("MOUSE_ZOOM");
             await Response.AppendStringAsync(""); // ??
             Response.AppendBool(true);

             await Response.AppendStringAsync("NAVIGATOR_ROOM_THUMBNAIL_CAMERA");
             await Response.AppendStringAsync(""); // ??
             Response.AppendBool(true);

            await SendResponse();


            await GetResponse().InitAsync(LibraryParser.OutgoingRequest("CitizenshipStatusMessageComposer"));
            await GetResponse().AppendStringAsync("citizenship");
            await GetResponse().AppendIntegerAsync(1);
            await GetResponse().AppendIntegerAsync(4);
            await SendResponse();

            await GetResponse().InitAsync(LibraryParser.OutgoingRequest("AchievementPointsMessageComposer"));
            await GetResponse().AppendIntegerAsync(Session.GetHabbo().AchievementPoints);
            await SendResponse();
            await GetResponse().InitAsync(LibraryParser.OutgoingRequest("FigureSetIdsMessageComposer"));
//            Session.GetHabbo().ClothingManager.Serialize(GetResponse());
//            await SendResponse();
            /*Response.Init(LibraryParser.OutgoingRequest("NewbieStatusMessageComposer"));
            Response.AppendInteger(0);// 2 = new - 1 = nothing - 0 = not new
            await SendResponse();*/
            if (Oblivion.GetGame().GetTargetedOfferManager().CurrentOffer != null)
            {
                Oblivion.GetGame().GetTargetedOfferManager().GenerateMessage(GetResponse());
                await SendResponse();
            }

            if (Session.GetHabbo().CurrentQuestId != 0)
            {
                var quest = Oblivion.GetGame().GetQuestManager().GetQuest(Session.GetHabbo().CurrentQuestId);
                await Session.SendMessageAsync(QuestStartedComposer.Compose(Session, quest));
            }
        }


        internal async Task OpenXmasCalendar()
        {
            var eventName = Request.GetString();
            var giftDay = Request.GetInteger();


            var currentDay = DateTime.Now.Day - 1;

            var data = Session.GetHabbo().Data;


            if (data.OpenedGifts.Contains(giftDay) || giftDay < (currentDay - 2) ||
                giftDay > currentDay || eventName != "xmas16")
            {
                return;
            }

            var itemId = Oblivion.GetGame().GetRandomRewardFurniHandler().GetRandomPrize(1, 1);
            var newItem = Oblivion.GetGame().GetItemManager().GetItem(itemId);
            if (newItem == null) return;

            data.OpenedGifts.Add(giftDay);


            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `user_stats` SET `calendar_gifts` = @giftData WHERE `id` = @habboId LIMIT 1");
                dbClient.AddParameter("giftData", string.Join(",", data.OpenedGifts));
                dbClient.AddParameter("habboId", Session.GetHabbo().Id);
                await dbClient.RunQueryAsync();
            }


            Session.GetHabbo().GetInventoryComponent().AddNewItem("0", itemId, "", 0, true, false, 0, 0);
            Session.GetHabbo().GetInventoryComponent().UpdateItems(false);

            await Response.InitAsync(LibraryParser.OutgoingRequest("CampaignCalendarGiftMessageComposer"));
            Response.AppendBool(true);
            await Response.AppendStringAsync(newItem.Name);
            await Response.AppendStringAsync("");
            await Response.AppendStringAsync(newItem.Name);
            await SendResponse();
        }

        /// <summary>
        ///     Habboes the camera.
        /// </summary>
        internal async Task HabboCamera()
        {
            if (!Session.GetHabbo().InRoom ||
                Session.GetHabbo().Credits < Oblivion.GetGame().GetCameraManager().PurchaseCoinsPrice ||
                Session.GetHabbo().ActivityPoints < Oblivion.GetGame().GetCameraManager().PurchaseDucketsPrice)
                return;

            var Room = Session.GetHabbo().CurrentRoom;

            var User = Room?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (User?.LastPhotoPreview == null)
                return;

            var preview = User.LastPhotoPreview;

            if (Oblivion.GetGame().GetCameraManager().PurchaseCoinsPrice > 0)
            {
                Session.GetHabbo().Credits -= Oblivion.GetGame().GetCameraManager().PurchaseCoinsPrice;
                Session.GetHabbo().UpdateCreditsBalance();
            }

            if (Oblivion.GetGame().GetCameraManager().PurchaseDucketsPrice > 0)
            {
                Session.GetHabbo().ActivityPoints -= Oblivion.GetGame().GetCameraManager().PurchaseDucketsPrice;
                Session.GetHabbo().UpdateActivityPointsBalance();
            }

            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                await dbClient.RunFastQueryAsync("UPDATE `camera_photos` SET `file_state` = 'purchased' WHERE `id` = '" +
                                                 preview.Id +
                                                 "' LIMIT 1");
            }

            var data = "{\"w\":\"" +
                       Oblivion.EscapeJSONString(
                           Oblivion.GetGame()
                               .GetCameraManager()
                               .GetPath(CameraPhotoType.PURCHASED, preview.Id, preview.CreatorId)) + "\", \"n\":\"" +
                       Oblivion.EscapeJSONString(Session.GetHabbo().UserName) + "\", \"s\":\"" +
                       Session.GetHabbo().Id + "\", \"u\":\"" + preview.Id + "\", \"t\":\"" + preview.CreatedAt + "\"}";

            var item = await Session.GetHabbo()
                .GetInventoryComponent()
                .AddNewItem("0", Oblivion.GetGame().GetCameraManager().PhotoPoster.ItemId, data, 0, true, false, 0, 0);
            await Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
            await Session.GetHabbo().GetInventoryComponent().SendNewItems(item.VirtualId);

            await Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_CameraPhotoCount", 1);

            await Response.InitAsync(LibraryParser.OutgoingRequest("CameraPurchaseOk"));
            await SendResponse();
        }

        /// <summary>
        ///     Called when [click].
        /// </summary>
        internal async Task OnClick()
        {
            //uselss only for debug reasons
        }

        /// <summary>
        ///     Gets the friends count.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>System.Int32.</returns>
        private static int GetFriendsCount(uint userId)
        {
            var client = Oblivion.GetHabboById(userId);
            if (client?.GetMessenger()?.Friends == null)
            {
                return 0;
            }

            return client.GetMessenger().Friends.Count;
        }

        /// <summary>
        ///     Targeteds the offer buy.
        /// </summary>
        internal async Task PurchaseTargetedOffer()
        {
            Request.GetInteger();
            var quantity = Request.GetInteger();
            var offer = Oblivion.GetGame().GetTargetedOfferManager().CurrentOffer;
            if (offer == null) return;
            if (Session.GetHabbo().Credits < offer.CostCredits * quantity) return;
            if (Session.GetHabbo().ActivityPoints < offer.CostDuckets * quantity) return;
            if (Session.GetHabbo().Diamonds < offer.CostDiamonds * quantity) return;
            if (Session.GetHabbo().Diamonds < offer.CostDiamonds * quantity) return;
            /* TODO CHECK */
            foreach (var item in offer.Products
                .Select(product => Oblivion.GetGame().GetItemManager().GetItemByName(product))
                .Where(item => item != null))
                Oblivion.GetGame().GetCatalog().DeliverItems(Session, item, quantity, string.Empty, 0, 0, string.Empty);
            Session.GetHabbo().Credits -= offer.CostCredits * quantity;
            Session.GetHabbo().ActivityPoints -= offer.CostDuckets * quantity;
            Session.GetHabbo().Diamonds -= offer.CostDiamonds * quantity;
            Session.GetHabbo().UpdateCreditsBalance(true);
            Session.GetHabbo().UpdateSeasonalCurrencyBalance(true);
            Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
        }

        /// <summary>
        ///     Goes the name of to room by.
        /// </summary>
        internal async Task GoToRoomByName()
        {
            var name = Request.GetString();
            uint roomId = 0;

            switch (name)
            {
                case "predefined_noob_lobby":
                    roomId = Convert.ToUInt32(Oblivion.GetDbConfig().DbData["noob.lobby.roomid"]);
                    break;

                case "random_friending_room":
                    var rooms = Oblivion.GetGame().GetRoomManager().GetActiveRooms().Select(room => room.Key)
                        .Where(room => room != null && room.UsersNow > 0).ToList();
                    if (!rooms.Any())
                        return;
                    if (rooms.Count == 1)
                    {
                        roomId = rooms.First().Id;
                        break;
                    }

                    roomId = rooms[Oblivion.GetRandomNumber(0, rooms.Count)].Id;
                    break;
            }

            if (roomId == 0)
                return;

            await Response.InitAsync(LibraryParser.OutgoingRequest("RoomForwardMessageComposer"));
            await Response.AppendIntegerAsync(roomId);
            await SendResponse();
        }

        /// <summary>
        ///     Gets the uc panel.
        /// </summary>
        internal async Task GetUcPanel()
        {
            var name = Request.GetString();
            switch (name)
            {
                case "new":

                    break;
            }
        }

        /// <summary>
        ///     Gets the uc panel hotel.
        /// </summary>
        internal async Task GetUcPanelHotel()
        {
            var id = Request.GetInteger();
        }

        /// <summary>
        ///     Saves the room thumbnail.
        /// </summary>
        internal async Task SaveRoomThumbnail()
        {
            try
            {
                //todo
/*

                int count = Request.GetInteger();

                byte[] bytes = Request.GetBytes(count);
                //var outData = Converter.Deflate(bytes);

                var url = Web.HttpPostJson(ExtraSettings.BaseJumpPort, null);

              

*/

                if (!Session.GetHabbo().InRoom)
                    return;

                var Room = Session.GetHabbo().CurrentRoom;

                var User = Room?.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

                if (User?.LastPhotoPreview == null)
                    return;

                var preview = User.LastPhotoPreview;

                using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    await dbClient.RunFastQueryAsync("UPDATE `camera_photos` SET `file_state` = 'purchased' WHERE `id` = '" +
                                                     preview.Id +
                                                     "' LIMIT 1");
                }

                var data = "{\"w\":\"" +
                           Oblivion.EscapeJSONString(
                               Oblivion.GetGame()
                                   .GetCameraManager()
                                   .GetPath(CameraPhotoType.PURCHASED, preview.Id, preview.CreatorId)) +
                           "\", \"n\":\"" +
                           Oblivion.EscapeJSONString(Session.GetHabbo().UserName) + "\", \"s\":\"" +
                           Session.GetHabbo().Id + "\", \"u\":\"" + preview.Id + "\", \"t\":\"" + preview.CreatedAt +
                           "\"}";

                var item = await Session.GetHabbo()
                    .GetInventoryComponent()
                    .AddNewItem("0", Oblivion.GetGame().GetCameraManager().PhotoPoster.ItemId, data, 0, true, false, 0,
                        0);
                await Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
                await Session.GetHabbo().GetInventoryComponent().SendNewItems(item.VirtualId);


                var thumb = new ServerMessage(LibraryParser.OutgoingRequest("ThumbnailSuccessMessageComposer"));
                thumb.AppendBool(true);
                thumb.AppendBool(false);
                await Session.SendMessageAsync(thumb);
            }
            catch
            {
                await Session.SendNotifyAsync("Por favor tente novamente, a área da foto possui muitos itens.");
            }
        }

        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            CurrentLoadingRoom = null;
            Response?.Dispose();
            Response = null;
            Session = null;

            Request?.Dispose();
            Request = null;
        }
    }
}