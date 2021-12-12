using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Lwt.Models;
using Newtonsoft.Json;

namespace Lwt.Clients;

public class JapaneseSegmenterClient : IJapaneseSegmenterClient
{
    private readonly HttpClient httpClient;
    private readonly AppSettings appSettings;

    public JapaneseSegmenterClient(AppSettings appSettings)
    {
        this.appSettings = appSettings;
        this.httpClient = new HttpClient();
    }

    public async Task<IEnumerable<string>> CutAsync(string text)
    {
        var body = new Dictionary<string, string> {{"text", text}};
        HttpResponseMessage response = await this.httpClient.PostAsync(
            $"{this.appSettings.JapaneseSegmenterEndpoint}/cut",
            new StringContent(
                JsonConvert.SerializeObject(body),
                Encoding.UTF8,
                "application/json"));

        if (response.IsSuccessStatusCode)
        {
            var result = JsonConvert.DeserializeObject<IEnumerable<string>>(await response.Content.ReadAsStringAsync());

            if (result is null)
            {
                throw new Exception("unexpected japanese segmentation result");
            }

            return result;
        }

        throw new Exception("failed to cut japanese text");
    }
}