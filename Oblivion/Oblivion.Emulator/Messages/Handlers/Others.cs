using System;
using System.Globalization;
using System.Linq;
using Oblivion.Configuration;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Quests.Composer;
using Oblivion.HabboHotel.Rooms;
using Oblivion.Messages.Parsers;
using Oblivion.Encryption.Encryption;
using Oblivion.Encryption.Encryption.Hurlant.Crypto.Prng;

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
        ///     Gets the session.
        /// </summary>
        /// <returns>GameClient.</returns>
        internal GameClient GetSession() => Session;

        /// <summary>
        ///     Gets the response.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage GetResponse() => Response;

        /// <summary>
        ///     Destroys this instance.
        /// </summary>
        internal void Destroy()
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
        internal void SendResponse()
        {
            if (Response != null && Response.Id > 0 && Session?.GetConnection() != null)
                Session.GetConnection().SendData(Response.GetReversedBytes());
        }

        /// <summary>
        ///     Adds the staff pick.
        /// </summary>
        internal void AddStaffPick()
        {
            Session.SendNotif(Oblivion.GetLanguage().GetVar("addstaffpick_error_1"));
        }

        /// <summary>
        ///     Gets the client version message event.
        /// </summary>
        internal void ReleaseVersion()
        {
            var release = Request.GetString();

            if (release.StartsWith("AIR63"))
                Session.IsAir = true;
        }

        /// <summary>
        ///     Pongs this instance.
        /// </summary>
        internal void Pong()
        {
            if (Session == null) return;

            Session.TimePingedReceived = DateTime.Now;
        }

        /// <summary>
        ///     Disconnects the event.
        /// </summary>
        internal void DisconnectEvent()
        {
            Session.Disconnect("close window");
        }

        /// <summary>
        ///     Latencies the test.
        /// </summary>
        internal void LatencyTest()
        {
            if (Session == null)
                return;

            lock (Session)
            {
                Oblivion.GetGame().GetAchievementManager()
                    .ProgressUserAchievement(Session, "ACH_AllTimeHotelPresence", 1, true);

                Session.TimePingedReceived = DateTime.Now;
                Response.Init(LibraryParser.OutgoingRequest("LatencyTestResponseMessageComposer"));
                Response.AppendInteger(Request.GetIntegerFromString());
                SendResponse();
            }
        }


        /// <summary>
        ///     Initializes the crypto.
        /// </summary>
        internal void InitCrypto()
        {
            if (Session.IsAir)
            {
                Response.Init(LibraryParser.OutgoingRequest("InitCryptoMessageComposer"));
                Response.AppendString(Handler.GetRsaDiffieHellmanPrimeKey().ToLower());
                Response.AppendString(Handler.GetRsaDiffieHellmanGeneratorKey().ToLower());
                SendResponse();
            }
        }


        /// <summary>
        ///     Secrets the key.
        /// </summary>
        internal void SecretKey()
        {
            var cipherKey = Request.GetString();
            var sharedKey = Handler.CalculateDiffieHellmanSharedKey(cipherKey);

            if (Session.IsAir)
            {
                Response.Init(LibraryParser.OutgoingRequest("SecretKeyMessageComposer"));
                Response.AppendString(Handler.GetRsaDiffieHellmanPublicKey().ToLower());
                Response.AppendBool(false);
                SendResponse();

                var data = sharedKey.ToByteArray();

                if (data[data.Length - 1] == 0)
                    Array.Resize(ref data, data.Length - 1);

                Array.Reverse(data, 0, data.Length);

                Session.GetConnection().Arc4ServerSide = new ARC4(data);
            }
        }

        internal void InitConsole()
        {
            Session.GetHabbo().InitMessenger();
        }

        /// <summary>
        ///     Machines the identifier.
        /// </summary>
        internal void MachineId()
        {
            Request.GetString();
            var machineId = Request.GetString();
            Session.MachineId = machineId;

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UniqueMachineIDMessageComposer"));
            serverMessage.AppendString(machineId);
            Session.SendMessage(serverMessage);
        }

        /// <summary>
        ///     Logins the with ticket.
        /// </summary>
        internal void LoginWithTicket()
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
        internal void InfoRetrieve()
        {
            if (Session?.GetHabbo() == null)
                return;
            var habbo = Session.GetHabbo();
            Response.Init(LibraryParser.OutgoingRequest("UserObjectMessageComposer"));
            Response.AppendInteger(habbo.Id);
            Response.AppendString(habbo.UserName);
            Response.AppendString(habbo.Look);
            Response.AppendString(habbo.Gender.ToUpper());
            Response.AppendString(habbo.Motto);
            Response.AppendString("");
            Response.AppendBool(false);
            Response.AppendInteger(habbo.Respect);
            Response.AppendInteger(habbo.DailyRespectPoints);
            Response.AppendInteger(habbo.DailyPetRespectPoints);
            Response.AppendBool(true);
            Response.AppendString(habbo.LastOnline.ToString(CultureInfo.InvariantCulture));
            Response.AppendBool(habbo.CanChangeName());
            Response.AppendBool(false);
            SendResponse();
            Response.Init(LibraryParser.OutgoingRequest("BuildersClubMembershipMessageComposer"));
            Response.AppendInteger(Session.GetHabbo().BuildersExpire);
            Response.AppendInteger(Session.GetHabbo().BuildersItemsMax);
            Response.AppendInteger(2);
            SendResponse();
            var tradeLocked = Session.GetHabbo().CheckTrading();
            var canUseFloorEditor = ExtraSettings.EveryoneUseFloor || Session.GetHabbo().Vip ||
                                    Session.GetHabbo().Rank >= 4;
            Response.Init(LibraryParser.OutgoingRequest("SendPerkAllowancesMessageComposer"));
            Response.AppendInteger(11);
            Response.AppendString("BUILDER_AT_WORK");
            Response.AppendString("");
            Response.AppendBool(canUseFloorEditor);
            Response.AppendString("VOTE_IN_COMPETITIONS");
            Response.AppendString("requirement.unfulfilled.helper_level_2");
            Response.AppendBool(false);
            Response.AppendString("USE_GUIDE_TOOL");
            Response.AppendString(Session.GetHabbo().TalentStatus == "helper" &&
                                  Session.GetHabbo().CurrentTalentLevel >= 4 ||
                                  Session.GetHabbo().Rank >= 4
                ? ""
                : "requirement.unfulfilled.helper_level_4");
            Response.AppendBool(Session.GetHabbo().TalentStatus == "helper" &&
                                Session.GetHabbo().CurrentTalentLevel >= 4 ||
                                Session.GetHabbo().Rank >= 4);
            Response.AppendString("JUDGE_CHAT_REVIEWS");
            Response.AppendString("requirement.unfulfilled.helper_level_6");
            Response.AppendBool(false);
            Response.AppendString("NAVIGATOR_ROOM_THUMBNAIL_CAMERA");
            Response.AppendString("");
            Response.AppendBool(true);
            Response.AppendString("CALL_ON_HELPERS");
            Response.AppendString("");
            Response.AppendBool(true);
            Response.AppendString("CITIZEN");
            Response.AppendString("");
            Response.AppendBool(Session.GetHabbo().TalentStatus == "helper" ||
                                Session.GetHabbo().CurrentTalentLevel >= 4);
            Response.AppendString("MOUSE_ZOOM");
            Response.AppendString("");
            Response.AppendBool(false);
            Response.AppendString("TRADE");
            Response.AppendString(tradeLocked ? "" : "requirement.unfulfilled.no_trade_lock");
            Response.AppendBool(tradeLocked);
            Response.AppendString("CAMERA");
            Response.AppendString("");
            Response.AppendBool(ExtraSettings.EnableBetaCamera);
            Response.AppendString("NAVIGATOR_PHASE_TWO_2014");
            Response.AppendString("");
            Response.AppendBool(true);
            SendResponse();


            GetResponse().Init(LibraryParser.OutgoingRequest("CitizenshipStatusMessageComposer"));
            GetResponse().AppendString("citizenship");
            GetResponse().AppendInteger(1);
            GetResponse().AppendInteger(4);
            SendResponse();

            GetResponse().Init(LibraryParser.OutgoingRequest("AchievementPointsMessageComposer"));
            GetResponse().AppendInteger(Session.GetHabbo().AchievementPoints);
            SendResponse();
            GetResponse().Init(LibraryParser.OutgoingRequest("FigureSetIdsMessageComposer"));
//            Session.GetHabbo().ClothingManager.Serialize(GetResponse());
//            SendResponse();
            /*Response.Init(LibraryParser.OutgoingRequest("NewbieStatusMessageComposer"));
            Response.AppendInteger(0);// 2 = new - 1 = nothing - 0 = not new
            SendResponse();*/
            if (Oblivion.GetGame().GetTargetedOfferManager().CurrentOffer != null)
            {
                Oblivion.GetGame().GetTargetedOfferManager().GenerateMessage(GetResponse());
                SendResponse();
            }
            if (Session.GetHabbo().CurrentQuestId != 0)
            {
                var quest = Oblivion.GetGame().GetQuestManager().GetQuest(Session.GetHabbo().CurrentQuestId);
                Session.SendMessage(QuestStartedComposer.Compose(Session, quest));
            }
        }


        internal void OpenXmasCalendar()
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
                dbClient.RunQuery();
            }


            Session.GetHabbo().GetInventoryComponent().AddNewItem(0, itemId, "", 0, true, false, 0, 0);
            Session.GetHabbo().GetInventoryComponent().UpdateItems(false);

            var message = new ServerMessage(LibraryParser.OutgoingRequest("CampaignCalendarGiftMessageComposer"));
            message.AppendBool(true);
            message.AppendString(newItem.Name);
            message.AppendString("");
            message.AppendString(newItem.Name);
        }

        /// <summary>
        ///     Habboes the camera.
        /// </summary>
        internal void HabboCamera()
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
                dbClient.RunFastQuery("UPDATE `camera_photos` SET `file_state` = 'purchased' WHERE `id` = '" +
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

            var item = Session.GetHabbo()
                .GetInventoryComponent()
                .AddNewItem(0, Oblivion.GetGame().GetCameraManager().PhotoPoster.ItemId, data, 0, true, false, 0, 0);
            Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
            Session.GetHabbo().GetInventoryComponent().SendNewItems(item.VirtualId);

            Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_CameraPhotoCount", 1);

            var message = new ServerMessage(LibraryParser.OutgoingRequest("CameraPurchaseOk"));
            Session.SendMessage(message);
        }

        /// <summary>
        ///     Called when [click].
        /// </summary>
        internal void OnClick()
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
        internal void PurchaseTargetedOffer()
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
        internal void GoToRoomByName()
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
            var roomFwd = new ServerMessage(LibraryParser.OutgoingRequest("RoomForwardMessageComposer"));
            roomFwd.AppendInteger(roomId);
            Session.SendMessage(roomFwd);
        }

        /// <summary>
        ///     Gets the uc panel.
        /// </summary>
        internal void GetUcPanel()
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
        internal void GetUcPanelHotel()
        {
            var id = Request.GetInteger();
        }

        /// <summary>
        ///     Saves the room thumbnail.
        /// </summary>
        internal void SaveRoomThumbnail()
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
                    dbClient.RunFastQuery("UPDATE `camera_photos` SET `file_state` = 'purchased' WHERE `id` = '" +
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

                var item = Session.GetHabbo()
                    .GetInventoryComponent()
                    .AddNewItem(0, Oblivion.GetGame().GetCameraManager().PhotoPoster.ItemId, data, 0, true, false, 0,
                        0);
                Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
                Session.GetHabbo().GetInventoryComponent().SendNewItems(item.VirtualId);


                var thumb = new ServerMessage(LibraryParser.OutgoingRequest("ThumbnailSuccessMessageComposer"));
                thumb.AppendBool(true);
                thumb.AppendBool(false);
                Session.SendMessage(thumb);
            }
            catch
            {
                Session.SendNotif("Por favor tente novamente, a área da foto possui muitos itens.");
            }
        }
    }
}