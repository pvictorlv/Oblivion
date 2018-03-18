using System;
using System.Linq;
using System.Threading;
using Oblivion.Configuration;

namespace Oblivion.Manager
{
    public static class Cache
    {
        private static Thread _thread;
        public static bool Working;

        public static void StartProcess()
        {
            _thread = new Thread(Process)
            {
                Name = "Cache Thread",
                IsBackground = true
            };
            _thread.Start();
            Working = true;
        }

        public static void StopProcess()
        {
            _thread.Abort(); //todo: use timer
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
                    Thread.Sleep(900000);
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
            foreach (var user in Oblivion.UsersCached.ToArray())
            {
                if (user.Value == null)
                {
                    RemoveUser(user.Key);
                    continue;
                }

                if (Oblivion.GetGame().GetClientManager().Clients.ContainsKey(user.Key))
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
                removedUser?.RemoveCached();
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
                        Oblivion.GetGame().GetRoomManager().LoadedRoomData.TryRemove(roomData.Id, out var unloadedRoom);
                        unloadedRoom.Dispose();
                    }
                }
            }
        }
    }
}