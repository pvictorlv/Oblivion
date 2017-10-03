using System;
using Oblivion.HabboHotel.Navigators.Enums;
using Oblivion.HabboHotel.Rooms.Data;
using Oblivion.Messages;

namespace Oblivion.HabboHotel.Navigators.Interfaces
{
    /// <summary>
    ///     Class PublicItem.
    /// </summary>
    internal class PublicItem
    {
        /// <summary>
        ///     The caption
        /// </summary>
        internal string Caption;

        /// <summary>
        ///     The category identifier
        /// </summary>
        internal int CategoryId;

        /// <summary>
        ///     The description
        /// </summary>
        internal string Description;

        /// <summary>
        ///     The image
        /// </summary>
        internal string Image;

        /// <summary>
        ///     The image type
        /// </summary>
        internal PublicImageType ImageType;

        /// <summary>
        ///     The item type
        /// </summary>
        internal PublicItemType ItemType;

        /// <summary>
        ///     The parent identifier
        /// </summary>
        internal int ParentId;

        /// <summary>
        ///     The recommended
        /// </summary>
        internal bool Recommended;

        /// <summary>
        ///     The room identifier
        /// </summary>
        internal uint RoomId;

        /// <summary>
        ///     The tags to search
        /// </summary>
        internal string TagsToSearch = string.Empty;

        /// <summary>
        ///     The type
        /// </summary>
        internal int Type;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PublicItem" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="type">The type.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="desc">The desc.</param>
        /// <param name="image">The image.</param>
        /// <param name="imageType">Type of the image.</param>
        /// <param name="roomId">The room identifier.</param>
        /// <param name="categoryId">The category identifier.</param>
        /// <param name="parentId">The parent identifier.</param>
        /// <param name="recommand">if set to <c>true</c> [recommand].</param>
        /// <param name="typeOfData">The type of data.</param>
        /// <param name="tags">The tags.</param>
        internal PublicItem(uint id, int type, string caption, string desc, string image, PublicImageType imageType,
            uint roomId, int categoryId, int parentId, bool recommand, int typeOfData, string tags)
        {
            Id = id;
            Type = type;
            Caption = caption;
            Description = desc;
            Image = image;
            ImageType = imageType;
            RoomId = roomId;
            CategoryId = categoryId;
            ParentId = parentId;

            Recommended = recommand;
            switch (typeOfData)
            {
                case 1:
                {
                    ItemType = PublicItemType.Tag;
                    break;
                }
                case 2:
                {
                    ItemType = PublicItemType.Flat;
                    break;
                }
                case 3:
                {
                    ItemType = PublicItemType.PublicFlat;
                    break;
                }
                case 4:
                {
                    ItemType = PublicItemType.Category;
                    break;
                }
                default:
                {
                    ItemType = PublicItemType.None;
                    break;
                }
            }
        }

        /// <summary>
        ///     Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        internal uint Id { get; set; }

        /// <summary>
        ///     Gets the room data.
        /// </summary>
        /// <value>The room data.</value>
        /// <exception cref="System.NullReferenceException"></exception>
        internal RoomData RoomData
        {
            get
            {
                if (RoomId == 0u) return new RoomData();
                if (Oblivion.GetGame() == null || Oblivion.GetGame().GetRoomManager() == null)
                    throw new NullReferenceException();
                return Oblivion.GetGame().GetRoomManager().GenerateRoomData(RoomId);
            }
        }

        /// <summary>
        ///     Gets the room information.
        /// </summary>
        /// <value>The room information.</value>
        internal RoomData RoomInfo
        {
            get
            {
                RoomData result;
                try
                {
                    result = RoomId > 0u ? Oblivion.GetGame().GetRoomManager().GenerateRoomData(RoomId) : null;
                }
                catch
                {
                    result = null;
                }
                return result;
            }
        }

        /// <summary>
        ///     Serializes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void Serialize(ServerMessage message)
        {
            try
            {
                message.AppendInteger(Id);
                message.AppendString(Caption);
                message.AppendString(Description);
                message.AppendInteger(Type);
                message.AppendString(Caption);
                message.AppendString(Image);
                message.AppendInteger(ParentId);
                message.AppendInteger(RoomInfo?.UsersNow ?? 0);
                message.AppendInteger((ItemType == PublicItemType.None)
                    ? 0
                    : ((ItemType == PublicItemType.Tag)
                        ? 1
                        : ((ItemType == PublicItemType.Flat)
                            ? 2
                            : ((ItemType == PublicItemType.PublicFlat)
                                ? 2
                                : ((ItemType != PublicItemType.Category) ? 0 : 4)))));
                switch (ItemType)
                {
                    case PublicItemType.Tag:
                    {
                        message.AppendString(TagsToSearch);
                        break;
                    }
                    case PublicItemType.Category:
                    {
                        message.AppendBool(false);
                        break;
                    }
                    case PublicItemType.Flat:
                    {
                        RoomInfo.Serialize(message);
                        break;
                    }
                    case PublicItemType.PublicFlat:
                    {
                        RoomInfo.Serialize(message);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception on publicitems composing: {0}", ex);
            }
        }

        /// <summary>
        ///     Serializes the new.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SerializeNew(ServerMessage message)
        {
            message.AppendInteger(RoomId);
            message.AppendInteger(12);
            message.AppendString(Image);
            message.AppendString(Caption);
        }
    }
}