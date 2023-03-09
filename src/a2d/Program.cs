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

            var emptyOption = new Option<bool>(
                name: "--empty",
                description: "Export empty-valued settings",
                getDefaultValue: () => false);

            var rootCommand = new RootCommand("a2d help");
            rootCommand.AddOption(fileOption);
            rootCommand.AddOption(emptyOption);

            rootCommand.SetHandler(async(file, empty) => await ConvertToDockerEnv(file!, empty!), fileOption, emptyOption);

            return await rootCommand.InvokeAsync(args);
        }

        static async Task ConvertToDockerEnv(string appSettingFile, bool exportEmptyParams)
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
                .Where(pair => exportEmptyParams || !string.IsNullOrEmpty(pair.Value))
                .Select(pair => new KeyValuePair<string, string?>(pair.Key.Replace(":", "__"), pair.Value))
                .OrderBy(pair => pair.Key)
                .Select(pair => $"{pair.Key}={pair.Value}");

            var plainSettings = string.Join(Environment.NewLine, keyPairs);

            await Console.Out.WriteAsync(plainSettings);
        }

    }
}