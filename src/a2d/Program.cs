using Microsoft.Extensions.Configuration;
using System.CommandLine;
using System.Text;

namespace a2d
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var fileOption = new Option<string?>(
                name: "--appsettings",
                description: "The appsettings.json file to convert",
                getDefaultValue: () => "");

            var rootCommand = new RootCommand("a2d help");
            rootCommand.AddOption(fileOption);

            rootCommand.SetHandler(async(file) => await ConvertToDockerEnv(file!), fileOption);

            return await rootCommand.InvokeAsync(args);
        }

        static async Task ConvertToDockerEnv(string appSettingFile)
        {
            if (string.IsNullOrEmpty(appSettingFile)) 
            {
                Console.WriteLine("You must specify a valid appSettings file, cannot proceed to conversion");
                return;
            }

            var fInfo = new FileInfo(appSettingFile);
            if (!fInfo.Exists) 
            {
                Console.WriteLine("Specified appSettings file not exists, cannot proceed to conversion");
                return;
            }

            var contents = await File.ReadAllTextAsync(appSettingFile);

            var builder = new ConfigurationBuilder();

            using var stream = new MemoryStream(contents.Length);
            using var sw = new StreamWriter(stream);

            await sw.WriteAsync(contents);
            await sw.FlushAsync();
            stream.Position = 0;

            builder.AddJsonStream(stream);

            var keyPairs = builder.Build()
                .AsEnumerable()
                .Where(pair => !string.IsNullOrEmpty(pair.Value))
                .OrderBy(pair => pair.Key);

            var convertedSettings = new StringBuilder();

            foreach ((string key, string? value) in keyPairs)
            {
                convertedSettings
                    .AppendFormat("{0}={1}", key.Replace(":", "__"), value)
                    .AppendLine();
            }

            await Console.Out.WriteAsync(convertedSettings.ToString());
        }

    }
}