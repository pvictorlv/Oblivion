using System;
using System.Data;

namespace Oblivion.HabboHotel.Users.Inventory
{
    /// <summary>
    ///     Class UserPreferences.
    /// </summary>
    internal class UserPreferences
    {
        /// <summary>
        ///     The _user identifier
        /// </summary>
        private readonly uint _userId;

        internal int ChatColor;
        internal bool DisableCameraFollow;
        internal bool IgnoreRoomInvite;
        internal int NewnaviHeight = 600;
        internal int NewnaviWidth = 580;
        internal int NewnaviX;
        internal int NewnaviY;

        internal bool PreferOldChat;
        internal string Volume = "100,100,100";

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserPreferences" /> class.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        internal UserPreferences(uint userId)
        {
            _userId = userId;

            DataRow row;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT prefer_old_chat,ignore_room_invite,disable_camera_follow,volume,newnavi_x,newnavi_y,newnavi_width,newnavi_height,chat_color FROM users_preferences WHERE userid = " + _userId);
                queryReactor.AddParameter("userid", _userId);
                row = queryReactor.GetRow();

                if (row == null)
                {
                    queryReactor.RunFastQuery("REPLACE INTO users_preferences (userid, volume) VALUES (" + _userId +
                                              ", '100,100,100')");
                    return;
                }
            }

            PreferOldChat = Oblivion.EnumToBool((string) row["prefer_old_chat"]);
            IgnoreRoomInvite = Oblivion.EnumToBool((string) row["ignore_room_invite"]);
            DisableCameraFollow = Oblivion.EnumToBool((string) row["disable_camera_follow"]);
            Volume = (string) row["volume"];
            NewnaviX = Convert.ToInt32(row["newnavi_x"]);
            NewnaviY = Convert.ToInt32(row["newnavi_y"]);
            NewnaviWidth = Convert.ToInt32(row["newnavi_width"]);
            NewnaviHeight = Convert.ToInt32(row["newnavi_height"]);
            ChatColor = Convert.ToInt32(row["chat_color"]);
        }

        internal async Task Save()
        {
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(
                    "UPDATE users_preferences SET volume = @volume, prefer_old_chat = @prefer_old_chat, ignore_room_invite = @ignore_room_invite, newnavi_x = @newnavi_x, newnavi_y = @newnavi_y, newnavi_width = @newnavi_width, newnavi_height = @newnavi_height, disable_camera_follow = @disable_camera_follow, chat_color = @chat_color WHERE userid = @userid");
                queryReactor.AddParameter("userid", _userId);
                queryReactor.AddParameter("prefer_old_chat", Oblivion.BoolToEnum(PreferOldChat));
                queryReactor.AddParameter("ignore_room_invite", Oblivion.BoolToEnum(IgnoreRoomInvite));
                queryReactor.AddParameter("volume", Volume);
                queryReactor.AddParameter("newnavi_x", NewnaviX);
                queryReactor.AddParameter("newnavi_y", NewnaviY);
                queryReactor.AddParameter("newnavi_width", NewnaviWidth);
                queryReactor.AddParameter("newnavi_height", NewnaviHeight);
                queryReactor.AddParameter("disable_camera_follow", Oblivion.BoolToEnum(DisableCameraFollow));
                queryReactor.AddParameter("chat_color", ChatColor);
                await queryReactor.RunQueryAsync();
            }
        }
    }
}