using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;

namespace SecureFeatherHttpApi
{
    class Program
    { 
        static async Task Main(string[] args)
        {
            var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);
            builder.Services.AddMicrosoftWebApiAuthentication(builder.Configuration)
                .AddMicrosoftWebApiCallsWebApi(builder.Configuration)
                .AddInMemoryTokenCaches();

            builder.Services.AddAuthorization();
            var app = builder.Build();
            
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.MapGet("/api/todos", GetTodos).RequireAuthorization();
            app.MapPost("api/todos", CreateTodo).RequireAuthorization();
            app.MapGet("/api/me", GetGraphData).RequireAuthorization();

            await app.RunAsync();
        }

        static async Task CreateTodo(HttpContext http)
        {
            var todo = await http.Request.ReadJsonAsync<TodoItem>();
            http.Response.StatusCode = 204;
        }

        static async Task GetTodos(HttpContext http)
        {
            http.VerifyUserHasAnyAcceptedScope(new string[] {"access_as_user"});

            var todos = new List<TodoItem> 
            {
                new TodoItem{Id = 1, Name = "test", IsComplete = false}, 
                new TodoItem{Id=2, Name="hello", IsComplete=true}
            };
            await http.Response.WriteJsonAsync(todos);
        }

        static async Task GetGraphData(HttpContext http)
        {
            http.VerifyUserHasAnyAcceptedScope(new string[]{"access_as_user"});

            var tokenAcquisition = http.RequestServices.GetRequiredService<ITokenAcquisition>();

            var authProvider = new DelegateAuthenticationProvider(async x => {
                var accessToken = await tokenAcquisition.GetAccessTokenForUserAsync(new string[] {"User.Read"});
                x.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            });
            var graphClient = new GraphServiceClient(authProvider);
            var me = await graphClient.Me.Request().GetAsync();

            await http.Response.WriteJsonAsync(new {Name = me.GivenName, Email= me.Mail});
        }
    }
}
