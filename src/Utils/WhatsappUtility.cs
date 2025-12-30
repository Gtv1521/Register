using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Models;
using FrameworkDriver_Api.src.Utils.Interfaces;
using Microsoft.Extensions.Options;

namespace FrameworkDriver_Api.src.Utils
{
    public class WhatsappUtility : WhatsappInterface
    {
        private readonly string _url;
        private readonly string _accessToken;
        private readonly string _from;

        public WhatsappUtility(IOptions<WhatsappModel> options)
        {
            try
            {
                _url = options.Value.Url;
                _accessToken = options.Value.AccessToken;
                _from = options.Value.From;
            }
            catch (System.Exception ex)
            {
                throw new Exception($"Failed connecting to Whastapp: {ex.Message}");
            }

        }
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
            }
        }
    }
}