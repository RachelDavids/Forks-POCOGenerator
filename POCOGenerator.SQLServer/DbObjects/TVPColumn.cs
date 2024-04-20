using POCOGenerator.DbObjects;

namespace POCOGenerator.SQLServer.DbObjects
{
	internal class TVPColumn : ITVPColumn
	{
		public string data_type { get; set; }
		public string name { get; set; }
		public int column_id { get; set; }
		public short max_length { get; set; }
		public byte precision { get; set; }
		public byte scale { get; set; }
		public bool? is_nullable { get; set; }
		public bool is_identity { get; set; }
		public bool is_computed { get; set; }

		/* not in use. reduce memory.
        public int object_id { get; set; }
        public byte system_type_id { get; set; }
        public int user_type_id { get; set; }
        public string collation_name { get; set; }
        public bool is_ansi_padded { get; set; }
        public bool is_rowguidcol { get; set; }
        public bool is_filestream { get; set; }
        public bool? is_replicated { get; set; }
        public bool? is_non_sql_subscribed { get; set; }
        public bool? is_merge_published { get; set; }
        public bool? is_dts_replicated { get; set; }
        public bool is_xml_document { get; set; }
        public int xml_collection_id { get; set; }
        public int default_object_id { get; set; }
        public int rule_object_id { get; set; }
        public bool? is_sparse { get; set; }
        public bool? is_column_set { get; set; }
        */

		public string ColumnName => name;
		public int? ColumnOrdinal => column_id;
		public string DataTypeName => data_type;

		public string DataTypeDisplay => data_type == "xml" ? "XML" : data_type;

		public int? StringPrecision => max_length > 0
					? data_type.ToLower() is "nchar" or "nvarchar" ? max_length / 2 : max_length
					: max_length;

		public int? NumericPrecision => precision;
		public int? NumericScale => scale;
		public int? DateTimePrecision => scale;
		public bool IsUnsigned => false;
		public bool IsNullable => is_nullable != null && is_nullable.Value;
		public bool IsIdentity { get => is_identity; set => is_identity = value; }
		public bool IsComputed { get => is_computed; set => is_computed = value; }

		public string Precision {
			get {
				string dataType = data_type.ToLower();
				string prc = dataType switch {
					"binary" or "varbinary" or "char" or "nchar" or "nvarchar" or "varchar"
						when max_length == -1 => "(max)",
					"binary" or "varbinary" or "char" or "nchar" or "nvarchar" or "varchar"
						when max_length > 0 => "(" + (dataType is "nchar" or "nvarchar" ? max_length / 2 : max_length) + ")",
					"decimal" or "numeric" => "(" + NumericPrecision + "," + NumericScale + ")",
					"datetime2" or "datetimeoffset" or "time" => "(" + DateTimePrecision + ")",
					"xml" => "(.)",
					_ => null,
				};
				return prc;
			}
		}

		public string Description { get; set; }

		public ITVP TVP { get; set; }

		public override string ToString()
		{
			return ColumnName + " (" + DataTypeDisplay + Precision + ", " + (IsNullable ? "null" : "not null") + ")";
		}
	}
}
