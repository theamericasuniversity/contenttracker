using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using contenttracker.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Web;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Formatters;
using contenttracker.Models;

namespace contenttracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static SearchServiceClient _serviceClient;
        private static ISearchIndexClient _indexClient, _indexClientDoc;
        private static IConfigurationBuilder _builder;
        private static IConfigurationRoot _configuration;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult> Index(SearchData model)
        {
            try
            {
                // Ensure the search string is valid.
                if (model.searchText == null)
                {
                    model.searchText = "";
                }

                // Make the Azure Cognitive Search call.
                await RunQueryAsync(model);
            }

            catch
            {
                return View("Error", new ErrorViewModel { RequestId = "1" });
            }
            return View(model);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult IndexVideo()
        {
            return View(new SearchDataVideo());
        }

        public IActionResult IndexDocument()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> IndexDocument(SearchDataDocument modeldoc)
        {
            try
            {
                // Ensure the search string is valid.
                if (modeldoc.searchText == null)
                {
                    modeldoc.searchText = "";
                }

                // Make the Azure Cognitive Search call.
                await RunQueryDocumentAsync(modeldoc);
            }

            catch
            {
                return View("Error", new ErrorViewModel { RequestId = "1" });
            }
            return View(modeldoc);
        }

        [HttpPost]
        public async Task<ActionResult> IndexVideo(SearchDataVideo model)
        {
            try
            {
                // Ensure the search string is valid.
                if (model.searchText == null)
                {
                    model.searchText = "";
                }

                // Make the Azure Video Indexer search call.
                await RunQueryVideoAsync(model);
            }

            catch(Exception e)
            {
                return View("Error", new ErrorViewModel { RequestId = "1" });
            }
            return View(model);
        }

        private void InitSearch()
        {
            // Create a configuration using the appsettings file.
            _builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            _configuration = _builder.Build();

            // Pull the values from the appsettings.json file.
            string searchServiceName = _configuration["SearchServiceName"];
            string queryApiKey = _configuration["SearchServiceQueryApiKey"];

            // Create a service and index client.
            _serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(queryApiKey));
            _indexClient = _serviceClient.Indexes.GetClient("azuresql-index");
            _indexClientDoc = _serviceClient.Indexes.GetClient("azureblob-index");
        }

        private async Task<string> GetVideoIndexerAccessToken()
        {
            HttpClient _httpClientAccessToken = new HttpClient();
            _builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            _configuration = _builder.Build();
            string uriAccessToken = _configuration["UriAccessToken"];
            string ocpApimSubscriptionKey = _configuration["OcpApimSubscriptionKey"];
            _httpClientAccessToken.DefaultRequestHeaders.Add("x-ms-client-request-id", "");
            _httpClientAccessToken.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ocpApimSubscriptionKey);
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["allowEdit"] = "true";

            try
            {
                var response = await _httpClientAccessToken.GetAsync(uriAccessToken + queryString);
                var content = await response.Content.ReadAsStringAsync();
                return content.ToString().Trim('"');
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task<ActionResult> RunQueryAsync(SearchData model)
        {
            InitSearch();

            var parameters = new SearchParameters
            {
                // Enter content property names into this list so only these values will be returned.
                // If Select is empty, all values will be returned, which can be inefficient.
                Select = new[] { "Content", "DisciplineName", "ID" }
            };

            // For efficiency, the search call should be asynchronous, so use SearchAsync rather than Search.
            try
            {
                model.resultList = await _indexClient.Documents.SearchAsync<ActualContent>(model.searchText, parameters);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            // Display the results.
            return View("Index", model);
        }

        private async Task<ActionResult> RunQueryVideoAsync(SearchDataVideo model)
        {
            // Retrieving the access token from the service
            string accessToken = await GetVideoIndexerAccessToken();

            // Setting up HTTP client request
            HttpClient _httpClientSearchVideos = new HttpClient();

            // Pulling off video indexer API information
            _builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            _configuration = _builder.Build();
            string uriSearchVideos = _configuration["UriSearchVideos"];
            string ocpApimSubscriptionKey = _configuration["OcpApimSubscriptionKey"];

            // Setting up request headers
            _httpClientSearchVideos.DefaultRequestHeaders.Add("x-ms-client-request-id", "");
            _httpClientSearchVideos.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ocpApimSubscriptionKey);

            // Request parameters
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            queryString["query"] = model.searchText;
            queryString["language"] = "pt-BR";
            queryString["pageSize"] = "100";
            queryString["skip"] = "0";
            queryString["accessToken"] = accessToken;

            try
            {
                var response = await _httpClientSearchVideos.GetAsync(uriSearchVideos + queryString);
                var result = JsonConvert.DeserializeObject<RootVideo>(await response.Content.ReadAsStringAsync());
                model.resultVideos = result.results;
                return View(model);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private async Task<ActionResult> RunQueryDocumentAsync(SearchDataDocument model)
        {
            InitSearch();

            var parameters = new SearchParameters
            {
                // Enter content property names into this list so only these values will be returned.
                // If Select is empty, all values will be returned, which can be inefficient.
                Select = new[] { "content", "metadata_author", "metadata_storage_path", "metadata_title" }
            };

            // For efficiency, the search call should be asynchronous, so use SearchAsync rather than Search.
            try
            {
                model.resultList = await _indexClientDoc.Documents.SearchAsync<ContentDocument>(model.searchText, parameters);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            // Display the results.
            return View("IndexDocument", model);
        }

        public async Task<ActionResult> Details(string id)
        {
            InitSearch();

            try
            {
                var _singleDocument = await _indexClient.Documents.GetAsync<ActualContent>(id);
                return PartialView("_DetailsModal", _singleDocument);
            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }
        }

        public async Task<ActionResult> DetailsVideo(string id)
        {
            SearchDataVideo model = new SearchDataVideo();

            // Retrieving the access token from the service
            string accessToken = await GetVideoIndexerAccessToken();

            // Setting up HTTP client request
            HttpClient _httpClientSearchVideos = new HttpClient();

            // Pulling off video indexer API information
            _builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            _configuration = _builder.Build();
            string uriSearchVideos = _configuration["UriSearchVideos"];
            string ocpApimSubscriptionKey = _configuration["OcpApimSubscriptionKey"];

            // Setting up request headers
            _httpClientSearchVideos.DefaultRequestHeaders.Add("x-ms-client-request-id", "");
            _httpClientSearchVideos.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ocpApimSubscriptionKey);

            // Request parameters
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["id"] = id;
            queryString["accessToken"] = accessToken;

            try
            {
                var response = await _httpClientSearchVideos.GetAsync(uriSearchVideos + queryString);
                var result = JsonConvert.DeserializeObject<RootVideo>(await response.Content.ReadAsStringAsync());
                model.resultVideos = result.results;
                return PartialView("_DetailsVideoModal", model);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<ActionResult> DetailsDocument(string id)
        {
            InitSearch();

            try
            {
                var _singleDocument = await _indexClientDoc.Documents.GetAsync<ContentDocument>(id);
                return PartialView("_DetailsDocumentModal", _singleDocument);
            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
