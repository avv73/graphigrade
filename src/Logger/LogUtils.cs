namespace GraphiGrade.Logger;

public static class LogUtils
{
    private const string AppAsciiArt = """
                                          _____                 _     _  _____               _      
                                         / ____|               | |   (_)/ ____|             | |     
                                        | |  __ _ __ __ _ _ __ | |__  _| |  __ _ __ __ _  __| | ___ 
                                        | | |_ | '__/ _` | '_ \| '_ \| | | |_ | '__/ _` |/ _` |/ _ \
                                        | |__| | | | (_| | |_) | | | | | |__| | | | (_| | (_| |  __/
                                         \_____|_|  \__,_| .__/|_| |_|_|\_____|_|  \__,_|\__,_|\___|
                                                         | |                                        
                                                         |_|                                        
                                       """;

    public static void PrintStartInfo(IHostApplicationBuilder builder)
    {
        Console.WriteLine(AppAsciiArt);
        Console.WriteLine($"GraphiGrade | ASPNETCORE_ENVIRONMENT: {builder.Environment.EnvironmentName}");
    }
}
