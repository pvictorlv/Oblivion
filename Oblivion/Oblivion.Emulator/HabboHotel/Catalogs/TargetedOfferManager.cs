using Oblivion.HabboHotel.Catalogs.Composers;
using Oblivion.HabboHotel.Catalogs.Interfaces;
using Oblivion.Messages;

namespace Oblivion.HabboHotel.Catalogs
{
    internal class TargetedOfferManager
    {
        internal TargetedOffer CurrentOffer;

        public TargetedOfferManager()
        {
            LoadOffer();
        }

        public void LoadOffer()
        {
            CurrentOffer = null;

            using (var queryReactor = Oblivion.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT * FROM catalog_targetedoffers WHERE enabled = '1' LIMIT 1");

                var row = queryReactor.GetRow();

                if (row == null)
                    return;

                CurrentOffer = new TargetedOffer((int)row["id"], (string)row["identifier"], (int)row["cost_credits"],
                    (int)row["cost_duckets"], (int)row["cost_diamonds"], (int)row["purchase_limit"],
                    (int)row["expiration_time"], (string)row["title"], (string)row["description"],
                    (string)row["image"], (string)row["products"]);
            }
        }

        public void GenerateMessage(ServerMessage message)
        {
            TargetedOfferComposer.GenerateMessage(message, CurrentOffer);
        }
    }
}