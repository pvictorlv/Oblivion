using System.Collections.Generic;
using System.Globalization;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.Data;
using Oblivion.HabboHotel.Support;
using Oblivion.Messages.Parsers;

namespace Oblivion.Messages.Handlers
{
    /// <summary>
    /// Class GameClientMessageHandler.
    /// </summary>
    partial class GameClientMessageHandler
    {
        /// <summary>
        /// Initializes the help tool.
        /// </summary>
        internal async Task InitHelpTool()
        {
            Response.Init(LibraryParser.OutgoingRequest("OpenHelpToolMessageComposer"));

            if (!Oblivion.GetGame().GetModerationTool().UsersHasPendingTicket(Session.GetHabbo().Id))
            {
                Response.AppendInteger(0); // It's okay, the user may send an new issue
                await SendResponse();
                return;
            }

            SupportTicket ticket = Oblivion.GetGame().GetModerationTool().GetPendingTicketForUser(Session.GetHabbo().Id);

            if (ticket == null) // null check to be sure
                return;

            Response.AppendInteger(1); // the user has an pending issue

            Response.AppendString(ticket.TicketId.ToString());
            Response.AppendString(ticket.Timestamp.ToString(CultureInfo.InvariantCulture));
            Response.AppendString(ticket.Message);
            await SendResponse();
        }

        /// <summary>
        /// Submits the help ticket.
        /// </summary>
        internal async Task SubmitHelpTicket()
        {
            string message = Request.GetString();
            int category = Request.GetInteger();
            uint reportedUser = Request.GetUInteger();

            Request.GetUInteger(); // roomId

            int messageCount = Request.GetInteger();

            List<string> chats = new List<string>();

            for (int i = 0; i < messageCount; i++)
            {
                Request.GetInteger();

                chats.Add(Request.GetString());
            }


            if (Oblivion.GetGame().GetModerationTool().UsersHasPendingTicket(Session.GetHabbo().Id))
            {
                Response.Init(LibraryParser.OutgoingRequest("TicketUserAlert"));

                SupportTicket ticket = Oblivion.GetGame().GetModerationTool().GetPendingTicketForUser(Session.GetHabbo().Id);
                Response.AppendInteger(1);
                Response.AppendString(ticket.TicketId.ToString());
                Response.AppendString(ticket.Timestamp.ToString(CultureInfo.InvariantCulture));
                Response.AppendString(ticket.Message);
                await SendResponse();

                return;
            }

            if (Oblivion.GetGame().GetModerationTool().UsersHasAbusiveCooldown(Session.GetHabbo().Id)) // the previous issue of the user was abusive
            {
                Response.Init(LibraryParser.OutgoingRequest("TicketUserAlert"));

                Response.AppendInteger(2);
                await SendResponse();

                return;
            }

//            Response.AppendInteger(0); // It's okay, the user may send an new issue
            Oblivion.GetGame().GetModerationTool().SendNewTicket(Session, category, 7, reportedUser, message, chats);

//            await SendResponse();
        }

        /// <summary>
        /// Deletes the pending CFH.
        /// </summary>
        internal async Task DeletePendingCfh()
        {
            if (!Oblivion.GetGame().GetModerationTool().UsersHasPendingTicket(Session.GetHabbo().Id))
                return;

            Oblivion.GetGame().GetModerationTool().DeletePendingTicketForUser(Session.GetHabbo().Id);

            Response.Init(LibraryParser.OutgoingRequest("OpenHelpToolMessageComposer"));
            Response.AppendInteger(0);
            await SendResponse();
        }

        /// <summary>
        /// Mods the get user information.
        /// </summary>
        internal async Task ModGetUserInfo()
        {
            if (Session.GetHabbo().HasFuse("fuse_mod"))
            {
                var num = Request.GetUInteger();

                if (Oblivion.GetGame().GetClientManager().GetNameById(num) != "Unknown User")
                    await Session.SendMessageAsync(ModerationTool.SerializeUserInfo(num));
                else
                    await Session.SendNotifyAsync(Oblivion.GetLanguage().GetVar("help_information_error"));
            }
        }

        /// <summary>
        /// Mods the get user chatlog.
        /// </summary>
        internal async Task ModGetUserChatlog()
        {
            if (!Session.GetHabbo().HasFuse("fuse_chatlogs"))
                return;

            await Session.SendMessageAsync(ModerationTool.SerializeUserChatlog(Request.GetUInteger()));
        }

        /// <summary>
        /// Mods the get room chatlog.
        /// </summary>
        internal async Task ModGetRoomChatlog()
        {
            if (!Session.GetHabbo().HasFuse("fuse_chatlogs"))
            {
                await Session.SendNotifyAsync(Oblivion.GetLanguage().GetVar("help_information_error_rank_low"));
                return;
            }

            Request.GetInteger();
            var roomId = Request.GetUInteger();

            if (Oblivion.GetGame().GetRoomManager().GetRoom(roomId) != null)
                await Session.SendMessageAsync(ModerationTool.SerializeRoomChatlog(roomId));
        }

        /// <summary>
        /// Mods the get room tool.
        /// </summary>
        internal async Task ModGetRoomTool()
        {
            if (!Session.GetHabbo().HasFuse("fuse_mod"))
                return;

            var roomId = Request.GetUInteger();
            var data = Oblivion.GetGame().GetRoomManager().GenerateNullableRoomData(roomId);

            await Session.SendMessageAsync(ModerationTool.SerializeRoomTool(data));
        }

        /// <summary>
        /// Mods the pick ticket.
        /// </summary>
        internal async Task ModPickTicket()
        {
            if (!Session.GetHabbo().HasFuse("fuse_mod"))
                return;

            Request.GetInteger();
            var ticketId = Request.GetUInteger();

            Oblivion.GetGame().GetModerationTool().PickTicket(Session, ticketId);
        }

        ///<summary>
        ///Mods the release ticket.
        ///</summary>
        internal async Task ModReleaseTicket()
        {
            if (!Session.GetHabbo().HasFuse("fuse_mod"))
                return;

            int ticketCount = Request.GetInteger();

            for (int i = 0; i < ticketCount; i++)
                Oblivion.GetGame().GetModerationTool().ReleaseTicket(Session, Request.GetUInteger());
        }

        /// <summary>
        /// Mods the close ticket.
        /// </summary>
        internal async Task ModCloseTicket()
        {
            if (!Session.GetHabbo().HasFuse("fuse_mod"))
                return;

            int result = Request.GetInteger();

            Request.GetInteger();

            uint ticketId = Request.GetUInteger();

            if (ticketId <= 0)
                return;

            Oblivion.GetGame().GetModerationTool().CloseTicket(Session, ticketId, result);
        }

        /// <summary>
        /// Mods the get ticket chatlog.
        /// </summary>
        internal async Task ModGetTicketChatlog()
        {
            if (!Session.GetHabbo().HasFuse("fuse_mod"))
                return;

            SupportTicket ticket = Oblivion.GetGame().GetModerationTool().GetTicket(Request.GetUInteger());

            if (ticket == null)
                return;

            RoomData roomData = Oblivion.GetGame().GetRoomManager().GenerateNullableRoomData(ticket.RoomId);

            if (roomData == null)
                return;

            await Session.SendMessageAsync(ModerationTool.SerializeTicketChatlog(ticket, roomData, ticket.Timestamp));
        }

        /// <summary>
        /// Mods the get room visits.
        /// </summary>
        internal async Task ModGetRoomVisits()
        {
            if (Session.GetHabbo().HasFuse("fuse_mod"))
            {
                uint userId = Request.GetUInteger();

                if (userId > 0)
                    await Session.SendMessageAsync(ModerationTool.SerializeRoomVisits(userId));
            }
        }

        /// <summary>
        /// Mods the send room alert.
        /// </summary>
        internal async Task ModSendRoomAlert()
        {
            if (!Session.GetHabbo().HasFuse("fuse_alert"))
                return;

            Request.GetInteger();

            string message = Request.GetString();

            ServerMessage serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
            serverMessage.AppendString("admin");
            serverMessage.AppendInteger(3);
            serverMessage.AppendString("message");
            serverMessage.AppendString($"{message}\r\n\r\n- {Session.GetHabbo().UserName}");
            serverMessage.AppendString("link");
            serverMessage.AppendString("event:");
            serverMessage.AppendString("linkTitle");
            serverMessage.AppendString("ok");

            Room room = Session.GetHabbo().CurrentRoom;

            room?.SendMessage(serverMessage);
        }

        /// <summary>
        /// Mods the perform room action.
        /// </summary>
        internal async Task ModPerformRoomAction()
        {
            if (!Session.GetHabbo().HasFuse("fuse_mod"))
                return;

            uint roomId = Request.GetUInteger();
            bool lockRoom = Request.GetIntegerAsBool();
            bool inappropriateRoom = Request.GetIntegerAsBool();
            bool kickUsers = Request.GetIntegerAsBool();

            ModerationTool.PerformRoomAction(Session, roomId, kickUsers, lockRoom, inappropriateRoom, Response);
        }

        /// <summary>
        /// Mods the send user caution.
        /// </summary>
        internal async Task ModSendUserCaution()
        {
            if (!Session.GetHabbo().HasFuse("fuse_alert"))
                return;

            var userId = Request.GetUInteger();
            var message = Request.GetString();

            ModerationTool.AlertUser(Session, userId, message, true);
        }

        /// <summary>
        /// Mods the send user message.
        /// </summary>
        internal async Task ModSendUserMessage()
        {
            if (!Session.GetHabbo().HasFuse("fuse_alert"))
                return;

            var userId = Request.GetUInteger();
            var message = Request.GetString();

            ModerationTool.AlertUser(Session, userId, message, false);
        }

        /// <summary>
        /// Mods the mute user.
        /// </summary>
        internal async Task ModMuteUser()
        {
            if (!Session.GetHabbo().HasFuse("fuse_mute"))
                return;

            var userId = Request.GetUInteger();
            var message = Request.GetString();
            var clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(userId);

            clientByUserId.GetHabbo().Mute();
            clientByUserId.SendNotif(message);
        }

        /// <summary>
        /// Mods the lock trade.
        /// </summary>
        internal async Task ModLockTrade()
        {
            if (!Session.GetHabbo().HasFuse("fuse_lock_trade"))
                return;

            var userId = Request.GetUInteger();
            var message = Request.GetString();
            var length = (Request.GetInteger() * 3600);

            ModerationTool.LockTrade(Session, userId, message, length);
        }

        /// <summary>
        /// Mods the kick user.
        /// </summary>
        internal async Task ModKickUser()
        {
            if (!Session.GetHabbo().HasFuse("fuse_kick"))
                return;

            var userId = Request.GetUInteger();
            var message = Request.GetString();

            ModerationTool.KickUser(Session, userId, message, false);
        }

        /// <summary>
        /// Mods the ban user.
        /// </summary>
        internal async Task ModBanUser()
        {
            if (Session?.GetHabbo() == null) return;

            if (!Session.GetHabbo().HasFuse("fuse_ban"))
                return;

            var userId = Request.GetUInteger();
            var message = Request.GetString();
            var length = (Request.GetInteger() * 3600);

            ModerationTool.BanUser(Session, userId, length, message);
        }
    }
}