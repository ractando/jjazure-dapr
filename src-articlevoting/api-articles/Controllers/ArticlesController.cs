using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace api_articles.Controllers
{
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        public const string StoreName = "jjstate-articles";

        // for testing only
        [HttpGet("hello")]
        public ActionResult<string> Get()
        {
            Console.WriteLine("Hello, World.");
            return "Hello from API Articles";
        }

        // create article
        [HttpPost("create")]
        public async Task<ActionResult<string>> Create(
                        ArticleItem item,
                        [FromServices] DaprClient daprClient)
        {
            // unique key for vote like (article and liker)
            string key = item.articleid;

            ArticleItem newItem = item;
            await daprClient.SaveStateAsync(StoreName, key, newItem);

            return string.Format("Article created key: {0}", key);
        }

        // consuming message from pubsub (defined in components)
        [Topic("pubsub","likeprocess")]
        [HttpPost("LikeProcess")]
        public async Task<ActionResult<ArticleItem>> LikeProcess(VoteItem vote, [FromServices] DaprClient daprClient)
        {
            Console.WriteLine("Enter LikeProcess");
            var state = await daprClient.GetStateEntryAsync<ArticleItem>(StoreName, vote.articleid);
            state.Value ??= new ArticleItem() { articleid = vote.articleid, voteCount = 0 };
            state.Value.voteCount++;
            await state.SaveAsync();
            Console.WriteLine("voteCount increased to: " + state.Value.voteCount.ToString());
            return state.Value;
        }
    }
}