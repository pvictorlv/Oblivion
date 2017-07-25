using System;
using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class RedeemCredits.
    /// </summary>
    internal sealed class RedeemCredits : Command
    {
        public RedeemCredits()
        {
            MinRank = 1;
            Description = "Redeems all Goldbars in your inventory to Credits.";
            Usage = ":redeemcredits";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            try
            {
                session.GetHabbo().GetInventoryComponent().Redeemcredits(session);
                session.SendNotif(Oblivion.GetLanguage().GetVar("command_redeem_credits"));
            }
            catch (Exception e)
            {
                Writer.Writer.LogException(e.ToString());
                session.SendNotif(Oblivion.GetLanguage().GetVar("command_redeem_credits"));
            }
            return true;
        }
    }
}