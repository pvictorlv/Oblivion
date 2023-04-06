using System;
using System.Threading.Tasks;
using Oblivion.Configuration;
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
        /// <param name="handler">The try {  await handler.</param>
        internal delegate void GetProperty(GameClientMessageHandler handler);

        /// <summary>
        /// Initializes the crypto.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void InitCrypto(GameClientMessageHandler handler)
        {
            try
            {
                await handler.InitCrypto();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Secrets the key.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void SecretKey(GameClientMessageHandler handler)
        {
            try
            {
                handler.SecretKey();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// InitConsole
        /// </summary>
        /// <param name="handler">Friends</param>
        internal static async void InitConsole(GameClientMessageHandler handler)
        {
            try
            {
                await handler.InitConsole();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Machines the identifier.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void MachineId(GameClientMessageHandler handler)
        {
            try
            {
                await handler.MachineId();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Guides the message.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GuideMessage(GameClientMessageHandler handler)
        {
            try
            {
                await handler.CallGuide();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Sets the chat preferrence.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static  void SetChatPreferrence(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SetChatPreferrence();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the helper tool.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetHelperTool(GameClientMessageHandler handler)
        {
            try
            {
                await handler.OpenGuideTool();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the guide detached.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetGuideDetached(GameClientMessageHandler handler)
        {
            try
            {
                await handler.AnswerGuideRequest();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Logins the with ticket.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void LoginWithTicket(GameClientMessageHandler handler)
        {
            try
            {
                await handler.LoginWithTicket();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Invites the guide.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void InviteGuide(GameClientMessageHandler handler)
        {
            try
            {
                await handler.InviteToRoom();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Visits the room guide.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void VisitRoomGuide(GameClientMessageHandler handler)
        {
            try
            {
                await handler.VisitRoom();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Guides the end session.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GuideEndSession(GameClientMessageHandler handler)
        {
            try
            {
                await handler.CloseGuideRequest();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Cancels the call guide.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void CancelCallGuide(GameClientMessageHandler handler)
        {
            try
            {
                await handler.CancelCallGuide();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Informations the retrieve.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void InfoRetrieve(GameClientMessageHandler handler)
        {
            try
            {
                await handler.InfoRetrieve();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Chats the specified try {  await handler.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void Chat(GameClientMessageHandler handler)
        {
            try
            {
                await handler.Chat();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Shouts the specified try {  await handler.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void Shout(GameClientMessageHandler handler)
        {
            try
            {
                await handler.Shout();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Requests the floor plan used coords.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void RequestFloorPlanUsedCoords(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetFloorPlanUsedCoords();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Requests the floor plan door.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void RequestFloorPlanDoor(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetFloorPlanDoor();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Opens the bully reporting.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void OpenBullyReporting(GameClientMessageHandler handler)
        {
            try
            {
                await handler.OpenBullyReporting();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Sends the bully report.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SendBullyReport(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SendBullyReport();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Loads the club gifts.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void LoadClubGifts(GameClientMessageHandler handler)
        {
            try
            {
                await handler.LoadClubGifts();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Saves the heightmap.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SaveHeightmap(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SaveHeightmap();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Accepts the poll.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void AcceptPoll(GameClientMessageHandler handler)
        {
            try
            {
                await handler.AcceptPoll();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Refuses the poll.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void RefusePoll(GameClientMessageHandler handler)
        {
            try
            {
                await handler.RefusePoll();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Answers the poll question.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void AnswerPollQuestion(GameClientMessageHandler handler)
        {
            try
            {
                await handler.AnswerPoll();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Retrieves the song identifier.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void RetrieveSongId(GameClientMessageHandler handler)
        {
            try
            {
                await handler.RetrieveSongId();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Tiles the height of the stack magic set.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void TileStackMagicSetHeight(GameClientMessageHandler handler)
        {
            try
            {
                await handler.TileStackMagicSetHeight();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Enables the inventory effect.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void EnableInventoryEffect(GameClientMessageHandler handler)
        {
            try
            {
                await handler.EnableEffect();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Promotes the room.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void PromoteRoom(GameClientMessageHandler handler)
        {
            try
            {
                await handler.PromoteRoom();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the promotionable rooms.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetPromotionableRooms(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetPromotionableRooms();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the room filter.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetRoomFilter(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetRoomFilter();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Alters the room filter.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void AlterRoomFilter(GameClientMessageHandler handler)
        {
            try
            {
                await handler.AlterRoomFilter();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the tv player.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetTvPlayer(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetTvPlayer();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Chooses the tv player video.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void ChooseTvPlayerVideo(GameClientMessageHandler handler)
        {
            try
            {
                handler.ChooseTvPlayerVideo();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the tv playlist.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetTvPlaylist(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ChooseTvPlaylist();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Places the bot.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void PlaceBot(GameClientMessageHandler handler)
        {
            try
            {
                await handler.PlaceBot();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Picks up bot.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void PickUpBot(GameClientMessageHandler handler)
        {
            try
            {
                await handler.PickUpBot();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the talents track.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetTalentsTrack(GameClientMessageHandler handler)
        {
            try
            {
                await handler.Talents();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Prepares the campaing.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void PrepareCampaing(GameClientMessageHandler handler)
        {
            try
            {
                await handler.PrepareCampaing();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Pongs the specified try {  await handler.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void Pong(GameClientMessageHandler handler)
        {
            try
            {
                handler.Pong();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Disconnects the event.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void DisconnectEvent(GameClientMessageHandler handler)
        {
            try
            {
                handler.DisconnectEvent();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Latencies the test.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void LatencyTest(GameClientMessageHandler handler)
        {
            try
            {
                await handler.LatencyTest();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Receptions the view.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ReceptionView(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GoToHotelView();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Called when [confirmation event].
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void OnlineConfirmationEvent(GameClientMessageHandler handler)
        {
            try
            {
                handler.OnlineConfirmationEvent();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Retrives the citizen ship status.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void RetriveCitizenShipStatus(GameClientMessageHandler handler)
        {
            try
            {
                await handler.RetrieveCitizenship();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Refreshes the promo event.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void RefreshPromoEvent(GameClientMessageHandler handler)
        {
            try
            {
                await handler.RefreshPromoEvent();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        internal static void RefreshCompetition(GameClientMessageHandler handler)
        {
            try
            {
                handler.RefreshCompetition();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Widgets the container.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void WidgetContainer(GameClientMessageHandler handler)
        {
            try
            {
                await handler.WidgetContainers();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Landings the community goal.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void LandingCommunityGoal(GameClientMessageHandler handler)
        {
            try
            {
                await handler.LandingCommunityGoal();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Removes the handitem.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void RemoveHanditem(GameClientMessageHandler handler)
        {
            try
            {
                await handler.RemoveHanditem();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Redeems the voucher.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void RedeemVoucher(GameClientMessageHandler handler)
        {
            try
            {
                await handler.RedeemVoucher();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gives the handitem.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GiveHanditem(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GiveHanditem();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Initializes the help tool.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void InitHelpTool(GameClientMessageHandler handler)
        {
            try
            {
                await handler.InitHelpTool();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Submits the help ticket.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SubmitHelpTicket(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SubmitHelpTicket();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Deletes the pending CFH.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void DeletePendingCfh(GameClientMessageHandler handler)
        {
            try
            {
                await handler.DeletePendingCfh();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the get user information.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ModGetUserInfo(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ModGetUserInfo();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the get user chatlog.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ModGetUserChatlog(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ModGetUserChatlog();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Messages from a guy.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void MessageFromAGuy(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GuideSpeak();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the get room chatlog.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ModGetRoomChatlog(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ModGetRoomChatlog();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the get room tool.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ModGetRoomTool(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ModGetRoomTool();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the pick ticket.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ModPickTicket(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ModPickTicket();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the release ticket.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ModReleaseTicket(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ModReleaseTicket();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the close ticket.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ModCloseTicket(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ModCloseTicket();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the get ticket chatlog.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ModGetTicketChatlog(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ModGetTicketChatlog();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the get room visits.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ModGetRoomVisits(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ModGetRoomVisits();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the send room alert.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ModSendRoomAlert(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ModSendRoomAlert();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the perform room action.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ModPerformRoomAction(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ModPerformRoomAction();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the send user caution.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ModSendUserCaution(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ModSendUserCaution();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the send user message.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ModSendUserMessage(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ModSendUserMessage();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the kick user.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ModKickUser(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ModKickUser();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the mute user.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ModMuteUser(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ModMuteUser();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the lock trade.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ModLockTrade(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ModLockTrade();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the ban user.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ModBanUser(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ModBanUser();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Initializes the messenger.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void InitMessenger(GameClientMessageHandler handler)
        {
            try
            {
                handler.InitMessenger();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Friendses the list update.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void FriendsListUpdate(GameClientMessageHandler handler)
        {
            try
            {
                handler.FriendsListUpdate();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Removes the buddy.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void RemoveBuddy(GameClientMessageHandler handler)
        {
            try
            {
                await handler.RemoveBuddy();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Searches the habbo.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SearchHabbo(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SearchHabbo();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Accepts the request.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void AcceptRequest(GameClientMessageHandler handler)
        {
            try
            {
                await handler.AcceptRequest();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Declines the request.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void DeclineRequest(GameClientMessageHandler handler)
        {
            try
            {
                await handler.DeclineRequest();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Requests the buddy.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void RequestBuddy(GameClientMessageHandler handler)
        {
            try
            {
                await handler.RequestBuddy();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Sends the instant messenger.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SendInstantMessenger(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SendInstantMessenger();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Follows the buddy.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void FollowBuddy(GameClientMessageHandler handler)
        {
            try
            {
                await handler.FollowBuddy();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Sends the instant invite.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SendInstantInvite(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SendInstantInvite();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Homes the room stuff.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void HomeRoomStuff(GameClientMessageHandler handler)
        {
            try
            {
                await handler.HomeRoom();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Adds the favorite.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void AddFavorite(GameClientMessageHandler handler)
        {
            try
            {
                await handler.AddFavorite();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Removes the favorite.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void RemoveFavorite(GameClientMessageHandler handler)
        {
            try
            {
                await handler.RemoveFavorite();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the flat cats.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetFlatCats(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetFlatCats();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Enters the inquired room.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void EnterInquiredRoom(GameClientMessageHandler handler)
        {
            try
            {
                handler.EnterInquiredRoom();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the pubs.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetPubs(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetPubs();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Saves the branding.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SaveBranding(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SaveBranding();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the room information.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetRoomInfo(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetRoomInfo();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }


        /// <summary>
        /// News the navigator flat cats.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void NewNavigatorFlatCats(GameClientMessageHandler handler)
        {
            try
            {
                await handler.NewNavigatorFlatCats();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the favorite rooms.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetFavoriteRooms(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetFavoriteRooms();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the recent rooms.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetRecentRooms(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetRecentRooms();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the popular tags.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetPopularTags(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetPopularTags();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Performs the search.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void PerformSearch(GameClientMessageHandler handler)
        {
            try
            {
                await handler.PerformSearch();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Searches the by tag.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SearchByTag(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SearchByTag();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Performs the search2.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void PerformSearch2(GameClientMessageHandler handler)
        {
            try
            {
                await handler.PerformSearch2();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Opens the flat.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void OpenFlat(GameClientMessageHandler handler)
        {
            try
            {
                await handler.OpenFlat();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the voume.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetVoume(GameClientMessageHandler handler)
        {
            try
            {
                await handler.LoadSettings();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Saves the volume.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void SaveVolume(GameClientMessageHandler handler)
        {
            try
            {
                handler.SaveSettings();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the pub.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void GetPub(GameClientMessageHandler handler)
        {
            try
            {
                handler.GetPub();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Opens the pub.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void OpenPub(GameClientMessageHandler handler)
        {
            try
            {
                await handler.OpenPub();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the inventory.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetInventory(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetInventory();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the inventory.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void OpenInventory(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetInventory();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Get maketplace config.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void MarketPlaceConfiguration(GameClientMessageHandler handler)
        {
            try
            {
                await handler.MarketPlaceConfiguration();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Check if user can make offer
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void CanMakeOffer(GameClientMessageHandler handler)
        {
            try
            {
                await handler.CanMakeOffer();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the room data1.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void GetRoomData1(GameClientMessageHandler handler)
        {
            try
            {
                handler.GetRoomData1();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the room data2.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetRoomData2(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetRoomData2();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the room data3.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetRoomData3(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetRoomData3();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Requests the floor items.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void RequestFloorItems(GameClientMessageHandler handler)
        {
            try
            {
                handler.RequestFloorItems();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Requests the wall items.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void RequestWallItems(GameClientMessageHandler handler)
        {
            try
            {
                handler.RequestWallItems();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Called when [room user add].
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void OnRoomUserAdd(GameClientMessageHandler handler)
        {
            try
            {
                await handler.OnRoomUserAdd();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Reqs the load room for user.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ReqLoadRoomForUser(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ReqLoadRoomForUser();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Enters the on room.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void EnterOnRoom(GameClientMessageHandler handler)
        {
            try
            {
                await handler.EnterOnRoom();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Clears the room loading.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void ClearRoomLoading(GameClientMessageHandler handler)
        {
            try
            {
                handler.ClearRoomLoading();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Moves the specified try {  await handler.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void Move(GameClientMessageHandler handler)
        {
            try
            {
                await handler.Move();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Determines whether this instance [can create room] the specified try {  await handler.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void CanCreateRoom(GameClientMessageHandler handler)
        {
            try
            {
                await handler.CanCreateRoom();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Creates the room.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void CreateRoom(GameClientMessageHandler handler)
        {
            try
            {
                await handler.CreateRoom();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the room information.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetRoomInformation(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ParseRoomDataInformation();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the room edit data.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetRoomEditData(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetRoomEditData();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Saves the room data.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SaveRoomData(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SaveRoomData();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gives the rights.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GiveRights(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GiveRights();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Takes the rights.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void TakeRights(GameClientMessageHandler handler)
        {
            try
            {
                await handler.TakeRights();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Takes all rights.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void TakeAllRights(GameClientMessageHandler handler)
        {
            try
            {
                await handler.TakeAllRights();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Habboes the camera.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void HabboCamera(GameClientMessageHandler handler)
        {
            try
            {
                await handler.HabboCamera();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Open xmas calendar
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void OpenXmasCalendar(GameClientMessageHandler handler)
        {
            try
            {
                await handler.OpenXmasCalendar();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Called when [click].
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void OnClick(GameClientMessageHandler handler)
        {
            try
            {
                handler.OnClick();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Kicks the user.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void KickUser(GameClientMessageHandler handler)
        {
            try
            {
                await handler.KickUser();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Bans the user.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void BanUser(GameClientMessageHandler handler)
        {
            try
            {
                await handler.BanUser();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Sets the home room.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SetHomeRoom(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SetHomeRoom();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Deletes the room.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void DeleteRoom(GameClientMessageHandler handler)
        {
            try
            {
                await handler.DeleteRoom();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Looks at.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void LookAt(GameClientMessageHandler handler)
        {
            try
            {
                await handler.LookAt();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        internal static async void AirClickUser(GameClientMessageHandler handler)
        {
            try
            {
                await handler.AirClickUser();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Starts the typing.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void StartTyping(GameClientMessageHandler handler)
        {
            try
            {
                await handler.StartTyping();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Stops the typing.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void StopTyping(GameClientMessageHandler handler)
        {
            try
            {
                await handler.StopTyping();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Ignores the user.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void IgnoreUser(GameClientMessageHandler handler)
        {
            try
            {
                await handler.IgnoreUser();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Unignores the user.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void UnignoreUser(GameClientMessageHandler handler)
        {
            try
            {
                await handler.UnignoreUser();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Determines whether this instance [can create room event] the specified try {  await handler.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void CanCreateRoomEvent(GameClientMessageHandler handler)
        {
            try
            {
                await handler.CanCreateRoomEvent();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Signs the specified try {  await handler.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void Sign(GameClientMessageHandler handler)
        {
            try
            {
                await handler.Sign();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the user tags.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetUserTags(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetUserTags();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the user badges.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetUserBadges(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetUserBadges();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Rates the room.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void RateRoom(GameClientMessageHandler handler)
        {
            try
            {
                await handler.RateRoom();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Dances the specified try {  await handler.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void Dance(GameClientMessageHandler handler)
        {
            try
            {
                await handler.Dance();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Answers the doorbell.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void AnswerDoorbell(GameClientMessageHandler handler)
        {
            try
            {
                await handler.AnswerDoorbell();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Applies the room effect.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ApplyRoomEffect(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ApplyRoomEffect();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Places the post it.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void PlacePostIt(GameClientMessageHandler handler)
        {
            try
            {
                await handler.PlacePostIt();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Places the item.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void PlaceItem(GameClientMessageHandler handler)
        {
            try
            {
                await handler.PlaceItem();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Places the item.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void AirPlaceItem(GameClientMessageHandler handler)
        {
            try
            {
                await handler.PlaceItem();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Takes the item.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void TakeItem(GameClientMessageHandler handler)
        {
            try
            {
                await handler.TakeItem();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Moves the item.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void MoveItem(GameClientMessageHandler handler)
        {
            try
            {
                await handler.MoveItem();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Moves the wall item.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void MoveWallItem(GameClientMessageHandler handler)
        {
            try
            {
                await handler.MoveWallItem();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Triggers the item.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void TriggerItem(GameClientMessageHandler handler)
        {
            try
            {
                await handler.TriggerItem();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Triggers the item dice special.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void TriggerItemDiceSpecial(GameClientMessageHandler handler)
        {
            try
            {
                await handler.TriggerItemDiceSpecial();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Opens the postit.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void OpenPostit(GameClientMessageHandler handler)
        {
            try
            {
                await handler.OpenPostit();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Saves the postit.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SavePostit(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SavePostit();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Deletes the postit.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void DeletePostit(GameClientMessageHandler handler)
        {
            try
            {
                await handler.DeletePostit();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Opens the present.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void OpenPresent(GameClientMessageHandler handler)
        {
            try
            {
                await handler.OpenGift();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the moodlight.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetMoodlight(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetMoodlight();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Updates the moodlight.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void UpdateMoodlight(GameClientMessageHandler handler)
        {
            try
            {
                await handler.UpdateMoodlight();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Switches the moodlight status.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SwitchMoodlightStatus(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SwitchMoodlightStatus();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Initializes the trade.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void InitTrade(GameClientMessageHandler handler)
        {
            try
            {
                await handler.InitTrade();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Offers the trade item.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void OfferTradeItem(GameClientMessageHandler handler)
        {
            try
            {
                await handler.OfferTradeItem();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Offers a specific amount of items.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void OfferTradeItems(GameClientMessageHandler handler)
        {
            try
            {
                await handler.OfferTradeItems();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Takes the back trade item.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void TakeBackTradeItem(GameClientMessageHandler handler)
        {
            try
            {
                await handler.TakeBackTradeItem();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Stops the trade.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void StopTrade(GameClientMessageHandler handler)
        {
            try
            {
                await handler.StopTrade();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Accepts the trade.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void AcceptTrade(GameClientMessageHandler handler)
        {
            try
            {
                await handler.AcceptTrade();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Unaccepts the trade.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void UnacceptTrade(GameClientMessageHandler handler)
        {
            try
            {
                await handler.UnacceptTrade();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Completes the trade.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void CompleteTrade(GameClientMessageHandler handler)
        {
            try
            {
                await handler.CompleteTrade();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gives the respect.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GiveRespect(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GiveRespect();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Applies the effect.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ApplyEffect(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ApplyEffect();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Enables the effect.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void EnableEffect(GameClientMessageHandler handler)
        {
            try
            {
                await handler.EnableEffect();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Recycles the items.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void RecycleItems(GameClientMessageHandler handler)
        {
            try
            {
                await handler.RecycleItems();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Redeems the exchange furni.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void RedeemExchangeFurni(GameClientMessageHandler handler)
        {
            try
            {
                await handler.RedeemExchangeFurni();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Kicks the bot.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void KickBot(GameClientMessageHandler handler)
        {
            try
            {
                await handler.KickBot();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Places the pet.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void PlacePet(GameClientMessageHandler handler)
        {
            try
            {
                await handler.PlacePet();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the pet information.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetPetInfo(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetPetInfo();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Picks up pet.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void PickUpPet(GameClientMessageHandler handler)
        {
            try
            {
                await handler.PickUpPet();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Composts the monsterplant.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void CompostMonsterplant(GameClientMessageHandler handler)
        {
            try
            {
                await handler.CompostMonsterplant();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Moves the pet.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void MovePet(GameClientMessageHandler handler)
        {
            try
            {
                await handler.MovePet();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Respects the pet.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void RespectPet(GameClientMessageHandler handler)
        {
            try
            {
                await handler.RespectPet();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Adds the saddle.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void AddSaddle(GameClientMessageHandler handler)
        {
            try
            {
                await handler.AddSaddle();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Removes the saddle.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void RemoveSaddle(GameClientMessageHandler handler)
        {
            try
            {
                await handler.RemoveSaddle();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Rides the specified try {  await handler.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void Ride(GameClientMessageHandler handler)
        {
            try
            {
                await handler.MountOnPet();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Unrides the specified try {  await handler.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void Unride(GameClientMessageHandler handler)
        {
            try
            {
                await handler.CancelMountOnPet();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Saves the wired.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SaveWired(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SaveWired();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Saves the wired condition.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SaveWiredCondition(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SaveWiredConditions();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the music data.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetMusicData(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetMusicData();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Adds the playlist item.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void AddPlaylistItem(GameClientMessageHandler handler)
        {
            try
            {
                await handler.AddPlaylistItem();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Removes the playlist item.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void RemovePlaylistItem(GameClientMessageHandler handler)
        {
            try
            {
                await handler.RemovePlaylistItem();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the disks.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetDisks(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetDisks();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the playlists.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetPlaylists(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetPlaylists();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the user information.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetUserInfo(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetUserInfo();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Loads the profile.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void LoadProfile(GameClientMessageHandler handler)
        {
            try
            {
                await handler.LoadProfile();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }


        /// <summary>
        /// Gets the balance.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetBalance(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetBalance();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the subscription data.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetSubscriptionData(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetSubscriptionData();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the badges.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetBadges(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetBadges();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Updates the badges.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void UpdateBadges(GameClientMessageHandler handler)
        {
            try
            {
                await handler.UpdateBadges();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the achievements.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetAchievements(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetAchievements();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Changes the look.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ChangeLook(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ChangeLook();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Changes the motto.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ChangeMotto(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ChangeMotto();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the wardrobe.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetWardrobe(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetWardrobe();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Allows all ride.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void AllowAllRide(GameClientMessageHandler handler)
        {
            try
            {
                await handler.AllowAllRide();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Saves the wardrobe.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SaveWardrobe(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SaveWardrobe();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the pets inventory.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetPetsInventory(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetPetsInventory();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Opens the quests.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void OpenQuests(GameClientMessageHandler handler)
        {
            try
            {
                await handler.OpenQuests();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Starts the quest.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void StartQuest(GameClientMessageHandler handler)
        {
            try
            {
                await handler.StartQuest();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Stops the quest.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void StopQuest(GameClientMessageHandler handler)
        {
            try
            {
                await handler.StopQuest();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the current quest.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetCurrentQuest(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetCurrentQuest();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the group badges.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void GetGroupBadges(GameClientMessageHandler handler)
        {
            try
            {
                handler.InitRoomGroupBadges();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the bot inv.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetBotInv(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetBotsInventory();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Saves the room bg.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SaveRoomBg(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SaveRoomBg();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Goes the room.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GoRoom(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GoRoom();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Sits the specified try {  await handler.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void Sit(GameClientMessageHandler handler)
        {
            try
            {
                handler.Sit();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the event rooms.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetEventRooms(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetEventRooms();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Starts the seasonal quest.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void StartSeasonalQuest(GameClientMessageHandler handler)
        {
            try
            {
                await handler.StartSeasonalQuest();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Saves the mannequin.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SaveMannequin(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SaveMannequin();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Saves the mannequin2.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SaveMannequin2(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SaveMannequin2();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Serializes the group purchase page.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SerializeGroupPurchasePage(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SerializeGroupPurchasePage();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Serializes the group purchase parts.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SerializeGroupPurchaseParts(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SerializeGroupPurchaseParts();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Purchases the group.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void PurchaseGroup(GameClientMessageHandler handler)
        {
            try
            {
                await handler.PurchaseGroup();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Serializes the group information.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SerializeGroupInfo(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SerializeGroupInfo();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Serializes the group members.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SerializeGroupMembers(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SerializeGroupMembers();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Makes the group admin.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void MakeGroupAdmin(GameClientMessageHandler handler)
        {
            try
            {
                await handler.MakeGroupAdmin();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Removes the group admin.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void RemoveGroupAdmin(GameClientMessageHandler handler)
        {
            try
            {
                await handler.RemoveGroupAdmin();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Accepts the membership.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void AcceptMembership(GameClientMessageHandler handler)
        {
            try
            {
                await handler.AcceptMembership();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Declines the membership.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void DeclineMembership(GameClientMessageHandler handler)
        {
            try
            {
                await handler.DeclineMembership();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Removes the member.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void RemoveMember(GameClientMessageHandler handler)
        {
            try
            {
                await handler.RemoveMember();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Joins the group.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void JoinGroup(GameClientMessageHandler handler)
        {
            try
            {
                await handler.JoinGroup();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Makes the fav.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void MakeFav(GameClientMessageHandler handler)
        {
            try
            {
                await handler.MakeFav();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Removes the fav.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void RemoveFav(GameClientMessageHandler handler)
        {
            try
            {
                await handler.RemoveFav();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Receives the nux gifts.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ReceiveNuxGifts(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ReceiveNuxGifts();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Accepts the nux gifts.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void AcceptNuxGifts(GameClientMessageHandler handler)
        {
            try
            {
                await handler.AcceptNuxGifts();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Reads the forum thread.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ReadForumThread(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ReadForumThread();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Publishes the forum thread.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void PublishForumThread(GameClientMessageHandler handler)
        {
            try
            {
                await handler.PublishForumThread();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Updates the forum thread.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void UpdateForumThread(GameClientMessageHandler handler)
        {
            try
            {
                await handler.UpdateThreadState();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Alters the state of the forum thread.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void AlterForumThreadState(GameClientMessageHandler handler)
        {
            try
            {
                await handler.AlterForumThreadState();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the forum thread root.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetForumThreadRoot(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetGroupForumThreadRoot();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the group forum data.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetGroupForumData(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetGroupForumData();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the group forums.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetGroupForums(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetGroupForums();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Manages the group.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ManageGroup(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ManageGroup();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Updates the name of the group.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void UpdateGroupName(GameClientMessageHandler handler)
        {
            try
            {
                await handler.UpdateGroupName();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Updates the group badge.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void UpdateGroupBadge(GameClientMessageHandler handler)
        {
            try
            {
                await handler.UpdateGroupBadge();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Updates the group colours.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void UpdateGroupColours(GameClientMessageHandler handler)
        {
            try
            {
                await handler.UpdateGroupColours();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Updates the group settings.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void UpdateGroupSettings(GameClientMessageHandler handler)
        {
            try
            {
                await handler.UpdateGroupSettings();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Serializes the group furni page.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SerializeGroupFurniPage(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SerializeGroupFurniPage();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Ejects the furni.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void EjectFurni(GameClientMessageHandler handler)
        {
            try
            {
                await handler.EjectFurni();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mutes the user.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void MuteUser(GameClientMessageHandler handler)
        {
            try
            {
                await handler.MuteUser();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Checks the name.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void CheckName(GameClientMessageHandler handler)
        {
            try
            {
                await handler.CheckName();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Changes the name.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ChangeName(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ChangeName();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the trainer panel.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetTrainerPanel(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetTrainerPanel();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Updates the event information.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void UpdateEventInfo(GameClientMessageHandler handler)
        {
            try
            {
                await handler.UpdateEventInfo();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the room banned users.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetRoomBannedUsers(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetBannedUsers();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Userses the with rights.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void UsersWithRights(GameClientMessageHandler handler)
        {
            try
            {
                await handler.UsersWithRights();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Unbans the user.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void UnbanUser(GameClientMessageHandler handler)
        {
            try
            {
                await handler.UnbanUser();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Manages the bot actions.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ManageBotActions(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ManageBotActions();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Handles the bot speech list.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void HandleBotSpeechList(GameClientMessageHandler handler)
        {
            try
            {
                await handler.HandleBotSpeechList();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the relationships.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetRelationships(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetRelationships();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Sets the relationship.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SetRelationship(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SetRelationship();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Automatics the room.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void AutoRoom(GameClientMessageHandler handler)
        {
            try
            {
                await handler.RoomOnLoad();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mutes all.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void MuteAll(GameClientMessageHandler handler)
        {
            try
            {
                await handler.MuteAll();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Completes the saftey quiz.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void CompleteSafteyQuiz(GameClientMessageHandler handler)
        {
            try
            {
                await handler.CompleteSafetyQuiz();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Removes the favourite room.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void RemoveFavouriteRoom(GameClientMessageHandler handler)
        {
            try
            {
                await handler.RemoveFavouriteRoom();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Rooms the user action.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void RoomUserAction(GameClientMessageHandler handler)
        {
            try
            {
                await handler.RoomUserAction();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Saves the football outfit.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SaveFootballOutfit(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SaveFootballOutfit();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Confirms the love lock.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ConfirmLoveLock(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ConfirmLoveLock();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Builderses the club update furni count.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void BuildersClubUpdateFurniCount(GameClientMessageHandler handler)
        {
            try
            {
                await handler.BuildersClubUpdateFurniCount();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the client version message event.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void ReleaseVersion(GameClientMessageHandler handler)
        {
            try
            {
                handler.ReleaseVersion();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Places the builders furniture.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void PlaceBuildersFurniture(GameClientMessageHandler handler)
        {
            try
            {
                await handler.PlaceBuildersFurniture();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Whispers the specified try {  await handler.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void Whisper(GameClientMessageHandler handler)
        {
            try
            {
                await handler.Whisper();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Catalogues the index.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void CatalogueIndex(GameClientMessageHandler handler)
        {
            try
            {
                await handler.CatalogueIndex();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Catalogues the index.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetCatalogMode(GameClientMessageHandler handler)
        {
            try
            {
                await handler.CatalogueMode();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Catalogues the page.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void CataloguePage(GameClientMessageHandler handler)
        {
            try
            {
                await handler.CataloguePage();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Catalogues the club page.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void CatalogueClubPage(GameClientMessageHandler handler)
        {
            try
            {
                await handler.CatalogueClubPage();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Catalogues the offers configuration.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void CatalogueOffersConfig(GameClientMessageHandler handler)
        {
            try
            {
                await handler.CatalogueOfferConfig();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        internal static async void PurchaseOffer(GameClientMessageHandler handler)
        {
            try
            {
                await handler.PurchaseOffer();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        internal static async void CancelOffer(GameClientMessageHandler handler)
        {
            try
            {
                await handler.CancelOffer();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        internal static async void GetItemStats(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetItemStats();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        internal static async void GetMyOffers(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetMyOffers();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        internal static async void MakeOffer(GameClientMessageHandler handler)
        {
            try
            {
                await handler.MakeOffer();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        internal static async void ReedemCredits(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ReedemCredits();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Catalogues the single offer.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void CatalogueSingleOffer(GameClientMessageHandler handler)
        {
            try
            {
                await handler.CatalogueOffer();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// get marketplace offers
        /// </summary>
        /// <param name="handler"></param>
        internal static async void GetOffers(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetOffers();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Checks the name of the pet.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void CheckPetName(GameClientMessageHandler handler)
        {
            try
            {
                await handler.CheckPetName();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Purchases the item.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void PurchaseItem(GameClientMessageHandler handler)
        {
            try
            {
                await handler.PurchaseItem();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Purchases the gift.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void PurchaseGift(GameClientMessageHandler handler)
        {
            try
            {
                await handler.PurchaseGift();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the pet breeds.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetPetBreeds(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetPetBreeds();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Reloads the ecotron.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ReloadEcotron(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ReloadEcotron();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gifts the wrapping configuration.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GiftWrappingConfig(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GiftWrappingConfig();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Recyclers the rewards.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void RecyclerRewards(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetRecyclerRewards();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Requests the leave group.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void RequestLeaveGroup(GameClientMessageHandler handler)
        {
            try
            {
                await handler.RequestLeaveGroup();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Confirms the leave group.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ConfirmLeaveGroup(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ConfirmLeaveGroup();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// News the navigator.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void NewNavigator(GameClientMessageHandler handler)
        {
            try
            {
                await handler.NewNavigator();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Searches the new navigator.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SearchNewNavigator(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SearchNewNavigator();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// News the navigator delete saved search.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void NewNavigatorDeleteSavedSearch(GameClientMessageHandler handler)
        {
            try
            {
                await handler.NewNavigatorDeleteSavedSearch();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// News the navigator resize.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void NewNavigatorResize(GameClientMessageHandler handler)
        {
            try
            {
                handler.NewNavigatorResize();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        internal static async void HabboAirGetUserRooms(GameClientMessageHandler handler)
        {
            try
            {
                await handler.HabboAirGetUserRooms();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        internal static async void HabboAirGetAllRooms(GameClientMessageHandler handler)
        {
            try
            {
                await handler.HabboAirGetAllRooms();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// News the navigator add saved search.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void NewNavigatorAddSavedSearch(GameClientMessageHandler handler)
        {
            try
            {
                await handler.NewNavigatorAddSavedSearch();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// News the navigator collapse category.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void NewNavigatorCollapseCategory(GameClientMessageHandler handler)
        {
            try
            {
                handler.NewNavigatorCollapseCategory();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// News the navigator uncollapse category.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void NewNavigatorUncollapseCategory(GameClientMessageHandler handler)
        {
            try
            {
                handler.NewNavigatorUncollapseCategory();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Pets the breed result.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void PetBreedResult(GameClientMessageHandler handler)
        {
            try
            {
                await handler.PetBreedResult();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Pets the breed cancel.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void PetBreedCancel(GameClientMessageHandler handler)
        {
            try
            {
                await handler.PetBreedCancel();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Games the center load game.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GameCenterLoadGame(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GameCenterLoadGame();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Get game lists
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetGameListing(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetGameListing();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Init the game center
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void InitializeGameCenter(GameClientMessageHandler handler)
        {
            try
            {
                handler.InitializeGameCenter();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Games the center join queue.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GameCenterJoinQueue(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GameCenterJoinQueue();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Hotels the view countdown.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void HotelViewCountdown(GameClientMessageHandler handler)
        {
            try
            {
                await handler.HotelViewCountdown();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Hotels the view dailyquest.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void HotelViewDailyquest(GameClientMessageHandler handler)
        {
            try
            {
                handler.HotelViewDailyquest();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Places the builders wall item.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void PlaceBuildersWallItem(GameClientMessageHandler handler)
        {
            try
            {
                await handler.PlaceBuildersWallItem();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Targeteds the offer buy.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void PurchaseTargetedOffer(GameClientMessageHandler handler)
        {
            try
            {
                await handler.PurchaseTargetedOffer();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Ambassadors the alert.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void AmbassadorAlert(GameClientMessageHandler handler)
        {
            try
            {
                handler.AmbassadorAlert();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Goes the name of to room by.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GoToRoomByName(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GoToRoomByName();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the uc panel.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void GetUcPanel(GameClientMessageHandler handler)
        {
            try
            {
                handler.GetUcPanel();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the uc panel hotel.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void GetUcPanelHotel(GameClientMessageHandler handler)
        {
            try
            {
                handler.GetUcPanelHotel();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Saves the room thumbnail.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SaveRoomThumbnail(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SaveRoomThumbnail();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Uses the purchasable clothing.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void UsePurchasableClothing(GameClientMessageHandler handler)
        {
            try
            {
                await handler.UsePurchasableClothing();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the user look.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void GetUserLook(GameClientMessageHandler handler)
        {
            try
            {
                handler.GetUserLook();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Sets the invitations preference.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void SetInvitationsPreference(GameClientMessageHandler handler)
        {
            try
            {
                handler.SetInvitationsPreference();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Finds the more friends.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void FindMoreFriends(GameClientMessageHandler handler)
        {
            try
            {
                await handler.FindMoreFriends();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Hotels the view request badge.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void HotelViewRequestBadge(GameClientMessageHandler handler)
        {
            try
            {
                await handler.HotelViewRequestBadge();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the camera price.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetCameraPrice(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetCameraPrice();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Toggles the staff pick.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void ToggleStaffPick(GameClientMessageHandler handler)
        {
            try
            {
                await handler.ToggleStaffPick();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the hotel view hall of fame.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetHotelViewHallOfFame(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetHotelViewHallOfFame();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Submits the room to competition.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void SubmitRoomToCompetition(GameClientMessageHandler handler)
        {
            try
            {
                await handler.SubmitRoomToCompetition();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Enters the room queue.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void EnterRoomQueue(GameClientMessageHandler handler)
        {
            try
            {
                await handler.EnterRoomQueue();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the camera request.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void GetCameraRequest(GameClientMessageHandler handler)
        {
            try
            {
                await handler.GetCameraRequest();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Votes for room.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void VoteForRoom(GameClientMessageHandler handler)
        {
            try
            {
                await handler.VoteForRoom();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Updates the forum settings.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void UpdateForumSettings(GameClientMessageHandler handler)
        {
            try
            {
                await handler.UpdateForumSettings();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Friends the request list load.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static  void FriendRequestListLoad(GameClientMessageHandler handler)
        {
            try
            {
                 handler.FriendRequestListLoad();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Sets the room camera preferences.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static void SetRoomCameraPreferences(GameClientMessageHandler handler)
        {
            try
            {
                handler.SetRoomCameraPreferences();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Deletes the group.
        /// </summary>
        /// <param name="handler">The try {  await handler.</param>
        internal static async void DeleteGroup(GameClientMessageHandler handler)
        {
            try
            {
                await handler.DeleteGroup();
            }
            catch (InvalidOperationException ex)
            {
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }
    }
}