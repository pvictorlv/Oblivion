using System.Data;
using Oblivion.Messages;

namespace Oblivion.HabboHotel.Items.Datas
{
    /// <summary>
    ///     Class TonerData.
    /// </summary>
    internal class TonerData
    {
        /// <summary>
        ///     The data1
        /// </summary>
        internal int Data1, Data2, Data3;

        /// <summary>
        ///     The enabled
        /// </summary>
        internal int Enabled;

        /// <summary>
        ///     The item identifier
        /// </summary>
        internal string ItemId;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TonerData" /> class.
        /// </summary>
        /// <param name="item">The item.</param>
        internal TonerData(string item)
        {
            ItemId = item;
            DataRow row;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery($"SELECT enabled,data1,data2,data3 FROM items_toners WHERE id={ItemId} LIMIT 1");
                row = queryReactor.GetRow();
            }

            if (row == null)
            {
                Data1 = Data2 = Data3 = 1;
                return;
            }

            Enabled = int.Parse(row[0].ToString());
            Data1 = (int)row[1];
            Data2 = (int)row[2];
            Data3 = (int)row[3];
        }

        /// <summary>
        ///     Generates the extra data.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage GenerateExtraData(ServerMessage message)
        {
            message.AppendInteger(0);
            message.AppendInteger(5);
            message.AppendInteger(4);
            message.AppendInteger(Enabled);
            message.AppendInteger(Data1);
            message.AppendInteger(Data2);
            message.AppendInteger(Data3);

            return message;
        }
    }
}