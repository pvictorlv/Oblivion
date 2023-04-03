using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Polls;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    public class StartPoll : Command
    {
        public StartPoll()
        {
            MinRank = -1;
            Description = "Start a quick question in room";
            Usage = ":poll [text]";
            MinParams = -1;
        }

        public override async Task<bool> Execute(GameClient client, string[] pms)
        {
            var title = string.Join(" ", pms);

            var room = client.GetHabbo().CurrentRoom;

            if (room == null) return true;

            if (title == "endquestion")
            {
                if (!Oblivion.GetGame().GetPollManager().Polls.TryGetValue(room.RoomId, out var endPoll)) return false;

                var result =
                    new ServerMessage(LibraryParser.OutgoingRequest("MatchingPollResultMessageComposer"));
                await result.AppendIntegerAsync(room.RoomId);
                await result.AppendIntegerAsync(2);
                await result.AppendStringAsync("0");
                await result.AppendIntegerAsync(endPoll.AnswersNegative);
                await result.AppendStringAsync("1");
                await result.AppendIntegerAsync(endPoll.AnswersPositive);
                await room.SendMessage(result);

                Oblivion.GetGame().GetPollManager().Polls.Remove(room.RoomId);

                return true;
            }


            var poll = new Poll(room.RoomId, room.RoomId, title, "", "", "", 3, null);

            Oblivion.GetGame().GetPollManager().Polls.Add(room.RoomId, poll);

            var message = new ServerMessage(LibraryParser.OutgoingRequest("MatchingPollMessageComposer"));
            await message.AppendStringAsync("MATCHING_POLL");
            await message.AppendIntegerAsync(poll.RoomId);
            await message.AppendIntegerAsync(poll.RoomId);
            await message.AppendIntegerAsync(1);
            await message.AppendIntegerAsync(poll.RoomId);
            await message.AppendIntegerAsync(120);
            await message.AppendIntegerAsync(3);
            await message.AppendStringAsync(title);
            client.GetHabbo().CurrentRoom.SendMessage(message);

            await client.SendWhisperAsync("Para finalizar a votação digite :poll endquestion");

            return true;
        }
    }
}