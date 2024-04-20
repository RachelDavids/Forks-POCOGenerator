using System;
using System.Collections.Generic;

using POCOGenerator.DbObjects;
using POCOGenerator.Utils;

namespace POCOGenerator.SQLServer.DbObjects
{
	internal class TableColumn : ITableColumn
	{
		public string column_name { get; set; }
		public int? ordinal_position { get; set; }
		public string column_default { get; set; }
		public string is_nullable { get; set; }
		public string data_type { get; set; }
		public int? character_maximum_length { get; set; }
		public byte? numeric_precision { get; set; }
		public int? numeric_scale { get; set; }
		public short? datetime_precision { get; set; }
		public bool is_identity { get; set; }
		public bool is_computed { get; set; }

		/* not in use. reduce memory.
        public string table_catalog { get; set; }
        public string table_schema { get; set; }
        public string table_name { get; set; }
        public int? character_octet_length { get; set; }
        public short? numeric_precision_radix { get; set; }
        public string character_set_catalog { get; set; }
        public string character_set_schema { get; set; }
        public string character_set_name { get; set; }
        public string collation_catalog { get; set; }
        public bool? is_sparse { get; set; }
        public bool? is_column_set { get; set; }
        public bool? is_filestream { get; set; }
        */

		public string ColumnName => column_name;
		public int? ColumnOrdinal => ordinal_position;
		public string DataTypeName => data_type;

		public string DataTypeDisplay => data_type == "xml" ? "XML" : data_type;

		public int? StringPrecision => character_maximum_length;
		public int? NumericPrecision => numeric_precision;
		public int? NumericScale => numeric_scale;
		public int? DateTimePrecision => datetime_precision;
		public bool IsUnsigned => false;
		public bool IsNullable => String.Equals(is_nullable, "YES", StringComparison.CurrentCultureIgnoreCase);
		public bool IsIdentity { get => is_identity; set => is_identity = value; }
		public bool IsComputed { get => is_computed; set => is_computed = value; }

		public string Precision {
			get {
				string precision = null;

				string dataType = data_type.ToLower();

				if (dataType is "binary" or "varbinary" or "char" or "nchar" or "nvarchar" or "varchar")
				{
					if (StringPrecision == -1)
					{
						precision = "(max)";
					}
					else if (StringPrecision > 0)
					{
						precision = "(" + StringPrecision + ")";
					}
				}
				else if (dataType is "decimal" or "numeric")
				{
					precision = "(" + NumericPrecision + "," + NumericScale + ")";
				}
				else if (dataType is "datetime2" or "datetimeoffset" or "time")
				{
					precision = "(" + DateTimePrecision + ")";
				}
				else if (dataType == "xml")
				{
					precision = "(.)";
				}

				return precision;
			}
		}

		public string Description { get; set; }

		public ITable Table { get; set; }

		public IPrimaryKeyColumn PrimaryKeyColumn { get; set; }
		public List<IUniqueKeyColumn> UniqueKeyColumns { get; set; }
		public List<IForeignKeyColumn> ForeignKeyColumns { get; set; }
		public List<IForeignKeyColumn> PrimaryForeignKeyColumns { get; set; }
		public List<IIndexColumn> IndexColumns { get; set; }
		public IComplexTypeTableColumn ComplexTypeTableColumn { get; set; }

		public string ColumnDefault => column_default;

		public string ToFullString()
		{
			return
				ColumnName + " (" +
				(PrimaryKeyColumn != null ? "PK, " : String.Empty) +
				(ForeignKeyColumns.HasAny() ? "FK, " : String.Empty) +
				(IsComputed ? "Computed, " : String.Empty) +
				DataTypeDisplay + Precision + ", " + (IsNullable ? "null" : "not null") + ")";
		}

		public override string ToString()
		{
			return ColumnName + " (" + DataTypeDisplay + Precision + ", " + (IsNullable ? "null" : "not null") + ")";
		}
	}
}
