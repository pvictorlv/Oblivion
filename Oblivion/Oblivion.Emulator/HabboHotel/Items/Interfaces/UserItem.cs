using System.Globalization;
using System.Threading.Tasks;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.Messages;

namespace Oblivion.HabboHotel.Items.Interfaces
{
    /// <summary>
    ///     Class UserItem.
    /// </summary>
    public class UserItem
    {
        /// <summary>
        ///     The base item
        /// </summary>
        internal Item BaseItem;
        

        /// <summary>
        ///     The extra data
        /// </summary>
        internal string ExtraData;

        /// <summary>
        ///     The group identifier
        /// </summary>
        internal uint GroupId;

        /// <summary>
        ///     The identifier
        /// </summary>
        internal string Id;

        /// <summary>
        ///     The is wall item
        /// </summary>
        internal bool IsWallItem;

        /// <summary>
        ///     The limited sell identifier
        /// </summary>
        internal uint LimitedSellId, LimitedStack;

        /// <summary>
        ///     The song code
        /// </summary>
        internal string SongCode;

        internal uint VirtualId;

        internal uint RoomId;
        /// <summary>
        ///     Initializes a new instance of the <see cref="UserItem" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="baseItemId">The base item identifier.</param>
        /// <param name="extraData">The extra data.</param>
        /// <param name="group">The group.</param>
        /// <param name="songCode">The song code.</param>
        internal UserItem(string id, uint baseItemId, string extraData, uint group, string songCode, int limitedSell,
            int limitedStack)
        {
            Id = id;
            ExtraData = extraData;
            GroupId = group;

            VirtualId = Oblivion.GetGame().GetItemManager().GetVirtualId(id);

            BaseItem = Oblivion.GetGame().GetItemManager().GetItem(baseItemId);


            LimitedSellId = (uint) limitedSell;
            LimitedStack = (uint) limitedStack;

            if (BaseItem == null)
                return;

            IsWallItem = (BaseItem.Type == 'i');
            SongCode = songCode;
        }


        public void Dispose(bool removeVirtual= true)
        {
            BaseItem = null;
            if (removeVirtual)
            Oblivion.GetGame().GetItemManager().RemoveVirtualItem(Id);
        }

        /// <summary>
        ///     Serializes the wall.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inventory">if set to <c>true</c> [inventory].</param>
        internal async Task SerializeWall(ServerMessage message, bool inventory)
        {
            await message.AppendIntegerAsync(VirtualId);
            await message.AppendStringAsync(BaseItem.Type.ToString().ToUpper());
            await message.AppendIntegerAsync(VirtualId);
            await message.AppendIntegerAsync(BaseItem.SpriteId);

            if (BaseItem.Name.Contains("a2") || BaseItem.Name == "floor")
                await message.AppendIntegerAsync(3);
            else if (BaseItem.Name.Contains("wallpaper") && BaseItem.Name != "wildwest_wallpaper")
                await message.AppendIntegerAsync(2);
            else if (BaseItem.Name.Contains("landscape"))
                await message.AppendIntegerAsync(4);
            else
                await message.AppendIntegerAsync(1);

            await message.AppendIntegerAsync(0);
            await message.AppendStringAsync(ExtraData);
            message.AppendBool(BaseItem.AllowRecycle);
            message.AppendBool(BaseItem.AllowTrade);
            message.AppendBool(BaseItem.AllowInventoryStack);
            message.AppendBool(BaseItem.AllowMarketplaceSell); //SELLABLE_ICON
            await message.AppendIntegerAsync(-1); //secondsToExpiration
            message.AppendBool(false); //hasRentPeriodStarted
            await message.AppendIntegerAsync(-1); //flatId
        }

        /// <summary>
        ///     Serializes the floor.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inventory">if set to <c>true</c> [inventory].</param>
        internal async Task SerializeFloor(ServerMessage message, bool inventory)
        {
            await message.AppendIntegerAsync(VirtualId);
            await message.AppendStringAsync(BaseItem.Type.ToString(CultureInfo.InvariantCulture).ToUpper());
            await message.AppendIntegerAsync(VirtualId);
            await message.AppendIntegerAsync(BaseItem.SpriteId);
            var extraParam = 0;

            try
            {
                if (BaseItem.InteractionType == Interaction.Gift)
                {
                    var split = ExtraData.Split((char) 9);
                    int.TryParse(split[2], out var ribbon);
                    int.TryParse(split[3], out var color);
                    extraParam = (ribbon * 1000) + color;
                }
            }
            catch
            {
                extraParam = 1001;
            }

            await message.AppendIntegerAsync(extraParam);

            if (BaseItem.IsGroupItem)
            {
                var group = Oblivion.GetGame().GetGroupManager().GetGroup(GroupId);

                if (group != null)
                {
                    await message.AppendIntegerAsync(2);
                    await message.AppendIntegerAsync(5);
                    await message.AppendStringAsync(ExtraData);
                    await message.AppendStringAsync(group.Id.ToString(CultureInfo.InvariantCulture));
                    await message.AppendStringAsync(group.Badge);
                    await message.AppendStringAsync(Oblivion.GetGame().GetGroupManager().GetGroupColour(group.Colour1, true));
                    await message.AppendStringAsync(Oblivion.GetGame().GetGroupManager().GetGroupColour(group.Colour2, false));
                }
                else
                {
                    await message.AppendIntegerAsync(0);
                    await message.AppendStringAsync(string.Empty);
                }
            }
            else if (LimitedStack > 0)
            {
                await message.AppendStringAsync(string.Empty);
                message.AppendBool(true);
                message.AppendBool(false);
                await message.AppendStringAsync(ExtraData);
            }
            else if ((BaseItem.InteractionType == Interaction.Moplaseed) &&
                     (BaseItem.InteractionType == Interaction.RareMoplaSeed))
            {
                await message.AppendIntegerAsync(1);
                await message.AppendIntegerAsync(1);
                await message.AppendStringAsync("rarity");
                await message.AppendStringAsync(ExtraData);
            }
            else
            {
                switch (BaseItem.InteractionType)
                {
                    case Interaction.BadgeDisplay:
                        var extra = ExtraData.Split('|');
                        await message.AppendIntegerAsync(2);
                        await message.AppendIntegerAsync(4);
                        await message.AppendStringAsync("0");
                        await message.AppendStringAsync(extra[0]);
                        await message.AppendStringAsync(extra.Length > 1 ? extra[1] : "");
                        await message.AppendStringAsync(extra.Length > 1 ? extra[2] : "");
                        break;

                    case Interaction.YoutubeTv:
                        await message.AppendIntegerAsync(1);
                        await message.AppendIntegerAsync(1);
                        await message.AppendStringAsync("THUMBNAIL_URL");
                        await message.AppendStringAsync(ExtraData);
                        break;

                    case Interaction.Mannequin:
                        await message.AppendIntegerAsync(1);
                        if (ExtraData.Length <= 0 || !ExtraData.Contains(";") || ExtraData.Split(';').Length < 3)
                        {
                            await message.AppendIntegerAsync(3); // Coun Of Values
                            await message.AppendStringAsync("GENDER");
                            await message.AppendStringAsync("m");
                            await message.AppendStringAsync("FIGURE");
                            await message.AppendStringAsync(string.Empty);
                            await message.AppendStringAsync("OUTFIT_NAME");
                            await message.AppendStringAsync(string.Empty);
                        }
                        else
                        {
                            var extradatas = ExtraData.Split(';');

                            await message.AppendIntegerAsync(3); // Count Of Values
                            await message.AppendStringAsync("GENDER");
                            await message.AppendStringAsync(extradatas[0]);
                            await message.AppendStringAsync("FIGURE");
                            await message.AppendStringAsync(extradatas[1]);
                            await message.AppendStringAsync("OUTFIT_NAME");
                            await message.AppendStringAsync(extradatas[2]);
                        }
                        break;

                    default:
                        await message.AppendIntegerAsync(0);
                        if (!BaseItem.IsGroupItem)
                            await message.AppendStringAsync(ExtraData);
                        break;
                }
            }

            if (LimitedSellId > 0)
            {
                await message.AppendIntegerAsync(LimitedSellId);
                await message.AppendIntegerAsync(LimitedStack);
            }

            /* message.AppendInteger((BaseItem.InteractionType == InteractionType.gift) ? 9 : 0);
                message.AppendInteger(0);
                message.AppendString((BaseItem.InteractionType == InteractionType.gift)
                    ? string.Empty
                    : ExtraData);*/

            message.AppendBool(BaseItem.AllowRecycle);
            message.AppendBool(BaseItem.AllowTrade);
            message.AppendBool(LimitedSellId <= 0 && BaseItem.AllowInventoryStack);
            message.AppendBool(true); // sellable
            await message.AppendIntegerAsync(-1); // expireTime
            message.AppendBool(false); // hasRentPeriodStarted
            await message.AppendIntegerAsync(-1); // flatId

            if (BaseItem.Type != 's') return;
            await message.AppendStringAsync(string.Empty); //slotId
            await message.AppendIntegerAsync(0);
        }
    }
}