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

            rootCommand.SetHandler((file) => ConvertToDockerEnv(file!), fileOption);

            return await rootCommand.InvokeAsync(args);
        }

        static void ConvertToDockerEnv(string appSettingFile)
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

            var contents = File.ReadAllText(appSettingFile);

            var builder = new ConfigurationBuilder();

            using var stream = new MemoryStream(contents.Length);
            using var sw = new StreamWriter(stream);

            sw.Write(contents);
            sw.Flush();
            stream.Position = 0;

            builder.AddJsonStream(stream);

            var configurationRoot = builder.Build();

            var keyPairs = configurationRoot
                .AsEnumerable()
                .Where(pair => !string.IsNullOrEmpty(pair.Value))
                .OrderBy(pair => pair.Key);

            var convertedSettings = new StringBuilder();

            foreach ((string key, string value) in keyPairs)
            {
                string key2 = key.Replace(":", "__");
                convertedSettings.AppendFormat("{0}={1}", key2, value);
                convertedSettings.AppendLine();
            }

            Console.Write(convertedSettings.ToString());
        }

    }
}