using FrameworkDriver_Api.src.Models;
using FrameworkDriver_Api.src.Utils.Interfaces;
using Microsoft.Extensions.Options;
using RestSharp;

namespace FrameworkDriver_Api.src.Utils
{
    public class WhatsappUtility : WhatsappInterface
    {
        private readonly string _url;
        private readonly string _accessToken;
        private ILogger<WhatsappUtility> _logger;

        private bool IsVideo(string url)
        {
            return url.EndsWith(".mp4") ||
                   url.EndsWith(".mov") ||
                   url.EndsWith(".avi") ||
                   url.EndsWith(".webm");
        }



        public WhatsappUtility(IOptions<WhatsappModel> options, ILogger<WhatsappUtility> logger)
        {

            try
            {
                _url = options.Value.Url;
                _accessToken = options.Value.AccessToken;
                _logger = logger;
            }
            catch (System.Exception ex)
            {
                throw new Exception($"Failed connecting to Whastapp: {ex.Message}");
            }

        }
        public async Task<bool> SendMenssageAsync(string Message, string Destiny, List<PhotosModel> image)
        {
            var client = new RestClient(_url);
            try
            {
                var request = new RestRequest($"{_url}/chat", Method.Post);
                request.AddParameter("token", _accessToken);
                request.AddParameter("to", $"+57{Destiny}");
                request.AddParameter("body", 
                $@"
                    *Observacion:*
                    {Message}
                    "
                );
                await client.ExecuteAsync(request);
                if (image != null)
                {
                    foreach (var url in image)
                    {
                        string endpoint = IsVideo(url.Photo) ? "video" : "image";

                        var requestImage = new RestRequest($"{_url}/{endpoint}", Method.Post);
                        requestImage.AddHeader("content-type", "application/x-www-form-urlencoded");
                        requestImage.AddParameter("token", _accessToken);
                        requestImage.AddParameter("to", $"+57{Destiny}");
                        requestImage.AddParameter(endpoint, url.Photo);
                        await client.ExecuteAsync(requestImage);
                        await Task.Delay(800);
                    }
                }
                _logger.LogInformation("mensaje enviado correctamente");
                return true;
            }
            catch (System.Exception ex)
            {
                _logger.LogInformation("Ha ocurrido un error al enviar los mensajes" + ex);

                throw new Exception("Ha ocurrido el error " + ex.Message);
            }
        }
    }
}