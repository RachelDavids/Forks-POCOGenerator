using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

using MySql.Data.MySqlClient;

using POCOGenerator.Db;
using POCOGenerator.Db.DbObjects;
using POCOGenerator.DbObjects;
using POCOGenerator.MySQL.DbObjects;
using POCOGenerator.Utils;

namespace POCOGenerator.MySQL
{
	internal class MySQLHelper(string connectionString)
		: DbHelper(connectionString, new MySQLSupport())
	{
		protected override DbConnection GetConnection(string connectionString)
		{
			return new MySqlConnection(connectionString);
		}

		protected override IDbCommand GetCommand()
		{
			return new MySqlCommand();
		}

		protected override IDataParameter GetParameter(IProcedureParameter parameter, IDatabase database)
		{
			MySqlParameter mySqlParameter = new() {
				ParameterName = parameter.ParameterName,
				Value = DBNull.Value
			};

			string dataType = (parameter.ParameterDataType ?? String.Empty).ToLower();

			// https://dev.mysql.com/doc/refman/8.0/en/data-types.html
			mySqlParameter.MySqlDbType = dataType switch {
				"bit" => Support["Version_At_Least_5.0.3"] ? MySqlDbType.Bit : /*tinyint(1)*/ MySqlDbType.Byte,
				"tinyint" => parameter.ParameterIsUnsigned ? MySqlDbType.UByte : MySqlDbType.Byte,
				"bool" or "boolean" => MySqlDbType.Byte,
				"smallint" => parameter.ParameterIsUnsigned ? MySqlDbType.UInt16 : MySqlDbType.Int16,
				"mediumint" => parameter.ParameterIsUnsigned ? MySqlDbType.UInt24 : MySqlDbType.Int24,
				"int" or "integer" => parameter.ParameterIsUnsigned ? MySqlDbType.UInt32 : MySqlDbType.Int32,
				"bigint" => parameter.ParameterIsUnsigned ? MySqlDbType.UInt64 : MySqlDbType.Int64,
				"serial" => MySqlDbType.UInt64,
				"decimal" or "dec" or "numeric" or "fixed" => Support["Version_At_Least_5.0.3"]
																  ? MySqlDbType.NewDecimal
																  : MySqlDbType.Decimal,
				"float" => MySqlDbType.Float,
				"double" => MySqlDbType.Double,
				"real" => Support["REAL_AS_FLOAT"] ? MySqlDbType.Float : MySqlDbType.Double,
				"timestamp" => MySqlDbType.Timestamp,
				"datetime" => MySqlDbType.DateTime,
				"date" => MySqlDbType.Date,
				"newdate" => MySqlDbType.Newdate,
				"time" => MySqlDbType.Time,
				"year" => MySqlDbType.Year,
				"char" => MySqlDbType.String,
				"nchar" => MySqlDbType.String,
				"national char" => MySqlDbType.String,
				"character" => MySqlDbType.String,
				"string" => MySqlDbType.String,
				"binary" or "char byte" => MySqlDbType.Binary,
				"varchar" => MySqlDbType.VarChar,
				"nvarchar" => MySqlDbType.VarChar,
				"national varchar" => MySqlDbType.VarChar,
				"character varying" => MySqlDbType.VarChar,
				"varbinary" => MySqlDbType.VarBinary,
				"varstring" => MySqlDbType.VarString,
				"tinytext" => MySqlDbType.TinyText,
				"mediumtext" => MySqlDbType.MediumText,
				"text" => MySqlDbType.Text,
				"longtext" => MySqlDbType.LongText,
				"tinyblob" => MySqlDbType.TinyBlob,
				"mediumblob" => MySqlDbType.MediumBlob,
				"blob" => MySqlDbType.Blob,
				"longblob" => MySqlDbType.LongBlob,
				"set" => MySqlDbType.Set,
				"enum" => MySqlDbType.Enum,
				"geometry" => MySqlDbType.Geometry,
				"geometrycollection" => MySqlDbType.Geometry,
				"geomcollection" => MySqlDbType.Geometry,
				"point" => MySqlDbType.Geometry,
				"multipoint" => MySqlDbType.Geometry,
				"linestring" => MySqlDbType.Geometry,
				"multilinestring" => MySqlDbType.Geometry,
				"polygon" => MySqlDbType.Geometry,
				"multipolygon" => MySqlDbType.Geometry,
				"json" => MySqlDbType.JSON,
				"guid" => MySqlDbType.Guid,
				_ => mySqlParameter.MySqlDbType,
			};

#pragma warning disable IDE0010 // Add missing cases
			switch (mySqlParameter.MySqlDbType)
			{
				case MySqlDbType.Int16:
				case MySqlDbType.UInt16:
				case MySqlDbType.Int24:
				case MySqlDbType.UInt24:
				case MySqlDbType.Int32:
				case MySqlDbType.UInt32:
				case MySqlDbType.Int64:
				case MySqlDbType.UInt64:
				case MySqlDbType.Float:
				case MySqlDbType.Decimal:
				case MySqlDbType.NewDecimal:
				case MySqlDbType.Double:
					if (parameter.ParameterSize is (-1) or > 0)
					{
						mySqlParameter.Size = parameter.ParameterSize.Value;
					}

					if (parameter.ParameterPrecision != null)
					{
						mySqlParameter.Precision = parameter.ParameterPrecision.Value;
					}

					if (parameter.ParameterScale != null)
					{
						mySqlParameter.Scale = (byte)parameter.ParameterScale.Value;
					}

					break;

				case MySqlDbType.String:
				case MySqlDbType.VarString:
				case MySqlDbType.VarChar:
					if (parameter.ParameterSize is (-1) or > 0)
					{
						mySqlParameter.Size = parameter.ParameterSize.Value;
					}

					break;
			}
#pragma warning restore IDE0010 // Add missing cases

			mySqlParameter.Direction = parameter.ParameterDirection;

			return mySqlParameter;
		}

		protected override string GetServerVersion()
		{
			try
			{
				string connectionString = ConnectionString;
				using (DbConnection connection = GetConnection(connectionString))
				{
					using (IDbCommand command = GetCommand())
					{
						command.Connection = connection;
						command.CommandText = GetScript(GetType(), "MySQL_Version.sql");
						command.CommandType = CommandType.Text;
						command.CommandTimeout = 60;

						connection.Open();
						return command.ExecuteScalar() as string;
					}
				}
			}
			catch
			{
				return null;
			}
		}

		protected override void GetServerConfiguration(IServer server)
		{
			Support["Version_At_Least_5.0.3"] = Support.Version != null && Support.Version.GreaterThanOrEqual(5, 0, 3);
			Support["Version_At_Least_8.0.13"] = Support.Version != null && Support.Version.GreaterThanOrEqual(8, 0, 13);

			SetRealAsFloat();
		}

		protected void SetRealAsFloat()
		{
			try
			{
				string connectionString = ConnectionString;
				using (DbConnection connection = GetConnection(connectionString))
				{
					using (IDbCommand command = GetCommand())
					{
						command.Connection = connection;
						command.CommandText = GetScript(GetType(), "MySQL_RealAsFloat.sql");
						command.CommandType = CommandType.Text;
						command.CommandTimeout = 60;

						connection.Open();
						string sql_mode = command.ExecuteScalar() as string;
						Support["REAL_AS_FLOAT"] = !String.IsNullOrEmpty(sql_mode) && sql_mode.Contains("REAL_AS_FLOAT");
					}
				}
			}
			catch
			{
				Support["REAL_AS_FLOAT"] = false;
			}
		}

		protected override IEnumerable<IDatabase> GetSchemaDatabases(DbConnection connection)
		{
			return connection.GetSchema("Databases").Cast<Database>();
		}

		protected override void RemoveSystemDatabases(IServer server)
		{
			if (server.Databases.HasAny())
			{
				server.Databases = server.Databases.Where(t => t.ToString() is not "sys" and not "mysql" and not "information_schema" and not "performance_schema").ToList();
			}
		}

		protected override List<IDbObjectDescription> GetDbObjectDescriptions(IDatabase database)
		{
			string connectionString = database.GetDatabaseConnectionString(ConnectionString);
			using (DbConnection connection = GetConnection(connectionString))
			{
				using (IDbCommand command = GetCommand())
				{
					command.Connection = connection;
					command.CommandText = GetScript(GetType(), "MySQL_Descriptions.sql");
					command.CommandType = CommandType.Text;
					command.CommandTimeout = 60;

					MySqlParameter mySqlParameter = new("@database_name", MySqlDbType.VarChar, 64) {
						Value = database.ToString()
					};
					command.Parameters.Add(mySqlParameter);

					connection.Open();
					using (IDataReader reader = command.ExecuteReader())
					{
						DataTable descriptionsDT = new();
						descriptionsDT.Load(reader);
						return (Support.IsSupportSchema ? descriptionsDT.Cast<DbObjectDescriptionSchema>() : descriptionsDT.Cast<DbObjectDescription>()).Cast<IDbObjectDescription>().ToList();
					}
				}
			}
		}

		protected override Tuple<List<IInternalKey>, List<IInternalKey>, List<IInternalForeignKey>> GetKeys(IDatabase database)
		{
			string connectionString = database.GetDatabaseConnectionString(ConnectionString);
			using (DbConnection connection = GetConnection(connectionString))
			{
				using (IDbCommand command = GetCommand())
				{
					command.Connection = connection;
					command.CommandText = GetScript(GetType(), "MySQL_Keys.sql");
					command.CommandType = CommandType.Text;
					command.CommandTimeout = 60;

					MySqlParameter mySqlParameter = new("@database_name", MySqlDbType.VarChar, 64) {
						Value = database.ToString()
					};
					command.Parameters.Add(mySqlParameter);

					connection.Open();
					using (IDataReader reader = command.ExecuteReader())
					{
						DataSet keysDS = new();
						keysDS.Tables.Add("PrimaryKeys");
						keysDS.Tables.Add("UniqueKeys");
						keysDS.Tables.Add("ForeignKeys");
						keysDS.Load(reader, LoadOption.PreserveChanges, keysDS.Tables["PrimaryKeys"], keysDS.Tables["UniqueKeys"], keysDS.Tables["ForeignKeys"]);
						return new Tuple<List<IInternalKey>, List<IInternalKey>, List<IInternalForeignKey>>(
							(Support.IsSupportSchema ? keysDS.Tables["PrimaryKeys"].Cast<PrimaryKeySchemaInternal>().Cast<IInternalKey>() : keysDS.Tables["PrimaryKeys"].Cast<PrimaryKeyInternal>().Cast<IInternalKey>()).ToList(),
							(Support.IsSupportSchema ? keysDS.Tables["UniqueKeys"].Cast<UniqueKeySchemaInternal>().Cast<IInternalKey>() : keysDS.Tables["UniqueKeys"].Cast<UniqueKeyInternal>().Cast<IInternalKey>()).ToList(),
							(Support.IsSupportSchema ? keysDS.Tables["ForeignKeys"].Cast<ForeignKeySchemaInternal>().Cast<IInternalForeignKey>() : keysDS.Tables["ForeignKeys"].Cast<ForeignKeyInternal>().Cast<IInternalForeignKey>()).ToList()
						);
					}
				}
			}
		}

		protected override List<IInternalIndex> GetIndexes(IDatabase database)
		{
			string connectionString = database.GetDatabaseConnectionString(ConnectionString);
			using (DbConnection connection = GetConnection(connectionString))
			{
				using (IDbCommand command = GetCommand())
				{
					command.Connection = connection;
					command.CommandText = GetScript(GetType(), "MySQL_Indexes.sql");
					command.CommandType = CommandType.Text;
					command.CommandTimeout = 60;

					MySqlParameter mySqlParameter = new("@database_name", MySqlDbType.VarChar, 64) {
						Value = database.ToString()
					};
					command.Parameters.Add(mySqlParameter);

					connection.Open();
					using (IDataReader reader = command.ExecuteReader())
					{
						DataTable indexesDT = new();
						indexesDT.Load(reader);
						return (Support.IsSupportSchema ? indexesDT.Cast<IndexSchemaInternal>().Cast<IInternalIndex>() : indexesDT.Cast<IndexInternal>().Cast<IInternalIndex>()).ToList();
					}
				}
			}
		}

		protected override List<IIdentityColumn> GetIdentityColumns(IDatabase database)
		{
			string connectionString = database.GetDatabaseConnectionString(ConnectionString);
			using (DbConnection connection = GetConnection(connectionString))
			{
				using (IDbCommand command = GetCommand())
				{
					command.Connection = connection;
					command.CommandText = GetScript(GetType(), "MySQL_IdentityColumns.sql");
					command.CommandType = CommandType.Text;
					command.CommandTimeout = 60;

					MySqlParameter mySqlParameter = new("@database_name", MySqlDbType.VarChar, 64) {
						Value = database.ToString()
					};
					command.Parameters.Add(mySqlParameter);

					connection.Open();
					using (IDataReader reader = command.ExecuteReader())
					{
						DataTable identityColumnsDT = new();
						identityColumnsDT.Load(reader);
						return (Support.IsSupportSchema ? identityColumnsDT.Cast<IdentityColumnSchema>() : identityColumnsDT.Cast<IdentityColumn>()).Cast<IIdentityColumn>().ToList();
					}
				}
			}
		}

		protected override List<IComputedColumn> GetComputedColumns(IDatabase database)
		{
			string connectionString = database.GetDatabaseConnectionString(ConnectionString);
			using (DbConnection connection = GetConnection(connectionString))
			{
				using (IDbCommand command = GetCommand())
				{
					command.Connection = connection;
					command.CommandText = GetScript(GetType(), "MySQL_ComputedColumns.sql");
					command.CommandType = CommandType.Text;
					command.CommandTimeout = 60;

					MySqlParameter mySqlParameter = new("@database_name", MySqlDbType.VarChar, 64) {
						Value = database.ToString()
					};
					command.Parameters.Add(mySqlParameter);

					connection.Open();
					using (IDataReader reader = command.ExecuteReader())
					{
						DataTable computedColumnsDT = new();
						computedColumnsDT.Load(reader);
						return (Support.IsSupportSchema ? computedColumnsDT.Cast<ComputedColumnSchema>() : computedColumnsDT.Cast<ComputedColumn>()).Cast<IComputedColumn>().ToList();
					}
				}
			}
		}

		protected override IEnumerable<ITable> GetSchemaTables(DbConnection connection, string database)
		{
			return connection.GetSchema("Tables", [null, database, null, "BASE TABLE"]).Cast<Table>();
		}

		protected override IEnumerable<ITableColumn> GetSchemaTableColumns(DbConnection connection, ITable table)
		{
			return connection.GetSchema("Columns", [null, table.Database.ToString(), table.Name, null]).Cast<TableColumn>();
		}

		protected override IEnumerable<IView> GetSchemaViews(DbConnection connection, string database)
		{
			return connection.GetSchema("Views", [null, database, null]).Cast<View>();
		}

		protected override IEnumerable<ITableColumn> GetSchemaViewColumns(DbConnection connection, IView view)
		{
			return connection.GetSchema("ViewColumns", [null, view.Database.ToString(), view.Name, null]).Cast<ViewColumn>();
		}

		protected override IEnumerable<IProcedure> GetSchemaProcedures(DbConnection connection, string database)
		{
			return connection.GetSchema("Procedures", [null, database, null, "PROCEDURE"])
							 .Cast<Procedure>();
		}

		protected override IEnumerable<IProcedureParameter> GetSchemaProcedureParameters(DbConnection connection, IProcedure procedure)
		{
			return connection.GetSchema("Procedure Parameters", [null, procedure.Database.ToString(), procedure.Name, "PROCEDURE"])
							 .Cast<ProcedureParameter>();
		}

		protected override string GetProcedureCommandText(IProcedure procedure)
		{
			return $"{procedure.Database.ToString()}.{procedure.Name}";
		}

		protected override IEnumerable<IProcedureColumn> GetSchemaProcedureColumns(DataTable schemaTable)
		{
			return schemaTable.Cast<ProcedureColumn>((columnName, _, value) => {
				return columnName == "IsIdentity" ? value as bool? ?? false : value;
			})
							  .Where(c => !String.IsNullOrEmpty(c.ColumnName));
		}
	}
}
