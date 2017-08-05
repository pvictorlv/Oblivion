using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class RefreshNavigator. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshNavigator : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshNavigator" /> class.
        /// </summary>
        public RefreshNavigator()
        {
            MinRank = 9;
            Description = "Refreshes navigator from Database.";
            Usage = ":refresh_navigator";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            using (var adapter = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                Oblivion.GetGame().GetNavigator().Initialize(adapter);
            }
            session.SendNotif(Oblivion.GetLanguage().GetVar("command_refresh_navigator"));
            return true;
        }
    }
}