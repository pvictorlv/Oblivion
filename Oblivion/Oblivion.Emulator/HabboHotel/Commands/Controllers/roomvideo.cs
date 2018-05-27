using System;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    internal sealed class RoomVideo : Command
    {
        public RoomVideo()
        {
            MinRank = -1;
            Description = "Send an video for the whole room";
            Usage = ":roomvideo [video id]";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {

            var video = pms[0];
            var room = session.GetHabbo().CurrentRoom;

            if (video.Length != 11)
            {
                room.RoomVideo = "";
                session.SendWhisper("Você desativou o vídeo da sala, para colocar um vídeo digite :roomvideo e o id (após o v=)");
                return true;
            }

            if (session.GetHabbo().WebSocketConnId == Guid.Empty)
            {
                session.SendWhisper("Error! Tente logar novamente!");
                return true;
            }
            room.SendWebSocketMessage($"2|{video}");
            room.RoomVideo = video;
            session.SendWhisper($"Você definiu o vídeo da sala como {video}, para desativar digite :roomvideo disable");
            return true;
        }
    }
}