using System.Drawing;
using System.Net.Http.Json;
namespace ScreenTest;
public static class OLLamaApi
{
    public async static Task<string> GetResponse(byte[] data)
    {
        HttpClient httpClient = new HttpClient();
        var model = new ApiModel();
        var b64img = Convert.ToBase64String(data);
        model.images[0] = b64img;

        httpClient.BaseAddress = new Uri("http://localhost:11434/");
        var result = await httpClient.PostAsJsonAsync("/api/generate", model);
        ResponseModel reply = (ResponseModel)await result.Content.ReadFromJsonAsync(typeof(ResponseModel));
        return reply.response;
    }
}