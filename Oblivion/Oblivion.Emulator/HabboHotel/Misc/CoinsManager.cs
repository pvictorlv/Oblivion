using System;
using System.Threading.Tasks;
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
        internal async Task StartTimer()
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
        internal async void GiveCoins(object source, ElapsedEventArgs e)
        {
            try
            {
                var clients = Oblivion.GetGame().GetClientManager().Clients.Values;
                foreach (var client in clients)
                {
                    if (client?.GetHabbo() == null) continue;
                    client.GetHabbo().Credits += ExtraSettings.CreditsToGive;
                    await client.GetHabbo().UpdateCreditsBalance();
                    client.GetHabbo().ActivityPoints += ExtraSettings.PixelsToGive;
                    if (ExtraSettings.DiamondsLoopEnabled)
                        if (ExtraSettings.DiamondsVipOnly)
                        {
                            if (client.GetHabbo().Vip || client.GetHabbo().Rank >= 6)
                                client.GetHabbo().Diamonds += ExtraSettings.DiamondsToGive * 2;
                        }
                        else client.GetHabbo().Diamonds += (client.GetHabbo().Vip) ? ExtraSettings.DiamondsToGive * 2 : ExtraSettings.DiamondsToGive;
                    await client.GetHabbo().UpdateSeasonalCurrencyBalance();
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