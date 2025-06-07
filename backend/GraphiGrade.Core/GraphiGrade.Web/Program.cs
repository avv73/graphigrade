using GraphiGrade.Business.Extensions.DependencyInjection;
using GraphiGrade.Web.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

namespace GraphiGrade.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        builder.Services.AddEndpointsApiExplorer();

        var config = builder.AddGraphiGradeConfig();

        builder.Services
            .AddSwaggerDocs()
            .AddGraphiGradeServices(config)
            .AddGraphiGradeAuthentication(config);

        builder.Services.AddHttpContextAccessor();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();

        app.UseAuthorization();

        PrettyPrintLogo();

        app.MapControllers();

        app.Run();
    }

    private static void PrettyPrintLogo()
    {
        const string asciiArt =
            @"   ____     ____        _       ____    _   _               ____     ____        _      ____  U _____ u 
U /""___|uU |  _""\ u U  /""\  u U|  _""\ u|'| |'|     ___   U /""___|uU |  _""\ u U  /""\  u |  _""\ \| ___""|/ 
\| |  _ / \| |_) |/  \/ _ \/  \| |_) |/| |_| |\   |_""_|  \| |  _ / \| |_) |/  \/ _ \/ /| | | | |  _|""   
 | |_| |   |  _ <    / ___ \   |  __/ U|  _  |u    | |    | |_| |   |  _ <    / ___ \ U| |_| |\| |___   
  \____|   |_| \_\  /_/   \_\  |_|     |_| |_|   U/| |\u   \____|   |_| \_\  /_/   \_\ |____/ u|_____|  
  _)(|_    //   \\_  \\    >>  ||>>_   //   \\.-,_|___|_,-._)(|_    //   \\_  \\    >>  |||_   <<   >>  
 (__)__)  (__)  (__)(__)  (__)(__)__) (_"") (""_)\_)-' '-(_/(__)__)  (__)  (__)(__)  (__)(__)_) (__) (__) ";

        Console.WriteLine(asciiArt);
        Console.WriteLine($"GraphiGrade Backend Starting");
    }
}
