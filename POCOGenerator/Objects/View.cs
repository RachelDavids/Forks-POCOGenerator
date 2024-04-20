using System.Collections.Generic;

using POCOGenerator.Utils;

namespace POCOGenerator.Objects
{
	/// <summary>Represents a database view.</summary>
	public sealed class View : IDbObject
	{
		private readonly DbObjects.IView view;

		internal View(DbObjects.IView view, Database database)
		{
			this.view = view;
			Database = database;
		}

		internal bool InternalEquals(DbObjects.IView view)
		{
			return this.view == view;
		}

		internal string ClassName => view.ClassName;

		/// <summary>Gets the error message that occurred during the generating process of this view.</summary>
		/// <value>The error message that occurred during the generating process of this view.</value>
		public string Error => view.Error?.Message;

		/// <summary>Gets the database that this view belongs to.</summary>
		/// <value>The database that this view belongs to.</value>
		public Database Database { get; private set; }

		/// <summary>Gets the collection of database columns that belong to this view.</summary>
		/// <value>Collection of database columns.</value>
		public IEnumerable<IDbColumn> Columns => ViewColumns;

		private CachedEnumerable<DbObjects.ITableColumn, ViewColumn> viewColumns;
		/// <summary>Gets the columns of the view.</summary>
		/// <value>The columns of the view.</value>
		public IEnumerable<ViewColumn> ViewColumns {
			get {
				if (view.TableColumns.IsNullOrEmpty())
				{
					yield break;
				}

				viewColumns ??= new(view.TableColumns, c => new(c, this));

				foreach (ViewColumn viewColumn in viewColumns)
				{
					yield return viewColumn;
				}
			}
		}

		private CachedEnumerable<DbObjects.IIndex, ViewIndex> indexes;
		/// <summary>Gets the indexes of the view.</summary>
		/// <value>The indexes of the view.</value>
		public IEnumerable<ViewIndex> Indexes {
			get {
				if (view.Indexes.IsNullOrEmpty())
				{
					yield break;
				}

				indexes ??= new(view.Indexes, i => new(i, this));

				foreach (ViewIndex index in indexes)
				{
					yield return index;
				}
			}
		}

		/// <summary>Gets the name of the view.</summary>
		/// <value>The name of the view.</value>
		public string Name => view.Name;

		/// <summary>Gets the schema of the view.
		/// <para>Returns <see langword="null" /> if the RDBMS doesn't support schema.</para></summary>
		/// <value>The schema of the view.</value>
		/// <seealso cref="Support.SupportSchema" />
		public string Schema => view is DbObjects.ISchema schema ? schema.Schema : null;

		/// <summary>Gets the description of the view.</summary>
		/// <value>The description of the view.</value>
		public string Description => view.Description;

		/// <summary>Returns a string that represents this view.</summary>
		/// <returns>A string that represents this view.</returns>
		public override string ToString()
		{
			return view.ToString();
		}
	}
}
