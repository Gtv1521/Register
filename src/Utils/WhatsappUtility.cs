using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CloudinaryDotNet;
using FrameworkDriver_Api.src.Dto;
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
                request.AddParameter("body", Message);
                await client.ExecuteAsync(request);
                foreach (var url in image)
                {
                    var requestImage = new RestRequest($"{_url}/image", Method.Post);
                    requestImage.AddHeader("content-type", "application/x-www-form-urlencoded");
                    requestImage.AddParameter("token", _accessToken);
                    requestImage.AddParameter("to", $"+57{Destiny}");
                    requestImage.AddParameter("image", url.Photo);
                    await client.ExecuteAsync(requestImage);
                    await Task.Delay(800);
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