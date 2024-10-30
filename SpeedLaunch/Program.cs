
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace SpeedLaunch
{
    static class Program
    {
        public static string name = "SpeedLaunch";

        public static Log log = new Log();

        // IS_DEBUG_MODE
        public static bool isDebugMode()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }

        public static SpeedLaunch speedLaunch = new SpeedLaunch();
        public static List<Assembly> assemblies = new List<Assembly>();
        public static List<Plugin> plugins = new List<Plugin>();
        
        public static void LoadPlugins() {
            List<string> directories = new List<string>();

            string localPluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
            if (Directory.Exists(localPluginPath)) {
                directories.Add(localPluginPath);
            }


            string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string roamingAppPath = Path.Combine(roamingPath, Program.name);
            string roamingPluginPath = Path.Combine(roamingAppPath, "plugins");
            if (Directory.Exists(roamingPluginPath))
            {
                directories.Add(roamingPluginPath);
            }

            try
            {
                try
                {
                    foreach (string dir in directories)
                    {
                        string[] dllFiles = Directory.GetFiles(dir, "*.dll", SearchOption.AllDirectories);

                        foreach (string dllPath in dllFiles)
                        {
                            Assembly assembly = Assembly.LoadFrom(dllPath);
                            assemblies.Add(assembly);

                            Type pluginInterface = typeof(Plugin);

                            Type[] types = assembly.GetTypes();
                            foreach (Type type in types)
                            {
                                if (pluginInterface.IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
                                {
                                    object instance = Activator.CreateInstance(type);
                                    Plugin plugin = (Plugin)instance;
                                    plugins.Add(plugin);
                                    plugin.InitAction(log, dllPath);
                                }
                            }
                        }
                    }
                }
                catch (DirectoryNotFoundException ex)
                {
                    Console.WriteLine($"Directory not found: {ex.Message}");
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"IO error: {ex.Message}");
                }         
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("DLL not found: " + ex.Message);
            }
            catch (BadImageFormatException ex)
            {
                Console.WriteLine("Invalid DLL format: " + ex.Message);
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.WriteLine("Error loading types: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("General error: " + ex.Message);
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            LoadPlugins();

            Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(speedLaunch);

            foreach (Plugin plugin in plugins)
            { 
                plugin.CloseAction();
            }
        }
    }
}
