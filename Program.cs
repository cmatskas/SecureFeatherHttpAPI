using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;

namespace SecureFeatherHttpApi
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddMicrosoftWebApiAuthentication(builder.Configuration);
            builder.Services.AddAuthorization();
            var app = builder.Build();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.MapGet("/api/todos", GetTodos);
            app.MapPost("api/todos", CreateTodo);

            await app.RunAsync();
        }

        static async Task CreateTodo(HttpContext http)
        {
            var todo = await http.Request.ReadJsonAsync<TodoItem>();
            http.Response.StatusCode = 204;
        }

        static async Task GetTodos(HttpContext http)
        {
            http.VerifyUserHasAnyAcceptedScope(new string[] {"test"});

            var todos = new List<TodoItem> 
            {
                new TodoItem{Id = 1, Name = "test", IsComplete = false}, 
                new TodoItem{Id=2, Name="hello", IsComplete=true}
            };
            await http.Response.WriteJsonAsync(todos);
        }
    }
}
