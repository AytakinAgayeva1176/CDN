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
    public class FolderController : Controller
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
        [HttpPost("CreateFolder")]
        public async Task<IActionResult> CreateFolder(string name, string access)
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
                //HttpContext.Session.SetString("Token", items.Token);
                return Json(item);
            }

            throw new Exception("Bad request");
        }


        #endregion


        #region GetByUnie_Id / FindOne

        [HttpPost("GetFolderByUniqId ")]
        public async Task<IActionResult> GetFolderByUniqId(string uniq_id)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("xc-auth", tokenSettings.token);
            var json = await client.GetAsync(uri+"/findOne?where=(uniq_id,like," + uniq_id + ")");
            if (json.IsSuccessStatusCode)
            {
                var EmpResponse = json.Content.ReadAsStringAsync().Result;
                var item = JsonConvert.DeserializeObject<Folder>(EmpResponse);
                return Json(item);

            }
            throw new Exception(message: "Bad request");
        }

        #endregion


        #region DeleteFolder

        [HttpDelete("DeleteFolder")]
        public async Task<SystemMessaging> DeleteFolder(string uniq_id)
        {
            SystemMessaging result;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("xc-auth", tokenSettings.token);
            var folder = await client.GetAsync(uri+"/findOne?where=(uniq_id,like," + uniq_id + ")");
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

                        }
                        else
                        {
                            result = new SystemMessaging(MesagesCode.Delete, "Folder couldn't deleted!");

                        }
                        return result;
                    }
                }
                else
                {
                    result = new SystemMessaging(MesagesCode.Delete, "Folder doesn't exist");
                    return result;
                }

            }

            throw new Exception("Bad request");
        }


        #endregion


    }
}
