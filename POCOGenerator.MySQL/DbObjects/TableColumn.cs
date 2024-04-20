using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using POCOGenerator.DbObjects;
using POCOGenerator.Utils;

namespace POCOGenerator.MySQL.DbObjects
{
	internal class TableColumn : ITableColumn, IEnumColumn
	{
		public string COLUMN_NAME { get; set; }
		public ulong? ORDINAL_POSITION { get; set; }
		public string COLUMN_DEFAULT { get; set; }
		public string IS_NULLABLE { get; set; }
		public string DATA_TYPE { get; set; }
		public ulong? CHARACTER_MAXIMUM_LENGTH { get; set; }
		public ulong? NUMERIC_PRECISION { get; set; }
		public ulong? NUMERIC_SCALE { get; set; }
		public ulong? DATETIME_PRECISION { get; set; }
		public string COLUMN_TYPE { get; set; }
		public string EXTRA { get; set; }

		/* not in use. reduce memory.
        public virtual string TABLE_CATALOG { get; set; }
        public virtual string TABLE_SCHEMA { get; set; }
        public virtual string TABLE_NAME { get; set; }
        public string CHARACTER_SET_NAME { get; set; }
        public string COLLATION_NAME { get; set; }
        public string COLUMN_KEY { get; set; }
        public string PRIVILEGES { get; set; }
        public string COLUMN_COMMENT { get; set; }
        public string GENERATION_EXPRESSION { get; set; }
        */

		public string ColumnName => COLUMN_NAME;
		public int? ColumnOrdinal => (int?)ORDINAL_POSITION;
		public string DataTypeName => DATA_TYPE;
		public string DataTypeDisplay => DATA_TYPE.ToLower();
		public int? StringPrecision => (int?)CHARACTER_MAXIMUM_LENGTH;
		public int? NumericPrecision => (int?)NUMERIC_PRECISION;
		public int? NumericScale => (int?)NUMERIC_SCALE;
		public int? DateTimePrecision => (int?)DATETIME_PRECISION;
		public bool IsUnsigned => !String.IsNullOrEmpty(COLUMN_TYPE) && COLUMN_TYPE.ToLower().Contains("unsigned");
		public bool IsNullable => String.Equals(IS_NULLABLE, "YES", StringComparison.CurrentCultureIgnoreCase);

		private bool? _isIdentity;
		public bool IsIdentity {
			get {
				_isIdentity ??= !String.IsNullOrEmpty(EXTRA) && EXTRA.ToLower().Contains("auto_increment");
				return _isIdentity.Value;
			}
			set => _isIdentity = value;
		}

		private bool? _isComputed;
		public bool IsComputed {
			get {
				_isComputed ??= !String.IsNullOrEmpty(EXTRA) && (EXTRA.ToUpper().Contains("VIRTUAL GENERATED") || EXTRA.ToUpper().Contains("VIRTUAL STORED"));
				return _isComputed.Value;
			}
			set => _isComputed = value;
		}

		public string Precision {
			get {
				string precision = null;

				string dataType = DATA_TYPE.ToLower();

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
						precision = "(" + NumericPrecision + ")";
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
				else if (dataType is "enum" or "set")
				{
					if (!String.IsNullOrEmpty(COLUMN_TYPE))
					{
						int startIndex = COLUMN_TYPE.IndexOf('(');
						if (startIndex != -1)
						{
							int endIndex = COLUMN_TYPE.LastIndexOf(')');
							if (endIndex != -1)
							{
								precision = COLUMN_TYPE.Substring(startIndex, endIndex - startIndex + 1);
							}
						}
					}
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

		public string ColumnDefault => COLUMN_DEFAULT;

		public string ToFullString()
		{
			return
				ColumnName + " (" +
				(PrimaryKeyColumn != null ? "PK, " : String.Empty) +
				(ForeignKeyColumns.HasAny() ? "FK, " : String.Empty) +
				(IsComputed ? "Generated, " : String.Empty) +
				DATA_TYPE + Precision + (IsUnsigned ? " unsigned" : String.Empty) + ", " + (IsNullable ? "null" : "not null") + ")";
		}

		public IColumn Column => this;
		public bool IsEnumDataType => DATA_TYPE.ToLower() == "enum";
		public bool IsSetDataType => DATA_TYPE.ToLower() == "set";

		private static readonly Regex enumLiteralsRegex = new(@"^(?i:enum|set)\s*\((?:\s*,?\s*'(?<literal>.*?)')+\)$", RegexOptions.Compiled);

		private List<string> enumLiterals;
		public List<string> EnumLiterals {
			get {
				if (enumLiterals == null)
				{
					Match match = enumLiteralsRegex.Match(COLUMN_TYPE);
					if (match.Success)
					{
						Group group = match.Groups["literal"];
						if (group.Success)
						{
							enumLiterals = group.Captures.Cast<Capture>().Select(c => c.Value).ToList();
						}
					}

					enumLiterals ??= [];
				}

				return enumLiterals;
			}
		}

		public override string ToString()
		{
			return ColumnName + " (" + DATA_TYPE + Precision + (IsUnsigned ? " unsigned" : String.Empty) + ", " + (IsNullable ? "null" : "not null") + ")";
		}
	}
}
