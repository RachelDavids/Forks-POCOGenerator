namespace POCOGenerator
{
	internal sealed partial class GeneratorSettings
	{
		private sealed class FunctionsSettings : DbObjectsSettingsBase, IFunctions
		{
			internal FunctionsSettings(object lockObject)
				: base(lockObject)
			{
			}
		}
	}
}
