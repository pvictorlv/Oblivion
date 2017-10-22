using System;
using System.Linq;
using System.Timers;
using Oblivion.Configuration;

namespace Oblivion.HabboHotel.Misc
{
    /// <summary>
    ///     Class CoinsManager.
    /// </summary>
    internal class CoinsManager
    {
        /// <summary>
        ///     The _timer
        /// </summary>
        private static Timer _timer;

        /// <summary>
        ///     Starts the timer.
        /// </summary>
        internal void StartTimer()
        {
            if (!ExtraSettings.CurrencyLoopEnabled)
                return;
            _timer = new Timer(ExtraSettings.CurrentyLoopTimeInMinutes*60000);
            _timer.Elapsed += GiveCoins;
            _timer.Enabled = true;
        }

        /// <summary>
        ///     Gives the coins.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs" /> instance containing the event data.</param>
        internal void GiveCoins(object source, ElapsedEventArgs e)
        {
            try
            {
                var clients = Oblivion.GetGame().GetClientManager().Clients.Values;
                /* TODO CHECK */ foreach (
                    var client in clients.Where(client => client != null && client.GetHabbo() != null))
                {
                    client.GetHabbo().Credits += ExtraSettings.CreditsToGive;
                    client.GetHabbo().UpdateCreditsBalance();
                    client.GetHabbo().ActivityPoints += ExtraSettings.PixelsToGive;
                    if (ExtraSettings.DiamondsLoopEnabled)
                        if (ExtraSettings.DiamondsVipOnly)
                            if (client.GetHabbo().Vip || client.GetHabbo().Rank >= 6)
                                client.GetHabbo().Diamonds += ExtraSettings.DiamondsToGive;
                            else client.GetHabbo().Diamonds += ExtraSettings.DiamondsToGive;
                    client.GetHabbo().UpdateSeasonalCurrencyBalance();
                }
            }
            catch (Exception ex)
            {
                Writer.Writer.LogException(ex.ToString());
            }
        }

        /// <summary>
        ///     Destroys this instance.
        /// </summary>
        internal void Destroy()
        {
            _timer.Dispose();
            _timer = null;
        }
    }
}