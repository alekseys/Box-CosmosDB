using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Box.Samples.WealthManagement.Azure.Component;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Box.Samples.WealthManagement.Azure.ASP.MVC.Models;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Box.Samples.WealthManagement.Azure.ASP.MVC.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration configuration;
        private AppSettings appSettings;
        private DocumentClient documentClient;

        public HomeController(IConfiguration configuration, IOptions<AppSettings> options, DocumentClient documentClient)
        {
            this.configuration = configuration;
            this.appSettings = options.Value;
            this.documentClient = documentClient;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                UserData userData = null;

                var urlReferrer = Request.Headers["Referer"].ToString();
                ClaimsIdentity identity = User.Identity as ClaimsIdentity;
                var signUpUrlReferrer = appSettings.AadB2CSignupUrlReferrer;
                var isNewUser = identity.Claims.FirstOrDefault(x => x.Type == "newUser")?.Value;

                // Create new user in Box by setting external id to AAD object id and get the Box user id
                // Sign in starts with https://login.microsoftonline.com/<YOUR_AZUREAD_ACCOUNT>.onmicrosoft.com/B2C_1_SiUpIn/api/CombinedSigninAndSignup/
                if (isNewUser == "true" && (!string.IsNullOrEmpty(urlReferrer) && urlReferrer.StartsWith(signUpUrlReferrer)))
                {
                    var user = new User(appSettings, documentClient);
                    userData = await user.PostSignUpHandlerAsync(identity);
                }
                HttpContext.Session.SetObjectAsJson("UserData", userData);
            }
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [Authorize]
        public async Task<IActionResult> Documents()
        {
            var userData = HttpContext.Session.GetObjectFromJson<UserData>("UserData");
            ViewData["Message"] = "Your documents.";
            ClaimsIdentity identity = User.Identity as ClaimsIdentity;
           
            if(userData == null)
            {
                var databaseName = appSettings.CosmosDb.DatabaseId;
                var userCollection = appSettings.CosmosDb.UsersCollection;
                var aadUserId = identity.Claims.FirstOrDefault(x => x.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

                var userRepository = new UserRepository(documentClient, databaseName, userCollection);
                userData = await userRepository.GetAsync(aadUserId);
            }

            var boxManager = new BoxManager(appSettings.Box.ConfigFilePath);
            var token = boxManager.GetToken(userData.BoxUserId);
            ViewData["Token"] = token;
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";
            return View();
        }

        public IActionResult Statements()
        {
            ViewData["Message"] = "Your statements";
            return View();
        }

        public IActionResult TaxForms()
        {
            ViewData["Message"] = "Your tax forms.";
            return View();
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

        public async Task AddFiles(string files)
        {
            var fileList = JsonConvert.DeserializeObject<List<FileData>>(files);
            //Add to Cosmos DB
            var databaseName = appSettings.CosmosDb.DatabaseId;
            var filesCollection = appSettings.CosmosDb.FilesCollection;

            var filesRepository = new FileRepository(documentClient, databaseName, filesCollection);

            foreach (var file in fileList)
            {
                await filesRepository.AddAsync(file);
            }
        }
    }
}
