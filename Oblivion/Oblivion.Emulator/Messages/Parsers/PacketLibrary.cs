using Oblivion.Messages.Handlers;

namespace Oblivion.Messages.Parsers
{
    /// <summary>
    /// Class PacketLibrary.
    /// </summary>
    internal class PacketLibrary
    {
        /// <summary>
        /// Delegate GetProperty
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal delegate void GetProperty(GameClientMessageHandler handler);

        /// <summary>
        /// Initializes the crypto.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void InitCrypto(GameClientMessageHandler handler)
        {
            await handler.InitCrypto();
        }

        /// <summary>
        /// Secrets the key.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SecretKey(GameClientMessageHandler handler)
        {
            await handler.SecretKey();
        }

        /// <summary>
        /// InitConsole
        /// </summary>
        /// <param name="handler">Friends</param>
        internal static async void InitConsole(GameClientMessageHandler handler)
        {
            await handler.InitConsole();
        }

        /// <summary>
        /// Machines the identifier.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void MachineId(GameClientMessageHandler handler)
        {
            await handler.MachineId();
        }

        /// <summary>
        /// Guides the message.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GuideMessage(GameClientMessageHandler handler)
        {
            await handler.CallGuide();
        }

        /// <summary>
        /// Sets the chat preferrence.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static void SetChatPreferrence(GameClientMessageHandler handler)
        {
            handler.SetChatPreferrence();
        }

        /// <summary>
        /// Gets the helper tool.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetHelperTool(GameClientMessageHandler handler)
        {
            await handler.OpenGuideTool();
        }

        /// <summary>
        /// Gets the guide detached.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetGuideDetached(GameClientMessageHandler handler)
        {
            await handler.AnswerGuideRequest();
        }

        /// <summary>
        /// Logins the with ticket.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void LoginWithTicket(GameClientMessageHandler handler)
        {
            await handler.LoginWithTicket();
        }

        /// <summary>
        /// Invites the guide.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void InviteGuide(GameClientMessageHandler handler)
        {
            await handler.InviteToRoom();
        }

        /// <summary>
        /// Visits the room guide.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void VisitRoomGuide(GameClientMessageHandler handler)
        {
            await handler.VisitRoom();
        }

        /// <summary>
        /// Guides the end session.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GuideEndSession(GameClientMessageHandler handler)
        {
            await handler.CloseGuideRequest();
        }

        /// <summary>
        /// Cancels the call guide.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void CancelCallGuide(GameClientMessageHandler handler)
        {
            await handler.CancelCallGuide();
        }

        /// <summary>
        /// Informations the retrieve.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void InfoRetrieve(GameClientMessageHandler handler)
        {
            await handler.InfoRetrieve();
        }

        /// <summary>
        /// Chats the specified await handler.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void Chat(GameClientMessageHandler handler)
        {
            await handler.Chat();
        }

        /// <summary>
        /// Shouts the specified await handler.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void Shout(GameClientMessageHandler handler)
        {
            await handler.Shout();
        }

        /// <summary>
        /// Requests the floor plan used coords.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void RequestFloorPlanUsedCoords(GameClientMessageHandler handler)
        {
            await handler.GetFloorPlanUsedCoords();
        }

        /// <summary>
        /// Requests the floor plan door.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void RequestFloorPlanDoor(GameClientMessageHandler handler)
        {
            await handler.GetFloorPlanDoor();
        }

        /// <summary>
        /// Opens the bully reporting.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void OpenBullyReporting(GameClientMessageHandler handler)
        {
            await handler.OpenBullyReporting();
        }

        /// <summary>
        /// Sends the bully report.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SendBullyReport(GameClientMessageHandler handler)
        {
            await handler.SendBullyReport();
        }

        /// <summary>
        /// Loads the club gifts.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void LoadClubGifts(GameClientMessageHandler handler)
        {
            await handler.LoadClubGifts();
        }

        /// <summary>
        /// Saves the heightmap.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SaveHeightmap(GameClientMessageHandler handler)
        {
            await handler.SaveHeightmap();
        }

        /// <summary>
        /// Accepts the poll.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void AcceptPoll(GameClientMessageHandler handler)
        {
            await handler.AcceptPoll();
        }

        /// <summary>
        /// Refuses the poll.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void RefusePoll(GameClientMessageHandler handler)
        {
            await handler.RefusePoll();
        }

        /// <summary>
        /// Answers the poll question.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void AnswerPollQuestion(GameClientMessageHandler handler)
        {
            await handler.AnswerPoll();
        }

        /// <summary>
        /// Retrieves the song identifier.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void RetrieveSongId(GameClientMessageHandler handler)
        {
            await handler.RetrieveSongId();
        }

        /// <summary>
        /// Tiles the height of the stack magic set.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void TileStackMagicSetHeight(GameClientMessageHandler handler)
        {
            await handler.TileStackMagicSetHeight();
        }

        /// <summary>
        /// Enables the inventory effect.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void EnableInventoryEffect(GameClientMessageHandler handler)
        {
            await handler.EnableEffect();
        }

        /// <summary>
        /// Promotes the room.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void PromoteRoom(GameClientMessageHandler handler)
        {
            await handler.PromoteRoom();
        }

        /// <summary>
        /// Gets the promotionable rooms.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetPromotionableRooms(GameClientMessageHandler handler)
        {
            await handler.GetPromotionableRooms();
        }

        /// <summary>
        /// Gets the room filter.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetRoomFilter(GameClientMessageHandler handler)
        {
            await handler.GetRoomFilter();
        }

        /// <summary>
        /// Alters the room filter.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void AlterRoomFilter(GameClientMessageHandler handler)
        {
            await handler.AlterRoomFilter();
        }

        /// <summary>
        /// Gets the tv player.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetTvPlayer(GameClientMessageHandler handler)
        {
            await handler.GetTvPlayer();
        }

        /// <summary>
        /// Chooses the tv player video.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ChooseTvPlayerVideo(GameClientMessageHandler handler)
        {
            await handler.ChooseTvPlayerVideo();
        }

        /// <summary>
        /// Gets the tv playlist.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetTvPlaylist(GameClientMessageHandler handler)
        {
            await handler.ChooseTvPlaylist();
        }

        /// <summary>
        /// Places the bot.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void PlaceBot(GameClientMessageHandler handler)
        {
            await handler.PlaceBot();
        }

        /// <summary>
        /// Picks up bot.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void PickUpBot(GameClientMessageHandler handler)
        {
            await handler.PickUpBot();
        }

        /// <summary>
        /// Gets the talents track.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetTalentsTrack(GameClientMessageHandler handler)
        {
            await handler.Talents();
        }

        /// <summary>
        /// Prepares the campaing.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void PrepareCampaing(GameClientMessageHandler handler)
        {
            await handler.PrepareCampaing();
        }

        /// <summary>
        /// Pongs the specified await handler.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void Pong(GameClientMessageHandler handler)
        {
            await handler.Pong();
        }

        /// <summary>
        /// Disconnects the event.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void DisconnectEvent(GameClientMessageHandler handler)
        {
            await handler.DisconnectEvent();
        }

        /// <summary>
        /// Latencies the test.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void LatencyTest(GameClientMessageHandler handler)
        {
            await handler.LatencyTest();
        }

        /// <summary>
        /// Receptions the view.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ReceptionView(GameClientMessageHandler handler)
        {
            await handler.GoToHotelView();
        }

        /// <summary>
        /// Called when [confirmation event].
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void OnlineConfirmationEvent(GameClientMessageHandler handler)
        {
            await handler.OnlineConfirmationEvent();
        }

        /// <summary>
        /// Retrives the citizen ship status.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void RetriveCitizenShipStatus(GameClientMessageHandler handler)
        {
            await handler.RetrieveCitizenship();
        }

        /// <summary>
        /// Refreshes the promo event.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void RefreshPromoEvent(GameClientMessageHandler handler)
        {
            await handler.RefreshPromoEvent();
        }

        internal static async void RefreshCompetition(GameClientMessageHandler handler)
        {
            await handler.RefreshCompetition();
        }

        /// <summary>
        /// Widgets the container.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void WidgetContainer(GameClientMessageHandler handler)
        {
            await handler.WidgetContainers();
        }

        /// <summary>
        /// Landings the community goal.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void LandingCommunityGoal(GameClientMessageHandler handler)
        {
            await handler.LandingCommunityGoal();
        }

        /// <summary>
        /// Removes the handitem.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void RemoveHanditem(GameClientMessageHandler handler)
        {
            await handler.RemoveHanditem();
        }

        /// <summary>
        /// Redeems the voucher.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void RedeemVoucher(GameClientMessageHandler handler)
        {
            await handler.RedeemVoucher();
        }

        /// <summary>
        /// Gives the handitem.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GiveHanditem(GameClientMessageHandler handler)
        {
            await handler.GiveHanditem();
        }

        /// <summary>
        /// Initializes the help tool.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void InitHelpTool(GameClientMessageHandler handler)
        {
            await handler.InitHelpTool();
        }

        /// <summary>
        /// Submits the help ticket.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SubmitHelpTicket(GameClientMessageHandler handler)
        {
            await handler.SubmitHelpTicket();
        }

        /// <summary>
        /// Deletes the pending CFH.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void DeletePendingCfh(GameClientMessageHandler handler)
        {
            await handler.DeletePendingCfh();
        }

        /// <summary>
        /// Mods the get user information.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ModGetUserInfo(GameClientMessageHandler handler)
        {
            await handler.ModGetUserInfo();
        }

        /// <summary>
        /// Mods the get user chatlog.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ModGetUserChatlog(GameClientMessageHandler handler)
        {
            await handler.ModGetUserChatlog();
        }

        /// <summary>
        /// Messages from a guy.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void MessageFromAGuy(GameClientMessageHandler handler)
        {
            await handler.GuideSpeak();
        }

        /// <summary>
        /// Mods the get room chatlog.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ModGetRoomChatlog(GameClientMessageHandler handler)
        {
            await handler.ModGetRoomChatlog();
        }

        /// <summary>
        /// Mods the get room tool.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ModGetRoomTool(GameClientMessageHandler handler)
        {
            await handler.ModGetRoomTool();
        }

        /// <summary>
        /// Mods the pick ticket.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ModPickTicket(GameClientMessageHandler handler)
        {
            await handler.ModPickTicket();
        }

        /// <summary>
        /// Mods the release ticket.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ModReleaseTicket(GameClientMessageHandler handler)
        {
            await handler.ModReleaseTicket();
        }

        /// <summary>
        /// Mods the close ticket.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ModCloseTicket(GameClientMessageHandler handler)
        {
            await handler.ModCloseTicket();
        }

        /// <summary>
        /// Mods the get ticket chatlog.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ModGetTicketChatlog(GameClientMessageHandler handler)
        {
            await handler.ModGetTicketChatlog();
        }

        /// <summary>
        /// Mods the get room visits.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ModGetRoomVisits(GameClientMessageHandler handler)
        {
            await handler.ModGetRoomVisits();
        }

        /// <summary>
        /// Mods the send room alert.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ModSendRoomAlert(GameClientMessageHandler handler)
        {
            await handler.ModSendRoomAlert();
        }

        /// <summary>
        /// Mods the perform room action.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ModPerformRoomAction(GameClientMessageHandler handler)
        {
            await handler.ModPerformRoomAction();
        }

        /// <summary>
        /// Mods the send user caution.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ModSendUserCaution(GameClientMessageHandler handler)
        {
            await handler.ModSendUserCaution();
        }

        /// <summary>
        /// Mods the send user message.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ModSendUserMessage(GameClientMessageHandler handler)
        {
            await handler.ModSendUserMessage();
        }

        /// <summary>
        /// Mods the kick user.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ModKickUser(GameClientMessageHandler handler)
        {
            await handler.ModKickUser();
        }

        /// <summary>
        /// Mods the mute user.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ModMuteUser(GameClientMessageHandler handler)
        {
            await handler.ModMuteUser();
        }

        /// <summary>
        /// Mods the lock trade.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ModLockTrade(GameClientMessageHandler handler)
        {
            await handler.ModLockTrade();
        }

        /// <summary>
        /// Mods the ban user.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ModBanUser(GameClientMessageHandler handler)
        {
            await handler.ModBanUser();
        }

        /// <summary>
        /// Initializes the messenger.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void InitMessenger(GameClientMessageHandler handler)
        {
            await handler.InitMessenger();
        }

        /// <summary>
        /// Friendses the list update.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void FriendsListUpdate(GameClientMessageHandler handler)
        {
            await handler.FriendsListUpdate();
        }

        /// <summary>
        /// Removes the buddy.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void RemoveBuddy(GameClientMessageHandler handler)
        {
            await handler.RemoveBuddy();
        }

        /// <summary>
        /// Searches the habbo.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SearchHabbo(GameClientMessageHandler handler)
        {
            await handler.SearchHabbo();
        }

        /// <summary>
        /// Accepts the request.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void AcceptRequest(GameClientMessageHandler handler)
        {
            await handler.AcceptRequest();
        }

        /// <summary>
        /// Declines the request.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void DeclineRequest(GameClientMessageHandler handler)
        {
            await handler.DeclineRequest();
        }

        /// <summary>
        /// Requests the buddy.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void RequestBuddy(GameClientMessageHandler handler)
        {
            await handler.RequestBuddy();
        }

        /// <summary>
        /// Sends the instant messenger.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SendInstantMessenger(GameClientMessageHandler handler)
        {
            await handler.SendInstantMessenger();
        }

        /// <summary>
        /// Follows the buddy.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void FollowBuddy(GameClientMessageHandler handler)
        {
            await handler.FollowBuddy();
        }

        /// <summary>
        /// Sends the instant invite.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SendInstantInvite(GameClientMessageHandler handler)
        {
            await handler.SendInstantInvite();
        }

        /// <summary>
        /// Homes the room stuff.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void HomeRoomStuff(GameClientMessageHandler handler)
        {
            await handler.HomeRoom();
        }

        /// <summary>
        /// Adds the favorite.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void AddFavorite(GameClientMessageHandler handler)
        {
            await handler.AddFavorite();
        }

        /// <summary>
        /// Removes the favorite.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void RemoveFavorite(GameClientMessageHandler handler)
        {
            await handler.RemoveFavorite();
        }

        /// <summary>
        /// Gets the flat cats.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetFlatCats(GameClientMessageHandler handler)
        {
            await handler.GetFlatCats();
        }

        /// <summary>
        /// Enters the inquired room.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void EnterInquiredRoom(GameClientMessageHandler handler)
        {
            await handler.EnterInquiredRoom();
        }

        /// <summary>
        /// Gets the pubs.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetPubs(GameClientMessageHandler handler)
        {
            await handler.GetPubs();
        }

        /// <summary>
        /// Saves the branding.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SaveBranding(GameClientMessageHandler handler)
        {
            await handler.SaveBranding();
        }

        /// <summary>
        /// Gets the room information.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetRoomInfo(GameClientMessageHandler handler)
        {
            await handler.GetRoomInfo();
        }


        /// <summary>
        /// News the navigator flat cats.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void NewNavigatorFlatCats(GameClientMessageHandler handler)
        {
            await handler.NewNavigatorFlatCats();
        }

        /// <summary>
        /// Gets the favorite rooms.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetFavoriteRooms(GameClientMessageHandler handler)
        {
            await handler.GetFavoriteRooms();
        }

        /// <summary>
        /// Gets the recent rooms.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetRecentRooms(GameClientMessageHandler handler)
        {
            await handler.GetRecentRooms();
        }

        /// <summary>
        /// Gets the popular tags.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetPopularTags(GameClientMessageHandler handler)
        {
            await handler.GetPopularTags();
        }

        /// <summary>
        /// Performs the search.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void PerformSearch(GameClientMessageHandler handler)
        {
            await handler.PerformSearch();
        }

        /// <summary>
        /// Searches the by tag.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SearchByTag(GameClientMessageHandler handler)
        {
            await handler.SearchByTag();
        }

        /// <summary>
        /// Performs the search2.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void PerformSearch2(GameClientMessageHandler handler)
        {
            await handler.PerformSearch2();
        }

        /// <summary>
        /// Opens the flat.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void OpenFlat(GameClientMessageHandler handler)
        {
            await handler.OpenFlat();
        }

        /// <summary>
        /// Gets the voume.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetVoume(GameClientMessageHandler handler)
        {
            await handler.LoadSettings();
        }

        /// <summary>
        /// Saves the volume.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static void SaveVolume(GameClientMessageHandler handler)
        {
            handler.SaveSettings();
        }

        /// <summary>
        /// Gets the pub.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetPub(GameClientMessageHandler handler)
        {
            await handler.GetPub();
        }

        /// <summary>
        /// Opens the pub.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void OpenPub(GameClientMessageHandler handler)
        {
            await handler.OpenPub();
        }

        /// <summary>
        /// Gets the inventory.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetInventory(GameClientMessageHandler handler)
        {
            await handler.GetInventory();
        }

        /// <summary>
        /// Gets the inventory.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void OpenInventory(GameClientMessageHandler handler)
        {
            await handler.GetInventory();
        }

        /// <summary>
        /// Get maketplace config.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void MarketPlaceConfiguration(GameClientMessageHandler handler)
        {
            await handler.MarketPlaceConfiguration();
        }

        /// <summary>
        /// Check if user can make offer
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void CanMakeOffer(GameClientMessageHandler handler)
        {
            await handler.CanMakeOffer();
        }

        /// <summary>
        /// Gets the room data1.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetRoomData1(GameClientMessageHandler handler)
        {
            await handler.GetRoomData1();
        }

        /// <summary>
        /// Gets the room data2.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetRoomData2(GameClientMessageHandler handler)
        {
            await handler.GetRoomData2();
        }

        /// <summary>
        /// Gets the room data3.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetRoomData3(GameClientMessageHandler handler)
        {
            await handler.GetRoomData3();
        }

        /// <summary>
        /// Requests the floor items.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void RequestFloorItems(GameClientMessageHandler handler)
        {
            await handler.RequestFloorItems();
        }

        /// <summary>
        /// Requests the wall items.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void RequestWallItems(GameClientMessageHandler handler)
        {
            await handler.RequestWallItems();
        }

        /// <summary>
        /// Called when [room user add].
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void OnRoomUserAdd(GameClientMessageHandler handler)
        {
            await handler.OnRoomUserAdd();
        }

        /// <summary>
        /// Reqs the load room for user.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ReqLoadRoomForUser(GameClientMessageHandler handler)
        {
            await handler.ReqLoadRoomForUser();
        }

        /// <summary>
        /// Enters the on room.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void EnterOnRoom(GameClientMessageHandler handler)
        {
            await handler.EnterOnRoom();
        }

        /// <summary>
        /// Clears the room loading.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static void ClearRoomLoading(GameClientMessageHandler handler)
        {
            handler.ClearRoomLoading();
        }

        /// <summary>
        /// Moves the specified await handler.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void Move(GameClientMessageHandler handler)
        {
            await handler.Move();
        }

        /// <summary>
        /// Determines whether this instance [can create room] the specified await handler.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void CanCreateRoom(GameClientMessageHandler handler)
        {
            await handler.CanCreateRoom();
        }

        /// <summary>
        /// Creates the room.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void CreateRoom(GameClientMessageHandler handler)
        {
            await handler.CreateRoom();
        }

        /// <summary>
        /// Gets the room information.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetRoomInformation(GameClientMessageHandler handler)
        {
            await handler.ParseRoomDataInformation();
        }

        /// <summary>
        /// Gets the room edit data.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetRoomEditData(GameClientMessageHandler handler)
        {
            await handler.GetRoomEditData();
        }

        /// <summary>
        /// Saves the room data.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SaveRoomData(GameClientMessageHandler handler)
        {
            await handler.SaveRoomData();
        }

        /// <summary>
        /// Gives the rights.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GiveRights(GameClientMessageHandler handler)
        {
            await handler.GiveRights();
        }

        /// <summary>
        /// Takes the rights.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void TakeRights(GameClientMessageHandler handler)
        {
            await handler.TakeRights();
        }

        /// <summary>
        /// Takes all rights.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void TakeAllRights(GameClientMessageHandler handler)
        {
            await handler.TakeAllRights();
        }

        /// <summary>
        /// Habboes the camera.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void HabboCamera(GameClientMessageHandler handler)
        {
            await handler.HabboCamera();
        }

        /// <summary>
        /// Open xmas calendar
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void OpenXmasCalendar(GameClientMessageHandler handler)
        {
            await handler.OpenXmasCalendar();
        }

        /// <summary>
        /// Called when [click].
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void OnClick(GameClientMessageHandler handler)
        {
            await handler.OnClick();
        }

        /// <summary>
        /// Kicks the user.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void KickUser(GameClientMessageHandler handler)
        {
            await handler.KickUser();
        }

        /// <summary>
        /// Bans the user.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void BanUser(GameClientMessageHandler handler)
        {
            await handler.BanUser();
        }

        /// <summary>
        /// Sets the home room.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SetHomeRoom(GameClientMessageHandler handler)
        {
            await handler.SetHomeRoom();
        }

        /// <summary>
        /// Deletes the room.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void DeleteRoom(GameClientMessageHandler handler)
        {
            await handler.DeleteRoom();
        }

        /// <summary>
        /// Looks at.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void LookAt(GameClientMessageHandler handler)
        {
            await handler.LookAt();
        }

        internal static async void AirClickUser(GameClientMessageHandler handler)
        {
            await handler.AirClickUser();
        }

        /// <summary>
        /// Starts the typing.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void StartTyping(GameClientMessageHandler handler)
        {
            await handler.StartTyping();
        }

        /// <summary>
        /// Stops the typing.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void StopTyping(GameClientMessageHandler handler)
        {
            await handler.StopTyping();
        }

        /// <summary>
        /// Ignores the user.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void IgnoreUser(GameClientMessageHandler handler)
        {
            await handler.IgnoreUser();
        }

        /// <summary>
        /// Unignores the user.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void UnignoreUser(GameClientMessageHandler handler)
        {
            await handler.UnignoreUser();
        }

        /// <summary>
        /// Determines whether this instance [can create room event] the specified await handler.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void CanCreateRoomEvent(GameClientMessageHandler handler)
        {
            await handler.CanCreateRoomEvent();
        }

        /// <summary>
        /// Signs the specified await handler.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void Sign(GameClientMessageHandler handler)
        {
            await handler.Sign();
        }

        /// <summary>
        /// Gets the user tags.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetUserTags(GameClientMessageHandler handler)
        {
            await handler.GetUserTags();
        }

        /// <summary>
        /// Gets the user badges.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetUserBadges(GameClientMessageHandler handler)
        {
            await handler.GetUserBadges();
        }

        /// <summary>
        /// Rates the room.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void RateRoom(GameClientMessageHandler handler)
        {
            await handler.RateRoom();
        }

        /// <summary>
        /// Dances the specified await handler.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void Dance(GameClientMessageHandler handler)
        {
            await handler.Dance();
        }

        /// <summary>
        /// Answers the doorbell.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void AnswerDoorbell(GameClientMessageHandler handler)
        {
            await handler.AnswerDoorbell();
        }

        /// <summary>
        /// Applies the room effect.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ApplyRoomEffect(GameClientMessageHandler handler)
        {
            await handler.ApplyRoomEffect();
        }

        /// <summary>
        /// Places the post it.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void PlacePostIt(GameClientMessageHandler handler)
        {
            await handler.PlacePostIt();
        }

        /// <summary>
        /// Places the item.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void PlaceItem(GameClientMessageHandler handler)
        {
            await handler.PlaceItem();
        }

        /// <summary>
        /// Places the item.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void AirPlaceItem(GameClientMessageHandler handler)
        {
            await handler.PlaceItem();
        }

        /// <summary>
        /// Takes the item.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void TakeItem(GameClientMessageHandler handler)
        {
            await handler.TakeItem();
        }

        /// <summary>
        /// Moves the item.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void MoveItem(GameClientMessageHandler handler)
        {
            await handler.MoveItem();
        }

        /// <summary>
        /// Moves the wall item.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void MoveWallItem(GameClientMessageHandler handler)
        {
            await handler.MoveWallItem();
        }

        /// <summary>
        /// Triggers the item.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void TriggerItem(GameClientMessageHandler handler)
        {
            await handler.TriggerItem();
        }

        /// <summary>
        /// Triggers the item dice special.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void TriggerItemDiceSpecial(GameClientMessageHandler handler)
        {
            await handler.TriggerItemDiceSpecial();
        }

        /// <summary>
        /// Opens the postit.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void OpenPostit(GameClientMessageHandler handler)
        {
            await handler.OpenPostit();
        }

        /// <summary>
        /// Saves the postit.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SavePostit(GameClientMessageHandler handler)
        {
            await handler.SavePostit();
        }

        /// <summary>
        /// Deletes the postit.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void DeletePostit(GameClientMessageHandler handler)
        {
            await handler.DeletePostit();
        }

        /// <summary>
        /// Opens the present.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void OpenPresent(GameClientMessageHandler handler)
        {
            await handler.OpenGift();
        }

        /// <summary>
        /// Gets the moodlight.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetMoodlight(GameClientMessageHandler handler)
        {
            await handler.GetMoodlight();
        }

        /// <summary>
        /// Updates the moodlight.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void UpdateMoodlight(GameClientMessageHandler handler)
        {
            await handler.UpdateMoodlight();
        }

        /// <summary>
        /// Switches the moodlight status.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SwitchMoodlightStatus(GameClientMessageHandler handler)
        {
            await handler.SwitchMoodlightStatus();
        }

        /// <summary>
        /// Initializes the trade.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void InitTrade(GameClientMessageHandler handler)
        {
            await handler.InitTrade();
        }

        /// <summary>
        /// Offers the trade item.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void OfferTradeItem(GameClientMessageHandler handler)
        {
            await handler.OfferTradeItem();
        }

        /// <summary>
        /// Offers a specific amount of items.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void OfferTradeItems(GameClientMessageHandler handler)
        {
            await handler.OfferTradeItems();
        }

        /// <summary>
        /// Takes the back trade item.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void TakeBackTradeItem(GameClientMessageHandler handler)
        {
            await handler.TakeBackTradeItem();
        }

        /// <summary>
        /// Stops the trade.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void StopTrade(GameClientMessageHandler handler)
        {
            await handler.StopTrade();
        }

        /// <summary>
        /// Accepts the trade.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void AcceptTrade(GameClientMessageHandler handler)
        {
            await handler.AcceptTrade();
        }

        /// <summary>
        /// Unaccepts the trade.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void UnacceptTrade(GameClientMessageHandler handler)
        {
            await handler.UnacceptTrade();
        }

        /// <summary>
        /// Completes the trade.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void CompleteTrade(GameClientMessageHandler handler)
        {
            await handler.CompleteTrade();
        }

        /// <summary>
        /// Gives the respect.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GiveRespect(GameClientMessageHandler handler)
        {
            await handler.GiveRespect();
        }

        /// <summary>
        /// Applies the effect.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ApplyEffect(GameClientMessageHandler handler)
        {
            await handler.ApplyEffect();
        }

        /// <summary>
        /// Enables the effect.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void EnableEffect(GameClientMessageHandler handler)
        {
            await handler.EnableEffect();
        }

        /// <summary>
        /// Recycles the items.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void RecycleItems(GameClientMessageHandler handler)
        {
            await handler.RecycleItems();
        }

        /// <summary>
        /// Redeems the exchange furni.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void RedeemExchangeFurni(GameClientMessageHandler handler)
        {
            await handler.RedeemExchangeFurni();
        }

        /// <summary>
        /// Kicks the bot.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void KickBot(GameClientMessageHandler handler)
        {
            await handler.KickBot();
        }

        /// <summary>
        /// Places the pet.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void PlacePet(GameClientMessageHandler handler)
        {
            await handler.PlacePet();
        }

        /// <summary>
        /// Gets the pet information.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetPetInfo(GameClientMessageHandler handler)
        {
            await handler.GetPetInfo();
        }

        /// <summary>
        /// Picks up pet.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void PickUpPet(GameClientMessageHandler handler)
        {
            await handler.PickUpPet();
        }

        /// <summary>
        /// Composts the monsterplant.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void CompostMonsterplant(GameClientMessageHandler handler)
        {
            await handler.CompostMonsterplant();
        }

        /// <summary>
        /// Moves the pet.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void MovePet(GameClientMessageHandler handler)
        {
            await handler.MovePet();
        }

        /// <summary>
        /// Respects the pet.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void RespectPet(GameClientMessageHandler handler)
        {
            await handler.RespectPet();
        }

        /// <summary>
        /// Adds the saddle.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void AddSaddle(GameClientMessageHandler handler)
        {
            await handler.AddSaddle();
        }

        /// <summary>
        /// Removes the saddle.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void RemoveSaddle(GameClientMessageHandler handler)
        {
            await handler.RemoveSaddle();
        }

        /// <summary>
        /// Rides the specified await handler.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void Ride(GameClientMessageHandler handler)
        {
            await handler.MountOnPet();
        }

        /// <summary>
        /// Unrides the specified await handler.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void Unride(GameClientMessageHandler handler)
        {
            await handler.CancelMountOnPet();
        }

        /// <summary>
        /// Saves the wired.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SaveWired(GameClientMessageHandler handler)
        {
            await handler.SaveWired();
        }

        /// <summary>
        /// Saves the wired condition.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SaveWiredCondition(GameClientMessageHandler handler)
        {
            await handler.SaveWiredConditions();
        }

        /// <summary>
        /// Gets the music data.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetMusicData(GameClientMessageHandler handler)
        {
            await handler.GetMusicData();
        }

        /// <summary>
        /// Adds the playlist item.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void AddPlaylistItem(GameClientMessageHandler handler)
        {
            await handler.AddPlaylistItem();
        }

        /// <summary>
        /// Removes the playlist item.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void RemovePlaylistItem(GameClientMessageHandler handler)
        {
            await handler.RemovePlaylistItem();
        }

        /// <summary>
        /// Gets the disks.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetDisks(GameClientMessageHandler handler)
        {
            await handler.GetDisks();
        }

        /// <summary>
        /// Gets the playlists.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetPlaylists(GameClientMessageHandler handler)
        {
            await handler.GetPlaylists();
        }

        /// <summary>
        /// Gets the user information.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetUserInfo(GameClientMessageHandler handler)
        {
            await handler.GetUserInfo();
        }

        /// <summary>
        /// Loads the profile.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void LoadProfile(GameClientMessageHandler handler)
        {
            await handler.LoadProfile();
        }


        /// <summary>
        /// Gets the balance.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetBalance(GameClientMessageHandler handler)
        {
            await handler.GetBalance();
        }

        /// <summary>
        /// Gets the subscription data.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetSubscriptionData(GameClientMessageHandler handler)
        {
            await handler.GetSubscriptionData();
        }

        /// <summary>
        /// Gets the badges.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetBadges(GameClientMessageHandler handler)
        {
            await handler.GetBadges();
        }

        /// <summary>
        /// Updates the badges.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void UpdateBadges(GameClientMessageHandler handler)
        {
            await handler.UpdateBadges();
        }

        /// <summary>
        /// Gets the achievements.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetAchievements(GameClientMessageHandler handler)
        {
            await handler.GetAchievements();
        }

        /// <summary>
        /// Changes the look.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ChangeLook(GameClientMessageHandler handler)
        {
            await handler.ChangeLook();
        }

        /// <summary>
        /// Changes the motto.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ChangeMotto(GameClientMessageHandler handler)
        {
            await handler.ChangeMotto();
        }

        /// <summary>
        /// Gets the wardrobe.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetWardrobe(GameClientMessageHandler handler)
        {
            await handler.GetWardrobe();
        }

        /// <summary>
        /// Allows all ride.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void AllowAllRide(GameClientMessageHandler handler)
        {
            await handler.AllowAllRide();
        }

        /// <summary>
        /// Saves the wardrobe.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SaveWardrobe(GameClientMessageHandler handler)
        {
            await handler.SaveWardrobe();
        }

        /// <summary>
        /// Gets the pets inventory.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetPetsInventory(GameClientMessageHandler handler)
        {
            await handler.GetPetsInventory();
        }

        /// <summary>
        /// Opens the quests.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void OpenQuests(GameClientMessageHandler handler)
        {
            await handler.OpenQuests();
        }

        /// <summary>
        /// Starts the quest.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void StartQuest(GameClientMessageHandler handler)
        {
            await handler.StartQuest();
        }

        /// <summary>
        /// Stops the quest.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void StopQuest(GameClientMessageHandler handler)
        {
            await handler.StopQuest();
        }

        /// <summary>
        /// Gets the current quest.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetCurrentQuest(GameClientMessageHandler handler)
        {
            await handler.GetCurrentQuest();
        }

        /// <summary>
        /// Gets the group badges.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetGroupBadges(GameClientMessageHandler handler)
        {
            await handler.InitRoomGroupBadges();
        }

        /// <summary>
        /// Gets the bot inv.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetBotInv(GameClientMessageHandler handler)
        {
            await handler.GetBotsInventory();
        }

        /// <summary>
        /// Saves the room bg.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SaveRoomBg(GameClientMessageHandler handler)
        {
            await handler.SaveRoomBg();
        }

        /// <summary>
        /// Goes the room.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GoRoom(GameClientMessageHandler handler)
        {
            await handler.GoRoom();
        }

        /// <summary>
        /// Sits the specified await handler.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void Sit(GameClientMessageHandler handler)
        {
            await handler.Sit();
        }

        /// <summary>
        /// Gets the event rooms.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetEventRooms(GameClientMessageHandler handler)
        {
            await handler.GetEventRooms();
        }

        /// <summary>
        /// Starts the seasonal quest.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void StartSeasonalQuest(GameClientMessageHandler handler)
        {
            await handler.StartSeasonalQuest();
        }

        /// <summary>
        /// Saves the mannequin.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SaveMannequin(GameClientMessageHandler handler)
        {
            await handler.SaveMannequin();
        }

        /// <summary>
        /// Saves the mannequin2.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SaveMannequin2(GameClientMessageHandler handler)
        {
            await handler.SaveMannequin2();
        }

        /// <summary>
        /// Serializes the group purchase page.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SerializeGroupPurchasePage(GameClientMessageHandler handler)
        {
            await handler.SerializeGroupPurchasePage();
        }

        /// <summary>
        /// Serializes the group purchase parts.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SerializeGroupPurchaseParts(GameClientMessageHandler handler)
        {
            await handler.SerializeGroupPurchaseParts();
        }

        /// <summary>
        /// Purchases the group.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void PurchaseGroup(GameClientMessageHandler handler)
        {
            await handler.PurchaseGroup();
        }

        /// <summary>
        /// Serializes the group information.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SerializeGroupInfo(GameClientMessageHandler handler)
        {
            await handler.SerializeGroupInfo();
        }

        /// <summary>
        /// Serializes the group members.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SerializeGroupMembers(GameClientMessageHandler handler)
        {
            await handler.SerializeGroupMembers();
        }

        /// <summary>
        /// Makes the group admin.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void MakeGroupAdmin(GameClientMessageHandler handler)
        {
            await handler.MakeGroupAdmin();
        }

        /// <summary>
        /// Removes the group admin.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void RemoveGroupAdmin(GameClientMessageHandler handler)
        {
            await handler.RemoveGroupAdmin();
        }

        /// <summary>
        /// Accepts the membership.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void AcceptMembership(GameClientMessageHandler handler)
        {
            await handler.AcceptMembership();
        }

        /// <summary>
        /// Declines the membership.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void DeclineMembership(GameClientMessageHandler handler)
        {
            await handler.DeclineMembership();
        }

        /// <summary>
        /// Removes the member.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void RemoveMember(GameClientMessageHandler handler)
        {
            await handler.RemoveMember();
        }

        /// <summary>
        /// Joins the group.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void JoinGroup(GameClientMessageHandler handler)
        {
            await handler.JoinGroup();
        }

        /// <summary>
        /// Makes the fav.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void MakeFav(GameClientMessageHandler handler)
        {
            await handler.MakeFav();
        }

        /// <summary>
        /// Removes the fav.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void RemoveFav(GameClientMessageHandler handler)
        {
            await handler.RemoveFav();
        }

        /// <summary>
        /// Receives the nux gifts.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ReceiveNuxGifts(GameClientMessageHandler handler)
        {
            await handler.ReceiveNuxGifts();
        }

        /// <summary>
        /// Accepts the nux gifts.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void AcceptNuxGifts(GameClientMessageHandler handler)
        {
            await handler.AcceptNuxGifts();
        }

        /// <summary>
        /// Reads the forum thread.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ReadForumThread(GameClientMessageHandler handler)
        {
            await handler.ReadForumThread();
        }

        /// <summary>
        /// Publishes the forum thread.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void PublishForumThread(GameClientMessageHandler handler)
        {
            await handler.PublishForumThread();
        }

        /// <summary>
        /// Updates the forum thread.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void UpdateForumThread(GameClientMessageHandler handler)
        {
            await handler.UpdateThreadState();
        }

        /// <summary>
        /// Alters the state of the forum thread.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void AlterForumThreadState(GameClientMessageHandler handler)
        {
            await handler.AlterForumThreadState();
        }

        /// <summary>
        /// Gets the forum thread root.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetForumThreadRoot(GameClientMessageHandler handler)
        {
            await handler.GetGroupForumThreadRoot();
        }

        /// <summary>
        /// Gets the group forum data.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetGroupForumData(GameClientMessageHandler handler)
        {
            await handler.GetGroupForumData();
        }

        /// <summary>
        /// Gets the group forums.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetGroupForums(GameClientMessageHandler handler)
        {
            await handler.GetGroupForums();
        }

        /// <summary>
        /// Manages the group.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ManageGroup(GameClientMessageHandler handler)
        {
            await handler.ManageGroup();
        }

        /// <summary>
        /// Updates the name of the group.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void UpdateGroupName(GameClientMessageHandler handler)
        {
            await handler.UpdateGroupName();
        }

        /// <summary>
        /// Updates the group badge.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void UpdateGroupBadge(GameClientMessageHandler handler)
        {
            await handler.UpdateGroupBadge();
        }

        /// <summary>
        /// Updates the group colours.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void UpdateGroupColours(GameClientMessageHandler handler)
        {
            await handler.UpdateGroupColours();
        }

        /// <summary>
        /// Updates the group settings.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void UpdateGroupSettings(GameClientMessageHandler handler)
        {
            await handler.UpdateGroupSettings();
        }

        /// <summary>
        /// Serializes the group furni page.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SerializeGroupFurniPage(GameClientMessageHandler handler)
        {
            await handler.SerializeGroupFurniPage();
        }

        /// <summary>
        /// Ejects the furni.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void EjectFurni(GameClientMessageHandler handler)
        {
            await handler.EjectFurni();
        }

        /// <summary>
        /// Mutes the user.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void MuteUser(GameClientMessageHandler handler)
        {
            await handler.MuteUser();
        }

        /// <summary>
        /// Checks the name.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void CheckName(GameClientMessageHandler handler)
        {
            await handler.CheckName();
        }

        /// <summary>
        /// Changes the name.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ChangeName(GameClientMessageHandler handler)
        {
            await handler.ChangeName();
        }

        /// <summary>
        /// Gets the trainer panel.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetTrainerPanel(GameClientMessageHandler handler)
        {
            await handler.GetTrainerPanel();
        }

        /// <summary>
        /// Updates the event information.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void UpdateEventInfo(GameClientMessageHandler handler)
        {
            await handler.UpdateEventInfo();
        }

        /// <summary>
        /// Gets the room banned users.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetRoomBannedUsers(GameClientMessageHandler handler)
        {
            await handler.GetBannedUsers();
        }

        /// <summary>
        /// Userses the with rights.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void UsersWithRights(GameClientMessageHandler handler)
        {
            await handler.UsersWithRights();
        }

        /// <summary>
        /// Unbans the user.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void UnbanUser(GameClientMessageHandler handler)
        {
            await handler.UnbanUser();
        }

        /// <summary>
        /// Manages the bot actions.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ManageBotActions(GameClientMessageHandler handler)
        {
            await handler.ManageBotActions();
        }

        /// <summary>
        /// Handles the bot speech list.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void HandleBotSpeechList(GameClientMessageHandler handler)
        {
            await handler.HandleBotSpeechList();
        }

        /// <summary>
        /// Gets the relationships.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetRelationships(GameClientMessageHandler handler)
        {
            await handler.GetRelationships();
        }

        /// <summary>
        /// Sets the relationship.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SetRelationship(GameClientMessageHandler handler)
        {
            await handler.SetRelationship();
        }

        /// <summary>
        /// Automatics the room.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void AutoRoom(GameClientMessageHandler handler)
        {
            await handler.RoomOnLoad();
        }

        /// <summary>
        /// Mutes all.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void MuteAll(GameClientMessageHandler handler)
        {
            await handler.MuteAll();
        }

        /// <summary>
        /// Completes the saftey quiz.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void CompleteSafteyQuiz(GameClientMessageHandler handler)
        {
            await handler.CompleteSafetyQuiz();
        }

        /// <summary>
        /// Removes the favourite room.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void RemoveFavouriteRoom(GameClientMessageHandler handler)
        {
            await handler.RemoveFavouriteRoom();
        }

        /// <summary>
        /// Rooms the user action.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void RoomUserAction(GameClientMessageHandler handler)
        {
            await handler.RoomUserAction();
        }

        /// <summary>
        /// Saves the football outfit.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SaveFootballOutfit(GameClientMessageHandler handler)
        {
            await handler.SaveFootballOutfit();
        }

        /// <summary>
        /// Confirms the love lock.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ConfirmLoveLock(GameClientMessageHandler handler)
        {
            await handler.ConfirmLoveLock();
        }

        /// <summary>
        /// Builderses the club update furni count.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void BuildersClubUpdateFurniCount(GameClientMessageHandler handler)
        {
            await handler.BuildersClubUpdateFurniCount();
        }

        /// <summary>
        /// Gets the client version message event.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static void ReleaseVersion(GameClientMessageHandler handler)
        {
            handler.ReleaseVersion();
        }

        /// <summary>
        /// Places the builders furniture.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void PlaceBuildersFurniture(GameClientMessageHandler handler)
        {
            await handler.PlaceBuildersFurniture();
        }

        /// <summary>
        /// Whispers the specified await handler.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void Whisper(GameClientMessageHandler handler)
        {
            await handler.Whisper();
        }

        /// <summary>
        /// Catalogues the index.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void CatalogueIndex(GameClientMessageHandler handler)
        {
            await handler.CatalogueIndex();
        }

        /// <summary>
        /// Catalogues the index.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetCatalogMode(GameClientMessageHandler handler)
        {
            await handler.CatalogueMode();
        }

        /// <summary>
        /// Catalogues the page.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void CataloguePage(GameClientMessageHandler handler)
        {
            await handler.CataloguePage();
        }

        /// <summary>
        /// Catalogues the club page.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void CatalogueClubPage(GameClientMessageHandler handler)
        {
            await handler.CatalogueClubPage();
        }

        /// <summary>
        /// Catalogues the offers configuration.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void CatalogueOffersConfig(GameClientMessageHandler handler)
        {
            await handler.CatalogueOfferConfig();
        }

        internal static async void PurchaseOffer(GameClientMessageHandler handler)
        {
            await handler.PurchaseOffer();
        }

        internal static async void CancelOffer(GameClientMessageHandler handler)
        {
            await handler.CancelOffer();
        }

        internal static async void GetItemStats(GameClientMessageHandler handler)
        {
            await handler.GetItemStats();
        }

        internal static async void GetMyOffers(GameClientMessageHandler handler)
        {
            await handler.GetMyOffers();
        }

        internal static async void MakeOffer(GameClientMessageHandler handler)
        {
            await handler.MakeOffer();
        }

        internal static async void ReedemCredits(GameClientMessageHandler handler)
        {
            await handler.ReedemCredits();
        }

        /// <summary>
        /// Catalogues the single offer.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void CatalogueSingleOffer(GameClientMessageHandler handler)
        {
            await handler.CatalogueOffer();
        }

        /// <summary>
        /// get marketplace offers
        /// </summary>
        /// <param name="handler"></param>
        internal static async void GetOffers(GameClientMessageHandler handler)
        {
            await handler.GetOffers();
        }

        /// <summary>
        /// Checks the name of the pet.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void CheckPetName(GameClientMessageHandler handler)
        {
            await handler.CheckPetName();
        }

        /// <summary>
        /// Purchases the item.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void PurchaseItem(GameClientMessageHandler handler)
        {
            await handler.PurchaseItem();
        }

        /// <summary>
        /// Purchases the gift.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void PurchaseGift(GameClientMessageHandler handler)
        {
            await handler.PurchaseGift();
        }

        /// <summary>
        /// Gets the pet breeds.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetPetBreeds(GameClientMessageHandler handler)
        {
            await handler.GetPetBreeds();
        }

        /// <summary>
        /// Reloads the ecotron.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ReloadEcotron(GameClientMessageHandler handler)
        {
            await handler.ReloadEcotron();
        }

        /// <summary>
        /// Gifts the wrapping configuration.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GiftWrappingConfig(GameClientMessageHandler handler)
        {
            await handler.GiftWrappingConfig();
        }

        /// <summary>
        /// Recyclers the rewards.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void RecyclerRewards(GameClientMessageHandler handler)
        {
            await handler.GetRecyclerRewards();
        }

        /// <summary>
        /// Requests the leave group.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void RequestLeaveGroup(GameClientMessageHandler handler)
        {
            await handler.RequestLeaveGroup();
        }

        /// <summary>
        /// Confirms the leave group.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ConfirmLeaveGroup(GameClientMessageHandler handler)
        {
            await handler.ConfirmLeaveGroup();
        }

        /// <summary>
        /// News the navigator.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void NewNavigator(GameClientMessageHandler handler)
        {
            await handler.NewNavigator();
        }

        /// <summary>
        /// Searches the new navigator.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SearchNewNavigator(GameClientMessageHandler handler)
        {
            await handler.SearchNewNavigator();
        }

        /// <summary>
        /// News the navigator delete saved search.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void NewNavigatorDeleteSavedSearch(GameClientMessageHandler handler)
        {
            await handler.NewNavigatorDeleteSavedSearch();
        }

        /// <summary>
        /// News the navigator resize.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void NewNavigatorResize(GameClientMessageHandler handler)
        {
            await handler.NewNavigatorResize();
        }

        internal static async void HabboAirGetUserRooms(GameClientMessageHandler handler)
        {
            await handler.HabboAirGetUserRooms();
        }

        internal static async void HabboAirGetAllRooms(GameClientMessageHandler handler)
        {
            await handler.HabboAirGetAllRooms();
        }

        /// <summary>
        /// News the navigator add saved search.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void NewNavigatorAddSavedSearch(GameClientMessageHandler handler)
        {
            await handler.NewNavigatorAddSavedSearch();
        }

        /// <summary>
        /// News the navigator collapse category.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void NewNavigatorCollapseCategory(GameClientMessageHandler handler)
        {
            await handler.NewNavigatorCollapseCategory();
        }

        /// <summary>
        /// News the navigator uncollapse category.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void NewNavigatorUncollapseCategory(GameClientMessageHandler handler)
        {
            await handler.NewNavigatorUncollapseCategory();
        }

        /// <summary>
        /// Pets the breed result.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void PetBreedResult(GameClientMessageHandler handler)
        {
            await handler.PetBreedResult();
        }

        /// <summary>
        /// Pets the breed cancel.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void PetBreedCancel(GameClientMessageHandler handler)
        {
            await handler.PetBreedCancel();
        }

        /// <summary>
        /// Games the center load game.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GameCenterLoadGame(GameClientMessageHandler handler)
        {
            await handler.GameCenterLoadGame();
        }

        /// <summary>
        /// Get game lists
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetGameListing(GameClientMessageHandler handler)
        {
            await handler.GetGameListing();
        }

        /// <summary>
        /// Init the game center
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void InitializeGameCenter(GameClientMessageHandler handler)
        {
            await handler.InitializeGameCenter();
        }

        /// <summary>
        /// Games the center join queue.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GameCenterJoinQueue(GameClientMessageHandler handler)
        {
            await handler.GameCenterJoinQueue();
        }

        /// <summary>
        /// Hotels the view countdown.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void HotelViewCountdown(GameClientMessageHandler handler)
        {
            await handler.HotelViewCountdown();
        }

        /// <summary>
        /// Hotels the view dailyquest.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void HotelViewDailyquest(GameClientMessageHandler handler)
        {
            await handler.HotelViewDailyquest();
        }

        /// <summary>
        /// Places the builders wall item.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void PlaceBuildersWallItem(GameClientMessageHandler handler)
        {
            await handler.PlaceBuildersWallItem();
        }

        /// <summary>
        /// Targeteds the offer buy.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void PurchaseTargetedOffer(GameClientMessageHandler handler)
        {
            await handler.PurchaseTargetedOffer();
        }

        /// <summary>
        /// Ambassadors the alert.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void AmbassadorAlert(GameClientMessageHandler handler)
        {
            await handler.AmbassadorAlert();
        }

        /// <summary>
        /// Goes the name of to room by.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GoToRoomByName(GameClientMessageHandler handler)
        {
            await handler.GoToRoomByName();
        }

        /// <summary>
        /// Gets the uc panel.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetUcPanel(GameClientMessageHandler handler)
        {
            await handler.GetUcPanel();
        }

        /// <summary>
        /// Gets the uc panel hotel.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetUcPanelHotel(GameClientMessageHandler handler)
        {
            await handler.GetUcPanelHotel();
        }

        /// <summary>
        /// Saves the room thumbnail.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SaveRoomThumbnail(GameClientMessageHandler handler)
        {
            await handler.SaveRoomThumbnail();
        }

        /// <summary>
        /// Uses the purchasable clothing.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void UsePurchasableClothing(GameClientMessageHandler handler)
        {
            await handler.UsePurchasableClothing();
        }

        /// <summary>
        /// Gets the user look.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetUserLook(GameClientMessageHandler handler)
        {
            await handler.GetUserLook();
        }

        /// <summary>
        /// Sets the invitations preference.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static void SetInvitationsPreference(GameClientMessageHandler handler)
        {
            handler.SetInvitationsPreference();
        }

        /// <summary>
        /// Finds the more friends.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void FindMoreFriends(GameClientMessageHandler handler)
        {
            await handler.FindMoreFriends();
        }

        /// <summary>
        /// Hotels the view request badge.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void HotelViewRequestBadge(GameClientMessageHandler handler)
        {
            await handler.HotelViewRequestBadge();
        }

        /// <summary>
        /// Gets the camera price.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetCameraPrice(GameClientMessageHandler handler)
        {
            await handler.GetCameraPrice();
        }

        /// <summary>
        /// Toggles the staff pick.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void ToggleStaffPick(GameClientMessageHandler handler)
        {
            await handler.ToggleStaffPick();
        }

        /// <summary>
        /// Gets the hotel view hall of fame.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetHotelViewHallOfFame(GameClientMessageHandler handler)
        {
            await handler.GetHotelViewHallOfFame();
        }

        /// <summary>
        /// Submits the room to competition.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void SubmitRoomToCompetition(GameClientMessageHandler handler)
        {
            await handler.SubmitRoomToCompetition();
        }

        /// <summary>
        /// Enters the room queue.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void EnterRoomQueue(GameClientMessageHandler handler)
        {
            await handler.EnterRoomQueue();
        }

        /// <summary>
        /// Gets the camera request.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void GetCameraRequest(GameClientMessageHandler handler)
        {
            await handler.GetCameraRequest();
        }

        /// <summary>
        /// Votes for room.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void VoteForRoom(GameClientMessageHandler handler)
        {
            await handler.VoteForRoom();
        }

        /// <summary>
        /// Updates the forum settings.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void UpdateForumSettings(GameClientMessageHandler handler)
        {
            await handler.UpdateForumSettings();
        }

        /// <summary>
        /// Friends the request list load.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void FriendRequestListLoad(GameClientMessageHandler handler)
        {
            await handler.FriendRequestListLoad();
        }

        /// <summary>
        /// Sets the room camera preferences.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static void SetRoomCameraPreferences(GameClientMessageHandler handler)
        {
            handler.SetRoomCameraPreferences();
        }

        /// <summary>
        /// Deletes the group.
        /// </summary>
        /// <param name="handler">The await handler.</param>
        internal static async void DeleteGroup(GameClientMessageHandler handler)
        {
            await handler.DeleteGroup();
        }
    }
}