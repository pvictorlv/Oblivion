using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Oblivion.Configuration;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Rooms.User;
using Oblivion.HabboHotel.Rooms.User.Path;

namespace Oblivion.HabboHotel.RoomBots
{
    /// <summary>
    ///     Class GenericBot.
    /// </summary>
    internal class GenericBot : BotAi
    {

        /// <summary>
        ///     The _is bartender
        /// </summary>
        private readonly bool _isBartender;

        /// <summary>
        ///     The _virtual identifier
        /// </summary>
        private readonly int _virtualId;

        /// <summary>
        ///     The _action count
        /// </summary>
        private int _actionCount;

        /// <summary>
        ///     The _chat timer
        /// </summary>
        private Timer _chatTimer;

        /// <summary>
        ///     The _speech interval
        /// </summary>
        private int _speechInterval;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GenericBot" /> class.
        /// </summary>
        /// <param name="roomBot">The room bot.</param>
        /// <param name="virtualId">The virtual identifier.</param>
        /// <param name="isBartender">if set to <c>true</c> [is bartender].</param>
        /// <param name="speechInterval">The speech interval.</param>
        internal GenericBot(RoomBot roomBot, int virtualId, bool isBartender, int speechInterval)
        {
            _virtualId = virtualId;
            _isBartender = isBartender;
            _speechInterval = speechInterval < 2 ? 2000 : speechInterval * 1000;

            if (roomBot != null && roomBot.AutomaticChat && roomBot.RandomSpeech != null && roomBot.RandomSpeech.Any())
                _chatTimer = new Timer(ChatTimerTick, null, _speechInterval, _speechInterval);
            _actionCount = Oblivion.GetRandomNumber(10, 30 + virtualId);
        }

        /// <summary>
        ///     Modifieds this instance.
        /// </summary>
        internal override void Modified()
        {
            try
            {
                if (GetBotData() == null) return;

                if (Disposed)
                {
                    return;
                }

                if (!GetBotData().AutomaticChat || GetBotData().RandomSpeech == null ||
                    !GetBotData().RandomSpeech.Any())
                {
                    StopTimerTick();
                    return;
                }
                _speechInterval = GetBotData().SpeechInterval < 2 ? 2000 : GetBotData().SpeechInterval * 1000;

                if (_chatTimer == null)
                {
                    _chatTimer = new Timer(ChatTimerTick, null, _speechInterval, _speechInterval);
                    return;
                }
                _chatTimer.Change(_speechInterval, _speechInterval);
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "Modified()");
            }
        }

        /// <summary>
        ///     Called when [timer tick].
        /// </summary>
        internal override async Task OnTimerTick()
        {
            if (GetBotData() == null) return;

            if (Disposed)
            {
                StopTimerTick();
                return;
            }

            if (GetBotData().RoomUser.FollowingOwner != null)
            {
                return;
            }

            if (_actionCount > 0)
            {
                _actionCount--;
                return;
            }
            _actionCount = Oblivion.GetRandomNumber(4, 45);

            switch (GetBotData().WalkingMode.ToLower())
            {
                case "freeroam":
                {
                    var randomPoint = GetRoom().GetGameMap().GetRandomWalkableSquare();
                    if (randomPoint.X == 0 || randomPoint.Y == 0) return;

                    await GetRoomUser().MoveTo(randomPoint.X, randomPoint.Y);
                    break;
                }
                case "specified_range":
                {
                    var list = GetRoom().GetGameMap().WalkableList.ToList();
                    if (!list.Any()) return;

                    var randomNumber = new Random(DateTime.Now.Millisecond + _virtualId ^ 2).Next(0, list.Count - 1);
                    await GetRoomUser().MoveTo(list[randomNumber].X, list[randomNumber].Y);
                    break;
                }
            }
        }

        /// <summary>
        ///     Called when [self enter room].
        /// </summary>
        internal override void OnSelfEnterRoom()
        {
        }

        /// <summary>
        ///     Called when [self leave room].
        /// </summary>
        /// <param name="kicked">if set to <c>true</c> [kicked].</param>
        internal override void OnSelfLeaveRoom(bool kicked)
        {
        }

        /// <summary>
        ///     Called when [user enter room].
        /// </summary>
        /// <param name="user">The user.</param>
        internal override Task OnUserEnterRoom(RoomUser user)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Called when [user leave room].
        /// </summary>
        /// <param name="client">The client.</param>
        internal override void OnUserLeaveRoom(GameClient client)
        {
        }

        /// <summary>
        ///     Called when [user say].
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="message">The message.</param>
        internal override async Task OnUserSay(RoomUser user, string message)
        {

            if (Disposed)
            {
                return;
            }

            if (Gamemap.TileDistance(GetRoomUser().X, GetRoomUser().Y, user.X, user.Y) > 16) return;

            if (!_isBartender) return;

            try
            {
                message = message.Substring(1);
            }
            catch
            {
                return;
            }
            switch (message.ToLower())
            {
                case "ven":
                case "comehere":
                case "come here":
                case "ven aquí":
                case "come":
                case "vem aqui":
                case "venha":
                case "venha aqui":
                case "vem aquí":
                    await GetRoomUser().Chat(null, "Estou Indo!", false, 0);
                    await GetRoomUser().MoveTo(user.SquareInFront);
                    return;

                case "sirve":
                case "serve":
                case "sirva":
                    if (GetRoom().CheckRights(user.GetClient()))
                    {
                        /* TODO CHECK */
                        foreach (var current in GetRoom().GetRoomUserManager().GetRoomUsers())
                          await  current.CarryItem(Oblivion.GetRandomNumber(1, 38));
                        await GetRoomUser().Chat(null, "Worth. Agora você tem algo para devorar todos.", false, 0);
                        return;
                    }
                    return;

                case "agua":
                case "água":
                case "té":
                case "te":
                case "tea":
                case "juice":
                case "water":
                case "zumo":
                    await GetRoomUser().Chat(null, "Aqui você vai.", false, 0);
                    await user.CarryItem(Oblivion.GetRandomNumber(1, 3));
                    return;

                case "helado":
                case "icecream":
                case "sorvete":
                case "ice cream":
                    await GetRoomUser()
                        .Chat(null, "Aqui você vai. Isso não é o idioma que se encaixam perto, hehe!", false, 0);
                    await user.CarryItem(4);
                    return;

                case "rose":
                case "rosa":
                    await GetRoomUser().Chat(null, "Aqui você vai ... você faz bem em sua nomeação.", false, 0);
                    await user.CarryItem(Oblivion.GetRandomNumber(1000, 1002));
                    return;

                case "girasol":
                case "girassol":
                case "sunflower":
                    await GetRoomUser().Chat(null, "Aqui estão algumas muito agradável natureza.", false, 0);
                    await user.CarryItem(1002);
                    return;

                case "flor":
                case "flower":
                    await GetRoomUser().Chat(null, "Aqui estão algumas muito agradável da natureza.", false, 0);
                    if (Oblivion.GetRandomNumber(1, 3) == 2)
                    {
                        await user.CarryItem(Oblivion.GetRandomNumber(1019, 1024));
                        return;
                    }
                    await user.CarryItem(Oblivion.GetRandomNumber(1006, 1010));
                    return;

                case "zanahoria":
                case "zana":
                case "carrot":
                case "cenoura":
                    await GetRoomUser().Chat(null, "Aqui está um bom vegetal. Divirta-se!", false, 0);
                    await user.CarryItem(3);
                    return;

                case "café":
                case "cafe":
                case "capuccino":
                case "coffee":
                case "latte":
                case "mocha":
                case "espresso":
                case "expreso":
                    await GetRoomUser().Chat(null, "Aqui está o seu café. É espumante!", false, 0);
                    await user.CarryItem(Oblivion.GetRandomNumber(11, 18));
                    return;

                case "fruta":
                case "fruit":
                    await GetRoomUser().Chat(null, "Aqui está um pouco saudável, fresco e natural. Aproveite!", false, 0);
                    await user.CarryItem(Oblivion.GetRandomNumber(36, 40));
                    return;

                case "naranja":
                case "orange":
                case "laranja":
                    await GetRoomUser().Chat(null, "Aqui está um pouco saudável, fresco e natural. Aproveite!", false, 0);
                    await user.CarryItem(38);
                    return;

                case "manzana":
                case "apple":
                case "maça":
                case "maçã":
                case "maca":
                case "macã":
                    await GetRoomUser().Chat(null, "Aqui está um pouco saudável, fresco e natural. Aproveite!", false, 0);
                    await user.CarryItem(37);
                    return;

                case "cola":
                case "habbocola":
                case "habbo cola":
                case "coca cola":
                case "cocacola":
                    await GetRoomUser().Chat(null, "Aqui é uma bebida muito famosa macio.", false, 0);
                    await user.CarryItem(19);
                    return;

                case "pear":
                case "pera":
                case "pêra":
                    await GetRoomUser().Chat(null, "Aqui está um pouco saudável, fresco e natural. Aproveite!", false, 0);
                    await user.CarryItem(36);
                    return;

                case "ananá":
                case "pineapple":
                case "piña":
                case "rodaja de piña":
                    await GetRoomUser().Chat(null, "Aqui está um pouco saudável, fresco e natural. Aproveite!", false, 0);
                    await user.CarryItem(39);
                    return;

                case "puta":
                case "puto":
                case "gilipollas":
                case "metemela":
                case "polla":
                case "pene":
                case "penis":
                case "idiot":
                case "fuck":
                case "bastardo":
                case "idiota":
                case "chupamela":
                case "tonta":
                case "tonto":
                case "mierda":
                case "vadia":
                case "prostituta":
                case "vaca":
                case "feiosa":
                case "filha da puta":
                case "gostosa":
                    await GetRoomUser().Chat(null, "Não me trate mal, eh!", true, 0);
                    return;

                case "case comigo":
                    await GetRoomUser().Chat(null, "Irei agora!", true, 0);
                    return;

                case "protocolo destruir":
                    await GetRoomUser().Chat(null, "Iniciando Auto Destruição do Mundo!", true, 0);
                    return;

                case "lindo":
                case "hermoso":
                case "linda":
                case "guapa":
                case "beautiful":
                case "handsome":
                case "love":
                case "guapo":
                case "i love you":
                case "hermosa":
                case "preciosa":
                case "te amo":
                case "amor":
                case "mi amor":
                    await GetRoomUser()
                        .Chat(null, "Eu sou um bot, err ... isto está a ficar desconfortável, você sabe?", false, 0);
                    return;
            }
            await GetRoomUser().Chat(null, "Precisa de Algo?", false, 0);
        }

        /// <summary>
        ///     Called when [user shout].
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="message">The message.</param>
        internal override async Task OnUserShout(RoomUser user, string message)
        {

            if (Disposed)
            {
                return;
            }

            if (_isBartender)
            {
                await GetRoomUser()
                    .Chat(null, "Não precisa gritar, caramba! Se precisa de algo basta vir aqui.", false, 0);
            }
        }

        /// <summary>
        ///     Stops the timer tick.
        /// </summary>
        internal override void StopTimerTick()
        {
            try
            {
                if (_chatTimer == null) return;
                _chatTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _chatTimer.Dispose();
                _chatTimer = null;
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "StopTimerTick");
            }
        }

        internal override async Task OnChatTick()
        {
            if (GetBotData() == null || GetRoomUser() == null || GetBotData().WasPicked ||
                GetBotData().RandomSpeech == null ||
                !GetBotData().RandomSpeech.Any() || Disposed)
            {
                StopTimerTick();
                return;
            }

            if (GetRoom() != null && GetRoom().MutedBots)
                return;

            var randomSpeech = GetBotData().GetRandomSpeech(GetBotData().MixPhrases);
            var user = GetRoomUser();
            try
            {
                switch (randomSpeech)
                {
                    case ":sit":
                    {
                        if (user == null) return;

                        if (user.RotBody % 2 != 0) user.RotBody--;

                        user.Z = GetRoom().GetGameMap().SqAbsoluteHeight(user.X, user.Y);
                        if (!user.Statusses.ContainsKey("sit"))
                        {
                            user.UpdateNeeded = true;
                            user.Statusses.TryAdd("sit", "0.55");
                        }
                        user.IsSitting = true;
                        return;
                    }
                    case ":stand":
                    {
                        if (user == null) return;

                        if (user.IsSitting)
                        {
                            user.Statusses.TryRemove("sit", out _);
                            user.IsSitting = false;
                            user.UpdateNeeded = true;
                        }
                        else if (user.IsLyingDown)
                        {
                            user.Statusses.TryRemove("lay", out _);
                            user.IsLyingDown = false;
                            user.UpdateNeeded = true;
                        }
                        return;
                    }
                }

                if (GetRoom() != null)
                {
                    randomSpeech = randomSpeech.Replace("%user_count%",
                        GetRoom().GetRoomUserManager().GetRoomUserCount().ToString());

                    randomSpeech = randomSpeech.Replace("%item_count%",
                        GetRoom().GetRoomItemHandler().TotalItems.ToString());
                    randomSpeech = randomSpeech.Replace("%floor_item_count%",
                        GetRoom().GetRoomItemHandler().FloorItems.Values.Count.ToString());
                    randomSpeech = randomSpeech.Replace("%wall_item_count%",
                        GetRoom().GetRoomItemHandler().WallItems.Values.Count.ToString());


                    if (GetRoom().RoomData != null)
                    {
                        randomSpeech = randomSpeech.Replace("%roomname%", GetRoom().RoomData.Name);
                        randomSpeech = randomSpeech.Replace("%owner%", GetRoom().RoomData.Owner);
                    }
                }

                if (GetBotData() != null) randomSpeech = randomSpeech.Replace("%name%", GetBotData().Name);
                if (user == null) return;

                await user.Chat(null, randomSpeech, false, 0);
            }
            catch (Exception e)
            {
                Writer.Writer.LogException(e.ToString());
            }
        }

        /// <summary>
        ///     Chats the timer tick.
        /// </summary>
        /// <param name="o">The o.</param>
        private async void ChatTimerTick(object o)
        {
            await OnChatTick();
        }
    }
}