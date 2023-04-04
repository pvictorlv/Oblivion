using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorMannequin : FurniInteractorModel
    {
        public override async Task OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (!item.ExtraData.Contains(Convert.ToChar(5).ToString()))
                return;

            var array = item.ExtraData.Split(Convert.ToChar(5));

            session.GetHabbo().Gender = (array[0].ToUpper() == "F" ? "F" : "M");

            var dictionary = new Dictionary<string, string>();


            var array2 = array[1].Split('.');

            foreach (var text in array2)
            {
                var array3 = session.GetHabbo().Look.Split('.');
                foreach (var text2 in array3)
                {
                    if (text2.Split('-')[0] == text.Split('-')[0])
                    {
                        dictionary[text2.Split('-')[0]] = text;
                    }
                    else
                    {
                        dictionary[text2.Split('-')[0]] = text2;
                    }
                }
            }

            var text3 = dictionary.Values.Aggregate("", (current1, current) => $"{current1}{current}.");

            session.GetHabbo().Look = text3.TrimEnd('.');

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    $"UPDATE users SET look = @look, gender = @gender WHERE id = {session.GetHabbo().Id}");
                queryReactor.AddParameter("look", session.GetHabbo().Look);
                queryReactor.AddParameter("gender", session.GetHabbo().Gender);
                await queryReactor.RunQueryAsync();
            }

            await session.GetMessageHandler()
                .GetResponse()
                .InitAsync(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
            await session.GetMessageHandler().GetResponse().AppendIntegerAsync(-1);
            await session.GetMessageHandler().GetResponse().AppendStringAsync(session.GetHabbo().Look);
            await session.GetMessageHandler().GetResponse().AppendStringAsync(session.GetHabbo().Gender.ToLower());
            await session.GetMessageHandler().GetResponse().AppendStringAsync(session.GetHabbo().Motto);
            await session.GetMessageHandler().GetResponse().AppendIntegerAsync(session.GetHabbo().AchievementPoints);
            await session.GetMessageHandler().SendResponse();

            var roomUserByHabbo = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));

            await serverMessage.AppendIntegerAsync(roomUserByHabbo.VirtualId);
            await serverMessage.AppendStringAsync(session.GetHabbo().Look);
            await serverMessage.AppendStringAsync(session.GetHabbo().Gender.ToLower());
            await serverMessage.AppendStringAsync(session.GetHabbo().Motto);
            await serverMessage.AppendIntegerAsync(session.GetHabbo().AchievementPoints);

            await session.GetHabbo().CurrentRoom.SendMessage(serverMessage);
        }
    }
}