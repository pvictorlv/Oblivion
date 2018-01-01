using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Oblivion.HabboHotel.RoomBots
{
    /// <summary>
    ///     Class BotManager.
    /// </summary>
    internal class BotManager
    {

        //todo pet manager?
        /// <summary>
        ///     Generates the bot from row.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns>RoomBot.</returns>
        internal static RoomBot GenerateBotFromRow(DataRow row)
        {
            if (row == null)
                return null;

            var id = Convert.ToUInt32(row["id"]);

            List<string> speeches = null;
            if (!row.IsNull("speech") && !string.IsNullOrEmpty(row["speech"].ToString()))
                speeches = row["speech"].ToString().Split(';').ToList();

            var bot = new RoomBot(id, Convert.ToUInt32(row["user_id"]), AiType.Generic,
                row["is_bartender"].ToString() == "1");

            bot.Update(Convert.ToUInt32(row["room_id"]), (string) row["walk_mode"], (string) row["name"],
                (string) row["motto"],
                (string) row["look"],
                int.Parse(row["x"].ToString()), int.Parse(row["y"].ToString()), int.Parse(row["z"].ToString()), 4, 0, 0,
                0, 0, speeches, null, (string) row["gender"], (int) row["dance"], (int) row["speaking_interval"],
                Convert.ToInt32(row["automatic_chat"]) == 1, Convert.ToInt32(row["mix_phrases"]) == 1);

            return bot;
        }

       
    }
}