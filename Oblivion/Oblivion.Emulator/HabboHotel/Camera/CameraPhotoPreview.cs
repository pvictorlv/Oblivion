namespace Oblivion.HabboHotel.Camera
{
    public class CameraPhotoPreview
    {
        public CameraPhotoPreview(int photoId, int creatorId, long createdAt)
        {
            Id = photoId;
            CreatorId = creatorId;
            CreatedAt = createdAt;
        }

        public int Id { get; }

        public int CreatorId { get; }

        public long CreatedAt { get; }
    }
}