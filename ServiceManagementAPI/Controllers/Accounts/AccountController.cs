using Microsoft.AspNetCore.Mvc;
using ServiceManagementAPI.Models.Requests.Accounts;

namespace ServiceManagementAPI.Controllers.Accounts
{
    [Route("[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        /*[HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }*/

        [HttpPost]
        [Route("Signup")]
        public void Post([FromBody] SignupRequest request)
        {
        }

        /*// PUT api/<AccountsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<AccountsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }*/
    }
}
