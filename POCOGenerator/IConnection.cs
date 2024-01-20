namespace POCOGenerator
{
	/// <summary>The settings determine the connection to the RDBMS server.</summary>
	public interface IConnection
	{
		/// <summary>Resets the connection settings to their default values.</summary>
		void Reset();

		/// <summary>
		/// Gets or sets the connection string.
		/// <para>When <see cref="RDBMS" /> is set to <see cref="RDBMS.None" />, the generator will try to determine the <see cref="RDBMS" /> based on the connection string.</para></summary>
		string ConnectionString { get; set; }

		/// <summary>
		/// Gets or sets the relational database management system.
		/// <para>When this setting is set to <see cref="RDBMS.None" />, the generator will try to determine this setting based on the connection string.</para></summary>
		RDBMS RDBMS { get; set; }
	}
}
