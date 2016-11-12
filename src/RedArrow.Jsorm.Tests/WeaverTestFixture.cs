using Mono.Cecil;
using System;
using System.IO;
using System.Reflection;

namespace RedArrow.Jsorm.Tests
{
    public class WeaverTestFixture
    {
        public string AssemblyPath { get; }
        public string WovenAssemblyPath { get; }

        public Assembly WovenAssembly { get; set; }

        public WeaverTestFixture()
        {
            var projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\AssemblyToWeave\AssemblyToWeave.csproj"));
            AssemblyPath = Path.Combine(Path.GetDirectoryName(projectPath), @"bin\Debug\AssemblyToWeave.dll");

#if (!DEBUG)
            AssemblyPath = AssemblyPath.Replace("Debug", "Release");
#endif
            WovenAssemblyPath = AssemblyPath.Replace(".dll", ".woven.dll");

            File.Copy(AssemblyPath, WovenAssemblyPath, true);

            var moduleDefinition = ModuleDefinition.ReadModule(WovenAssemblyPath);
            var weaver = new ModuleWeaver
            {
                ModuleDefinition = moduleDefinition,
                AssemblyResolver = moduleDefinition.AssemblyResolver
            };

            weaver.Execute();
            moduleDefinition.Write(WovenAssemblyPath);

            WovenAssembly = Assembly.LoadFile(WovenAssemblyPath);
        }
    }
}