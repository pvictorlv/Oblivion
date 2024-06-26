﻿using System.Text;
using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class About. This class cannot be inherited.
    /// </summary>
    internal sealed class About : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="About" /> class.
        /// </summary>
        public About()
        {
            MinRank = 1;
            Description = "Shows information about the server.";
            Usage = ":about";
            MinParams = 0;
            BlockBad = false;
        }

        public override async Task<bool> Execute(GameClient client, string[] pms)
        {
            var message =
                new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));

            await message.AppendStringAsync("");
            await message.AppendIntegerAsync(4);
            await message.AppendStringAsync("title");
            await message.AppendStringAsync("Oblivion Emulator v2");
            await message.AppendStringAsync("message");
            var info = new StringBuilder();
            info.Append("Oblivion Emulator v2 - A new world!\n\r\r");
            info.AppendFormat("Developed by Oblivion Team \n\r\r");
            info.AppendFormat("Credits to Dark, Droppy, Claudio, XDR, Maritnemenite and lots of cool ppl :p");
            info.AppendFormat("\n\r\r");
            info.AppendFormat("Stats:\r");
            var userCount = Oblivion.GetGame().GetClientManager().Clients.Count * Oblivion.Multipy;
            var roomsCount = Oblivion.GetGame().GetRoomManager().LoadedRooms.Count;
            info.Append($"{userCount} Users in {roomsCount}{((roomsCount == 1) ? " room" : " rooms")}.\n\r\r");
            await message.AppendStringAsync(info.ToString());
            await message.AppendStringAsync("linkUrl");
            await message.AppendStringAsync("event:");
            await message.AppendStringAsync("linkTitle");
            await message.AppendStringAsync("ok");
            await client.SendMessage(message);

            return true;
        }
    }
}