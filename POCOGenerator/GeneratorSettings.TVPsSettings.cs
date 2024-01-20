
namespace POCOGenerator
{
	internal sealed partial class GeneratorSettings
	{
		private sealed class TVPsSettings
			: DbObjectsSettingsBase, ITVPs
		{
			internal TVPsSettings(object lockObject)
				: base(lockObject)
			{
			}
		}
	}
}
