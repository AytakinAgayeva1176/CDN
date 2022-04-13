using CDN.Helper;
using Microsoft.AspNetCore.Mvc;
using CDN.Models;
using CDN.ViewModels;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace CDN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FolderController : ControllerBase
    {
        #region ctor
        private readonly TokenSettings tokenSettings;
        private readonly FtpSettings ftpSettings;
        private readonly string uri = "http://172.16.10.132:3574/nc/ferrum_cdn_r8o0/api/v1/f_folder";
        public FolderController(TokenSettings tokenSettings, FtpSettings ftpSettings)
        {
            this.tokenSettings = tokenSettings;
            this.ftpSettings = ftpSettings;
        }

        #endregion


        #region Create
        /// <summary>
        /// Create Folde
        /// </summary>
        /// <param name="name"> Folde Name </param>
        /// <param name="access">Access</param>
        /// <returns></returns>
        [HttpPost("Create")]
        public async Task<IActionResult> Create(string name, string access)
        {
            var uniq_id = Guid.NewGuid().ToString();
            FolderVM model = new FolderVM()
            {
                name = name,
                access = access,
                uniq_id = uniq_id,
                internal_folder_name = uniq_id,
            };

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("xc-auth", tokenSettings.token);
            var data = JsonConvert.SerializeObject(model);
            StringContent queryString = new StringContent(data, Encoding.UTF8, "application/json");
            var json = await client.PostAsync(uri, queryString);

            if (json.IsSuccessStatusCode)
            {
                #region Ftp
                FtpWebRequest request =
                (FtpWebRequest)WebRequest.Create(ftpSettings.ServerLink + uniq_id);
                request.Credentials = new NetworkCredential(ftpSettings.UserName, ftpSettings.Password);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.GetResponse();
                #endregion

                var EmpResponse = json.Content.ReadAsStringAsync().Result;
                var item = JsonConvert.DeserializeObject<Folder>(EmpResponse);
                return Ok(item);
            }

            return BadRequest();
        }


        #endregion


        #region GetByUnique_Id / FindOne
        /// <summary>
        /// Get Folder By UniqId
        /// </summary>
        /// <param name="uniq_id"> Unique id of Folder </param>
        /// <returns></returns>
        [HttpPost("GetByUniqueId/{uniq_id}")]
        public async Task<IActionResult> GetByUniqueId(string uniq_id)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("xc-auth", tokenSettings.token);
            var json = await client.GetAsync(uri + "/findOne?where=(uniq_id,like," + uniq_id + ")");
            if (json.IsSuccessStatusCode)
            {
                var EmpResponse = json.Content.ReadAsStringAsync().Result;
                var item = JsonConvert.DeserializeObject<Folder>(EmpResponse);
                if (item == null)
                {
                    var result = new SystemMessaging(MesagesCode.Delete, "Folder doesn't exist");
                    return Ok(result);
                }
                return Ok(item);

            }
            return BadRequest();
        }

        #endregion


        #region Delete
        /// <summary>
        /// Delete Folder By UniqId
        /// </summary>
        /// <param name="uniq_id"> Unique id of Folder </param>
        /// <returns></returns>
        [HttpDelete("Delete/{uniq_id}")]
        public async Task<IActionResult> Delete(string uniq_id)
        {
            SystemMessaging result;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("xc-auth", tokenSettings.token);
            var folder = await client.GetAsync(uri + "/findOne?where=(uniq_id,like," + uniq_id + ")");
            if (folder.IsSuccessStatusCode)
            {
                var EmpResponse = folder.Content.ReadAsStringAsync().Result;
                var item = JsonConvert.DeserializeObject<Folder>(EmpResponse);
                if (item != null)
                {
                    var json = await client.DeleteAsync(uri + "/" + item.id);

                    if (json.IsSuccessStatusCode)
                    {
                        EmpResponse = json.Content.ReadAsStringAsync().Result;
                        if (EmpResponse == "1")
                        {
                            result = new SystemMessaging(MesagesCode.Delete, "Folder deleted succesfully!");
                            return Ok(result);
                        }
                        else
                        {
                            result = new SystemMessaging(MesagesCode.Delete, "Folder couldn't deleted!");
                            return BadRequest(result);
                        }

                    }
                }
                else
                {
                    result = new SystemMessaging(MesagesCode.Delete, "Folder doesn't exist");
                    return BadRequest(result);
                }

            }

            return BadRequest();
        }


        #endregion


    }
}
