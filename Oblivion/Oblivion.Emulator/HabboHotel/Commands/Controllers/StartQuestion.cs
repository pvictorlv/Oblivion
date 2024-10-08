﻿using System.Threading;
using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Polls;
using Oblivion.HabboHotel.Polls.Enums;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class StartQuestion. This class cannot be inherited.
    /// </summary>
    internal sealed class StartQuestion : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="StartQuestion" /> class.
        /// </summary>
        public StartQuestion()
        {
            MinRank = 7;
            Description = "Starts a matching question.";
            Usage = ":startquestion [id]";
            MinParams = 1;
        }

        public override async Task<bool> Execute(GameClient client, string[] pms)
        {
            var id = uint.Parse(pms[0]);
            var poll = Oblivion.GetGame().GetPollManager().TryGetPollById(id);
            if (poll == null || poll.Type != PollType.Matching)
            {
                await client.SendWhisperAsync("Poll doesn't exists or isn't a matching poll.");
                return true;
            }
            poll.AnswersPositive = 0;
            poll.AnswersNegative = 0;
            await MatchingPollAnswer(client, poll);
            var showPoll = new Thread(async delegate () { await MatchingPollResults(client, poll); });
            showPoll.Start();
            return true;
        }

        internal static async Task MatchingPollAnswer(GameClient client, Poll poll)
        {
            if (poll == null || poll.Type != PollType.Matching)
                return;
            var message = new ServerMessage(LibraryParser.OutgoingRequest("MatchingPollMessageComposer"));
            await message.AppendStringAsync("MATCHING_POLL");
            await message.AppendIntegerAsync(poll.Id);
            await message.AppendIntegerAsync(poll.Id);
            await message.AppendIntegerAsync(15580);
            await message.AppendIntegerAsync(poll.Id);
            await message.AppendIntegerAsync(29);
            await message.AppendIntegerAsync(5);
            await message.AppendStringAsync(poll.PollName);
            await client.GetHabbo().CurrentRoom.SendMessage(message);
        }

        internal static async Task MatchingPollResults(GameClient client, Poll poll)
        {
            var room = client.GetHabbo().CurrentRoom;
            if (poll == null || poll.Type != PollType.Matching || room == null)
                return;

            var users = room.GetRoomUserManager().GetRoomUsers();

            for (var i = 0; i < 10; i++)
            {
                Thread.Sleep(1000);
                /* TODO CHECK */ foreach (var roomUser in users)
                {
                    var user = Oblivion.GetHabboById(roomUser.UserId);
                    if (user.AnsweredPool)
                    {
                        var result =
                            new ServerMessage(LibraryParser.OutgoingRequest("MatchingPollResultMessageComposer"));
                        await result.AppendIntegerAsync(poll.Id);
                        await result.AppendIntegerAsync(2);
                        await result.AppendStringAsync("0");
                        await result.AppendIntegerAsync(poll.AnswersNegative);
                        await result.AppendStringAsync("1");
                        await result.AppendIntegerAsync(poll.AnswersPositive);
                        await client.SendMessage(result);
                    }
                }
            }

            /* TODO CHECK */ foreach (var roomUser in users)
                Oblivion.GetHabboById(roomUser.UserId).AnsweredPool = false;
        }
    }
}