namespace api_votes
{
    using System.ComponentModel.DataAnnotations;
    public class ArticleItem
    {
        [Required]
        public string articleid { get; set; }
        [Required]
        public int voteCount { get; set; }
    }
}