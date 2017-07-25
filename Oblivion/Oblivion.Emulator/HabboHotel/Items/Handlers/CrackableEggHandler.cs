using System;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.Messages;

namespace Oblivion.HabboHotel.Items.Handlers
{
    /// <summary>
    ///     Class CrackableEggHandler.
    /// </summary>
    internal class CrackableEggHandler
    {
        /// <summary>
        ///     Initializes the specified database client.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal void Initialize(IQueryAdapter dbClient)
        {
        }

        internal int MaxCracks(string itemName)
        {
            switch (itemName)
            {
                case "easter13_egg_0":
                    return 1000;

                case "easter13_egg_1":
                    return 5000;

                case "easter13_egg_2":
                    return 10000;

                case "easter13_egg_3":
                    return 20000;

                default:
                    return 1;
            }
        }

        internal ServerMessage GetServerMessage(ServerMessage message, RoomItem item)
        {
            var cracks = 0;
            var cracksMax = MaxCracks(item.GetBaseItem().Name);

            if (Oblivion.IsNum(item.ExtraData))
                cracks = Convert.ToInt16(item.ExtraData);

            var state = "0";

            if (cracks >= cracksMax)
                state = "14";
            else if (cracks >= cracksMax * 6 / 7)
                state = "12";
            else if (cracks >= cracksMax * 5 / 7)
                state = "10";
            else if (cracks >= cracksMax * 4 / 7)
                state = "8";
            else if (cracks >= cracksMax * 3 / 7)
                state = "6";
            else if (cracks >= cracksMax * 2 / 7)
                state = "4";
            else if (cracks >= cracksMax * 1 / 7)
                state = "2";

            message.AppendInteger(7);
            message.AppendString(state); //state (0-7)
            message.AppendInteger(cracks); //actual
            message.AppendInteger(cracksMax); //max

            return message;
        }
    }
}