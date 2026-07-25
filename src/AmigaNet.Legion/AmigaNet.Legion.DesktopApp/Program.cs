using System.Reflection;

namespace AmigaNet.Legion.DesktopApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            // dotnet core sets current directory to the src folder by default
            // we need to change it to the folder where executable file location is
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

            var libsLoader = new MonoGameLibLoader();
            libsLoader.LoadLibs();

            var langId = "pl";
            var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "../../../../../../original/legion");
            var scale = 2.0f;

            if (args.Length >= 1)
                langId = args[0];
            if (args.Length >= 2)
                dataPath = args[1];
            if (args.Length >= 3)
                float.TryParse(args[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out scale);

            var resourcesPath = Path.Combine(Directory.GetCurrentDirectory(), "data", langId);

            using (var game = new LegionGame(langId, resourcesPath, dataPath, scale))
            {
                game.Run();
            }
        }
    }
}



