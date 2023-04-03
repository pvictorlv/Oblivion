using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oblivion.HabboHotel.Quests;
using Oblivion.Messages.Parsers;
using Oblivion.Security;

namespace Oblivion.Messages.Handlers
{
    /// <summary>
    /// Class GameClientMessageHandler.
    /// </summary>
    internal partial class GameClientMessageHandler
    {
        /// <summary>
        /// Initializes the messenger.
        /// </summary>
        internal async Task InitMessenger()
        {
        }

        /// <summary>
        /// Friendses the list update.
        /// </summary>
        internal async Task FriendsListUpdate()
        {
            Session.GetHabbo().GetMessenger();
        }

        /// <summary>
        /// Removes the buddy.
        /// </summary>
        internal async Task RemoveBuddy()
        {
            if (Session.GetHabbo().GetMessenger() == null) return;
            var num = Request.GetInteger();
            for (var i = 0; i < num; i++)
            {
                var num2 = Request.GetUInteger();
                if (Session.GetHabbo().Data.Relations.ContainsKey(Convert.ToUInt32(num2)))
                {
                    await Session.SendNotifyAsync(Oblivion.GetLanguage().GetVar("buddy_error_1"));
                    return;
                }

                await Session.GetHabbo().GetMessenger().DestroyFriendship(num2);
            }
        }

        /// <summary>
        /// Searches the habbo.
        /// </summary>
        internal async Task SearchHabbo()
        {
            if (Session.GetHabbo().GetMessenger() == null) return;
            await Session.SendMessageAsync(await Session.GetHabbo().GetMessenger().PerformSearch(Request.GetString()));
        }

        /// <summary>
        /// Accepts the request.
        /// </summary>
        internal async Task AcceptRequest()
        {
            if (Session.GetHabbo().GetMessenger() == null) return;
            var num = Request.GetInteger();
            for (var i = 0; i < num; i++)
            {
                var num2 = Request.GetUInteger();
                var request = Session.GetHabbo().GetMessenger().GetRequest(num2);
                if (request == null) continue;
                if (request.To != Session.GetHabbo().Id) return;
                if (!Session.GetHabbo().GetMessenger().FriendshipExists(request.To))
                    await Session.GetHabbo().GetMessenger().CreateFriendship(request.From);
                await Session.GetHabbo().GetMessenger().HandleRequest(num2);
            }
        }

        /// <summary>
        /// Declines the request.
        /// </summary>
        internal async Task DeclineRequest()
        {
            if (Session.GetHabbo().GetMessenger() == null) return;
            var flag = Request.GetBool();
            Request.GetInteger();
            if (!flag)
            {
                var sender = Request.GetUInteger();
                await Session.GetHabbo().GetMessenger().HandleRequest(sender);
                return;
            }

            await Session.GetHabbo().GetMessenger().HandleAllRequests();
        }

        /// <summary>
        /// Requests the buddy.
        /// </summary>
        internal async Task RequestBuddy()
        {
            if (Session?.GetHabbo()?.GetMessenger() == null) return;

            if (await Session.GetHabbo().GetMessenger().RequestBuddy(Request.GetString()))
                await Oblivion.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SocialFriend);
        }

        /// <summary>
        /// Sends the instant messenger.
        /// </summary>
        internal async Task SendInstantMessenger()
        {
            var toId = Request.GetInteger();
            var text = Request.GetString();
            if (Session.GetHabbo().GetMessenger() == null) return;
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (toId > 0)
                {
                    await Session.GetHabbo().GetMessenger().SendInstantMessage((uint) toId, text);
                    return;
                }

                var gp = Oblivion.GetGame().GetGroupManager().GetGroup((uint) -toId);
                if (gp == null)
                {
                    return;
                }

                if (!gp.HasChat)
                {
                    await Session.SendNotifyAsync("Não foi possível enviar mensagem para esse grupo!");
                    return;
                }

                await Session.GetHabbo().GetMessenger().SendInstantMessage(gp, text);
            }
        }

        /// <summary>
        /// Follows the buddy.
        /// </summary>
        internal async Task FollowBuddy()
        {
            if (Session?.GetHabbo() == null)
                return;
            var userId = Request.GetUInteger();
            var clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(userId);

            if (clientByUserId?.GetHabbo() == null) return;
            if (clientByUserId.GetHabbo().GetMessenger() == null || clientByUserId.GetHabbo().CurrentRoom == null)
            {
                if (Session.GetHabbo().GetMessenger() == null) return;
                await Response.InitAsync(LibraryParser.OutgoingRequest("FollowFriendErrorMessageComposer"));
                await Response.AppendIntegerAsync(2);
                await SendResponse();
                await Session.GetHabbo().GetMessenger().UpdateFriend(userId, clientByUserId, true);
                return;
            }

            if (Session.GetHabbo().Rank < 4 && Session.GetHabbo().GetMessenger() != null &&
                !Session.GetHabbo().GetMessenger().FriendshipExists(userId))
            {
                await Response.InitAsync(LibraryParser.OutgoingRequest("FollowFriendErrorMessageComposer"));
                await Response.AppendIntegerAsync(0);
                await SendResponse();
                return;
            }

            var roomFwd = new ServerMessage(LibraryParser.OutgoingRequest("RoomForwardMessageComposer"));
            await roomFwd.AppendIntegerAsync(clientByUserId.GetHabbo().CurrentRoom.RoomId);
            await Session.SendMessageAsync(roomFwd);
        }


        /// <summary>
        /// Sends the instant invite.
        /// </summary>
        internal async Task SendInstantInvite()
        {
            if (Session?.GetHabbo() == null)
                return;

            if (!await Session.GetHabbo().CanTalk()) return;

            var num = Request.GetInteger();
            var list = new List<uint>();
            for (var i = 0; i < num; i++) list.Add(Request.GetUInteger());
            var s = Request.GetString();

            if (!BobbaFilter.CanTalk(Session, s)) return;

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("ConsoleInvitationMessageComposer"));
            await serverMessage.AppendIntegerAsync(Session.GetHabbo().Id);
            await serverMessage.AppendStringAsync(s);

            foreach (var current in list)
            {
                if (!Session.GetHabbo().GetMessenger().FriendshipExists(current)) continue;
                var clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(current);
                clientByUserId?.SendMessage(serverMessage);
            }
        }
    }
}