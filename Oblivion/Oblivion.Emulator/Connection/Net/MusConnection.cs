﻿using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Oblivion.HabboHotel.Camera;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;
using Oblivion.Util;

namespace Oblivion.Connection.Net
{
    internal class MusConnection
    {
        private readonly Socket _conn;
        private readonly byte[] _dataBuffering = new byte[1024]; // 1024

        internal MusConnection(Socket conn)
        {
            _conn = conn;
            conn.BeginReceive(_dataBuffering, 0, _dataBuffering.Length, SocketFlags.None, RecieveData, _conn);
        }

        internal async void RecieveData(IAsyncResult iAr)
        {
            try
            {
                int bytes;
                try
                {
                    bytes = _conn.EndReceive(iAr);
                }
                catch
                {
                    MDisconnect();
                    return;
                }

                var data = Encoding.Default.GetString(_dataBuffering, 0, bytes);

                if (data.Length > 0)
                   await DArrival(data);
            }
            catch
            {
            }

            MDisconnect();
        }

        private async Task DArrival(string data)
        {
            try
            {
                var Params = data.Split(Convert.ToChar(1));
                var header = Oblivion.FilterInjectionChars(Params[0]);
                var param = Oblivion.FilterInjectionChars(Params[1]).Split(':');

                GameClient clientByUserId;
                uint userId;
                switch (header)
                {
                    #region FastFood

                    case "ff_progess_achievement":
                    {
                        userId = Convert.ToUInt32(param[0]);
                        string Type = param[1];
                        int Amount = Convert.ToInt32(param[2]);

                        clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(userId);

                        if (clientByUserId?.GetHabbo() == null)
                            break;

                        if (Amount == 0)
                            break;

                        await Oblivion.GetGame().GetAchievementManager()
                            .ProgressUserAchievement(clientByUserId, Type, Amount);
                        break;
                    }
                    case "ff_takecredits":
                    {
                        userId = Convert.ToUInt32(param[0]);
                        string Quantity = param[1];

                        clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(userId);

                        if (clientByUserId?.GetHabbo() == null)
                            break;

                        clientByUserId.GetHabbo().Credits -= int.Parse(Quantity);
                        await clientByUserId.GetHabbo().UpdateCreditsBalance();
                        break;
                    }

                    #endregion

                    case "ha":
                        var hotelAlert =
                            new ServerMessage(LibraryParser.OutgoingRequest("BroadcastNotifMessageComposer"));
                        await hotelAlert.AppendStringAsync($"{param}\r\n- Hotel Management");
                        await Oblivion.GetGame().GetClientManager().SendMessageAsync(hotelAlert);
                        break;

                    case "alert":
                        var pUserId = param[0];
                        var pMessage = param[1];

                        clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(uint.Parse(pUserId));
                        if (clientByUserId == null) return;
                        await clientByUserId.SendNotif(pMessage);
                        break;

                    case "kill":
                        clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserName(param[0]);
                        clientByUserId?.Disconnect("MUS Disconnection");
                        break;

                    case "add_preview":
                    {
                        var photoId = Convert.ToInt32(param[0]);
                        var cUserId = Convert.ToInt32(param[1]);
                        var createdAt = Convert.ToInt64(param[2]);
                        clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId((uint) cUserId);

                        if (clientByUserId?.GetHabbo() == null || clientByUserId.GetHabbo().CurrentRoomId < 1)
                            return;

                        Oblivion.GetGame()
                            .GetCameraManager()
                            .AddPreview(new CameraPhotoPreview(photoId, cUserId, createdAt));
                        break;
                    }

                    case "updatediamonds":
                    {
                        var UserId = uint.Parse(param[0]);
                        clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(UserId);

                        if (clientByUserId == null) return;

                        int diamonds;
                        using (var dbClient = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                        {
                            dbClient.SetQuery("SELECT diamonds FROM users WHERE id = " + UserId);
                            var row = dbClient.GetRow();
                            if (row == null) return;

                            diamonds = Convert.ToInt32(row["diamonds"]);
                        }

                        clientByUserId.GetHabbo().Diamonds = diamonds;
                        await clientByUserId.GetHabbo().UpdateActivityPointsBalance();
                        break;
                    }

                    case "giveemeralds":
                    {
                        var dUserId = uint.Parse(param[0]);
                        var amount = int.Parse(param[1]);
                        clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(dUserId);

                        if (clientByUserId == null) return;

                        int emeralds;
                        using (var dbClient = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                        {
                            await dbClient.RunFastQueryAsync($"UPDATE users SET vip_points = vip_points + {amount}");
                            dbClient.SetQuery("SELECT vip_points FROM users WHERE id = " + dUserId);
                            var row = dbClient.GetRow();
                            if (row == null) return;

                            emeralds = Convert.ToInt32(row["vip_points"]);
                        }

                        clientByUserId.GetHabbo().Graffiti = emeralds;
                        await clientByUserId.GetHabbo().UpdateActivityPointsBalance();
                        break;
                    }

                    case "updatemotto":
                        var UserID = uint.Parse(param[0]);
                        clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(UserID);

                        if (clientByUserId == null) return;

                        string motto;
                        using (var conn = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                        {
                            conn.SetQuery("SELECT motto FROM users WHERE id = " + UserID);
                            motto = conn.GetString();
                        }

                        clientByUserId.GetHabbo().Motto = motto;
                        if (clientByUserId.GetHabbo().InRoom)
                        {
                            var room = clientByUserId.GetHabbo().CurrentRoom;

                            var user = room?.GetRoomUserManager().GetRoomUserByHabbo(clientByUserId.GetHabbo().Id);
                            if (user == null) return;

                            var message =
                                new ServerMessage(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
                            await message.AppendIntegerAsync(user.VirtualId);
                            await message.AppendStringAsync(clientByUserId.GetHabbo().Look);
                            await message.AppendStringAsync(clientByUserId.GetHabbo().Gender.ToLower());
                            await message.AppendStringAsync(motto);
                            await message.AppendIntegerAsync(clientByUserId.GetHabbo().AchievementPoints);
                            await clientByUserId.SendMessage(message);
                        }

                        break;

                    case "addtoinventory":
                        userId = Convert.ToUInt32(param[0]);
                        var furniId = Convert.ToUInt32(param[1]);
                        //todo
                        clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(userId);
                        if (clientByUserId?.GetHabbo() == null ||
                            clientByUserId.GetHabbo().GetInventoryComponent() == null)
                            return;

                        await clientByUserId.GetHabbo().GetInventoryComponent().UpdateItems(true);
                        await clientByUserId.GetHabbo().GetInventoryComponent().SendNewItems(furniId);

                        break;

                    case "updatecredits":
                        userId = Convert.ToUInt32(param[0]);
                        var credits = Convert.ToInt32(param[1]);

                        clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(userId);
                        if (clientByUserId?.GetHabbo() != null)
                        {
                            clientByUserId.GetHabbo().Credits = credits;
                            await clientByUserId.GetHabbo().UpdateCreditsBalance();
                        }

                        return;

                    case "updatesubscription":
                        userId = Convert.ToUInt32(param[0]);

                        clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(userId);
                        if (clientByUserId?.GetHabbo() == null) return;
                        await clientByUserId.GetHabbo().GetSubscriptionManager().ReloadSubscription();
                        await clientByUserId.GetHabbo().SerializeClub();
                        break;
                    case "reload_bans":
                        using (var adapter3 = await Oblivion.GetDatabaseManager().GetQueryReactorAsync())
                        {
                            await Oblivion.GetGame().GetBanManager().LoadBans(adapter3);
                        }

                        break;
                    case "shutdown":
                    {
                        Oblivion.PerformShutDown();
                    }
                        break;
                    default:
                        return;
                    case "goto":
                    {
                        var muserId = Convert.ToUInt32(param[0]);

                        clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(muserId);
                        if (clientByUserId?.GetHabbo() == null)
                            break;

                        if (!uint.TryParse(param[1], out uint roomId))
                            break;

                        if (roomId == clientByUserId.GetHabbo().CurrentRoomId)
                            break;

                        var room = await Oblivion.GetGame().GetRoomManager().LoadRoom(roomId);
                        if (room == null)
                        {
                          await  clientByUserId.SendNotif("Failed to find the requested room!");
                        }
                        else
                        {
                            var roomFwd =
                                new ServerMessage(LibraryParser.OutgoingRequest("RoomForwardMessageComposer"));
                            await roomFwd.AppendIntegerAsync(clientByUserId.GetHabbo().CurrentRoom.RoomId);
                            await clientByUserId.SendMessage(roomFwd);
                        }
                    }
                        break;
                }

                Out.WriteLine("Parsed " + header + " MUS command");
            }
            catch
            {
            }
            finally
            {
                MDisconnect();
            }
        }

        private void MDisconnect()
        {
            try
            {
                _conn.Close();
            }
            catch
            {
            }
        }
    }
}