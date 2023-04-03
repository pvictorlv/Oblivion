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

        public override bool Execute(GameClient client, string[] pms)
        {
            var title = string.Join(" ", pms);

            var room = client.GetHabbo().CurrentRoom;

            if (room == null) return true;

            if (title == "endquestion")
            {
                if (!Oblivion.GetGame().GetPollManager().Polls.TryGetValue(room.RoomId, out var endPoll)) return false;

                var result =
                    new ServerMessage(LibraryParser.OutgoingRequest("MatchingPollResultMessageComposer"));
                result.AppendInteger(room.RoomId);
                result.AppendInteger(2);
                result.AppendString("0");
                result.AppendInteger(endPoll.AnswersNegative);
                result.AppendString("1");
                result.AppendInteger(endPoll.AnswersPositive);
                await room.SendMessage(result);

                Oblivion.GetGame().GetPollManager().Polls.Remove(room.RoomId);

                return true;
            }


            var poll = new Poll(room.RoomId, room.RoomId, title, "", "", "", 3, null);

            Oblivion.GetGame().GetPollManager().Polls.Add(room.RoomId, poll);

            var message = new ServerMessage(LibraryParser.OutgoingRequest("MatchingPollMessageComposer"));
            message.AppendString("MATCHING_POLL");
            message.AppendInteger(poll.RoomId);
            message.AppendInteger(poll.RoomId);
            message.AppendInteger(1);
            message.AppendInteger(poll.RoomId);
            message.AppendInteger(120);
            message.AppendInteger(3);
            message.AppendString(title);
            client.GetHabbo().CurrentRoom.SendMessage(message);

            client.SendWhisper("Para finalizar a votação digite :poll endquestion");

            return true;
        }
    }
}