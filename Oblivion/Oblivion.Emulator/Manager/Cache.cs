using System;
using System.Linq;
using System.Threading;
using Oblivion.Configuration;

namespace Oblivion.Manager
{
    public static class Cache
    {
        private static Timer _thread;
        public static bool Working;

        public static void StartProcess()
        {
            _thread = new Timer(_ => Process(), null, 0, 900000);
            Working = true;
        }

        public static void StopProcess()
        {
            _thread.Dispose();
            Working = false;
        }

        private static void Process()
        {
            try
            {
                while (Working)
                {
                    ClearUserCache();
                    ClearRoomsCache();

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    //todo remove this long task.
                }
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "cache.cs");
            }
        }

        private static void ClearUserCache()
        {
            foreach (var user in Oblivion.UsersCached)
            {
                if (user.Value?.GetClient() == null)
                {
                    RemoveUser(user.Key);
                    continue;
                }

                if (Oblivion.GetGame().GetClientManager().Clients.ContainsKey(user.Value.GetClient().ConnectionId))
                {
                    RemoveUser(user.Key);
                    continue;
                }

                if ((DateTime.Now - user.Value.LastUsed).TotalMilliseconds < 1800000)
                    continue;

                RemoveUser(user.Key);
            }
        }

        private static void RemoveUser(uint user)
        {
            if (Oblivion.UsersCached.TryRemove(user, out var removedUser))
            {
                removedUser?.Dispose();
            }
        }

        private static void ClearRoomsCache()
        {
            if (Oblivion.GetGame() == null || Oblivion.GetGame().GetRoomManager() == null ||
                Oblivion.GetGame().GetRoomManager().LoadedRoomData == null)
                return;

            foreach (var roomData in Oblivion.GetGame().GetRoomManager().LoadedRoomData.Values.ToList())
            {
                if (roomData != null && roomData.UsersNow <= 0)
                {
                    if (((DateTime.Now - roomData.LastUsed).TotalMilliseconds >= 1800000))
                    {
                        if (Oblivion.GetGame().GetRoomManager().LoadedRoomData
                            .TryRemove(roomData.Id, out var unloadedRoom))
                            unloadedRoom.Dispose();
                    }
                }
            }
        }
    }
}