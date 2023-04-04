using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Items.Interfaces;
using Oblivion.Configuration;
using Oblivion.Util;

namespace Oblivion.HabboHotel.Camera
{
    public class CameraPhotoManager
    {
        private readonly Dictionary<int, CameraPhotoPreview> _previews;
        private int _maxPreviewCacheCount = 1000;

        private Item _photoPoster;

        private string _previewPath = "preview/{1}-{0}.png";

        private string _purchasedPath = "purchased/{1}-{0}.png";

        public CameraPhotoManager() => _previews = new Dictionary<int, CameraPhotoPreview>();

        public int PurchaseCoinsPrice { get; private set; } = 999;

        public int PurchaseDucketsPrice { get; private set; } = 999;

        public int PublishDucketsPrice { get; private set; } = 999;

        internal Item PhotoPoster => _photoPoster;

        internal Task Init(ItemManager itemDataManager)
        {
            ConfigurationData.Data.TryGetValue("camera.path.preview", out _previewPath);
            ConfigurationData.Data.TryGetValue("camera.path.purchased", out _purchasedPath);

            if (ConfigurationData.Data.ContainsKey("camera.preview.maxcache"))
                _maxPreviewCacheCount = int.Parse(ConfigurationData.Data["camera.preview.maxcache"]);

            if (Oblivion.GetDbConfig().DbData.ContainsKey("camera.photo.purchase.price.coins"))
                PurchaseCoinsPrice = int.Parse(Oblivion.GetDbConfig().DbData["camera.photo.purchase.price.coins"]);

            if (Oblivion.GetDbConfig().DbData.ContainsKey("camera.photo.purchase.price.duckets"))
                PurchaseDucketsPrice =
                    int.Parse(Oblivion.GetDbConfig().DbData["camera.photo.purchase.price.duckets"]);

            if (Oblivion.GetDbConfig().DbData.ContainsKey("camera.photo.publish.price.duckets"))
                PublishDucketsPrice =
                    int.Parse(Oblivion.GetDbConfig().DbData["camera.photo.publish.price.duckets"]);

            var ItemId = uint.Parse(Oblivion.GetDbConfig().DbData["camera.photo.purchase.item_id"]);

            if (!itemDataManager.GetItem(ItemId, out _photoPoster))
                Logging.LogException("Couldn't load photo poster item " + ItemId + ", no furniture record found.");

            Out.WriteLine("Loaded Camera Photo Manager", "Oblivion.Camera");
            return Task.CompletedTask;
        }

        public CameraPhotoPreview GetPreview(int PhotoId) =>
            _previews.TryGetValue(PhotoId, out var preview) ? preview : null;

        public void AddPreview(CameraPhotoPreview preview)
        {
            if (_previews.Count >= _maxPreviewCacheCount)
                _previews.Remove(_previews.Keys.First());

            _previews.Add(preview.Id, preview);
        }

        public string GetPath(CameraPhotoType type, int PhotoId, int CreatorId)
        {
            var path = "{1}-{0}.png";

            if (type == CameraPhotoType.PREVIEW)
                path = _previewPath;
            else if (type == CameraPhotoType.PURCHASED)
            {
                path = _purchasedPath;
            }

            return string.Format(path, PhotoId, CreatorId);
        }
    }
}

public enum CameraPhotoType
{
    PREVIEW,
    PURCHASED
}