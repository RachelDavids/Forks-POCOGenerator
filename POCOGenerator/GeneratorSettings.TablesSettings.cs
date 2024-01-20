namespace POCOGenerator
{
	internal sealed partial class GeneratorSettings
	{
		private sealed class TablesSettings
			: DbObjectsSettingsBase, ITables
		{
			internal TablesSettings(object lockObject)
				: base(lockObject)
			{
			}
		}
	}
}
