using System;
using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorMannequin : FurniInteractorModel
    {
        public override void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (!item.ExtraData.Contains(Convert.ToChar(5).ToString()))
                return;

            var array = item.ExtraData.Split(Convert.ToChar(5));

            session.GetHabbo().Gender = (array[0].ToUpper() == "F" ? "F" : "M");

            var dictionary = new Dictionary<string, string>();

            dictionary.Clear();

            var array2 = array[1].Split('.');

            /* TODO CHECK */ foreach (var text in array2)
            {
                var array3 = session.GetHabbo().Look.Split('.');

                /* TODO CHECK */ foreach (var text2 in array3)
                {
                    if (text2.Split('-')[0] == text.Split('-')[0])
                    {
                        if (dictionary.ContainsKey(text2.Split('-')[0]) && !dictionary.ContainsValue(text))
                        {
                            dictionary.Remove(text2.Split('-')[0]);
                            dictionary.Add(text2.Split('-')[0], text);
                        }
                        else
                        {
                            if (!dictionary.ContainsKey(text2.Split('-')[0]) && !dictionary.ContainsValue(text))
                                dictionary.Add(text2.Split('-')[0], text);
                        }
                    }
                    else
                    {
                        if (!dictionary.ContainsKey(text2.Split('-')[0]))
                            dictionary.Add(text2.Split('-')[0], text2);
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
                queryReactor.RunQuery();
            }

            session.GetMessageHandler()
                .GetResponse()
                .Init(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
            session.GetMessageHandler().GetResponse().AppendInteger(-1);
            session.GetMessageHandler().GetResponse().AppendString(session.GetHabbo().Look);
            session.GetMessageHandler().GetResponse().AppendString(session.GetHabbo().Gender.ToLower());
            session.GetMessageHandler().GetResponse().AppendString(session.GetHabbo().Motto);
            session.GetMessageHandler().GetResponse().AppendInteger(session.GetHabbo().AchievementPoints);
            session.GetMessageHandler().SendResponse();

            var roomUserByHabbo = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));

            serverMessage.AppendInteger(roomUserByHabbo.VirtualId);
            serverMessage.AppendString(session.GetHabbo().Look);
            serverMessage.AppendString(session.GetHabbo().Gender.ToLower());
            serverMessage.AppendString(session.GetHabbo().Motto);
            serverMessage.AppendInteger(session.GetHabbo().AchievementPoints);

            session.GetHabbo().CurrentRoom.SendMessage(serverMessage);
        }
    }
}