using System;

using MySql.Data.MySqlClient;

using POCOGenerator.DbObjects;

namespace POCOGenerator.MySQL.DbObjects
{
	internal class ProcedureColumn
		: IProcedureColumn
	{
		public string ColumnName { get; set; }
		public int? ColumnOrdinal { get; set; }
		public int? ColumnSize { get; set; }
		public int? NumericPrecision { get; set; }
		public int? NumericScale { get; set; }
		public Type DataType { get; set; }
		public bool? AllowDBNull { get; set; }
		public int? ProviderType { get; set; }
		public bool IsIdentity { get; set; } // originally bool?
		public bool? IsLong { get; set; }

		/* not in use. reduce memory.
        public bool? IsUnique { get; set; }
        public bool? IsKey { get; set; }
        public string BaseCatalogName { get; set; }
        public string BaseColumnName { get; set; }
        public string BaseSchemaName { get; set; }
        public string BaseTableName { get; set; }
        public bool? IsAliased { get; set; }
        public bool? IsExpression { get; set; }
        public bool? IsAutoIncrement { get; set; }
        public bool? IsRowVersion { get; set; }
        public bool? IsHidden { get; set; }
        public bool? IsReadOnly { get; set; }
        */

		public string DataTypeName {
			get {
				if (ProviderType == null)
				{
					return String.Empty;
				}

				MySqlDbType mySqlDbType = (MySqlDbType)Enum.ToObject(typeof(MySqlDbType), ProviderType.Value);

				return mySqlDbType switch {
					MySqlDbType.Decimal => "decimal",
					MySqlDbType.Byte => "tinyint",
					MySqlDbType.Int16 => "smallint",
					MySqlDbType.Int32 => "int",
					MySqlDbType.Float => "float",
					MySqlDbType.Double => "double",
					MySqlDbType.Timestamp => "timestamp",
					MySqlDbType.Int64 => "bigint",
					MySqlDbType.Int24 => "mediumint",
					MySqlDbType.Date => "date",
					MySqlDbType.Time => "time",
					MySqlDbType.DateTime => "datetime",
					MySqlDbType.Year => "year",
					MySqlDbType.Newdate => "date",
					MySqlDbType.VarString => "varchar",
					MySqlDbType.Bit => "bit",
					MySqlDbType.JSON => "json",
					MySqlDbType.NewDecimal => "decimal",
					MySqlDbType.Enum => "enum",
					MySqlDbType.Set => "set",
					MySqlDbType.TinyBlob => "tinyblob",
					MySqlDbType.MediumBlob => "mediumblob",
					MySqlDbType.LongBlob => "longblob",
					MySqlDbType.VarChar => "varchar",
					MySqlDbType.String => "char",//  MySqlDbType.String returns for enum & set
					MySqlDbType.Geometry => "geometry",
					MySqlDbType.UByte => "tinyint",// tinyint unsigned
					MySqlDbType.UInt16 => "smallint",// smallint unsigned
					MySqlDbType.UInt32 => "int",// int unsigned
					MySqlDbType.UInt64 => "bigint",// bigint unsigned
					MySqlDbType.UInt24 => "mediumint",// mediumint unsigned
					MySqlDbType.Binary => "binary",
					MySqlDbType.VarBinary => "varbinary",
					MySqlDbType.TinyText => "tinytext",
					MySqlDbType.MediumText => "mediumtext",
					MySqlDbType.LongText => "longtext",
					MySqlDbType.Guid => "varbinary",// varbinary(16)
					MySqlDbType.Blob => ColumnSize.GetValueOrDefault() switch {
						255 => "tinyblob",
						65535 => "blob",
						16777215 => "mediumblob",
						-1 => "longblob",
						_ => "blob",
					},
					MySqlDbType.Text => ColumnSize.GetValueOrDefault() switch {
						255 => "tinytext",
						65535 => "text",
						16777215 => "mediumtext",
						-1 => "longtext",
						_ => "text",
					},
					_ => String.Empty,
				};
			}
		}

		public bool IsUnsigned {
			get {
				if (ProviderType == null)
				{
					return false;
				}

				MySqlDbType mySqlDbType = (MySqlDbType)Enum.ToObject(typeof(MySqlDbType), ProviderType.Value);

#pragma warning disable IDE0072 // Add missing cases
				return mySqlDbType switch {
					// tinyint unsigned
					MySqlDbType.UByte => true,
					MySqlDbType.UInt16 => true,
					MySqlDbType.UInt32 => true,
					MySqlDbType.UInt64 => true,
					MySqlDbType.UInt24 => true,
					_ => false,
				};
#pragma warning restore IDE0072 // Add missing cases
			}
		}

		public int? StringPrecision => IsLong == true ? -1 : ColumnSize;
		public int? DateTimePrecision => NumericScale;
		public bool IsNullable => AllowDBNull ?? false;
		public bool IsComputed { get => false; set { } }

		public string DataTypeDisplay => DataTypeName.ToLower();

		public string Precision {
			get {
				string precision = null;

				string dataType = DataTypeName.ToLower();

				if (dataType is "binary" or "char byte" or
					"char" or "character" or
					"nchar" or "national char" or
					"nvarchar" or "national varchar" or
					"varbinary" or
					"varchar" or "character varying")
				{
					if (StringPrecision != null)
					{
						precision = "(" + StringPrecision + ")";
					}
				}
				else if (dataType == "bit")
				{
					if (NumericPrecision != null)
					{
						precision = "(" + ColumnSize + ")";
					}
				}
				else if (dataType is "decimal" or "numeric" or "dec" or "fixed")
				{
					if (NumericPrecision != null && NumericScale != null)
					{
						precision = "(" + NumericPrecision + "," + NumericScale + ")";
					}
				}
				else if (dataType is "datetime" or "time" or "timestamp")
				{
					if (DateTimePrecision is not null and > 0)
					{
						precision = "(" + DateTimePrecision + ")";
					}
				}

				return precision;
			}
		}

		public IProcedure Procedure { get; set; }

		public override string ToString()
		{
			return ColumnName + " (" + DataTypeDisplay + Precision + (IsUnsigned ? " unsigned" : String.Empty) + ", " + (IsNullable ? "null" : "not null") + ")";
		}
	}
}
