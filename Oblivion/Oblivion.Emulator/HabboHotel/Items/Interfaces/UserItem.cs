using System.Globalization;
using Oblivion.HabboHotel.Items.Interactions.Enums;
using Oblivion.Messages;

namespace Oblivion.HabboHotel.Items.Interfaces
{
    /// <summary>
    ///     Class UserItem.
    /// </summary>
    internal class UserItem
    {
        /// <summary>
        ///     The base item
        /// </summary>
        internal Item BaseItem;

        /// <summary>
        ///     The base item identifier
        /// </summary>
        internal uint BaseItemId;

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
        internal long Id;

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
        /// <summary>
        ///     Initializes a new instance of the <see cref="UserItem" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="baseItemId">The base item identifier.</param>
        /// <param name="extraData">The extra data.</param>
        /// <param name="group">The group.</param>
        /// <param name="songCode">The song code.</param>
        internal UserItem(long id, uint baseItemId, string extraData, uint group, string songCode)
        {
            Id = id;
            BaseItemId = baseItemId;
            ExtraData = extraData;
            GroupId = group;
            VirtualId = Oblivion.GetGame().GetItemManager().GetVirtualId(id);
            BaseItem = Oblivion.GetGame().GetItemManager().GetItem(baseItemId);

            if (BaseItem == null)
                return;

            if (BaseItem.IsRare)
                using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.SetQuery($"SELECT * FROM items_limited WHERE item_id={id} LIMIT 1");
                    var row = queryReactor.GetRow();

                    if (row != null)
                    {
                        uint.TryParse(row[1].ToString(), out LimitedSellId);
                        uint.TryParse(row[2].ToString(), out LimitedStack);
                    }
                }

            IsWallItem = (BaseItem.Type == 'i');
            SongCode = songCode;
        }


        public void Dispose()
        {
            BaseItem = null;
            Oblivion.GetGame().GetItemManager().RemoveVirtualItem(Id);
        }
        /// <summary>
        ///     Serializes the wall.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inventory">if set to <c>true</c> [inventory].</param>
        internal void SerializeWall(ServerMessage message, bool inventory)
        {
            message.AppendInteger(VirtualId);
            message.AppendString(BaseItem.Type.ToString().ToUpper());
            message.AppendInteger(VirtualId);
            message.AppendInteger(BaseItem.SpriteId);

            if (BaseItem.Name.Contains("a2") || BaseItem.Name == "floor")
                message.AppendInteger(3);
            else if (BaseItem.Name.Contains("wallpaper") && BaseItem.Name != "wildwest_wallpaper")
                message.AppendInteger(2);
            else if (BaseItem.Name.Contains("landscape"))
                message.AppendInteger(4);
            else
                message.AppendInteger(1);

            message.AppendInteger(0);
            message.AppendString(ExtraData);
            message.AppendBool(BaseItem.AllowRecycle);
            message.AppendBool(BaseItem.AllowTrade);
            message.AppendBool(BaseItem.AllowInventoryStack);
            message.AppendBool(false); //SELLABLE_ICON
            message.AppendInteger(-1); //secondsToExpiration
            message.AppendBool(true); //hasRentPeriodStarted
            message.AppendInteger(-1); //flatId
        }

        /// <summary>
        ///     Serializes the floor.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inventory">if set to <c>true</c> [inventory].</param>
        internal void SerializeFloor(ServerMessage message, bool inventory)
        {
            message.AppendInteger(VirtualId);
            message.AppendString(BaseItem.Type.ToString(CultureInfo.InvariantCulture).ToUpper());
            message.AppendInteger(VirtualId);
            message.AppendInteger(BaseItem.SpriteId);
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

            message.AppendInteger(extraParam);

            if (BaseItem.IsGroupItem)
            {
                var group = Oblivion.GetGame().GetGroupManager().GetGroup(GroupId);

                if (group != null)
                {
                    message.AppendInteger(2);
                    message.AppendInteger(5);
                    message.AppendString(ExtraData);
                    message.AppendString(group.Id.ToString(CultureInfo.InvariantCulture));
                    message.AppendString(group.Badge);
                    message.AppendString(Oblivion.GetGame().GetGroupManager().GetGroupColour(group.Colour1, true));
                    message.AppendString(Oblivion.GetGame().GetGroupManager().GetGroupColour(group.Colour2, false));
                }
                else
                {
                    message.AppendInteger(0);
                    message.AppendString(string.Empty);
                }
            }
            else if (LimitedStack > 0)
            {
                message.AppendString(string.Empty);
                message.AppendBool(true);
                message.AppendBool(false);
                message.AppendString(ExtraData);
            }
            else if ((BaseItem.InteractionType == Interaction.Moplaseed) &&
                     (BaseItem.InteractionType == Interaction.RareMoplaSeed))
            {
                message.AppendInteger(1);
                message.AppendInteger(1);
                message.AppendString("rarity");
                message.AppendString(ExtraData);
            }
            else
            {
                switch (BaseItem.InteractionType)
                {
                    case Interaction.BadgeDisplay:
                        var extra = ExtraData.Split('|');
                        message.AppendInteger(2);
                        message.AppendInteger(4);
                        message.AppendString("0");
                        message.AppendString(extra[0]);
                        message.AppendString(extra.Length > 1 ? extra[1] : "");
                        message.AppendString(extra.Length > 1 ? extra[2] : "");
                        break;

                    case Interaction.YoutubeTv:
                        message.AppendInteger(1);
                        message.AppendInteger(1);
                        message.AppendString("THUMBNAIL_URL");
                        message.AppendString(ExtraData);
                        break;

                    case Interaction.Mannequin:
                        message.AppendInteger(1);
                        if (ExtraData.Length <= 0 || !ExtraData.Contains(";") || ExtraData.Split(';').Length < 3)
                        {
                            message.AppendInteger(3); // Coun Of Values
                            message.AppendString("GENDER");
                            message.AppendString("m");
                            message.AppendString("FIGURE");
                            message.AppendString(string.Empty);
                            message.AppendString("OUTFIT_NAME");
                            message.AppendString(string.Empty);
                        }
                        else
                        {
                            var extradatas = ExtraData.Split(';');

                            message.AppendInteger(3); // Count Of Values
                            message.AppendString("GENDER");
                            message.AppendString(extradatas[0]);
                            message.AppendString("FIGURE");
                            message.AppendString(extradatas[1]);
                            message.AppendString("OUTFIT_NAME");
                            message.AppendString(extradatas[2]);
                        }
                        break;

                    default:
                        message.AppendInteger(0);
                        if (!BaseItem.IsGroupItem)
                            message.AppendString(ExtraData);
                        break;
                }
            }

            if (LimitedSellId > 0)
            {
                message.AppendInteger(LimitedSellId);
                message.AppendInteger(LimitedStack);
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
            message.AppendInteger(-1); // expireTime
            message.AppendBool(false); // hasRentPeriodStarted
            message.AppendInteger(-1); // flatId

            if (BaseItem.Type != 's') return;
            message.AppendString(string.Empty); //slotId
            message.AppendInteger(0);
        }
    }
}