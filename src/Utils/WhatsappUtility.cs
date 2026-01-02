using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
<<<<<<< HEAD
using CloudinaryDotNet;
=======
>>>>>>> ef7d612 (update: funtions add branch Duvan)
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Models;
using FrameworkDriver_Api.src.Utils.Interfaces;
using Microsoft.Extensions.Options;
<<<<<<< HEAD
using RestSharp;
=======
>>>>>>> ef7d612 (update: funtions add branch Duvan)

namespace FrameworkDriver_Api.src.Utils
{
    public class WhatsappUtility : WhatsappInterface
    {
        private readonly string _url;
        private readonly string _accessToken;
<<<<<<< HEAD
        private ILogger<WhatsappUtility> _logger;


        public WhatsappUtility(IOptions<WhatsappModel> options, ILogger<WhatsappUtility> logger)
        {

=======
        private readonly string _from;

        public WhatsappUtility(IOptions<WhatsappModel> options)
        {
>>>>>>> ef7d612 (update: funtions add branch Duvan)
            try
            {
                _url = options.Value.Url;
                _accessToken = options.Value.AccessToken;
<<<<<<< HEAD
                _logger = logger;
=======
                _from = options.Value.From;
>>>>>>> ef7d612 (update: funtions add branch Duvan)
            }
            catch (System.Exception ex)
            {
                throw new Exception($"Failed connecting to Whastapp: {ex.Message}");
            }

        }
<<<<<<< HEAD
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
=======
        public async Task<bool> SendMenssageAsync(string Message, string Destiny)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

            var requestbody = new WhatsappMessageDTO
            {
                From = _from,
                To = $"57{Destiny}",
                Type = "template",
                Template = new SendZenTemplate
                {
                    name = "sandbox_order_notification",
                    lang_code = "en_US"
                }
            };
            var json = JsonSerializer.Serialize(requestbody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                var response = await client.PostAsync(_url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return true;
                return false;

            }
            catch (System.Exception ex)
            {
                throw new Exception($"Failed to send message : {ex.Message}");
>>>>>>> ef7d612 (update: funtions add branch Duvan)
            }
        }
    }
}