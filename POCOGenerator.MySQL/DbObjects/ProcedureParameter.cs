using System;
using System.Data;

using POCOGenerator.DbObjects;

namespace POCOGenerator.MySQL.DbObjects
{
	internal class ProcedureParameter : IProcedureParameter
	{
		#region Database Properties

		public int? ORDINAL_POSITION { get; set; }
		public string PARAMETER_MODE { get; set; }
		public string PARAMETER_NAME { get; set; }
		public string DATA_TYPE { get; set; }
		public int? CHARACTER_MAXIMUM_LENGTH { get; set; }
		public byte? NUMERIC_PRECISION { get; set; }
		public int? NUMERIC_SCALE { get; set; }
		public string DTD_IDENTIFIER { get; set; }

		/* not in use. reduce memory.
        public string SPECIFIC_CATALOG { get; set; }
        public string SPECIFIC_SCHEMA { get; set; }
        public string SPECIFIC_NAME { get; set; }
        public int? CHARACTER_OCTET_LENGTH { get; set; }
        public string CHARACTER_SET_NAME { get; set; }
        public string COLLATION_NAME { get; set; }
        public string ROUTINE_TYPE { get; set; }
        */

		#endregion

		#region IProcedureParameter

		public IProcedure Procedure { get; set; }
		public string ParameterName => PARAMETER_NAME;
		public string ParameterDataType => DATA_TYPE;
		public bool ParameterIsUnsigned => !String.IsNullOrEmpty(DTD_IDENTIFIER) && DTD_IDENTIFIER.ToLower().Contains("unsigned");
		public int? ParameterOrdinal => ORDINAL_POSITION;
		public int? ParameterSize => CHARACTER_MAXIMUM_LENGTH;
		public byte? ParameterPrecision => NUMERIC_PRECISION;
		public int? ParameterScale => NUMERIC_SCALE;
		public int? ParameterDateTimePrecision => CHARACTER_MAXIMUM_LENGTH;
		public string ParameterMode => PARAMETER_MODE;

		public ParameterDirection ParameterDirection {
			get {
				if (String.Compare(ParameterMode, "IN", true) == 0)
				{
					return ParameterDirection.Input;
				}
				else if (String.Compare(ParameterMode, "INOUT", true) == 0)
				{
					return ParameterDirection.InputOutput;
				}
				else if (String.Compare(ParameterMode, "OUT", true) == 0)
				{
					return ParameterDirection.Output;
				}

				return ParameterDirection.Input;
			}
		}

		public bool IsResult => false;

		#endregion

		#region IDescription

		public string Description { get; set; }

		#endregion

		#region IDbObject

		public override string ToString()
		{
			return ParameterName + " (" + DataTypeDisplay + Precision + (ParameterIsUnsigned ? " unsigned" : String.Empty) + ", " + Direction + ")";
		}

		public string DataTypeDisplay => DATA_TYPE.ToLower();

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
					if (ParameterSize != null)
					{
						precision = "(" + ParameterSize + ")";
					}
				}
				else if (dataType == "bit")
				{
					if (ParameterSize != null)
					{
						precision = "(" + ParameterSize + ")";
					}
				}
				else if (dataType is "decimal" or "numeric" or "dec" or "fixed")
				{
					if (ParameterPrecision != null && ParameterScale != null)
					{
						precision = "(" + ParameterPrecision + "," + ParameterScale + ")";
					}
				}
				else if (dataType is "datetime" or "time" or "timestamp")
				{
					if (ParameterDateTimePrecision is not null and > 0)
					{
						precision = "(" + ParameterDateTimePrecision + ")";
					}
				}
				else if (dataType is "enum" or "set")
				{
					if (!String.IsNullOrEmpty(DTD_IDENTIFIER))
					{
						int startIndex = DTD_IDENTIFIER.IndexOf('(');
						if (startIndex != -1)
						{
							int endIndex = DTD_IDENTIFIER.LastIndexOf(')');
							if (endIndex != -1)
							{
								precision = DTD_IDENTIFIER.Substring(startIndex, endIndex - startIndex + 1);
							}
						}
					}
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

		#endregion
	}
}
