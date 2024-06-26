using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Oblivion.Database.Manager.Database.Session_Details.Interfaces;

namespace Oblivion.HabboHotel.Misc
{
    /// <summary>
    ///     Class LowPriorityWorker.
    /// </summary>
    internal class LowPriorityWorker
    {
        /// <summary>
        ///     The _user peak
        /// </summary>
        private static int _userPeak;

        private static bool _isExecuted;
        private static Stopwatch _lowPriorityStopWatch;

        /// <summary>
        ///     Initializes the specified database client.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal static void Init(IQueryAdapter dbClient)
        {
            dbClient.SetQuery("SELECT userpeak FROM server_status");
            _userPeak = dbClient.GetInteger();
            _lowPriorityStopWatch = new Stopwatch();
            _lowPriorityStopWatch.Start();
        }

        /// <summary>
        ///     Processes the specified caller.
        /// </summary>
        internal static async Task Process()
        {
            if (_lowPriorityStopWatch.ElapsedMilliseconds >= 60000 || !_isExecuted)
            {
                _isExecuted = true;
                _lowPriorityStopWatch.Restart();
                try
                {
                    var clientCount = Oblivion.GetGame().GetClientManager().ClientCount();
                    var loadedRoomsCount = Oblivion.GetGame().GetRoomManager().LoadedRoomsCount;
                    var dateTime = new DateTime((DateTime.Now - Oblivion.ServerStarted).Ticks);

                    Console.Title = string.Concat(
                        "OblivionEmulator v" + Oblivion.Version + "." + Oblivion.Build + " | TIME: ",
                        int.Parse(dateTime.ToString("dd")) - 1 + dateTime.ToString(":HH:mm:ss"), " | ONLINE COUNT: ",
                        clientCount, " | ROOM COUNT: ", loadedRoomsCount);
                    
                    using (var queryReactor = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                    {
                        _userPeak = clientCount;
                        await queryReactor.RunFastQueryAsync(string.Concat("UPDATE server_status SET stamp = '",
                            Oblivion.GetUnixTimeStamp(), "', users_online = ", clientCount, ", rooms_loaded = ",
                            loadedRoomsCount, ", server_ver = 'Oblivion Emulator', userpeak = ", _userPeak));
                    }
                    await Oblivion.GetGame().GetNavigator().LoadNewPublicRooms();
                }
                catch (Exception e)
                {
                    Writer.Writer.LogException(e.ToString());
                }
            }
        }
    }
}