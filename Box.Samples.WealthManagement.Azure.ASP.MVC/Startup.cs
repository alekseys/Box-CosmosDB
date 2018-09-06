using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Box.Samples.WealthManagement.Azure.Component;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Box.Samples.WealthManagement.Azure.ASP.MVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        private DocumentClient documentClient;
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddAuthentication(AzureADB2CDefaults.AuthenticationScheme)
                .AddAzureADB2C(options => Configuration.Bind("AzureAdB2C", options));

            services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(20);//You can set Time
            });

            services.AddOptions();
            services.Configure<AppSettings>(Configuration.GetSection("App"));

            //Add to Cosmos DB
            var endpointUri = Configuration["App:CosmosDb:EndpointUrl"];
            var primaryKey = Configuration["App:CosmosDb:PrimaryKey"];
            var databaseName = Configuration["App:CosmosDb:DatabaseId"];
            var userCollection = Configuration["App:CosmosDb:UsersCollection"];
            var filesCollection = Configuration["App:CosmosDb:FilesCollection"];

            // Creating a new client instance
            documentClient = new DocumentClient(new Uri(endpointUri), primaryKey);

            documentClient.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseName }).Wait();

            // Create any collections.
            var userRepository = new UserRepository(documentClient, databaseName, userCollection);
            userRepository.CreateCollectionIfNotExists();

            var fileRepository = new FileRepository(documentClient, databaseName, filesCollection);
            fileRepository.CreateCollectionIfNotExists();

            services.AddSingleton(documentClient);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();
            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
