using System;
using System.Collections.Generic;
using System.Linq;
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
            var toRemove = new List<uint>();

            foreach (var user in Oblivion.UsersCached)
            {
                if (user.Value == null)
                {
                    toRemove.Add(user.Key);
                    return;
                }

                if (Oblivion.GetGame().GetClientManager().Clients.ContainsKey(user.Key))
                    continue;

                if ((DateTime.Now - user.Value.LastUsed).TotalMilliseconds < 1800000)
                    continue;

                toRemove.Add(user.Key);
            }

            foreach (var userId in toRemove)
            {
                Oblivion.UsersCached.TryRemove(userId, out _);
            }
        }

        private static void ClearRoomsCache()
        {
            if (Oblivion.GetGame() == null || Oblivion.GetGame().GetRoomManager() == null || Oblivion.GetGame().GetRoomManager().LoadedRoomData == null)
                return;

            var toRemove = (from roomData in Oblivion.GetGame().GetRoomManager().LoadedRoomData where roomData.Value != null && roomData.Value.UsersNow <= 0 where !((DateTime.Now - roomData.Value.LastUsed).TotalMilliseconds < 1800000) select roomData.Key).ToList();

            foreach (var roomId in toRemove)
            {
                Oblivion.GetGame().GetRoomManager().LoadedRoomData.TryRemove(roomId, out _);
            }
        }
    }
}