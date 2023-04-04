using System;
using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages.Parsers;

namespace Oblivion.Messages.Handlers
{
    /// <summary>
    /// Class GameClientMessageHandler.
    /// </summary>
    partial class GameClientMessageHandler
    {
        /// <summary>
        /// Calls the guide.
        /// </summary>
        internal async Task CallGuide()
        {
            Request.GetBool();

            var userId = Request.GetIntegerFromString();
            var message = Request.GetString();

            var guideManager = Oblivion.GetGame().GetGuideManager();

            if (guideManager.GuidesCount <= 0)
            {
                await Response.InitAsync(LibraryParser.OutgoingRequest("OnGuideSessionError"));
                await Response.AppendIntegerAsync(0);
                await SendResponse();
                return;
            }

            var guide = guideManager.GetRandomGuide();

            if (guide == null)
            {
                await Response.InitAsync(LibraryParser.OutgoingRequest("OnGuideSessionError"));
                await Response.AppendIntegerAsync(0);
                await SendResponse();
                return;
            }

            var onGuideSessionAttached = new ServerMessage(LibraryParser.OutgoingRequest("OnGuideSessionAttachedMessageComposer"));
            onGuideSessionAttached.AppendBool(false);
            await onGuideSessionAttached.AppendIntegerAsync(userId);
            await onGuideSessionAttached.AppendStringAsync(message);
            await onGuideSessionAttached.AppendIntegerAsync(30);
            await Session.SendMessageAsync(onGuideSessionAttached);

            lock (guide)
            {


                var onGuideSessionAttached2 =
                    new ServerMessage(LibraryParser.OutgoingRequest("OnGuideSessionAttachedMessageComposer"));
                onGuideSessionAttached2.AppendBool(true);
                onGuideSessionAttached2.AppendInteger(userId);
                onGuideSessionAttached2.AppendString(message);
                onGuideSessionAttached2.AppendInteger(15);
                guide.SendMessage(onGuideSessionAttached2);
                guide.GetHabbo().GuideOtherUser = Session;
                Session.GetHabbo().GuideOtherUser = guide;
            }
        }

        /// <summary>
        /// Answers the guide request.
        /// </summary>
        internal async Task AnswerGuideRequest()
        {
            var state = Request.GetBool();

            if (!state)
                return;

            var requester = Session.GetHabbo().GuideOtherUser;
            var message = new ServerMessage(LibraryParser.OutgoingRequest("OnGuideSessionStartedMessageComposer"));

            await message.AppendIntegerAsync(requester.GetHabbo().Id);
            await message.AppendStringAsync(requester.GetHabbo().UserName);
            await message.AppendStringAsync(requester.GetHabbo().Look);
            await message.AppendIntegerAsync(Session.GetHabbo().Id);
            await message.AppendStringAsync(Session.GetHabbo().UserName);
            await message.AppendStringAsync(Session.GetHabbo().Look);
            await requester.SendMessage(message);
            await Session.SendMessageAsync(message);
        }

        /// <summary>
        /// Opens the guide tool.
        /// </summary>
        internal async Task OpenGuideTool()
        {
            var guideManager = Oblivion.GetGame().GetGuideManager();
            var onDuty = Request.GetBool();

            Request.GetBool();
            Request.GetBool();
            Request.GetBool();

            if (onDuty)
                guideManager.AddGuide(Session);
            else
                guideManager.RemoveGuide(Session);

            Session.GetHabbo().OnDuty = onDuty;
            await Response.InitAsync(LibraryParser.OutgoingRequest("HelperToolConfigurationMessageComposer"));
            Response.AppendBool(onDuty);
            await Response.AppendIntegerAsync(guideManager.GuidesCount);
            await Response.AppendIntegerAsync(guideManager.HelpersCount);
            await Response.AppendIntegerAsync(guideManager.GuardiansCount);
            await SendResponse();
        }

        /// <summary>
        /// Invites to room.
        /// </summary>
        internal async Task InviteToRoom()
        {
            var requester = Session.GetHabbo().GuideOtherUser;

            var room = Session.GetHabbo().CurrentRoom;

            var message = new ServerMessage(LibraryParser.OutgoingRequest("OnGuideSessionInvitedToGuideRoomMessageComposer"));

            if (room == null)
            {
                await message.AppendIntegerAsync(0);
                await message.AppendStringAsync(string.Empty);
            }
            else
            {
                await message.AppendIntegerAsync(room.RoomId);
                await message.AppendStringAsync(room.RoomData.Name);
            }

            requester.SendMessage(message);
            await Session.SendMessageAsync(message);
        }

        /// <summary>
        /// Visits the room.
        /// </summary>
        internal async Task VisitRoom()
        {
            if (Session.GetHabbo().GuideOtherUser == null)
                return;

            var requester = Session.GetHabbo().GuideOtherUser;
            var visitRoom = new ServerMessage(LibraryParser.OutgoingRequest("RoomForwardMessageComposer"));
            await visitRoom.AppendIntegerAsync(requester.GetHabbo().CurrentRoomId);
            await Session.SendMessageAsync(visitRoom);
        }

        /// <summary>
        /// Guides the speak.
        /// </summary>
        internal async Task GuideSpeak()
        {
            var message = Request.GetString();
            var requester = Session.GetHabbo().GuideOtherUser;
            var messageC = new ServerMessage(LibraryParser.OutgoingRequest("OnGuideSessionMsgMessageComposer"));
            await messageC.AppendStringAsync(message);
            await messageC.AppendIntegerAsync(Session.GetHabbo().Id);
            await requester.SendMessage(messageC);
            await Session.SendMessageAsync(messageC);
        }

        /// <summary>
        /// BETA
        /// Closes the guide request.
        /// </summary>
        internal async Task CloseGuideRequest()
        {
            //Request.GetBool();

            var requester = Session.GetHabbo().GuideOtherUser;
            var message = new ServerMessage(LibraryParser.OutgoingRequest("OnGuideSessionDetachedMessageComposer"));

            /* guide - close session  */
            await message.AppendIntegerAsync(2);
            await requester.SendMessage(message);

            /* user - close session */
            var message2 = new ServerMessage(LibraryParser.OutgoingRequest("OnGuideSessionDetachedMessageComposer"));
            await message.AppendIntegerAsync(0);
            await Session.SendMessageAsync(message2);

            /* user - detach session */
            var message3 = new ServerMessage(LibraryParser.OutgoingRequest("OnGuideSessionDetachedMessageComposer"));
            await Session.SendMessageAsync(message3);

            /* guide - detach session */
            var message4 = new ServerMessage(LibraryParser.OutgoingRequest("OnGuideSessionDetachedMessageComposer"));
            await requester.SendMessage(message4);

            Console.WriteLine("The Close was Called");

            requester.GetHabbo().GuideOtherUser = null;
            Session.GetHabbo().GuideOtherUser = null;
        }

        /// <summary>
        /// Cancels the call guide.
        /// BETA
        /// </summary>
        internal async Task CancelCallGuide()
        {
            //Response.Init(3485);
            //await SendResponse();

            // Request.GetBool();

//            var requester = Session.GetHabbo().GuideOtherUser;

            /* user - cancell session */
            var message = new ServerMessage(LibraryParser.OutgoingRequest("OnGuideSessionDetachedMessageComposer"));
            await message.AppendIntegerAsync(2);
            await Session.SendMessageAsync(message);

        }

        /// <summary>
        /// Guides the feedback.
        /// </summary>
        internal async Task GuideFeedback()
        {
            var message = new ServerMessage(LibraryParser.OutgoingRequest("OnGuideSessionDetachedMessageComposer"));

            await Session.SendMessageAsync(message);

            await Oblivion.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_GuideFeedbackGiver", 1);
        }

        /// <summary>
        /// Ambassadors the alert.
        /// </summary>
        internal Task AmbassadorAlert()
        {
            if (Session.GetHabbo().Rank < Convert.ToUInt32(Oblivion.GetDbConfig().DbData["ambassador.minrank"]))
                return Task.CompletedTask;

            uint userId = Request.GetUInteger();

            GameClient user = Oblivion.GetGame().GetClientManager().GetClientByUserId(userId);

            user?.SendNotif("${notification.ambassador.alert.warning.message}", "${notification.ambassador.alert.warning.title}");
            return Task.CompletedTask;
        }
    }
}