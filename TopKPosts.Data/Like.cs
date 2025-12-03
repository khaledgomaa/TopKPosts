using System.ComponentModel.DataAnnotations.Schema;

namespace TopKPosts.Data
{
    public class Like
    {
        public int Id { get; set; }

        [ForeignKey("Post")]
        public int PostId { get; set; }
        public Post Post { get; set; }

        public DateTime LikedAt { get; set; }
    }
}
