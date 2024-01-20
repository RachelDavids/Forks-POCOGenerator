namespace POCOGenerator
{
	internal sealed partial class GeneratorSettings
	{
		private sealed class StoredProceduresSettings : DbObjectsSettingsBase, IStoredProcedures
		{
			internal StoredProceduresSettings(object lockObject)
				: base(lockObject)
			{
			}
		}
	}
}
