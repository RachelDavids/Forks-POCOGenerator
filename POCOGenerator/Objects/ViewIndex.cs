using System.Collections.Generic;

using POCOGenerator.Utils;

namespace POCOGenerator.Objects
{
	/// <summary>Represents a database index of a database view.</summary>
	public sealed class ViewIndex : Index
	{
		internal ViewIndex(DbObjects.IIndex index, View view)
			: base(index)
		{
			View = view;
		}

		/// <summary>Gets the view that this view index belongs to.</summary>
		/// <value>The view that this view index belongs to.</value>
		public View View { get; private set; }

		private CachedEnumerable<DbObjects.IIndexColumn, ViewIndexColumn> indexColumns;
		/// <summary>Gets the columns of the view index.</summary>
		/// <value>The columns of the view index.</value>
		public IEnumerable<ViewIndexColumn> IndexColumns {
			get {
				if (index.IndexColumns.IsNullOrEmpty())
				{
					yield break;
				}

				indexColumns ??= new(index.IndexColumns, ic => new(ic, this));

				foreach (ViewIndexColumn indexColumn in indexColumns)
				{
					yield return indexColumn;
				}
			}
		}
	}
}
