using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Echo
{
    [Route("api/[controller]")]
    public class NewsController : Controller
    {
        [HttpGet]
        public IEnumerable<string> Get()
            => new string[]
            {
                "A piece of news 1",
                "A piece of news 2"
            };

        [HttpGet("{id}")]
        public string Get(int id)
            => $"Blog Post #{id}";

        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
