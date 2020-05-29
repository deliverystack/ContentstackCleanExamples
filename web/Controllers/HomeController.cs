namespace web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using Contentstack.Core;
    using Contentstack.Core.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ViewEngines;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using web.Models;

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

//        public HomeController(ILogger<HomeController> logger)
//        {
//            _logger = logger;
//        }

        //TODO: this is for JW trying to determine model class from view
        private readonly ICompositeViewEngine _viewEngine;
        private ContentstackClient _client;

        public HomeController(
            ContentstackClient client, 
            ICompositeViewEngine engine,
            ILogger<HomeController> logger)
        {
            _client = client;
            _viewEngine = engine;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public ContentResult ContentTypes()
        {
            string result = "{" + Environment.NewLine;

            foreach (JObject contentType in _client.GetContentTypes(new Dictionary<string, object>()).Result)
            {
                result += "  \"" + contentType["uid"] + "\": \"" + contentType["title"] +
                    "\", url: \"/home/ContentType?ctid=" + contentType["uid"] + "\"," + Environment.NewLine;
            }

            return Content(result + '}');
        }

        public ContentResult ContentType([FromQuery(Name = "ctid")] string ctid)
        {
            foreach (JObject contentType in _client.GetContentTypes(new Dictionary<string, object>()).Result)
            {
                if (String.Equals(contentType["uid"].ToString().ToLower(), ctid, StringComparison.InvariantCultureIgnoreCase))
                {
                    // render the JSON representation of the Content Type identified by the ctid query string
                    return Content(contentType.ToString());
                }
            }

            return ContentTypes();
        }

        public ContentResult Entries([FromQuery(Name = "ctid")] string ctid)
        {
            string result = "{" + Environment.NewLine;

            foreach (Entry element in _client.ContentType(ctid).Query().Find<Entry>().Result)
            {
                result += "  { ctid: \"" + ctid + "\" uid:\"" + element.Uid + " \", title: \"" + element.Title + "\" }" + Environment.NewLine;
            }

            return Content(result + '}');
        }

        public ActionResult Entry([FromQuery(Name = "ctid")] string ctid, [FromQuery(Name = "uid")] string uid, [FromQuery(Name = "json")] bool json = false)
        {
            Entry entry = _client.ContentType(ctid).Entry(uid).Fetch<Entry>().Result;

            if (json)
            {
                return Content(entry.ToJson().ToString());
            }

            ViewData[typeof(ContentstackClient).ToString()] = _client;
            return View(entry);
        }

        public IActionResult Query([FromQuery(Name = "query")] string query)
        {
            using (var w = new WebClient())
            {
                string url = "https://graphql.contentstack.com/stacks/" +
                    _client.GetApplicationKey() + "?access_token=" +
                    _client.GetAccessToken() + "&environment=" +
                    _client.GetEnvironment() + "&query=" + query;
                return Content(JObject.Parse(w.DownloadString(url)).ToString());
            }
        }
    }
}
