using System.Text;
using System.Threading.Tasks;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class UserInfo. This class cannot be inherited.
    /// </summary>
    internal sealed class UserInfo : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UserInfo" /> class.
        /// </summary>
        public UserInfo()
        {
            MinRank = 5;
            Description = "Tells you information of the typed username.";
            Usage = ":userinfo [USERNAME]";
            MinParams = 1;
        }

        public override async Task<bool> Execute(GameClient session, string[] pms)
        {
            var userName = pms[0];
            if (string.IsNullOrEmpty(userName)) return true;
            var clientByUserName = Oblivion.GetGame().GetClientManager().GetClientByUserName(userName);
            if (clientByUserName?.GetHabbo() == null)
            {
                using (var adapter = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    adapter.SetQuery(
                        "SELECT username, rank, id, credits, activity_points, diamonds, email FROM users WHERE username=@user LIMIT 1");
                    adapter.AddParameter("user", userName);
                    var row = adapter.GetRow();

                    if (row == null)
                    {
                         await session.SendWhisperAsync(Oblivion.GetLanguage().GetVar("user_not_found"));
                        return true;
                    }
                    await session.SendNotif(string.Format((Oblivion.GetLanguage().GetVar("user_info_all")), userName, row["id"],
                        row["rank"], row["credits"], row["activity_points"], row["diamonds"], row["email"]));
                }
                return true;
            }
            using (var adapter = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                adapter.SetQuery(
                    "SELECT email FROM users WHERE username=@user LIMIT 1");
                adapter.AddParameter("user", userName);
                var row = adapter.GetRow();

                if (row == null)
                {
                     await session.SendWhisperAsync(Oblivion.GetLanguage().GetVar("user_not_found"));
                    return true;
                }

                var habbo = clientByUserName.GetHabbo();
                var builder = new StringBuilder();
                if (habbo.CurrentRoom != null)
                {
                    builder.AppendFormat(" - ROOM INFORMATION [{0}] - \r", habbo.CurrentRoom.RoomId);
                    builder.AppendFormat("Owner: {0}\r", habbo.CurrentRoom.RoomData.Owner);
                    builder.AppendFormat("Room Name: {0}\r", habbo.CurrentRoom.RoomData.Name);
                    builder.Append(
                        string.Concat("Current Users: ", habbo.CurrentRoom.UserCount, "/",
                            habbo.CurrentRoom.RoomData.UsersMax));
                }
                await session.SendNotif(string.Concat("User info for: ", userName, " \rUser ID: ", habbo.Id, "\rEmail: ",
                    row[0], ":\rRank: ",
                    habbo.Rank, "\rCurrentTalentLevel: ", habbo.CurrentTalentLevel, " \rCurrent Room: ",
                    habbo.CurrentRoomId,
                    " \rCredits: ", habbo.Credits, "\rDuckets: ", habbo.ActivityPoints, "\rDiamonds: ", habbo.Diamonds,
                    "\rMuted: ", habbo.Muted.ToString(), "\r\r\r", builder.ToString()));

                return true;
            }
        }
    }
}