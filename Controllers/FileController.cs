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
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
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
        /// <summary>
        /// Create File
        /// </summary>
        /// <param name="name"> File Name </param>
        /// /// <param name="related_folder">Related folder</param>
        /// /// <param name="file">File</param>
        /// <returns></returns>
        [HttpPost("Create")]
        public async Task<IActionResult> Create(string name, string related_folder, IFormFile file)
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
                #region FTP
                FtpWebRequest request =
                (FtpWebRequest)WebRequest.Create(ftpSettings.ServerLink + related_folder + "/" + model.path);
                request.Credentials = new NetworkCredential(ftpSettings.UserName, ftpSettings.Password);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                using (Stream ftpStream = request.GetRequestStream())
                {
                    file.CopyTo(ftpStream);
                }
                #endregion
                var EmpResponse = json.Content.ReadAsStringAsync().Result;
                var item = JsonConvert.DeserializeObject<File_>(EmpResponse);
                return Ok(item);
            }

            return BadRequest();
        }

        #endregion


        #region GetByUnique Id / FindOne
        /// <summary>
        /// Get File By Unique Id
        /// </summary>
        /// <param name="uniq_id"> Unique id of File </param>
        /// <returns></returns>
        [HttpPost("GetByUniqId/{uniq_id}")]
        public async Task<IActionResult> GetByUniqId(string uniq_id)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("xc-auth", tokenSettings.token);
            var json = await client.GetAsync(uri + "/findOne?where=(uniq_id,like," + uniq_id + ")");
            if (json.IsSuccessStatusCode)
            {
                var EmpResponse = json.Content.ReadAsStringAsync().Result;
                var item = JsonConvert.DeserializeObject<File_>(EmpResponse);
                if (item == null) return Ok(new SystemMessaging(MesagesCode.NotFound, "File doesn't exist"));
                else return Ok(item);
            }
            return BadRequest();
        }


        #endregion


        #region Delete
        /// <summary>
        /// Delete File By Unique Id
        /// </summary>
        /// <param name="uniq_id"> Unique id of File </param>
        /// <returns></returns>
        [HttpDelete("Delete/{uniq_id}")]
        public async Task<IActionResult> Delete(string uniq_id)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("xc-auth", tokenSettings.token);
            var folder = await client.GetAsync(uri + "/findOne?where=(uniq_id,like," + uniq_id + ")");

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
                        if (EmpResponse == "1") return Ok(new SystemMessaging(MesagesCode.Delete, "File deleted succesfully!",item));
                        else return BadRequest(new SystemMessaging(MesagesCode.Exception, "File couldn't deleted!",item));
                    }
                }
                else return BadRequest(new SystemMessaging(MesagesCode.NotFound, "File doesn't exist"));
            }

            return BadRequest();
        }


        #endregion


    }
}
