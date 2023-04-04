using System.Threading.Tasks;
using System.Timers;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.HabboHotel.Items.Interactions.Models;
using Oblivion.HabboHotel.Items.Interfaces;

namespace Oblivion.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorVikingCotie : FurniInteractorModel
    {
        private RoomItem _mItem;

        public override async Task OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            var user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);

            if (user == null)
                return;

            if (user.CurrentEffect != 172 && user.CurrentEffect != 5 && user.CurrentEffect != 173)
                return;

            if (item.ExtraData != "5")
            {
                if (item.VikingCotieBurning)
                    return;

                item.ExtraData = "1";
                await  item.UpdateState();

                item.VikingCotieBurning = true;

                var clientByUsername =
                    Oblivion.GetGame().GetClientManager().GetClientByUserName(item.GetRoom().RoomData.Owner);

                if (clientByUsername != null)
                {
                    if (clientByUsername.GetHabbo().UserName != item.GetRoom().RoomData.Owner)
                        await clientByUsername.SendNotif(string.Format(Oblivion.GetLanguage().GetVar("viking_burn_started"),
                            user.GetUserName()));
                }

                _mItem = item;

                var timer = new Timer(5000);
                timer.Elapsed += OnElapse;
                timer.Enabled = true;
            }
            else
                await session.SendNotif(Oblivion.GetLanguage().GetVar("user_viking_error"));
        }

        private async void OnElapse(object sender, ElapsedEventArgs e)
        {
            if (_mItem == null)
                return;

            switch (_mItem.ExtraData)
            {
                case "1":
                    _mItem.ExtraData = "2";
                    await _mItem.UpdateState();
                    return;

                case "2":
                    _mItem.ExtraData = "3";
                    await _mItem.UpdateState();
                    return;

                case "3":
                    _mItem.ExtraData = "4";
                    await _mItem.UpdateState();
                    return;

                case "4":
                    ((Timer)sender).Stop();
                    _mItem.ExtraData = "5";
                    await _mItem.UpdateState();
                    return;
            }
        }
    }
}