﻿using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class CopyLook. This class cannot be inherited.
    /// </summary>
    internal sealed class CopyLook : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CopyLook" /> class.
        /// </summary>
        public CopyLook()
        {
            MinRank = 1;
            Description = "Copys a look of another user.";
            Usage = ":copy [USERNAME]";
            MinParams = 1;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(pms[0]);
            if (user == null)
            {
                 await session.SendWhisperAsync(Oblivion.GetLanguage().GetVar("user_not_found"));
                return true;
            }

            var gender = user.GetClient().GetHabbo().Gender;
            var look = user.GetClient().GetHabbo().Look;
            session.GetHabbo().Gender = gender;
            session.GetHabbo().Look = look;
            using (var adapter = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
            {
                adapter.SetQuery(
                    "UPDATE users SET gender = @gender, look = @look WHERE id = " + session.GetHabbo().Id);
                adapter.AddParameter("gender", gender);
                adapter.AddParameter("look", look);
                await adapter.RunQueryAsync();
            }

            var myUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (myUser == null) return true;

            var message = new ServerMessage(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
            await message.AppendIntegerAsync(myUser.VirtualId);
            await message.AppendStringAsync(session.GetHabbo().Look);
            await message.AppendStringAsync(session.GetHabbo().Gender.ToLower());
            await message.AppendStringAsync(session.GetHabbo().Motto);
            await message.AppendIntegerAsync(session.GetHabbo().AchievementPoints);
            await room.SendMessage(message);

            return true;
        }
    }
}