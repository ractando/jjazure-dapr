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
        public const string StoreName = "jjstate-votes";

        // for testing only
        [HttpGet("hello")]
        public ActionResult<string> Get()
        {
            Console.WriteLine("Hello, World.");
            return "Hello from API Articles";
        }
    }
}