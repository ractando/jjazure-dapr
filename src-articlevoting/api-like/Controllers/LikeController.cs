using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace api_like.Controllers
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
            return "World";
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

            return string.Format("Vote liked key: {0}", key);
        }
    }
}