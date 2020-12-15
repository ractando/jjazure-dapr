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

            // getting etag to avoid concurrent writes (https://github.com/dapr/dotnet-sdk/pull/498/files)
            var (state, etag) = await daprClient.GetStateAndETagAsync<ArticleItem>(StoreName, vote.articleid);
            state??= new ArticleItem() { articleid = vote.articleid, voteCount = 0 };

            state.voteCount++;
            Console.WriteLine("Article {0} voteCount increased to: {1} etag {2}", vote.articleid, state.voteCount, etag);

            try {                
                var options = new StateOptions() {Concurrency = ConcurrencyMode.FirstWrite};
                bool isSaveStateSuccess = await daprClient.TrySaveStateAsync<ArticleItem>(StoreName, vote.articleid, state, etag);
                if (isSaveStateSuccess)
                    Console.WriteLine("Article {0} voteCount saved.", vote.articleid);
                else
                {
                    Console.WriteLine("Article {0} voteCount NOT saved, error eTag {1}.", vote.articleid, isSaveStateSuccess);
                    throw new Exception("Wrong eTag - version has changed !");
                    // TODO: retry to get etag and update it again
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Article {0} voteCount ERROR {1}.", vote.articleid, ex.Message);
                return BadRequest();
            }
            return state;
        }
    }
}