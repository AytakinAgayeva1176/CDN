using CDN.Helper;
using CDN.Models;
using CDN.ViewModels;
using FluentFTP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace CDN.Controllers
{
    public class FileController : Controller
    {
        #region ctor
        private readonly TokenSettings tokenSettings;
        private readonly FtpSettings ftpSettings;

        private readonly string uri = "http://172.16.10.132:3574/nc/ferrum_cdn_r8o0/api/v1/f_index";
        public FileController(TokenSettings tokenSettings, FtpSettings ftpSettings)
        {
            this.tokenSettings = tokenSettings;
            this.ftpSettings = ftpSettings;
        }

        #endregion



        #region Create

        [HttpPost("CreateFile")]
        public async Task<IActionResult> CreateFile(string related_folder, string name, IFormFile file)
        {

            string contentType;
            new FileExtensionContentTypeProvider().TryGetContentType(file.FileName, out contentType);
            var extension = Path.GetExtension(file.FileName);
            FileVM model = new FileVM()
            {
                uniq_id = Guid.NewGuid().ToString(),
                name = name,
                type = contentType,
                size = file.Length.ToString(),
                related_folder = related_folder
            };

            model.path = model.uniq_id + extension;
            model.external_link = "https://cdn.ferrumcapital.az/" + related_folder + "/" + model.uniq_id + extension;


            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("xc-auth", tokenSettings.token);
            var data = JsonConvert.SerializeObject(model);
            StringContent queryString = new StringContent(data, Encoding.UTF8, "application/json");
            var json = await client.PostAsync(uri, queryString);

            if (json.IsSuccessStatusCode)
            {

                FtpWebRequest request =
                (FtpWebRequest)WebRequest.Create(ftpSettings.ServerLink + related_folder + "/" + model.path);
                request.Credentials = new NetworkCredential(ftpSettings.UserName, ftpSettings.Password);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                using (Stream ftpStream = request.GetRequestStream())
                {
                    file.CopyTo(ftpStream);
                }

                var EmpResponse = json.Content.ReadAsStringAsync().Result;
                var item = JsonConvert.DeserializeObject<File_>(EmpResponse);
                return Json(item);
            }

            throw new Exception("Bad request");
        }

        #endregion


        #region GetByUniq_Id / FindOne

        [HttpPost("GetFileByUniqId")]
        public async Task<IActionResult> GetFileByUniqId(string uniq_id)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("xc-auth", tokenSettings.token);
            var json = await client.GetAsync(uri + "/findOne?where=(uniq_id,like," + uniq_id + ")");
            if (json.IsSuccessStatusCode)
            {
                var EmpResponse = json.Content.ReadAsStringAsync().Result;
                var item = JsonConvert.DeserializeObject<File_>(EmpResponse);
                return Json(item);
            }
            throw new Exception(message: "Bad request");
        }


        #endregion


        #region Delete

        [HttpDelete("DeleteFile")]
        public async Task<SystemMessaging> DeleteFile(string uniq_id)
        {
            SystemMessaging result;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("xc-auth", tokenSettings.token);
            var folder = await client.GetAsync(uri +  "/findOne?where=(uniq_id,like," + uniq_id + ")");

            if (folder.IsSuccessStatusCode)
            {
                var EmpResponse = folder.Content.ReadAsStringAsync().Result;
                var item = JsonConvert.DeserializeObject<File_>(EmpResponse);
                if (item != null)
                {
                    var json = await client.DeleteAsync(uri + "/" + item.id);

                    if (json.IsSuccessStatusCode)
                    {
                        EmpResponse = json.Content.ReadAsStringAsync().Result;
                        if (EmpResponse == "1")
                        {
                            result = new SystemMessaging(MesagesCode.Delete, "File deleted succesfully!");

                        }
                        else
                        {
                            result = new SystemMessaging(MesagesCode.Delete, "File couldn't deleted!");

                        }
                        return result;
                    }
                }
                else
                {
                    result = new SystemMessaging(MesagesCode.Delete, "File doesn't exist");
                    return result;
                }

            }

            throw new Exception("Bad request");
        }


        #endregion


    }
}
