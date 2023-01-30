using Microsoft.Identity.Web;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;

namespace BlazorClient.Data
{
    public static class WeatherForecastServiceExtensions
    {
        public static void AddWeatherForecastService(this IServiceCollection services, IConfiguration configuration)
        {
            // https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
            services.AddHttpClient<WeatherForecastService>();
        }
    }
    public class WeatherForecastService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly HttpClient _httpClient;
        private readonly string _weatherForecastListScope = string.Empty;
        private readonly string _weatherForecastListBaseAddress = string.Empty;
        private readonly ITokenAcquisition _tokenAcquisition;

        public WeatherForecastService(ITokenAcquisition tokenAcquisition, HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor contextAccessor)
        {
            _httpClient = httpClient;
            _tokenAcquisition = tokenAcquisition;
            _contextAccessor = contextAccessor;
            _weatherForecastListScope = configuration["TodoList:TodoListScope"];
            _weatherForecastListBaseAddress = configuration["TodoList:TodoListBaseAddress"];
        }

        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public async Task<WeatherForecast[]> GetForecastAsync(DateTime startDate)
        {
            await PrepareAuthenticatedClient();
            var response = await _httpClient.GetAsync($"{_weatherForecastListBaseAddress}/WeatherForecast");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                //IEnumerable<ToDo> todolist = JsonConvert.DeserializeObject<IEnumerable<ToDo>>(content);
                return new WeatherForecast[0];
            }

            throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}.");
        }

        /// <summary>
        /// Retrieves the Access Token for the Web API.
        /// Sets Authorization and Accept headers for the request.
        /// </summary>
        /// <returns></returns>
        private async Task PrepareAuthenticatedClient()
        {
            var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(new[] { _weatherForecastListScope });
            //Debug.WriteLine($"access token-{accessToken}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}