using System.Collections.Generic;

using POCOGenerator.Utils;

namespace POCOGenerator.Objects
{
	/// <summary>Represents a database table-valued function.</summary>
	public sealed class Function : IDbRoutine
	{
		private readonly DbObjects.IFunction function;

		internal Function(DbObjects.IFunction function, Database database)
		{
			this.function = function;
			Database = database;
		}

		internal bool InternalEquals(DbObjects.IFunction function)
		{
			return this.function == function;
		}

		internal string ClassName => function.ClassName;

		/// <summary>Gets the error message that occurred during the generating process of this function.</summary>
		/// <value>The error message that occurred during the generating process of this function.</value>
		public string Error => function.Error?.Message;

		/// <summary>Gets the database that this function belongs to.</summary>
		/// <value>The database that this function belongs to.</value>
		public Database Database { get; private set; }

		/// <summary>Gets the collection of database parameters that belong to this function.</summary>
		/// <value>Collection of database parameters.</value>
		public IEnumerable<IDbParameter> Parameters => FunctionParameters;

		private CachedEnumerable<DbObjects.IProcedureParameter, FunctionParameter> functionParameters;
		/// <summary>Gets the parameters of the function.</summary>
		/// <value>The parameters of the function.</value>
		public IEnumerable<FunctionParameter> FunctionParameters {
			get {
				if (function.ProcedureParameters.IsNullOrEmpty())
				{
					yield break;
				}

				functionParameters ??= new(function.ProcedureParameters, pp => new(pp, this));

				foreach (FunctionParameter functionParameter in functionParameters)
				{
					yield return functionParameter;
				}
			}
		}

		/// <summary>Gets the collection of database columns that belong to this function.</summary>
		/// <value>Collection of database columns.</value>
		public IEnumerable<IDbColumn> Columns => FunctionColumns;

		private CachedEnumerable<DbObjects.IProcedureColumn, FunctionColumn> functionColumns;
		/// <summary>Gets the columns of the function.</summary>
		/// <value>The columns of the function.</value>
		public IEnumerable<FunctionColumn> FunctionColumns {
			get {
				if (function.ProcedureColumns.IsNullOrEmpty())
				{
					yield break;
				}

				functionColumns ??= new(function.ProcedureColumns, pc => new(pc, this));

				foreach (FunctionColumn functionColumn in functionColumns)
				{
					yield return functionColumn;
				}
			}
		}

		/// <summary>Gets the name of the function.</summary>
		/// <value>The name of the function.</value>
		public string Name => function.Name;

		/// <summary>Gets the schema of the function.
		/// <para>Returns <see langword="null" /> if the RDBMS doesn't support schema.</para></summary>
		/// <value>The schema of the function.</value>
		/// <seealso cref="Support.SupportSchema" />
		public string Schema => function is DbObjects.ISchema schema ? schema.Schema : null;

		/// <summary>Gets the description of the function.</summary>
		/// <value>The description of the function.</value>
		public string Description => function.Description;

		/// <summary>Returns a string that represents this function.</summary>
		/// <returns>A string that represents this function.</returns>
		public override string ToString()
		{
			return function.ToString();
		}
	}
}
