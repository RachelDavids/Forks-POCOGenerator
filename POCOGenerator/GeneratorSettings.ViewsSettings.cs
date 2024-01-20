namespace POCOGenerator
{
	internal sealed partial class GeneratorSettings
	{
		private sealed class ViewsSettings : DbObjectsSettingsBase, IViews
		{
			internal ViewsSettings(object lockObject)
				: base(lockObject)
			{
			}
		}
	}
}
