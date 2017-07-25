using Oblivion.HabboHotel.Rooms.Items.Enums;

namespace Oblivion.HabboHotel.Rooms.Items
{
    /// <summary>
    ///     Class ByteToItemEffectEnum.
    /// </summary>
    internal static class ByteToItemEffectEnum
    {
        /// <summary>
        ///     Parses the specified p byte.
        /// </summary>
        /// <param name="pByte">The p byte.</param>
        /// <returns>ItemEffectType.</returns>
        internal static ItemEffectType Parse(byte pByte)
        {
            switch (pByte)
            {
                case 0:
                    return ItemEffectType.None;

                case 1:
                    return ItemEffectType.Swim;

                case 2:
                    return ItemEffectType.Normalskates;

                case 3:
                    return ItemEffectType.Iceskates;

                case 4:
                    return ItemEffectType.SwimLow;

                case 5:
                    return ItemEffectType.SwimHalloween;

                case 6:
                    return ItemEffectType.PublicPool;

                case 7:
                    return ItemEffectType.SnowBoard;

                default:
                    return ItemEffectType.None;
            }
        }
    }
}