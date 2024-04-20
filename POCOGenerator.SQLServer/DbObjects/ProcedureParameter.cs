using System;
using System.Data;

using POCOGenerator.DbObjects;

namespace POCOGenerator.SQLServer.DbObjects
{
	internal class ProcedureParameter
		: IProcedureParameter
	{
		public int? ordinal_position { get; set; }
		public string parameter_mode { get; set; }
		public string is_result { get; set; }
		public string parameter_name { get; set; }
		public string data_type { get; set; }
		public int? character_maximum_length { get; set; }
		public byte? numeric_precision { get; set; }
		public int? numeric_scale { get; set; }
		public short? datetime_precision { get; set; }

		/* not in use. reduce memory.
        public string specific_catalog { get; set; }
        public string specific_schema { get; set; }
        public string specific_name { get; set; }
        public string as_locator { get; set; }
        public int? character_octet_length { get; set; }
        public string collation_catalog { get; set; }
        public string collation_schema { get; set; }
        public string collation_name { get; set; }
        public string character_set_catalog { get; set; }
        public string character_set_schema { get; set; }
        public string character_set_name { get; set; }
        public short? numeric_precision_radix { get; set; }
        public string interval_type { get; set; }
        public short? interval_precision { get; set; }
        */

		public IProcedure Procedure { get; set; }
		public string ParameterName => parameter_name;
		public string ParameterDataType => data_type;
		public bool ParameterIsUnsigned => false;
		public int? ParameterOrdinal => ordinal_position;
		public int? ParameterSize => character_maximum_length;
		public byte? ParameterPrecision => numeric_precision;
		public int? ParameterScale => numeric_scale;
		public int? ParameterDateTimePrecision => datetime_precision;
		public string ParameterMode => parameter_mode;

		public ParameterDirection ParameterDirection =>
			ParameterMode.ToLower(null) switch {
				"in" => ParameterDirection.Input,
				"inout" => ParameterDirection.InputOutput,
				"out" => ParameterDirection.Output,
				_ => ParameterDirection.Input
			};

		public bool IsResult => String.Equals(is_result, "YES", StringComparison.CurrentCultureIgnoreCase);

		public string Description { get; set; }

		public override string ToString()
		{
			return IsResult
				? "Returns " + DataTypeDisplay + Precision
				: ParameterName + " (" + DataTypeDisplay + Precision + ", " + Direction + ")";
		}

		public string DataTypeDisplay => ParameterDataType == "xml" ? "XML" : ParameterDataType;

		public string Precision {
			get {
				string precision = null;

				string dataType = ParameterDataType.ToLower();

				if (dataType is "binary" or "varbinary" or "char" or "nchar" or "nvarchar" or "varchar")
				{
					if (ParameterSize == -1)
					{
						precision = "(max)";
					}
					else if (ParameterSize > 0)
					{
						precision = "(" + ParameterSize + ")";
					}
				}
				else if (dataType is "decimal" or "numeric")
				{
					precision = "(" + ParameterPrecision + "," + ParameterScale + ")";
				}
				else if (dataType is "datetime2" or "datetimeoffset" or "time")
				{
					precision = "(" + ParameterDateTimePrecision + ")";
				}
				else if (dataType == "xml")
				{
					precision = "(.)";
				}

				return precision;
			}
		}

		public string Direction {
			get {
				if (String.Compare(ParameterMode, "IN", true) == 0)
				{
					return "Input";
				}
				else if (String.Compare(ParameterMode, "INOUT", true) == 0)
				{
					return "Input/Output";
				}
				else if (String.Compare(ParameterMode, "OUT", true) == 0)
				{
					return "Output";
				}

				return null;
			}
		}
	}
}
