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
        /// <param name="handler">The try {  handler.</param>
        internal delegate void GetProperty(GameClientMessageHandler handler);

        /// <summary>
        /// Initializes the crypto.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void InitCrypto(GameClientMessageHandler handler)
        {
            try
            {
                 handler.InitCrypto().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Secrets the key.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SecretKey(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SecretKey().GetAwaiter().GetResult();
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
                 handler.InitConsole().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Machines the identifier.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void MachineId(GameClientMessageHandler handler)
        {
            try
            {
                 handler.MachineId().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Guides the message.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GuideMessage(GameClientMessageHandler handler)
        {
            try
            {
                 handler.CallGuide().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Sets the chat preferrence.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static void SetChatPreferrence(GameClientMessageHandler handler)
        {
            try
            {
                handler.SetChatPreferrence().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the helper tool.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetHelperTool(GameClientMessageHandler handler)
        {
            try
            {
                 handler.OpenGuideTool().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the guide detached.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetGuideDetached(GameClientMessageHandler handler)
        {
            try
            {
                 handler.AnswerGuideRequest().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Logins the with ticket.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void LoginWithTicket(GameClientMessageHandler handler)
        {
            try
            {
                 handler.LoginWithTicket().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Invites the guide.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void InviteGuide(GameClientMessageHandler handler)
        {
            try
            {
                 handler.InviteToRoom().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Visits the room guide.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void VisitRoomGuide(GameClientMessageHandler handler)
        {
            try
            {
                 handler.VisitRoom().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Guides the end session.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GuideEndSession(GameClientMessageHandler handler)
        {
            try
            {
                 handler.CloseGuideRequest().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Cancels the call guide.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void CancelCallGuide(GameClientMessageHandler handler)
        {
            try
            {
                 handler.CancelCallGuide().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Informations the retrieve.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void InfoRetrieve(GameClientMessageHandler handler)
        {
            try
            {
                 handler.InfoRetrieve().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Chats the specified try {  handler.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void Chat(GameClientMessageHandler handler)
        {
            try
            {
                 handler.Chat().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Shouts the specified try {  handler.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void Shout(GameClientMessageHandler handler)
        {
            try
            {
                 handler.Shout().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Requests the floor plan used coords.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void RequestFloorPlanUsedCoords(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetFloorPlanUsedCoords().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Requests the floor plan door.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void RequestFloorPlanDoor(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetFloorPlanDoor().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Opens the bully reporting.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void OpenBullyReporting(GameClientMessageHandler handler)
        {
            try
            {
                 handler.OpenBullyReporting().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Sends the bully report.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SendBullyReport(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SendBullyReport().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Loads the club gifts.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void LoadClubGifts(GameClientMessageHandler handler)
        {
            try
            {
                 handler.LoadClubGifts().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Saves the heightmap.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SaveHeightmap(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SaveHeightmap().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Accepts the poll.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void AcceptPoll(GameClientMessageHandler handler)
        {
            try
            {
                 handler.AcceptPoll().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Refuses the poll.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void RefusePoll(GameClientMessageHandler handler)
        {
            try
            {
                 handler.RefusePoll().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Answers the poll question.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void AnswerPollQuestion(GameClientMessageHandler handler)
        {
            try
            {
                 handler.AnswerPoll().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Retrieves the song identifier.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void RetrieveSongId(GameClientMessageHandler handler)
        {
            try
            {
                 handler.RetrieveSongId().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Tiles the height of the stack magic set.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void TileStackMagicSetHeight(GameClientMessageHandler handler)
        {
            try
            {
                 handler.TileStackMagicSetHeight().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Enables the inventory effect.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void EnableInventoryEffect(GameClientMessageHandler handler)
        {
            try
            {
                 handler.EnableEffect().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Promotes the room.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void PromoteRoom(GameClientMessageHandler handler)
        {
            try
            {
                 handler.PromoteRoom().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the promotionable rooms.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetPromotionableRooms(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetPromotionableRooms().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the room filter.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetRoomFilter(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetRoomFilter().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Alters the room filter.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void AlterRoomFilter(GameClientMessageHandler handler)
        {
            try
            {
                 handler.AlterRoomFilter().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the tv player.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetTvPlayer(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetTvPlayer().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Chooses the tv player video.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ChooseTvPlayerVideo(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ChooseTvPlayerVideo().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the tv playlist.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetTvPlaylist(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ChooseTvPlaylist().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Places the bot.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void PlaceBot(GameClientMessageHandler handler)
        {
            try
            {
                 handler.PlaceBot().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Picks up bot.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void PickUpBot(GameClientMessageHandler handler)
        {
            try
            {
                 handler.PickUpBot().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the talents track.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetTalentsTrack(GameClientMessageHandler handler)
        {
            try
            {
                 handler.Talents().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Prepares the campaing.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void PrepareCampaing(GameClientMessageHandler handler)
        {
            try
            {
                 handler.PrepareCampaing().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Pongs the specified try {  handler.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void Pong(GameClientMessageHandler handler)
        {
            try
            {
                 handler.Pong().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Disconnects the event.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void DisconnectEvent(GameClientMessageHandler handler)
        {
            try
            {
                 handler.DisconnectEvent().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Latencies the test.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void LatencyTest(GameClientMessageHandler handler)
        {
            try
            {
                 handler.LatencyTest().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Receptions the view.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ReceptionView(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GoToHotelView().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Called when [confirmation event].
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void OnlineConfirmationEvent(GameClientMessageHandler handler)
        {
            try
            {
                 handler.OnlineConfirmationEvent().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Retrives the citizen ship status.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void RetriveCitizenShipStatus(GameClientMessageHandler handler)
        {
            try
            {
                 handler.RetrieveCitizenship().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Refreshes the promo event.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void RefreshPromoEvent(GameClientMessageHandler handler)
        {
            try
            {
                 handler.RefreshPromoEvent().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        internal static async void RefreshCompetition(GameClientMessageHandler handler)
        {
            try
            {
                 handler.RefreshCompetition().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Widgets the container.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void WidgetContainer(GameClientMessageHandler handler)
        {
            try
            {
                 handler.WidgetContainers().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Landings the community goal.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void LandingCommunityGoal(GameClientMessageHandler handler)
        {
            try
            {
                 handler.LandingCommunityGoal().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Removes the handitem.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void RemoveHanditem(GameClientMessageHandler handler)
        {
            try
            {
                 handler.RemoveHanditem().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Redeems the voucher.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void RedeemVoucher(GameClientMessageHandler handler)
        {
            try
            {
                 handler.RedeemVoucher().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gives the handitem.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GiveHanditem(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GiveHanditem().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Initializes the help tool.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void InitHelpTool(GameClientMessageHandler handler)
        {
            try
            {
                 handler.InitHelpTool().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Submits the help ticket.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SubmitHelpTicket(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SubmitHelpTicket().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Deletes the pending CFH.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void DeletePendingCfh(GameClientMessageHandler handler)
        {
            try
            {
                 handler.DeletePendingCfh().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the get user information.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ModGetUserInfo(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ModGetUserInfo().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the get user chatlog.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ModGetUserChatlog(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ModGetUserChatlog().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Messages from a guy.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void MessageFromAGuy(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GuideSpeak().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the get room chatlog.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ModGetRoomChatlog(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ModGetRoomChatlog().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the get room tool.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ModGetRoomTool(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ModGetRoomTool().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the pick ticket.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ModPickTicket(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ModPickTicket().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the release ticket.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ModReleaseTicket(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ModReleaseTicket().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the close ticket.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ModCloseTicket(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ModCloseTicket().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the get ticket chatlog.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ModGetTicketChatlog(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ModGetTicketChatlog().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the get room visits.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ModGetRoomVisits(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ModGetRoomVisits().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the send room alert.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ModSendRoomAlert(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ModSendRoomAlert().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the perform room action.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ModPerformRoomAction(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ModPerformRoomAction().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the send user caution.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ModSendUserCaution(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ModSendUserCaution().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the send user message.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ModSendUserMessage(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ModSendUserMessage().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the kick user.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ModKickUser(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ModKickUser().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the mute user.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ModMuteUser(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ModMuteUser().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the lock trade.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ModLockTrade(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ModLockTrade().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mods the ban user.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ModBanUser(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ModBanUser().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Initializes the messenger.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void InitMessenger(GameClientMessageHandler handler)
        {
            try
            {
                 handler.InitMessenger().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Friendses the list update.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void FriendsListUpdate(GameClientMessageHandler handler)
        {
            try
            {
                 handler.FriendsListUpdate().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Removes the buddy.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void RemoveBuddy(GameClientMessageHandler handler)
        {
            try
            {
                 handler.RemoveBuddy().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Searches the habbo.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SearchHabbo(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SearchHabbo().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Accepts the request.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void AcceptRequest(GameClientMessageHandler handler)
        {
            try
            {
                 handler.AcceptRequest().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Declines the request.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void DeclineRequest(GameClientMessageHandler handler)
        {
            try
            {
                 handler.DeclineRequest().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Requests the buddy.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void RequestBuddy(GameClientMessageHandler handler)
        {
            try
            {
                 handler.RequestBuddy().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Sends the instant messenger.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SendInstantMessenger(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SendInstantMessenger().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Follows the buddy.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void FollowBuddy(GameClientMessageHandler handler)
        {
            try
            {
                 handler.FollowBuddy().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Sends the instant invite.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SendInstantInvite(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SendInstantInvite().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Homes the room stuff.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void HomeRoomStuff(GameClientMessageHandler handler)
        {
            try
            {
                 handler.HomeRoom().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Adds the favorite.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void AddFavorite(GameClientMessageHandler handler)
        {
            try
            {
                 handler.AddFavorite().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Removes the favorite.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void RemoveFavorite(GameClientMessageHandler handler)
        {
            try
            {
                 handler.RemoveFavorite().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the flat cats.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetFlatCats(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetFlatCats().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Enters the inquired room.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void EnterInquiredRoom(GameClientMessageHandler handler)
        {
            try
            {
                 handler.EnterInquiredRoom().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the pubs.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetPubs(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetPubs().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Saves the branding.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SaveBranding(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SaveBranding().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the room information.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetRoomInfo(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetRoomInfo().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }


        /// <summary>
        /// News the navigator flat cats.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void NewNavigatorFlatCats(GameClientMessageHandler handler)
        {
            try
            {
                 handler.NewNavigatorFlatCats().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the favorite rooms.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetFavoriteRooms(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetFavoriteRooms().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the recent rooms.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetRecentRooms(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetRecentRooms().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the popular tags.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetPopularTags(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetPopularTags().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Performs the search.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void PerformSearch(GameClientMessageHandler handler)
        {
            try
            {
                 handler.PerformSearch().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Searches the by tag.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SearchByTag(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SearchByTag().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Performs the search2.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void PerformSearch2(GameClientMessageHandler handler)
        {
            try
            {
                 handler.PerformSearch2().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Opens the flat.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void OpenFlat(GameClientMessageHandler handler)
        {
            try
            {
                 handler.OpenFlat().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the voume.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetVoume(GameClientMessageHandler handler)
        {
            try
            {
                 handler.LoadSettings().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Saves the volume.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static void SaveVolume(GameClientMessageHandler handler)
        {
            try
            {
                handler.SaveSettings();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the pub.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetPub(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetPub().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Opens the pub.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void OpenPub(GameClientMessageHandler handler)
        {
            try
            {
                 handler.OpenPub().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the inventory.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetInventory(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetInventory().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the inventory.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void OpenInventory(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetInventory().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Get maketplace config.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void MarketPlaceConfiguration(GameClientMessageHandler handler)
        {
            try
            {
                 handler.MarketPlaceConfiguration().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Check if user can make offer
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void CanMakeOffer(GameClientMessageHandler handler)
        {
            try
            {
                 handler.CanMakeOffer().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the room data1.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetRoomData1(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetRoomData1().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the room data2.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetRoomData2(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetRoomData2().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the room data3.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetRoomData3(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetRoomData3().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Requests the floor items.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void RequestFloorItems(GameClientMessageHandler handler)
        {
            try
            {
                 handler.RequestFloorItems().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Requests the wall items.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void RequestWallItems(GameClientMessageHandler handler)
        {
            try
            {
                 handler.RequestWallItems().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Called when [room user add].
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void OnRoomUserAdd(GameClientMessageHandler handler)
        {
            try
            {
                 handler.OnRoomUserAdd().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Reqs the load room for user.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ReqLoadRoomForUser(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ReqLoadRoomForUser().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Enters the on room.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void EnterOnRoom(GameClientMessageHandler handler)
        {
            try
            {
                 handler.EnterOnRoom().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Clears the room loading.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static void ClearRoomLoading(GameClientMessageHandler handler)
        {
            try
            {
                handler.ClearRoomLoading();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Moves the specified try {  handler.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void Move(GameClientMessageHandler handler)
        {
            try
            {
                 handler.Move().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Determines whether this instance [can create room] the specified try {  handler.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void CanCreateRoom(GameClientMessageHandler handler)
        {
            try
            {
                 handler.CanCreateRoom().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Creates the room.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void CreateRoom(GameClientMessageHandler handler)
        {
            try
            {
                 handler.CreateRoom().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the room information.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetRoomInformation(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ParseRoomDataInformation().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the room edit data.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetRoomEditData(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetRoomEditData().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Saves the room data.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SaveRoomData(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SaveRoomData().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gives the rights.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GiveRights(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GiveRights().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Takes the rights.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void TakeRights(GameClientMessageHandler handler)
        {
            try
            {
                 handler.TakeRights().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Takes all rights.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void TakeAllRights(GameClientMessageHandler handler)
        {
            try
            {
                 handler.TakeAllRights().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Habboes the camera.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void HabboCamera(GameClientMessageHandler handler)
        {
            try
            {
                 handler.HabboCamera().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Open xmas calendar
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void OpenXmasCalendar(GameClientMessageHandler handler)
        {
            try
            {
                 handler.OpenXmasCalendar().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Called when [click].
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void OnClick(GameClientMessageHandler handler)
        {
            try
            {
                 handler.OnClick().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Kicks the user.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void KickUser(GameClientMessageHandler handler)
        {
            try
            {
                 handler.KickUser().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Bans the user.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void BanUser(GameClientMessageHandler handler)
        {
            try
            {
                 handler.BanUser().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Sets the home room.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SetHomeRoom(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SetHomeRoom().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Deletes the room.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void DeleteRoom(GameClientMessageHandler handler)
        {
            try
            {
                 handler.DeleteRoom().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Looks at.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void LookAt(GameClientMessageHandler handler)
        {
            try
            {
                 handler.LookAt().GetAwaiter().GetResult();
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
                 handler.AirClickUser().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Starts the typing.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void StartTyping(GameClientMessageHandler handler)
        {
            try
            {
                 handler.StartTyping().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Stops the typing.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void StopTyping(GameClientMessageHandler handler)
        {
            try
            {
                 handler.StopTyping().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Ignores the user.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void IgnoreUser(GameClientMessageHandler handler)
        {
            try
            {
                 handler.IgnoreUser().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Unignores the user.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void UnignoreUser(GameClientMessageHandler handler)
        {
            try
            {
                 handler.UnignoreUser().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Determines whether this instance [can create room event] the specified try {  handler.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void CanCreateRoomEvent(GameClientMessageHandler handler)
        {
            try
            {
                 handler.CanCreateRoomEvent().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Signs the specified try {  handler.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void Sign(GameClientMessageHandler handler)
        {
            try
            {
                 handler.Sign().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the user tags.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetUserTags(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetUserTags().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the user badges.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetUserBadges(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetUserBadges().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Rates the room.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void RateRoom(GameClientMessageHandler handler)
        {
            try
            {
                 handler.RateRoom().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Dances the specified try {  handler.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void Dance(GameClientMessageHandler handler)
        {
            try
            {
                 handler.Dance().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Answers the doorbell.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void AnswerDoorbell(GameClientMessageHandler handler)
        {
            try
            {
                 handler.AnswerDoorbell().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Applies the room effect.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ApplyRoomEffect(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ApplyRoomEffect().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Places the post it.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void PlacePostIt(GameClientMessageHandler handler)
        {
            try
            {
                 handler.PlacePostIt().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Places the item.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void PlaceItem(GameClientMessageHandler handler)
        {
            try
            {
                 handler.PlaceItem().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Places the item.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void AirPlaceItem(GameClientMessageHandler handler)
        {
            try
            {
                 handler.PlaceItem().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Takes the item.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void TakeItem(GameClientMessageHandler handler)
        {
            try
            {
                 handler.TakeItem().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Moves the item.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void MoveItem(GameClientMessageHandler handler)
        {
            try
            {
                 handler.MoveItem().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Moves the wall item.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void MoveWallItem(GameClientMessageHandler handler)
        {
            try
            {
                 handler.MoveWallItem().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Triggers the item.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void TriggerItem(GameClientMessageHandler handler)
        {
            try
            {
                 handler.TriggerItem().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Triggers the item dice special.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void TriggerItemDiceSpecial(GameClientMessageHandler handler)
        {
            try
            {
                 handler.TriggerItemDiceSpecial().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Opens the postit.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void OpenPostit(GameClientMessageHandler handler)
        {
            try
            {
                 handler.OpenPostit().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Saves the postit.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SavePostit(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SavePostit().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Deletes the postit.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void DeletePostit(GameClientMessageHandler handler)
        {
            try
            {
                 handler.DeletePostit().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Opens the present.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void OpenPresent(GameClientMessageHandler handler)
        {
            try
            {
                 handler.OpenGift().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the moodlight.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetMoodlight(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetMoodlight().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Updates the moodlight.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void UpdateMoodlight(GameClientMessageHandler handler)
        {
            try
            {
                 handler.UpdateMoodlight().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Switches the moodlight status.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SwitchMoodlightStatus(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SwitchMoodlightStatus().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Initializes the trade.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void InitTrade(GameClientMessageHandler handler)
        {
            try
            {
                 handler.InitTrade().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Offers the trade item.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void OfferTradeItem(GameClientMessageHandler handler)
        {
            try
            {
                 handler.OfferTradeItem().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Offers a specific amount of items.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void OfferTradeItems(GameClientMessageHandler handler)
        {
            try
            {
                 handler.OfferTradeItems().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Takes the back trade item.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void TakeBackTradeItem(GameClientMessageHandler handler)
        {
            try
            {
                 handler.TakeBackTradeItem().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Stops the trade.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void StopTrade(GameClientMessageHandler handler)
        {
            try
            {
                 handler.StopTrade().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Accepts the trade.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void AcceptTrade(GameClientMessageHandler handler)
        {
            try
            {
                 handler.AcceptTrade().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Unaccepts the trade.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void UnacceptTrade(GameClientMessageHandler handler)
        {
            try
            {
                 handler.UnacceptTrade().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Completes the trade.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void CompleteTrade(GameClientMessageHandler handler)
        {
            try
            {
                 handler.CompleteTrade().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gives the respect.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GiveRespect(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GiveRespect().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Applies the effect.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ApplyEffect(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ApplyEffect().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Enables the effect.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void EnableEffect(GameClientMessageHandler handler)
        {
            try
            {
                 handler.EnableEffect().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Recycles the items.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void RecycleItems(GameClientMessageHandler handler)
        {
            try
            {
                 handler.RecycleItems().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Redeems the exchange furni.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void RedeemExchangeFurni(GameClientMessageHandler handler)
        {
            try
            {
                 handler.RedeemExchangeFurni().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Kicks the bot.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void KickBot(GameClientMessageHandler handler)
        {
            try
            {
                 handler.KickBot().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Places the pet.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void PlacePet(GameClientMessageHandler handler)
        {
            try
            {
                 handler.PlacePet().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the pet information.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetPetInfo(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetPetInfo().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Picks up pet.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void PickUpPet(GameClientMessageHandler handler)
        {
            try
            {
                 handler.PickUpPet().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Composts the monsterplant.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void CompostMonsterplant(GameClientMessageHandler handler)
        {
            try
            {
                 handler.CompostMonsterplant().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Moves the pet.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void MovePet(GameClientMessageHandler handler)
        {
            try
            {
                 handler.MovePet().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Respects the pet.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void RespectPet(GameClientMessageHandler handler)
        {
            try
            {
                 handler.RespectPet().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Adds the saddle.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void AddSaddle(GameClientMessageHandler handler)
        {
            try
            {
                 handler.AddSaddle().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Removes the saddle.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void RemoveSaddle(GameClientMessageHandler handler)
        {
            try
            {
                 handler.RemoveSaddle().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Rides the specified try {  handler.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void Ride(GameClientMessageHandler handler)
        {
            try
            {
                 handler.MountOnPet().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Unrides the specified try {  handler.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void Unride(GameClientMessageHandler handler)
        {
            try
            {
                 handler.CancelMountOnPet().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Saves the wired.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SaveWired(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SaveWired().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Saves the wired condition.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SaveWiredCondition(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SaveWiredConditions().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the music data.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetMusicData(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetMusicData().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Adds the playlist item.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void AddPlaylistItem(GameClientMessageHandler handler)
        {
            try
            {
                 handler.AddPlaylistItem().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Removes the playlist item.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void RemovePlaylistItem(GameClientMessageHandler handler)
        {
            try
            {
                 handler.RemovePlaylistItem().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the disks.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetDisks(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetDisks().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the playlists.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetPlaylists(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetPlaylists().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the user information.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetUserInfo(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetUserInfo().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Loads the profile.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void LoadProfile(GameClientMessageHandler handler)
        {
            try
            {
                 handler.LoadProfile().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }


        /// <summary>
        /// Gets the balance.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetBalance(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetBalance().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the subscription data.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetSubscriptionData(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetSubscriptionData().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the badges.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetBadges(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetBadges().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Updates the badges.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void UpdateBadges(GameClientMessageHandler handler)
        {
            try
            {
                 handler.UpdateBadges().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the achievements.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetAchievements(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetAchievements().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Changes the look.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ChangeLook(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ChangeLook().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Changes the motto.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ChangeMotto(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ChangeMotto().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the wardrobe.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetWardrobe(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetWardrobe().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Allows all ride.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void AllowAllRide(GameClientMessageHandler handler)
        {
            try
            {
                 handler.AllowAllRide().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Saves the wardrobe.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SaveWardrobe(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SaveWardrobe().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the pets inventory.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetPetsInventory(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetPetsInventory().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Opens the quests.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void OpenQuests(GameClientMessageHandler handler)
        {
            try
            {
                 handler.OpenQuests().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Starts the quest.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void StartQuest(GameClientMessageHandler handler)
        {
            try
            {
                 handler.StartQuest().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Stops the quest.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void StopQuest(GameClientMessageHandler handler)
        {
            try
            {
                 handler.StopQuest().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the current quest.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetCurrentQuest(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetCurrentQuest().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the group badges.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetGroupBadges(GameClientMessageHandler handler)
        {
            try
            {
                 handler.InitRoomGroupBadges().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the bot inv.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetBotInv(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetBotsInventory().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Saves the room bg.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SaveRoomBg(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SaveRoomBg().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Goes the room.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GoRoom(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GoRoom().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Sits the specified try {  handler.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void Sit(GameClientMessageHandler handler)
        {
            try
            {
                 handler.Sit().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the event rooms.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetEventRooms(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetEventRooms().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Starts the seasonal quest.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void StartSeasonalQuest(GameClientMessageHandler handler)
        {
            try
            {
                 handler.StartSeasonalQuest().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Saves the mannequin.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SaveMannequin(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SaveMannequin().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Saves the mannequin2.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SaveMannequin2(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SaveMannequin2().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Serializes the group purchase page.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SerializeGroupPurchasePage(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SerializeGroupPurchasePage().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Serializes the group purchase parts.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SerializeGroupPurchaseParts(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SerializeGroupPurchaseParts().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Purchases the group.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void PurchaseGroup(GameClientMessageHandler handler)
        {
            try
            {
                 handler.PurchaseGroup().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Serializes the group information.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SerializeGroupInfo(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SerializeGroupInfo().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Serializes the group members.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SerializeGroupMembers(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SerializeGroupMembers().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Makes the group admin.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void MakeGroupAdmin(GameClientMessageHandler handler)
        {
            try
            {
                 handler.MakeGroupAdmin().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Removes the group admin.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void RemoveGroupAdmin(GameClientMessageHandler handler)
        {
            try
            {
                 handler.RemoveGroupAdmin().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Accepts the membership.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void AcceptMembership(GameClientMessageHandler handler)
        {
            try
            {
                 handler.AcceptMembership().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Declines the membership.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void DeclineMembership(GameClientMessageHandler handler)
        {
            try
            {
                 handler.DeclineMembership().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Removes the member.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void RemoveMember(GameClientMessageHandler handler)
        {
            try
            {
                 handler.RemoveMember().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Joins the group.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void JoinGroup(GameClientMessageHandler handler)
        {
            try
            {
                 handler.JoinGroup().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Makes the fav.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void MakeFav(GameClientMessageHandler handler)
        {
            try
            {
                 handler.MakeFav().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Removes the fav.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void RemoveFav(GameClientMessageHandler handler)
        {
            try
            {
                 handler.RemoveFav().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Receives the nux gifts.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ReceiveNuxGifts(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ReceiveNuxGifts().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Accepts the nux gifts.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void AcceptNuxGifts(GameClientMessageHandler handler)
        {
            try
            {
                 handler.AcceptNuxGifts().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Reads the forum thread.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ReadForumThread(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ReadForumThread().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Publishes the forum thread.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void PublishForumThread(GameClientMessageHandler handler)
        {
            try
            {
                 handler.PublishForumThread().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Updates the forum thread.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void UpdateForumThread(GameClientMessageHandler handler)
        {
            try
            {
                 handler.UpdateThreadState().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Alters the state of the forum thread.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void AlterForumThreadState(GameClientMessageHandler handler)
        {
            try
            {
                 handler.AlterForumThreadState().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the forum thread root.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetForumThreadRoot(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetGroupForumThreadRoot().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the group forum data.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetGroupForumData(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetGroupForumData().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the group forums.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetGroupForums(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetGroupForums().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Manages the group.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ManageGroup(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ManageGroup().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Updates the name of the group.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void UpdateGroupName(GameClientMessageHandler handler)
        {
            try
            {
                 handler.UpdateGroupName().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Updates the group badge.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void UpdateGroupBadge(GameClientMessageHandler handler)
        {
            try
            {
                 handler.UpdateGroupBadge().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Updates the group colours.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void UpdateGroupColours(GameClientMessageHandler handler)
        {
            try
            {
                 handler.UpdateGroupColours().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Updates the group settings.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void UpdateGroupSettings(GameClientMessageHandler handler)
        {
            try
            {
                 handler.UpdateGroupSettings().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Serializes the group furni page.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SerializeGroupFurniPage(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SerializeGroupFurniPage().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Ejects the furni.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void EjectFurni(GameClientMessageHandler handler)
        {
            try
            {
                 handler.EjectFurni().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mutes the user.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void MuteUser(GameClientMessageHandler handler)
        {
            try
            {
                 handler.MuteUser().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Checks the name.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void CheckName(GameClientMessageHandler handler)
        {
            try
            {
                 handler.CheckName().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Changes the name.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ChangeName(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ChangeName().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the trainer panel.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetTrainerPanel(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetTrainerPanel().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Updates the event information.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void UpdateEventInfo(GameClientMessageHandler handler)
        {
            try
            {
                 handler.UpdateEventInfo().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the room banned users.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetRoomBannedUsers(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetBannedUsers().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Userses the with rights.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void UsersWithRights(GameClientMessageHandler handler)
        {
            try
            {
                 handler.UsersWithRights().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Unbans the user.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void UnbanUser(GameClientMessageHandler handler)
        {
            try
            {
                 handler.UnbanUser().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Manages the bot actions.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ManageBotActions(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ManageBotActions().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Handles the bot speech list.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void HandleBotSpeechList(GameClientMessageHandler handler)
        {
            try
            {
                 handler.HandleBotSpeechList().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the relationships.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetRelationships(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetRelationships().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Sets the relationship.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SetRelationship(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SetRelationship().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Automatics the room.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void AutoRoom(GameClientMessageHandler handler)
        {
            try
            {
                 handler.RoomOnLoad().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Mutes all.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void MuteAll(GameClientMessageHandler handler)
        {
            try
            {
                 handler.MuteAll().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Completes the saftey quiz.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void CompleteSafteyQuiz(GameClientMessageHandler handler)
        {
            try
            {
                 handler.CompleteSafetyQuiz().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Removes the favourite room.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void RemoveFavouriteRoom(GameClientMessageHandler handler)
        {
            try
            {
                 handler.RemoveFavouriteRoom().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Rooms the user action.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void RoomUserAction(GameClientMessageHandler handler)
        {
            try
            {
                 handler.RoomUserAction().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Saves the football outfit.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SaveFootballOutfit(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SaveFootballOutfit().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Confirms the love lock.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ConfirmLoveLock(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ConfirmLoveLock().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Builderses the club update furni count.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void BuildersClubUpdateFurniCount(GameClientMessageHandler handler)
        {
            try
            {
                 handler.BuildersClubUpdateFurniCount().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the client version message event.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static void ReleaseVersion(GameClientMessageHandler handler)
        {
            try
            {
                handler.ReleaseVersion();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Places the builders furniture.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void PlaceBuildersFurniture(GameClientMessageHandler handler)
        {
            try
            {
                 handler.PlaceBuildersFurniture().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Whispers the specified try {  handler.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void Whisper(GameClientMessageHandler handler)
        {
            try
            {
                 handler.Whisper().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Catalogues the index.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void CatalogueIndex(GameClientMessageHandler handler)
        {
            try
            {
                 handler.CatalogueIndex().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Catalogues the index.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetCatalogMode(GameClientMessageHandler handler)
        {
            try
            {
                 handler.CatalogueMode().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Catalogues the page.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void CataloguePage(GameClientMessageHandler handler)
        {
            try
            {
                 handler.CataloguePage().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Catalogues the club page.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void CatalogueClubPage(GameClientMessageHandler handler)
        {
            try
            {
                 handler.CatalogueClubPage().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Catalogues the offers configuration.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void CatalogueOffersConfig(GameClientMessageHandler handler)
        {
            try
            {
                 handler.CatalogueOfferConfig().GetAwaiter().GetResult();
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
                 handler.PurchaseOffer().GetAwaiter().GetResult();
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
                 handler.CancelOffer().GetAwaiter().GetResult();
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
                 handler.GetItemStats().GetAwaiter().GetResult();
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
                 handler.GetMyOffers().GetAwaiter().GetResult();
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
                 handler.MakeOffer().GetAwaiter().GetResult();
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
                 handler.ReedemCredits().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Catalogues the single offer.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void CatalogueSingleOffer(GameClientMessageHandler handler)
        {
            try
            {
                 handler.CatalogueOffer().GetAwaiter().GetResult();
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
                 handler.GetOffers().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Checks the name of the pet.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void CheckPetName(GameClientMessageHandler handler)
        {
            try
            {
                 handler.CheckPetName().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Purchases the item.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void PurchaseItem(GameClientMessageHandler handler)
        {
            try
            {
                 handler.PurchaseItem().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Purchases the gift.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void PurchaseGift(GameClientMessageHandler handler)
        {
            try
            {
                 handler.PurchaseGift().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the pet breeds.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetPetBreeds(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetPetBreeds().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Reloads the ecotron.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ReloadEcotron(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ReloadEcotron().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gifts the wrapping configuration.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GiftWrappingConfig(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GiftWrappingConfig().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Recyclers the rewards.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void RecyclerRewards(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetRecyclerRewards().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Requests the leave group.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void RequestLeaveGroup(GameClientMessageHandler handler)
        {
            try
            {
                 handler.RequestLeaveGroup().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Confirms the leave group.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ConfirmLeaveGroup(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ConfirmLeaveGroup().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// News the navigator.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void NewNavigator(GameClientMessageHandler handler)
        {
            try
            {
                 handler.NewNavigator().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Searches the new navigator.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SearchNewNavigator(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SearchNewNavigator().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// News the navigator delete saved search.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void NewNavigatorDeleteSavedSearch(GameClientMessageHandler handler)
        {
            try
            {
                 handler.NewNavigatorDeleteSavedSearch().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// News the navigator resize.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void NewNavigatorResize(GameClientMessageHandler handler)
        {
            try
            {
                 handler.NewNavigatorResize().GetAwaiter().GetResult();
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
                 handler.HabboAirGetUserRooms().GetAwaiter().GetResult();
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
                 handler.HabboAirGetAllRooms().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// News the navigator add saved search.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void NewNavigatorAddSavedSearch(GameClientMessageHandler handler)
        {
            try
            {
                 handler.NewNavigatorAddSavedSearch().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// News the navigator collapse category.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void NewNavigatorCollapseCategory(GameClientMessageHandler handler)
        {
            try
            {
                 handler.NewNavigatorCollapseCategory().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// News the navigator uncollapse category.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void NewNavigatorUncollapseCategory(GameClientMessageHandler handler)
        {
            try
            {
                 handler.NewNavigatorUncollapseCategory().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Pets the breed result.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void PetBreedResult(GameClientMessageHandler handler)
        {
            try
            {
                 handler.PetBreedResult().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Pets the breed cancel.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void PetBreedCancel(GameClientMessageHandler handler)
        {
            try
            {
                 handler.PetBreedCancel().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Games the center load game.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GameCenterLoadGame(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GameCenterLoadGame().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Get game lists
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetGameListing(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetGameListing().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Init the game center
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void InitializeGameCenter(GameClientMessageHandler handler)
        {
            try
            {
                 handler.InitializeGameCenter().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Games the center join queue.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GameCenterJoinQueue(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GameCenterJoinQueue().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Hotels the view countdown.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void HotelViewCountdown(GameClientMessageHandler handler)
        {
            try
            {
                 handler.HotelViewCountdown().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Hotels the view dailyquest.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void HotelViewDailyquest(GameClientMessageHandler handler)
        {
            try
            {
                 handler.HotelViewDailyquest().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Places the builders wall item.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void PlaceBuildersWallItem(GameClientMessageHandler handler)
        {
            try
            {
                 handler.PlaceBuildersWallItem().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Targeteds the offer buy.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void PurchaseTargetedOffer(GameClientMessageHandler handler)
        {
            try
            {
                 handler.PurchaseTargetedOffer().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Ambassadors the alert.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void AmbassadorAlert(GameClientMessageHandler handler)
        {
            try
            {
                 handler.AmbassadorAlert().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Goes the name of to room by.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GoToRoomByName(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GoToRoomByName().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the uc panel.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetUcPanel(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetUcPanel().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the uc panel hotel.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetUcPanelHotel(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetUcPanelHotel().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Saves the room thumbnail.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SaveRoomThumbnail(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SaveRoomThumbnail().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Uses the purchasable clothing.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void UsePurchasableClothing(GameClientMessageHandler handler)
        {
            try
            {
                 handler.UsePurchasableClothing().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the user look.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetUserLook(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetUserLook().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Sets the invitations preference.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static void SetInvitationsPreference(GameClientMessageHandler handler)
        {
            try
            {
                handler.SetInvitationsPreference();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Finds the more friends.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void FindMoreFriends(GameClientMessageHandler handler)
        {
            try
            {
                 handler.FindMoreFriends().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Hotels the view request badge.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void HotelViewRequestBadge(GameClientMessageHandler handler)
        {
            try
            {
                 handler.HotelViewRequestBadge().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the camera price.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetCameraPrice(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetCameraPrice().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Toggles the staff pick.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void ToggleStaffPick(GameClientMessageHandler handler)
        {
            try
            {
                 handler.ToggleStaffPick().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the hotel view hall of fame.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetHotelViewHallOfFame(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetHotelViewHallOfFame().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Submits the room to competition.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void SubmitRoomToCompetition(GameClientMessageHandler handler)
        {
            try
            {
                 handler.SubmitRoomToCompetition().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Enters the room queue.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void EnterRoomQueue(GameClientMessageHandler handler)
        {
            try
            {
                 handler.EnterRoomQueue().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Gets the camera request.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void GetCameraRequest(GameClientMessageHandler handler)
        {
            try
            {
                 handler.GetCameraRequest().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Votes for room.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void VoteForRoom(GameClientMessageHandler handler)
        {
            try
            {
                 handler.VoteForRoom().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Updates the forum settings.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void UpdateForumSettings(GameClientMessageHandler handler)
        {
            try
            {
                 handler.UpdateForumSettings().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Friends the request list load.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void FriendRequestListLoad(GameClientMessageHandler handler)
        {
            try
            {
                 handler.FriendRequestListLoad().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Sets the room camera preferences.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static void SetRoomCameraPreferences(GameClientMessageHandler handler)
        {
            try
            {
                handler.SetRoomCameraPreferences();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }

        /// <summary>
        /// Deletes the group.
        /// </summary>
        /// <param name="handler">The try {  handler.</param>
        internal static async void DeleteGroup(GameClientMessageHandler handler)
        {
            try
            {
                 handler.DeleteGroup().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logging.HandleException(ex, "PacketLibrary.Parse");
            }
        }
    }
}