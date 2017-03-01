using System.Reflection;

namespace RedArrow.Argo
{
    public partial class ModuleWeaver
    {
	    private void PrintIntro()
		{
			LogInfo(@"     _");
			LogInfo(@"    / \   _ __ __ _  ___");
			LogInfo(@"   / _ \ | '__/ _` |/ _ \");
			LogInfo(@"  / ___ \| | | (_| | (_) |");
			LogInfo(@" /_/   \_\_|  \__, |\___/");
			LogInfo(@"              |___/");
			LogInfo($"                v{typeof(ModuleWeaver).GetTypeInfo().Assembly.GetName().Version}");
		}
    }
}
