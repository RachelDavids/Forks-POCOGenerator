using System.Collections.Generic;

namespace POCOGenerator
{
	/// <summary>The settings for the navigation properties.</summary>
	public interface INavigationProperties
	{
		/// <summary>Resets the navigation properties settings to their default values.</summary>
		void Reset();

		/// <summary>Gets or sets a value indicating whether to enable navigation properties.</summary>
		bool Enable { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether navigation property methods will be modified to be virtual methods.
		/// <para>This setting is applicable only when <see cref="Enable" /> is set to <c>true</c>.</para></summary>
		bool VirtualNavigationProperties { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether navigation property methods will be modified to be override methods.
		/// <para>This setting is applicable only when <see cref="Enable" /> is set to <c>true</c>.</para></summary>
		bool OverrideNavigationProperties { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to generate a join table in a many-to-many relationship. In a many-to-many relationship, the join table is hidden by default.
		/// <para>This setting is applicable only when <see cref="Enable" /> is set to <c>true</c>.</para></summary>
		bool ManyToManyJoinTable { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to generate a comment of the original SQL Server foreign key.
		/// <para>This setting is applicable only when <see cref="Enable" /> is set to <c>true</c>.</para></summary>
		bool Comments { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to generate navigation properties as <see cref="List{T}" />.
		/// <para>This setting is applicable only when <see cref="Enable" /> is set to <c>true</c>. The default value is <c>true</c>.</para></summary>
		bool ListNavigationProperties { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to generate navigation properties as <see cref="IList{T}" />.
		/// <para>This setting is applicable only when <see cref="Enable" /> is set to <c>true</c>. The default value is <c>true</c>.</para></summary>
		bool IListNavigationProperties { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to generate navigation properties as <see cref="ICollection{T}" />.
		/// <para>This setting is applicable only when <see cref="Enable" /> is set to <c>true</c>. The default value is <c>false</c>.</para></summary>
		bool ICollectionNavigationProperties { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to generate navigation properties as <see cref="IEnumerable{T}" />.
		/// <para>This setting is applicable only when <see cref="Enable" /> is set to <c>true</c>. The default value is <c>false</c>.</para></summary>
		bool IEnumerableNavigationProperties { get; set; }
	}
}
