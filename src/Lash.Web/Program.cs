using Lash.Web.Configuration;
using Lash.Web.Extensions;

EnvironmentConfiguration.Load();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLashWeb(builder.Configuration);

var app = builder.Build();
await app.Services.InitializeLashModulesAsync();
app.UseLashWeb();
app.MapLashEndpoints();
app.Run();

public partial class Program
{
}
