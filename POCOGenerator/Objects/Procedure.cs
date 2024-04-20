using System.Collections.Generic;

using POCOGenerator.Utils;

namespace POCOGenerator.Objects
{
	/// <summary>Represents a database stored procedure.</summary>
	public sealed class Procedure : IDbRoutine
	{
		private readonly DbObjects.IProcedure procedure;

		internal Procedure(DbObjects.IProcedure procedure, Database database)
		{
			this.procedure = procedure;
			Database = database;
		}

		internal bool InternalEquals(DbObjects.IProcedure procedure)
		{
			return this.procedure == procedure;
		}

		internal string ClassName => procedure.ClassName;

		/// <summary>Gets the error message that occurred during the generating process of this stored procedure.</summary>
		/// <value>The error message that occurred during the generating process of this stored procedure.</value>
		public string Error => procedure.Error?.Message;

		/// <summary>Gets the database that this stored procedure belongs to.</summary>
		/// <value>The database that this stored procedure belongs to.</value>
		public Database Database { get; private set; }

		/// <summary>Gets the collection of database parameters that belong to this stored procedure.</summary>
		/// <value>Collection of database parameters.</value>
		public IEnumerable<IDbParameter> Parameters => ProcedureParameters;

		private CachedEnumerable<DbObjects.IProcedureParameter, ProcedureParameter> procedureParameters;
		/// <summary>Gets the parameters of the stored procedure.</summary>
		/// <value>The parameters of the stored procedure.</value>
		public IEnumerable<ProcedureParameter> ProcedureParameters {
			get {
				if (procedure.ProcedureParameters.IsNullOrEmpty())
				{
					yield break;
				}

				procedureParameters ??= new(procedure.ProcedureParameters, pp => new(pp, this));

				foreach (ProcedureParameter procedureParameter in procedureParameters)
				{
					yield return procedureParameter;
				}
			}
		}

		/// <summary>Gets the collection of database columns that belong to this stored procedure.</summary>
		/// <value>Collection of database columns.</value>
		public IEnumerable<IDbColumn> Columns => ProcedureColumns;

		private CachedEnumerable<DbObjects.IProcedureColumn, ProcedureColumn> procedureColumns;
		/// <summary>Gets the columns of the stored procedure.</summary>
		/// <value>The columns of the stored procedure.</value>
		public IEnumerable<ProcedureColumn> ProcedureColumns {
			get {
				if (procedure.ProcedureColumns.IsNullOrEmpty())
				{
					yield break;
				}

				procedureColumns ??= new(procedure.ProcedureColumns, pc => new(pc, this));

				foreach (ProcedureColumn procedureColumn in procedureColumns)
				{
					yield return procedureColumn;
				}
			}
		}

		/// <summary>Gets the name of the stored procedure.</summary>
		/// <value>The name of the stored procedure.</value>
		public string Name => procedure.Name;

		/// <summary>Gets the schema of the stored procedure.
		/// <para>Returns <see langword="null" /> if the RDBMS doesn't support schema.</para></summary>
		/// <value>The schema of the stored procedure.</value>
		/// <seealso cref="Support.SupportSchema" />
		public string Schema => procedure is DbObjects.ISchema schema ? schema.Schema : null;

		/// <summary>Gets the description of the stored procedure.</summary>
		/// <value>The description of the stored procedure.</value>
		public string Description => procedure.Description;

		/// <summary>Returns a string that represents this stored procedure.</summary>
		/// <returns>A string that represents this stored procedure.</returns>
		public override string ToString()
		{
			return procedure.ToString();
		}
	}
}
