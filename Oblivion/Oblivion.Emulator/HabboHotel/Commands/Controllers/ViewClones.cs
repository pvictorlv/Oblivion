using System.Data;
using System.Text;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    internal sealed class ViewClones : Command
    {
        public ViewClones()
        {
            MinRank = 7;
            Description = "View user clones";
            Usage = ":viewclones [user]";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            DataTable data;
            var name = pms[0];
            var user = Oblivion.GetGame().GetClientManager().GetClientByUserName(name);
            if (user?.GetConnection() == null) return false;
            var col = Oblivion.GetDbConfig().DbData["emerald.column"];

            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery($"SELECT username,mail,online,{col},rank FROM users WHERE ip_last = @ip OR ip_reg = @ip");
                dbClient.AddParameter("ip", user.GetConnection().GetIp());
                data = dbClient.GetTable();
            }

            var builder = new StringBuilder();

            foreach (DataRow row in data.Rows)
            {
                builder.Append("Name: " + row["username"] + "\r");
                builder.Append("Mail: " + row["mail"] + "\r");
                builder.Append("Diamonds: " + row[col] + "\r");
                builder.Append("Rank: " + row["rank"] + "\r");
                builder.Append("Online: " + Oblivion.EnumToBool(row["online"].ToString() == "1" ? "Yes" : "No") + "\r\r");
                builder.AppendLine("------------------");
            }
            session.SendNotifWithScroll(builder.ToString());
            return true;
        }
    }
}