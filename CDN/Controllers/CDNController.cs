using CDN.Helper;
using CDN.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CDN.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CDNController : Controller
    {
        #region ctor
        private readonly TokenSettings tokenSettings;
        public CDNController(TokenSettings tokenSettings)
        {
            this.tokenSettings = tokenSettings;
        }
        #endregion


        #region User
        [HttpPost]
        public async Task<User> GetUserByEmail(string email)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("xc-auth", tokenSettings.token);
            var json = await client.GetAsync("http://172.16.10.132:3574//nc/ferrum_cdn_r8o0/api/v1/f_auth/findOne?where=(email,like," + email + ")");
            if (json.IsSuccessStatusCode)
            {
                var EmpResponse = json.Content.ReadAsStringAsync().Result;
                var item = JsonConvert.DeserializeObject<User>(EmpResponse);
                if (item != null)
                {
                    item.token = TokenGenerator.GenerateToken(item.email, item.uniq_id);
                    return item;
                }
                throw new Exception(message: "User couldnt find");
            }
            throw new Exception(message: "Bad request");
        }
        #endregion



    }
}

