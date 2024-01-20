namespace POCOGenerator
{
	/// <summary>The settings determine the database connection, what database objects will be generated to classes and how the POCO classes will be generated.</summary>
	public interface ISettings
	{
		/// <summary>Resets all settings to their default values.</summary>
		void Reset();

		/// <summary>Gets the Connection settings.</summary>
		IConnection Connection { get; }

		/// <summary>Gets the POCO settings.</summary>
		IPOCO POCO { get; }

		/// <summary>Gets the class name settings.</summary>
		IClassName ClassName { get; }

		/// <summary>Gets the navigation properties settings.</summary>
		INavigationProperties NavigationProperties { get; }

		/// <summary>Gets the EF annotations settings.</summary>
		IEFAnnotations EFAnnotations { get; }

		/// <summary>Gets the settings that determine which database objects to generate classes out of and which database objects to exclude from generating classes.</summary>
		IDatabaseObjects DatabaseObjects { get; }

		/// <summary>Gets the settings that determine the colors for syntax elements.</summary>
		ISyntaxHighlight SyntaxHighlight { get; }
	}
}
