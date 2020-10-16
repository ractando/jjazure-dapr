namespace api_articles
{
    using System.ComponentModel.DataAnnotations;
    public class VoteItem
    {
        [Required]
        public string articleid { get; set; }
        [Required]
        public string userid { get; set; }
    }
}