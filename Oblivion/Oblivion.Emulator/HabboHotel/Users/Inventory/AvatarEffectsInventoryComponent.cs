using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Rooms;
using Oblivion.Messages;
using Oblivion.Messages.Parsers;

namespace Oblivion.HabboHotel.Users.Inventory
{
    /// <summary>
    ///     Class AvatarEffectsInventoryComponent.
    /// </summary>
    internal class AvatarEffectsInventoryComponent
    {
        /// <summary>
        ///     The _user identifier
        /// </summary>
        private readonly uint _userId;

        /// <summary>
        ///     The _effects
        /// </summary>
        private List<AvatarEffect> _effects;

        /// <summary>
        ///     The _session
        /// </summary>
        private GameClient _session;

        /// <summary>
        ///     The current effect
        /// </summary>
        internal int CurrentEffect;


        /// <summary>
        ///     Initializes a new instance of the <see cref="AvatarEffectsInventoryComponent" /> class.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="client">The client.</param>
        internal AvatarEffectsInventoryComponent(uint userId, GameClient client)
        {
            _userId = userId;
            _session = client;
            _effects = new List<AvatarEffect>();

            using (var dbClient = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    $"SELECT effect_id,total_duration,is_activated,activated_stamp,type FROM users_effects WHERE user_id = {userId}");
                var effectsTable = dbClient.GetTable();


                foreach (var current in from DataRow row in effectsTable.Rows let effectId = (int) row["effect_id"] let totalDuration = (int) row["total_duration"] let activated = Oblivion.EnumToBool((string) row["is_activated"]) let activateTimestamp = (double) row["activated_stamp"] let type = Convert.ToInt16(row["type"]) select new AvatarEffect(effectId, totalDuration, activated, activateTimestamp, type))
                {
                    if (!current.HasExpired)
                        _effects.Add(current);
                    else
                        dbClient.RunFastQuery("DELETE FROM users_effects WHERE user_id = " + userId +
                                              " AND effect_id = " + current.EffectId + "; ");
                }
            }
        }

        /// <summary>
        ///     Gets the packet.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage GetPacket()
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("EffectsInventoryMessageComposer"));
            serverMessage.AppendInteger(_effects.Count);

            /* TODO CHECK */
            foreach (var current in _effects)
            {
                serverMessage.AppendInteger(current.EffectId);
                serverMessage.AppendInteger(current.Type); // type (0 : normal - 1 : costume)
                serverMessage.AppendInteger(current.TotalDuration);
                serverMessage.AppendInteger(0); // count (0 : 1 - 1 : 2 ...)
                serverMessage.AppendInteger(current.TimeLeft);
                serverMessage.AppendBool(current.TotalDuration == -1); // permanent
            }

            return serverMessage;
        }

        /// <summary>
        ///     Adds the new effect.
        /// </summary>
        /// <param name="effectId">The effect identifier.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="type">The type.</param>
        internal async Task AddNewEffect(int effectId, int duration, short type)
        {
            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                await queryReactor.RunFastQueryAsync(
                    string.Concat(
                        "INSERT INTO users_effects (user_id,effect_id,total_duration,is_activated,activated_stamp) VALUES (",
                        _userId, ",", effectId, ",", duration, ",'0',0)"));

            _effects.Add(new AvatarEffect(effectId, duration, false, 0.0, type));
            await GetClient()
                .GetMessageHandler()
                .GetResponse()
                .InitAsync(LibraryParser.OutgoingRequest("AddEffectToInventoryMessageComposer"));
            await GetClient().GetMessageHandler().GetResponse().AppendIntegerAsync(effectId);
            await GetClient().GetMessageHandler().GetResponse().AppendIntegerAsync(type);
            await GetClient().GetMessageHandler().GetResponse().AppendIntegerAsync(duration);
            GetClient().GetMessageHandler().GetResponse().AppendBool(duration == -1);
            await GetClient().GetMessageHandler().SendResponse();
        }

        /// <summary>
        ///     Determines whether the specified effect identifier has effect.
        /// </summary>
        /// <param name="effectId">The effect identifier.</param>
        /// <returns><c>true</c> if the specified effect identifier has effect; otherwise, <c>false</c>.</returns>
        internal bool HasEffect(int effectId) => effectId < 1 || (
                                                     from x in _effects
                                                     where x.EffectId == effectId
                                                     select x).Any();

        /// <summary>
        ///     Activates the effect.
        /// </summary>
        /// <param name="effectId">The effect identifier.</param>
        internal async Task ActivateEffect(int effectId)
        {
            if (!_session.GetHabbo().InRoom)
                return;

            if (!HasEffect(effectId))
                return;

            if (effectId < 1)
            {
                await ActivateCustomEffect(effectId);
                return;
            }

            var avatarEffect = (
                from x in _effects
                where x.EffectId == effectId
                select x).Last();

            avatarEffect.Activate();

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                await queryReactor.RunFastQueryAsync(
                    string.Concat("UPDATE users_effects SET is_activated = '1', activated_stamp = ",
                        Oblivion.GetUnixTimeStamp(), " WHERE user_id = ", _userId, " AND effect_id = ", effectId));

            await EnableInRoom(effectId);
        }

        /// <summary>
        ///     Activates the custom effect.
        /// </summary>
        /// <param name="effectId">The effect identifier.</param>
        /// <param name="setAsCurrentEffect">if set to <c>true</c> [set as current effect].</param>
        internal async Task ActivateCustomEffect(int effectId, bool setAsCurrentEffect = true)
        {
            await EnableInRoom(effectId, setAsCurrentEffect);
        }

        /// <summary>
        ///     Called when [room exit].
        /// </summary>
        internal async Task OnRoomExit()
        {
            CurrentEffect = 0;
        }

        /// <summary>
        ///     Checks the expired.
        /// </summary>
        internal async Task CheckExpired()
        {
            if (!_effects.Any())
                return;
            var list = _effects.Where(current => current.HasExpired).ToList();
            /* TODO CHECK */
            foreach (var current2 in list)
                await StopEffect(current2.EffectId);
            list.Clear();
        }

        /// <summary>
        ///     Stops the effect.
        /// </summary>
        /// <param name="effectId">The effect identifier.</param>
        internal async Task StopEffect(int effectId)
        {
            var avatarEffect = new List<AvatarEffect>();
            foreach (AvatarEffect x in _effects)
            {
                if (x.EffectId == effectId) 
                    avatarEffect.Add(x);
            }

            if (!avatarEffect.Any())
                return;

            var effect = avatarEffect.Last();

            if (effect == null || !effect.HasExpired)
                return;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                await queryReactor.RunFastQueryAsync(string.Concat("DELETE FROM users_effects WHERE user_id = ", _userId,
                    " AND effect_id = ", effectId, " AND is_activated = 1"));

            _effects.Remove(effect);

            await GetClient()
                .GetMessageHandler()
                .GetResponse()
                .InitAsync(LibraryParser.OutgoingRequest("StopAvatarEffectMessageComposer"));

            await GetClient().GetMessageHandler().GetResponse().AppendIntegerAsync(effectId);
            await GetClient().GetMessageHandler().SendResponse();

            if (CurrentEffect >= 0)
                await ActivateCustomEffect(-1);
        }

        /// <summary>
        ///     Disposes this instance.
        /// </summary>
        internal async Task Dispose()
        {
            _effects.Clear();
            _effects = null;
            _session = null;
        }

        /// <summary>
        ///     Gets the client.
        /// </summary>
        /// <returns>GameClient.</returns>
        internal GameClient GetClient()
        {
            return _session;
        }

        /// <summary>
        ///     Enables the in room.
        /// </summary>
        /// <param name="effectId">The effect identifier.</param>
        /// <param name="setAsCurrentEffect">if set to <c>true</c> [set as current effect].</param>
        private async Task EnableInRoom(int effectId, bool setAsCurrentEffect = true)
        {
            var userRoom = GetUserRoom();

            var roomUserByHabbo = userRoom?.GetRoomUserManager().GetRoomUserByHabbo(GetClient().GetHabbo().Id);

            if (roomUserByHabbo == null)
                return;

            if (setAsCurrentEffect)
                CurrentEffect = effectId;

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("ApplyEffectMessageComposer"));
            await serverMessage.AppendIntegerAsync(roomUserByHabbo.VirtualId);
            await serverMessage.AppendIntegerAsync(effectId);
            await serverMessage.AppendIntegerAsync(0);
            await userRoom.SendMessageAsync(serverMessage);
        }
        /// <summary>
        ///     Enables the in room.
        /// </summary>
        /// <param name="effectId">The effect identifier.</param>
        /// <param name="setAsCurrentEffect">if set to <c>true</c> [set as current effect].</param>
        private async Task EnableInClient(int effectId, uint userId)
        {
            var userRoom = GetUserRoom();

            var roomUserByHabbo = userRoom?.GetRoomUserManager().GetRoomUserByHabbo(userId);

            if (roomUserByHabbo == null)
                return;

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("ApplyEffectMessageComposer"));
            await serverMessage.AppendIntegerAsync(roomUserByHabbo.VirtualId);
            await serverMessage.AppendIntegerAsync(effectId);
            await serverMessage.AppendIntegerAsync(0);
            await roomUserByHabbo.GetClient().SendMessageAsync(serverMessage);
        }

        /// <summary>
        ///     Gets the user room.
        /// </summary>
        /// <returns>Room.</returns>
        private Room GetUserRoom()
        {
            return _session.GetHabbo().CurrentRoom;
        }
    }
}