using System;
using System.IO;
using System.Reflection;
using Mono.Cecil;

namespace RedArrow.Argo.Fody.Tests
{
    public class WeaverTestFixture
    {
        public string AssemblyPath { get; }
        public string WovenAssemblyPath { get; }

        public Assembly WovenAssembly { get; set; }

        public WeaverTestFixture()
        {
            var projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\WovenByTest\WovenByTest.csproj"));
            AssemblyPath = Path.Combine(Path.GetDirectoryName(projectPath), @"bin\Debug\WovenByTest.dll");

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