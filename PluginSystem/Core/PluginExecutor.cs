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
        public PluginExecutor(string libDir, bool loadOnlySigned = false)
        {
            LibraryDirectory = libDir;
            assemblies = LoadAssemblies(libDir, loadOnlySigned);
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

        private Assembly[] LoadAssemblies(string libDir, bool loadOnlySigned)
        {
            List<Assembly> tmp = new List<Assembly>();
            byte[] ourPK = Assembly.GetExecutingAssembly().GetName().GetPublicKey();

            foreach (var file in Directory.EnumerateFiles(libDir))
            {
                try
                {
                    byte[] targetPK = AssemblyName.GetAssemblyName(file).GetPublicKey();
                    if (loadOnlySigned && !Enumerable.SequenceEqual(ourPK, targetPK))
                        continue;

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
