using System.Text.Json.Serialization;
using GraphiGrade.Judge.Extensions.DependencyInjection;

namespace GraphiGrade.Judge
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            PrintASCIIArt();

            builder.ConfigureDatabase();

            builder.Services.AddJudgeServices(builder.Configuration);

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });
            
            var app = builder.Build();

            // Configure the HTTP request pipeline.

            //app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static void PrintASCIIArt()
        {
            string asciiArt =
                @"   _____                 _     _  _____               _            _           _            
  / ____|               | |   (_)/ ____|             | |          | |         | |           
 | |  __ _ __ __ _ _ __ | |__  _| |  __ _ __ __ _  __| | ___      | |_   _  __| | __ _  ___ 
 | | |_ | '__/ _` | '_ \| '_ \| | | |_ | '__/ _` |/ _` |/ _ \ _   | | | | |/ _` |/ _` |/ _ \
 | |__| | | | (_| | |_) | | | | | |__| | | | (_| | (_| |  __/| |__| | |_| | (_| | (_| |  __/
  \_____|_|  \__,_| .__/|_| |_|_|\_____|_|  \__,_|\__,_|\___(_)____/ \__,_|\__,_|\__, |\___|
                  | |                                                             __/ |     
                  |_|                                                            |___/      ";

            Console.WriteLine(asciiArt);
            Console.WriteLine($"GraphiGrade.Judge starting up");
        }
    }
}
