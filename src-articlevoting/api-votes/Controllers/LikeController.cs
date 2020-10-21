using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace api_votes.Controllers
{
    [ApiController]
    public class LikeController : ControllerBase
    {
        public const string StoreName = "jjstate-votes";

        // for testing only
        [HttpGet("hello")]
        public ActionResult<string> Get()
        {
            Console.WriteLine("Hello, World.");
            return "Hello from API Votes";
        }

        // save vote in store
        [HttpPost("like")]
        public async Task<ActionResult<string>> Like(
                        VoteItem item,
                        [FromServices] DaprClient daprClient)
        {
            // unique key for vote like (article and liker)
            string key = item.userid + "|" + item.articleid;

            VoteItem newItem = item;
            await daprClient.SaveStateAsync(StoreName, key, newItem);

            // publish message in topic
            await daprClient.PublishEventAsync<VoteItem>("pubsub", "likeprocess", newItem);

            return string.Format("Vote liked key: {0}", key);
        }
    }
}