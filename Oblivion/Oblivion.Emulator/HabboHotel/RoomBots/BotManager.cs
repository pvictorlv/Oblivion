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
        /// <summary>
        ///     The _bots
        /// </summary>
        private readonly List<RoomBot> _bots;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BotManager" /> class.
        /// </summary>
        internal BotManager()
        {
            _bots = new List<RoomBot>();
        }

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

        /// <summary>
        ///     Gets the bots for room.
        /// </summary>
        /// <param name="roomId">The room identifier.</param>
        /// <returns>List&lt;RoomBot&gt;.</returns>
        internal List<RoomBot> GetBotsForRoom(uint roomId)
        {
            return new List<RoomBot>(
                from p in _bots
                where p.RoomId == roomId
                select p);
        }

        /// <summary>
        ///     Gets the bot.
        /// </summary>
        /// <param name="botId">The bot identifier.</param>
        /// <returns>RoomBot.</returns>
        internal RoomBot GetBot(uint botId)
        {
            return (
                from p in _bots
                where p.BotId == botId
                select p).FirstOrDefault();
        }
    }
}