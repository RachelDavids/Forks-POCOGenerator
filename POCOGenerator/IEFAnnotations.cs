namespace POCOGenerator
{
	/// <summary>The settings for the Entity Framework annotations.</summary>
	public interface IEFAnnotations
	{
		/// <summary>Resets the Entity Framework annotations settings to their default values.</summary>
		void Reset();

		/// <summary>Gets or sets a value indicating whether Table, Key, MaxLength, Timestamp and DatabaseGenerated attributes are added to data members.</summary>
		bool Enable { get; set; }

		/// <summary>Gets or sets a value indicating whether Column attribute is added to data members.
		/// <para>This setting is applicable only when <see cref="Enable" /> is set to <c>true</c>.</para></summary>
		bool Column { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether Required attribute is added to data members that are not nullable.
		/// <para>This setting is applicable only when <see cref="Enable" /> is set to <c>true</c>.</para></summary>
		bool Required { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether Required attribute, with an error message, is added to data members that are not nullable.
		/// <para>This setting is applicable only when <see cref="Enable" /> is set to <c>true</c>.</para></summary>
		bool RequiredWithErrorMessage { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether ConcurrencyCheck attribute is added to Timestamp and RowVersion data members.
		/// <para>This setting is applicable only when <see cref="Enable" /> is set to <c>true</c>.</para></summary>
		bool ConcurrencyCheck { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether StringLength attribute is added to string data members.
		/// <para>This setting is applicable only when <see cref="Enable" /> is set to <c>true</c>.</para></summary>
		bool StringLength { get; set; }

		/// <summary>Gets or sets a value indicating whether Display attribute is added to data members.
		/// <para>This setting is applicable only when <see cref="Enable" /> is set to <c>true</c>.</para></summary>
		bool Display { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether Description attribute is added to data members.
		/// <para>The description is taken from SQL Server's extended properties (MS_Description) and MySQL's comment columns.
		/// This setting is applicable only when <see cref="Enable" /> is set to <c>true</c>.</para></summary>
		bool Description { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to group properties into a ComplexType based on the first underscore in their SQL column name.
		/// <para>This setting is applicable only when <see cref="Enable" /> is set to <c>true</c>.</para></summary>
		bool ComplexType { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether Index attribute is added to data members. Index attribute is applicable for EF6 and above.
		/// <para>This setting is applicable only when <see cref="Enable" /> is set to <c>true</c>.</para></summary>
		bool Index { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether ForeignKey and InverseProperty attributes are added to navigation properties.
		/// <para>This setting is applicable only when <see cref="Enable" /> is set to <c>true</c>.</para></summary>
		bool ForeignKeyAndInverseProperty { get; set; }
	}
}
