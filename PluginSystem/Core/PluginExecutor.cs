using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class PluginExecutor
    {
        public string LibraryDirectory { get; private set; }
        private Assembly[] assemblies;
        public PluginExecutor(string libDir)
        {
            LibraryDirectory = libDir;
            assemblies = LoadAssemblies(libDir);
        }

        public void ExecuteAllPlugins<TPlugin>(Action<TPlugin> Execute, object[] constructorParams) where TPlugin : class
        {
            foreach (var assembly in assemblies)
            {
                var plugins = assembly.GetTypes().Where(t => typeof(TPlugin).IsAssignableFrom(t) && !t.IsInterface);
                foreach (var pluginType in plugins)
                {
                    TPlugin plugin = Activator.CreateInstance(pluginType, constructorParams) as TPlugin;
                    Execute(plugin);
                }
            }
        }

        private Assembly[] LoadAssemblies(string libDir)
        {
            List<Assembly> tmp = new List<Assembly>();
            foreach (var file in Directory.EnumerateFiles(libDir))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(file);
                    tmp.Add(assembly);
                }
                catch (Exception)
                { }
            }
            return tmp.ToArray();
        }
    }
}
