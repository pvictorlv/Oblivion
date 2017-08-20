using System;
using System.Data;
using System.Net.Sockets;
using System.Text;
using MySql.Data.MySqlClient.Memcached;
using Oblivion.HabboHotel.Camera;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;
using Oblivion.Util;

namespace Oblivion.Connection.Net
{
    internal class MusConnection
    {
        private readonly Socket Conn;
        private readonly byte[] dataBuffering = new byte[1024]; // 1024

        internal MusConnection(Socket Conn)
        {
            this.Conn = Conn;
            Conn.BeginReceive(dataBuffering, 0, dataBuffering.Length, SocketFlags.None, RecieveData, this.Conn);
        }

        internal void RecieveData(IAsyncResult iAr)
        {
            try
            {
                var bytes = 0;
                try
                {
                    bytes = Conn.EndReceive(iAr);
                }
                catch
                {
                    mDisconnect();
                    return;
                }

                var data = Encoding.Default.GetString(dataBuffering, 0, bytes);

                if (data.Length > 0)
                    dArrival(data);
            }
            catch
            {
            }
            mDisconnect();
        }

        private void dArrival(string Data)
        {
            try
            {
                var Params = Data.Split(Convert.ToChar(1));
                var header = Oblivion.FilterInjectionChars(Params[0]);
                var param = Oblivion.FilterInjectionChars(Params[1]).Split(':');

                GameClient clientByUserId;
                uint userId;
                switch (header)
                {
                    case "ha":
                        var HotelAlert =
                            new ServerMessage(LibraryParser.OutgoingRequest("BroadcastNotifMessageComposer"));
                        HotelAlert.AppendString($"{param}\r\n- {"Hotel Management"}");
                        Oblivion.GetGame().GetClientManager().QueueBroadcaseMessage(HotelAlert);
                        break;

                    case "alert":
                        var pUserId = param[0];
                        var pMessage = param[1];

                        clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(uint.Parse(pUserId));
                        if (clientByUserId == null) return;
                        clientByUserId.SendNotif(pMessage);
                        break;

                    case "kill":
                        clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(uint.Parse(param[0]));
                        clientByUserId?.Disconnect("MUS Disconnection");
                        break;

                    case "add_preview":
                    {
                        var PhotoId = Convert.ToInt32(param[0]);
                        var cUserId = Convert.ToInt32(param[1]);
                        var CreatedAt = Convert.ToInt64(param[2]);
                        clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId((uint) cUserId);

                        if (clientByUserId?.GetHabbo() == null || clientByUserId.GetHabbo().CurrentRoomId < 1)
                        {
                            return;
                        }

                        Oblivion.GetGame()
                            .GetCameraManager()
                            .AddPreview(new CameraPhotoPreview(PhotoId, cUserId, CreatedAt));
                        break;
                    }

                    case "updatediamonds":
                        var UserId = uint.Parse(param[0]);
                        clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(UserId);

                        if (clientByUserId == null) return;

                        int diamonds;
                        using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
                        {
                            DataRow row;
                            dbClient.SetQuery("SELECT diamonds FROM users WHERE id = " + UserId);
                            row = dbClient.GetRow();
                            if (row == null) return;

                            diamonds = Convert.ToInt32(row["diamonds"]);
                        }
                        clientByUserId.GetHabbo().Diamonds = diamonds;
                        clientByUserId.GetHabbo().UpdateActivityPointsBalance();
                        break;

                    case "updatemotto":
                        var UserID = uint.Parse(param[0]);
                        clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(UserID);

                        if (clientByUserId == null) return;

                        string motto;
                        using (var conn = Oblivion.GetDatabaseManager().GetQueryReactor())
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
                            message.AppendInteger(user.VirtualId);
                            message.AppendString(clientByUserId.GetHabbo().Look);
                            message.AppendString(clientByUserId.GetHabbo().Gender.ToLower());
                            message.AppendString(motto);
                            message.AppendInteger(clientByUserId.GetHabbo().AchievementPoints);
                            clientByUserId.SendMessage(message);
                        }
                        break;

                    case "addtoinventory":
                        userId = Convert.ToUInt32(param[0]);
                        var furniId = Convert.ToInt32(param[1]);

                        clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(userId);
                        if (clientByUserId == null || clientByUserId.GetHabbo() == null ||
                            clientByUserId.GetHabbo().GetInventoryComponent() == null)
                            return;

                        clientByUserId.GetHabbo().GetInventoryComponent().UpdateItems(true);
                        clientByUserId.GetHabbo().GetInventoryComponent().SendNewItems((uint) furniId);

                        break;

                    case "updatecredits":
                        userId = Convert.ToUInt32(param[0]);
                        var credits = Convert.ToInt32(param[1]);

                        clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(userId);
                        if (clientByUserId != null && clientByUserId.GetHabbo() != null)
                        {
                            clientByUserId.GetHabbo().Credits = credits;
                            clientByUserId.GetHabbo().UpdateCreditsBalance();
                        }
                        return;

                    case "updatesubscription":
                        userId = Convert.ToUInt32(param[0]);

                        clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(userId);
                        if (clientByUserId?.GetHabbo() == null) return;
                        clientByUserId.GetHabbo().GetSubscriptionManager().ReloadSubscription();
                        clientByUserId.GetHabbo().SerializeClub();
                        break;
                    case "reload_bans":
                        
                        break;
                    default:
                        return;
                    case "goto":
                    {
                        var muserId = Convert.ToUInt32(Params[0]);
                        var roomStr = Params[1];

                        clientByUserId = Oblivion.GetGame().GetClientManager().GetClientByUserId(muserId);
                        if (clientByUserId?.GetHabbo() == null)
                            break;

                        if (!uint.TryParse(Params[1], out uint roomId))
                            break;

                        if (roomId == clientByUserId.GetHabbo().CurrentRoomId)
                            break;

                        var room = Oblivion.GetGame().GetRoomManager().LoadRoom(roomId);
                        if (room == null)
                        {
                            clientByUserId.SendNotif("Failed to find the requested room!");
                        }
                        else
                        {
                            var roomFwd =
                                new ServerMessage(LibraryParser.OutgoingRequest("RoomForwardMessageComposer"));
                            roomFwd.AppendInteger(clientByUserId.GetHabbo().CurrentRoom.RoomId);
                            clientByUserId.SendMessage(roomFwd);
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
                mDisconnect();
            }
        }

        private void mDisconnect()
        {
            try
            {
                Conn.Close();
            }
            catch
            {
            }
        }
    }
}