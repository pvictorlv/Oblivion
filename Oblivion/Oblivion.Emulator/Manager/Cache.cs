using System;
using System.Threading.Tasks;
using Oblivion.Configuration;

namespace Oblivion.Manager
{
    public static class Cache
    {
        private static Task _thread;
        public static bool Working;
        public static void StartProcess()
        {
            _thread = new Task(Process);
            _thread.Start();
            Working = true;
        }

        public static void StopProcess()
        {
            _thread.Dispose();//todo: use timer
            Working = false;
        }

        private static async void Process()
        {
            try
            {
                while (Working)
                {
                    ClearUserCache();
                    ClearRoomsCache();

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    await Task.Delay(900000);
                }
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "cache.cs");
            }
        }

        private static void ClearUserCache()
        {

            foreach (var user in Oblivion.UsersCached.ToArray())
            {
                if (user.Value == null)
                {
                    Oblivion.UsersCached.TryRemove(user.Key, out _);
                    return;
                }

                if (Oblivion.GetGame().GetClientManager().Clients.ContainsKey(user.Key))
                    continue;

                if ((DateTime.Now - user.Value.LastUsed).TotalMilliseconds < 1800000)
                    continue;

                Oblivion.UsersCached.TryRemove(user.Key, out _);
            }
        }

        private static void ClearRoomsCache()
        {
            if (Oblivion.GetGame() == null || Oblivion.GetGame().GetRoomManager() == null || Oblivion.GetGame().GetRoomManager().LoadedRoomData == null)
                return;

            foreach (var roomData in Oblivion.GetGame().GetRoomManager().LoadedRoomData.ToArray())
            {
                if (roomData.Value != null && roomData.Value.UsersNow <= 0)
                {
                    if (!((DateTime.Now - roomData.Value.LastUsed).TotalMilliseconds < 1800000))
                        Oblivion.GetGame().GetRoomManager().LoadedRoomData.TryRemove(roomData.Key, out _);
                }
            }
            
        }
    }
}