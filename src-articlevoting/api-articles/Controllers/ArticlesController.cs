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
            Console.WriteLine("Enter Create for article {0}", key);

            ArticleItem newItem = item;
            var options = new StateOptions() {Concurrency = ConcurrencyMode.FirstWrite};            
            await daprClient.SaveStateAsync(StoreName, key, newItem, options);

            return string.Format("Article created key: {0}", key);
        }

        // consuming message from pubsub (defined in components)
        [Topic("pubsub","likeprocess")]
        [HttpPost("LikeProcess")]
        public async Task<ActionResult<ArticleItem>> LikeProcess(VoteItem vote, [FromServices] DaprClient daprClient)
        {
            Console.WriteLine("Enter LikeProcess for article {0}", vote.articleid);
            var state = await daprClient.GetStateEntryAsync<ArticleItem>(StoreName, vote.articleid, ConsistencyMode.Strong);
            state.Value ??= new ArticleItem() { articleid = vote.articleid, voteCount = 0 };

            state.Value.voteCount++;
            var options = new StateOptions() {Concurrency = ConcurrencyMode.FirstWrite, Consistency = ConsistencyMode.Strong};
            Console.WriteLine("Article {0} voteCount increased to: {1}", vote.articleid, state.Value.voteCount);

            try {
                await state.SaveAsync(options);
                Console.WriteLine("Article {0} voteCount saved.", vote.articleid);
            }
            catch
            {
                Console.WriteLine("Article {0} voteCount ERROR.", vote.articleid);
            }
            return state.Value;
        }
    }
}