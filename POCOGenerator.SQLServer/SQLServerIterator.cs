using System;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

using Microsoft.SqlServer.Types;

using POCOGenerator.DbObjects;
using POCOGenerator.POCOIterators;
using POCOGenerator.POCOWriters;

namespace POCOGenerator.SQLServer
{
	internal class SQLServerIterator(IWriter writer, IDbSupport support, IDbIteratorSettings settings)
		: DbIterator(writer, support, settings)
	{
		protected override void WriteSpecialSQLTypesUsingClause(string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.WriteKeyword("using");
			Writer.WriteLine(" Microsoft.SqlServer.Types;");
		}

		protected override bool IsSQLTypeMappedToBool(string dataTypeName, bool isUnsigned, int? numericPrecision)
		{
			return dataTypeName == "bit";
		}

		protected override bool IsSQLTypeMappedToBoolTrue(string dataTypeName, bool isUnsigned, int? numericPrecision, string cleanColumnDefault)
		{
			return
				IsSQLTypeMappedToBool(dataTypeName, isUnsigned, numericPrecision) &&
				(cleanColumnDefault.IndexOf("1", StringComparison.OrdinalIgnoreCase) != -1);
		}

		protected override bool IsSQLTypeMappedToBoolFalse(string dataTypeName, bool isUnsigned, int? numericPrecision, string cleanColumnDefault)
		{
			return
				IsSQLTypeMappedToBool(dataTypeName, isUnsigned, numericPrecision) &&
				(cleanColumnDefault.IndexOf("0", StringComparison.OrdinalIgnoreCase) != -1);
		}

		protected override bool IsSQLTypeMappedToByte(string dataTypeName, bool isUnsigned, int? numericPrecision)
		{
			return dataTypeName == "tinyint";
		}

		protected override bool IsSQLTypeMappedToShort(string dataTypeName, bool isUnsigned, int? numericPrecision)
		{
			return dataTypeName == "smallint";
		}

		protected override bool IsSQLTypeMappedToInt(string dataTypeName, bool isUnsigned, int? numericPrecision)
		{
			return dataTypeName == "int";
		}

		protected override bool IsSQLTypeMappedToLong(string dataTypeName, bool isUnsigned, int? numericPrecision)
		{
			return dataTypeName == "bigint";
		}

		protected override bool IsSQLTypeMappedToFloat(string dataTypeName, bool isUnsigned, int? numericPrecision, int? numericScale)
		{
			return dataTypeName == "real";
		}

		protected override bool IsSQLTypeMappedToDouble(string dataTypeName, bool isUnsigned, int? numericPrecision, int? numericScale)
		{
			return dataTypeName == "float";
		}

		protected override bool IsSQLTypeMappedToDecimal(string dataTypeName, bool isUnsigned, int? numericPrecision, int? numericScale)
		{
			return dataTypeName is "decimal" or "numeric" or "money" or "smallmoney";
		}

		protected override bool IsSQLTypeMappedToDateTime(string dataTypeName, int? dateTimePrecision)
		{
			return dataTypeName is "date" or "datetime" or "datetime2" or "smalldatetime";
		}

		protected override bool IsSQLTypeMappedToTimeSpan(string dataTypeName, int? dateTimePrecision)
		{
			return dataTypeName == "time";
		}

		protected override bool IsSQLTypeMappedToDateTimeOffset(string dataTypeName, int? dateTimePrecision)
		{
			return dataTypeName == "datetimeoffset";
		}

		protected override bool IsColumnDefaultNow(string cleanColumnDefault)
		{
			return
				cleanColumnDefault.IndexOf("getdate", StringComparison.OrdinalIgnoreCase) != -1 ||
				cleanColumnDefault.IndexOf("sysdatetime", StringComparison.OrdinalIgnoreCase) != -1 ||
				cleanColumnDefault.IndexOf("current_timestamp", StringComparison.OrdinalIgnoreCase) != -1
			;
		}

		protected override bool IsColumnDefaultUtcNow(string cleanColumnDefault)
		{
			return
				cleanColumnDefault.IndexOf("getutcdate", StringComparison.OrdinalIgnoreCase) != -1 ||
				cleanColumnDefault.IndexOf("sysutcdatetime", StringComparison.OrdinalIgnoreCase) != -1
			;
		}

		protected override bool IsColumnDefaultOffsetNow(string cleanColumnDefault)
		{
			return cleanColumnDefault.IndexOf("sysdatetimeoffset", StringComparison.OrdinalIgnoreCase) != -1;
		}

		protected override bool IsSQLTypeMappedToString(string dataTypeName, int? stringPrecision)
		{
			return dataTypeName is "char" or "nchar" or "ntext" or "nvarchar" or "text" or "varchar" or "xml";
		}

		protected override bool IsSQLTypeMappedToByteArray(string dataTypeName)
		{
			return dataTypeName is "binary" or "varbinary" or "filestream" or "image" or "timestamp" or "rowversion";
		}

		protected override bool IsSQLTypeMappedToGuid(string dataTypeName)
		{
			return dataTypeName == "uniqueidentifier";
		}

		protected override bool IsSQLTypeMappedToGuidNewGuid(string dataTypeName, string cleanColumnDefault)
		{
			return
				IsSQLTypeMappedToGuid(dataTypeName) &&
				(cleanColumnDefault.IndexOf("newid", StringComparison.OrdinalIgnoreCase) != -1);
		}

		protected override bool IsSQLTypeRDBMSSpecificType(string dataTypeName)
		{
			return IsSQLTypeMappedToSqlGeography(dataTypeName) || IsSQLTypeMappedToSqlGeometry(dataTypeName) || IsSQLTypeMappedToSqlHierarchyId(dataTypeName);
		}

		protected virtual bool IsSQLTypeMappedToSqlGeography(string dataTypeName)
		{
			return dataTypeName.Contains("geography");
		}

		protected virtual bool IsSQLTypeMappedToSqlGeometry(string dataTypeName)
		{
			return dataTypeName.Contains("geometry");
		}

		protected virtual bool IsSQLTypeMappedToSqlHierarchyId(string dataTypeName)
		{
			return dataTypeName.Contains("hierarchyid");
		}

		protected override void WriteColumnDefaultConstructorInitializationByteArray(string columnDefault, ITableColumn column, string namespaceOffset)
		{
			WriteColumnDefaultConstructorInitializationByteArray_Hex(columnDefault, column, namespaceOffset);
		}

		protected override void WriteColumnDefaultConstructorInitializationRDBMSSpecificType(string columnDefault, ITableColumn column, string namespaceOffset)
		{
			string dataTypeName = (column.DataTypeName ?? String.Empty).ToLower();
			if (IsSQLTypeMappedToSqlGeography(dataTypeName) || IsSQLTypeMappedToSqlGeometry(dataTypeName))
			{
				WriteColumnDefaultConstructorInitializationSqlGeographySqlGeometry(columnDefault, column, namespaceOffset, IsSQLTypeMappedToSqlGeography(dataTypeName));
			}
			else if (IsSQLTypeMappedToSqlHierarchyId(dataTypeName))
			{
				WriteColumnDefaultConstructorInitializationSqlHierarchyId(columnDefault, column, namespaceOffset);
			}
			else
			{
				WriteColumnDefaultConstructorInitializationComment(column.ColumnDefault, column, namespaceOffset);
			}
		}

		protected static readonly Regex regexSqlGeographySqlGeometry1 = new(@"\(\[(?i:geography|geometry)]:+(?i:Parse)\(+'(?<param1>.*?)'\s*\)+", RegexOptions.Compiled);
		protected static readonly Regex regexSqlGeographySqlGeometry2 = new(@"\(\[(?i:geography|geometry)]:+(?i:Point)\(+(?<param1>[0-9.-]+)\)+\s*,\s*\(+(?<param2>[0-9.-]+)\)+\s*,\s*\(+(?<param3>\d+)\)+", RegexOptions.Compiled);
		protected static readonly Regex regexSqlGeographySqlGeometry3 = new(@"\(\[(?i:geography|geometry)]:+(?<methodName>[a-zA-Z]+)\(+'?(?<param1>.*?)'?\s*,\s*\(*(?<param2>\d+)\)+", RegexOptions.Compiled);

		protected virtual void WriteColumnDefaultConstructorInitializationSqlGeographySqlGeometry(string columnDefault, ITableColumn column, string namespaceOffset, bool isGeography)
		{
			Match match1 = regexSqlGeographySqlGeometry1.Match(columnDefault);
			Match match2 = regexSqlGeographySqlGeometry2.Match(columnDefault);
			Match match3 = regexSqlGeographySqlGeometry3.Match(columnDefault);

			string methodName = null;
			string param1 = null;
			string param2 = null;
			string param3 = null;

			if (match1.Success)
			{
				methodName = "Parse";
				param1 = match1.Groups["param1"].Value;
			}
			else if (match2.Success)
			{
				methodName = "Point";
				param1 = match2.Groups["param1"].Value;
				param2 = match2.Groups["param2"].Value;
				param3 = match2.Groups["param3"].Value;
			}
			else if (match3.Success)
			{
				methodName = match3.Groups["methodName"].Value;
				param1 = match3.Groups["param1"].Value;
				param2 = match3.Groups["param2"].Value;
			}
			else
			{
				methodName = "Parse";
				param1 = columnDefault;
			}

			if (methodName == "Parse")
			{
				bool isComment = false;
				try
				{
					if (isGeography)
					{
						SqlGeography.Parse(new SqlString(param1));
					}
					else
					{
						SqlGeometry.Parse(new SqlString(param1));
					}
				}
				catch
				{
					isComment = true;
				}

				WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset, isComment);

				if (isComment)
				{
					if (!Settings.POCOIteratorSettings.Using)
					{
						Writer.WriteComment("Microsoft.SqlServer.Types.");
					}

					Writer.WriteComment(isGeography ? "SqlGeography" : "SqlGeometry");
					Writer.WriteComment(".Parse(");
					Writer.WriteComment("new");
					Writer.WriteComment(" System.Data.SqlTypes.");
					Writer.WriteComment("SqlString");
					Writer.WriteComment("(");
					Writer.WriteComment("\"");
					Writer.WriteComment(param1.Replace("\\", "\\\\").Replace("\"", "\\\""));
					Writer.WriteComment("\"");
					Writer.WriteComment("))");
				}
				else
				{
					if (!Settings.POCOIteratorSettings.Using)
					{
						Writer.Write("Microsoft.SqlServer.Types.");
					}

					Writer.WriteUserType(isGeography ? "SqlGeography" : "SqlGeometry");
					Writer.Write(".Parse(");
					Writer.WriteKeyword("new");
					Writer.Write(" System.Data.SqlTypes.");
					Writer.WriteUserType("SqlString");
					Writer.Write("(");
					Writer.WriteString("\"");
					Writer.WriteString(param1.Replace("\\", "\\\\").Replace("\"", "\\\""));
					Writer.WriteString("\"");
					Writer.Write("))");
				}

				WriteColumnDefaultConstructorInitializationEnd(isComment);
			}
			else if (methodName == "Point")
			{
				bool isComment = false;
				try
				{
					if (isGeography)
					{
						SqlGeography.Point(Double.Parse(param1), Double.Parse(param2), Int32.Parse(param3));
					}
					else
					{
						SqlGeometry.Point(Double.Parse(param1), Double.Parse(param2), Int32.Parse(param3));
					}
				}
				catch
				{
					isComment = true;
				}

				WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset, isComment);

				if (isComment)
				{
					if (!Settings.POCOIteratorSettings.Using)
					{
						Writer.WriteComment("Microsoft.SqlServer.Types.");
					}

					Writer.WriteComment(isGeography ? "SqlGeography" : "SqlGeometry");
					Writer.WriteComment(".Point(");
					Writer.WriteComment(param1);
					Writer.WriteComment(", ");
					Writer.WriteComment(param2);
					Writer.WriteComment(", ");
					Writer.WriteComment(param3);
					Writer.WriteComment(")");
				}
				else
				{
					if (!Settings.POCOIteratorSettings.Using)
					{
						Writer.Write("Microsoft.SqlServer.Types.");
					}

					Writer.WriteUserType(isGeography ? "SqlGeography" : "SqlGeometry");
					Writer.Write(".Point(");
					Writer.Write(param1);
					Writer.Write(", ");
					Writer.Write(param2);
					Writer.Write(", ");
					Writer.Write(param3);
					Writer.Write(")");
				}

				WriteColumnDefaultConstructorInitializationEnd(isComment);
			}
			else if (methodName == "GeomFromGml")
			{
				bool isComment = false;
				try
				{
					if (isGeography)
					{
						SqlGeography.GeomFromGml(new SqlXml(XmlReader.Create(new StringReader(param1))), Int32.Parse(param2));
					}
					else
					{
						SqlGeometry.GeomFromGml(new SqlXml(XmlReader.Create(new StringReader(param1))), Int32.Parse(param2));
					}
				}
				catch
				{
					isComment = true;
				}

				WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset, isComment);

				if (isComment)
				{
					if (!Settings.POCOIteratorSettings.Using)
					{
						Writer.WriteComment("Microsoft.SqlServer.Types.");
					}

					Writer.WriteComment(isGeography ? "SqlGeography" : "SqlGeometry");
					Writer.WriteComment(".GeomFromGml(");
					Writer.WriteComment("new");
					Writer.WriteComment(" System.Data.SqlTypes.");
					Writer.WriteComment("SqlXml");
					Writer.WriteComment("(System.Xml.");
					Writer.WriteComment("XmlReader");
					Writer.WriteComment(".Create(");
					Writer.WriteComment("new");
					Writer.WriteComment(" System.IO.");
					Writer.WriteComment("StringReader");
					Writer.WriteComment("(");
					Writer.WriteComment("\"");
					Writer.WriteComment(param1.Replace("\\", "\\\\").Replace("\"", "\\\""));
					Writer.WriteComment("\"");
					Writer.WriteComment("))), ");
					Writer.WriteComment(param2);
					Writer.WriteComment(")");
				}
				else
				{
					if (!Settings.POCOIteratorSettings.Using)
					{
						Writer.Write("Microsoft.SqlServer.Types.");
					}

					Writer.WriteUserType(isGeography ? "SqlGeography" : "SqlGeometry");
					Writer.Write(".GeomFromGml(");
					Writer.WriteKeyword("new");
					Writer.Write(" System.Data.SqlTypes.");
					Writer.WriteUserType("SqlXml");
					Writer.Write("(System.Xml.");
					Writer.WriteUserType("XmlReader");
					Writer.Write(".Create(");
					Writer.WriteKeyword("new");
					Writer.Write(" System.IO.");
					Writer.WriteUserType("StringReader");
					Writer.Write("(");
					Writer.WriteString("\"");
					Writer.WriteString(param1.Replace("\\", "\\\\").Replace("\"", "\\\""));
					Writer.WriteString("\"");
					Writer.Write("))), ");
					Writer.Write(param2);
					Writer.Write(")");
				}

				WriteColumnDefaultConstructorInitializationEnd(isComment);
			}
			else if (methodName is "STGeomCollFromText" or "STGeomFromText" or "STLineFromText" or "STMLineFromText" or "STMPointFromText" or "STMPolyFromText" or "STPointFromText" or "STPolyFromText")
			{
				bool isComment = false;
				try
				{
					if (methodName == "STGeomCollFromText")
					{
						if (isGeography)
						{
							SqlGeography.STGeomCollFromText(new SqlChars(new SqlString(param1)), Int32.Parse(param2));
						}
						else
						{
							SqlGeometry.STGeomCollFromText(new SqlChars(new SqlString(param1)), Int32.Parse(param2));
						}
					}
					else if (methodName == "STGeomFromText")
					{
						if (isGeography)
						{
							SqlGeography.STGeomFromText(new SqlChars(new SqlString(param1)), Int32.Parse(param2));
						}
						else
						{
							SqlGeometry.STGeomFromText(new SqlChars(new SqlString(param1)), Int32.Parse(param2));
						}
					}
					else if (methodName == "STLineFromText")
					{
						if (isGeography)
						{
							SqlGeography.STLineFromText(new SqlChars(new SqlString(param1)), Int32.Parse(param2));
						}
						else
						{
							SqlGeometry.STLineFromText(new SqlChars(new SqlString(param1)), Int32.Parse(param2));
						}
					}
					else if (methodName == "STMLineFromText")
					{
						if (isGeography)
						{
							SqlGeography.STMLineFromText(new SqlChars(new SqlString(param1)), Int32.Parse(param2));
						}
						else
						{
							SqlGeometry.STMLineFromText(new SqlChars(new SqlString(param1)), Int32.Parse(param2));
						}
					}
					else if (methodName == "STMPointFromText")
					{
						if (isGeography)
						{
							SqlGeography.STMPointFromText(new SqlChars(new SqlString(param1)), Int32.Parse(param2));
						}
						else
						{
							SqlGeometry.STMPointFromText(new SqlChars(new SqlString(param1)), Int32.Parse(param2));
						}
					}
					else if (methodName == "STMPolyFromText")
					{
						if (isGeography)
						{
							SqlGeography.STMPolyFromText(new SqlChars(new SqlString(param1)), Int32.Parse(param2));
						}
						else
						{
							SqlGeometry.STMPolyFromText(new SqlChars(new SqlString(param1)), Int32.Parse(param2));
						}
					}
					else if (methodName == "STPointFromText")
					{
						if (isGeography)
						{
							SqlGeography.STPointFromText(new SqlChars(new SqlString(param1)), Int32.Parse(param2));
						}
						else
						{
							SqlGeometry.STPointFromText(new SqlChars(new SqlString(param1)), Int32.Parse(param2));
						}
					}
					else if (methodName == "STPolyFromText")
					{
						if (isGeography)
						{
							SqlGeography.STPolyFromText(new SqlChars(new SqlString(param1)), Int32.Parse(param2));
						}
						else
						{
							SqlGeometry.STPolyFromText(new SqlChars(new SqlString(param1)), Int32.Parse(param2));
						}
					}
				}
				catch
				{
					isComment = true;
				}

				WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset, isComment);

				if (isComment)
				{
					if (!Settings.POCOIteratorSettings.Using)
					{
						Writer.WriteComment("Microsoft.SqlServer.Types.");
					}

					Writer.WriteComment(isGeography ? "SqlGeography" : "SqlGeometry");
					Writer.WriteComment(".");
					Writer.WriteComment(methodName);
					Writer.WriteComment("(");
					Writer.WriteComment("new");
					Writer.WriteComment(" System.Data.SqlTypes.");
					Writer.WriteComment("SqlChars");
					Writer.WriteComment("(");
					Writer.WriteComment("new");
					Writer.WriteComment(" System.Data.SqlTypes.");
					Writer.WriteComment("SqlString");
					Writer.WriteComment("(");
					Writer.WriteComment("\"");
					Writer.WriteComment(param1.Replace("\\", "\\\\").Replace("\"", "\\\""));
					Writer.WriteComment("\"");
					Writer.WriteComment(")), ");
					Writer.WriteComment(param2);
					Writer.WriteComment(")");
				}
				else
				{
					if (!Settings.POCOIteratorSettings.Using)
					{
						Writer.Write("Microsoft.SqlServer.Types.");
					}

					Writer.WriteUserType(isGeography ? "SqlGeography" : "SqlGeometry");
					Writer.Write(".");
					Writer.Write(methodName);
					Writer.Write("(");
					Writer.WriteKeyword("new");
					Writer.Write(" System.Data.SqlTypes.");
					Writer.WriteUserType("SqlChars");
					Writer.Write("(");
					Writer.WriteKeyword("new");
					Writer.Write(" System.Data.SqlTypes.");
					Writer.WriteUserType("SqlString");
					Writer.Write("(");
					Writer.WriteString("\"");
					Writer.WriteString(param1.Replace("\\", "\\\\").Replace("\"", "\\\""));
					Writer.WriteString("\"");
					Writer.Write(")), ");
					Writer.Write(param2);
					Writer.Write(")");
				}

				WriteColumnDefaultConstructorInitializationEnd(isComment);
			}
			else if (methodName is "STGeomCollFromWKB" or "STGeomFromWKB" or "STLineFromWKB" or "STMLineFromWKB" or "STMPointFromWKB" or "STMPolyFromWKB" or "STPointFromWKB" or "STPolyFromWKB")
			{
				if (param1.StartsWith("0x"))
				{
					param1 = param1[2..];
				}

				bool isComment = false;
				try
				{
					if (methodName == "STGeomCollFromWKB")
					{
						if (isGeography)
						{
							SqlGeography.STGeomCollFromWKB(new SqlBytes(Enumerable.Range(0, param1.Length / 2).Select(x => Convert.ToByte(param1.Substring(x * 2, 2), 16)).ToArray()), Int32.Parse(param2));
						}
						else
						{
							SqlGeometry.STGeomCollFromWKB(new SqlBytes(Enumerable.Range(0, param1.Length / 2).Select(x => Convert.ToByte(param1.Substring(x * 2, 2), 16)).ToArray()), Int32.Parse(param2));
						}
					}
					else if (methodName == "STGeomFromWKB")
					{
						if (isGeography)
						{
							SqlGeography.STGeomFromWKB(new SqlBytes(Enumerable.Range(0, param1.Length / 2).Select(x => Convert.ToByte(param1.Substring(x * 2, 2), 16)).ToArray()), Int32.Parse(param2));
						}
						else
						{
							SqlGeometry.STGeomFromWKB(new SqlBytes(Enumerable.Range(0, param1.Length / 2).Select(x => Convert.ToByte(param1.Substring(x * 2, 2), 16)).ToArray()), Int32.Parse(param2));
						}
					}
					else if (methodName == "STLineFromWKB")
					{
						if (isGeography)
						{
							SqlGeography.STLineFromWKB(new SqlBytes(Enumerable.Range(0, param1.Length / 2).Select(x => Convert.ToByte(param1.Substring(x * 2, 2), 16)).ToArray()), Int32.Parse(param2));
						}
						else
						{
							SqlGeometry.STLineFromWKB(new SqlBytes(Enumerable.Range(0, param1.Length / 2).Select(x => Convert.ToByte(param1.Substring(x * 2, 2), 16)).ToArray()), Int32.Parse(param2));
						}
					}
					else if (methodName == "STMLineFromWKB")
					{
						if (isGeography)
						{
							SqlGeography.STMLineFromWKB(new SqlBytes(Enumerable.Range(0, param1.Length / 2).Select(x => Convert.ToByte(param1.Substring(x * 2, 2), 16)).ToArray()), Int32.Parse(param2));
						}
						else
						{
							SqlGeometry.STMLineFromWKB(new SqlBytes(Enumerable.Range(0, param1.Length / 2).Select(x => Convert.ToByte(param1.Substring(x * 2, 2), 16)).ToArray()), Int32.Parse(param2));
						}
					}
					else if (methodName == "STMPointFromWKB")
					{
						if (isGeography)
						{
							SqlGeography.STMPointFromWKB(new SqlBytes(Enumerable.Range(0, param1.Length / 2).Select(x => Convert.ToByte(param1.Substring(x * 2, 2), 16)).ToArray()), Int32.Parse(param2));
						}
						else
						{
							SqlGeometry.STMPointFromWKB(new SqlBytes(Enumerable.Range(0, param1.Length / 2).Select(x => Convert.ToByte(param1.Substring(x * 2, 2), 16)).ToArray()), Int32.Parse(param2));
						}
					}
					else if (methodName == "STMPolyFromWKB")
					{
						if (isGeography)
						{
							SqlGeography.STMPolyFromWKB(new SqlBytes(Enumerable.Range(0, param1.Length / 2).Select(x => Convert.ToByte(param1.Substring(x * 2, 2), 16)).ToArray()), Int32.Parse(param2));
						}
						else
						{
							SqlGeometry.STMPolyFromWKB(new SqlBytes(Enumerable.Range(0, param1.Length / 2).Select(x => Convert.ToByte(param1.Substring(x * 2, 2), 16)).ToArray()), Int32.Parse(param2));
						}
					}
					else if (methodName == "STPointFromWKB")
					{
						if (isGeography)
						{
							SqlGeography.STPointFromWKB(new SqlBytes(Enumerable.Range(0, param1.Length / 2).Select(x => Convert.ToByte(param1.Substring(x * 2, 2), 16)).ToArray()), Int32.Parse(param2));
						}
						else
						{
							SqlGeometry.STPointFromWKB(new SqlBytes(Enumerable.Range(0, param1.Length / 2).Select(x => Convert.ToByte(param1.Substring(x * 2, 2), 16)).ToArray()), Int32.Parse(param2));
						}
					}
					else if (methodName == "STPolyFromWKB")
					{
						if (isGeography)
						{
							SqlGeography.STPolyFromWKB(new SqlBytes(Enumerable.Range(0, param1.Length / 2).Select(x => Convert.ToByte(param1.Substring(x * 2, 2), 16)).ToArray()), Int32.Parse(param2));
						}
						else
						{
							SqlGeometry.STPolyFromWKB(new SqlBytes(Enumerable.Range(0, param1.Length / 2).Select(x => Convert.ToByte(param1.Substring(x * 2, 2), 16)).ToArray()), Int32.Parse(param2));
						}
					}
				}
				catch
				{
					isComment = true;
				}

				WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset, isComment);

				if (isComment)
				{
					if (!Settings.POCOIteratorSettings.Using)
					{
						Writer.WriteComment("Microsoft.SqlServer.Types.");
					}

					Writer.WriteComment(isGeography ? "SqlGeography" : "SqlGeometry");
					Writer.WriteComment(".");
					Writer.WriteComment(methodName);
					Writer.WriteComment("(");
					Writer.WriteComment("new");
					Writer.WriteComment(" System.Data.SqlTypes.");
					Writer.WriteComment("SqlBytes");
					Writer.WriteComment("(System.Linq.");
					Writer.WriteComment("Enumerable");
					Writer.WriteComment(".Range(0, ");
					Writer.WriteComment((param1.Length / 2).ToString());
					Writer.WriteComment(").Select(x => ");
					Writer.WriteComment("Convert");
					Writer.WriteComment(".ToByte(");
					Writer.WriteComment("\"");
					Writer.WriteComment(param1.Replace("\\", "\\\\").Replace("\"", "\\\""));
					Writer.WriteComment("\"");
					Writer.WriteComment(".Substring(x * 2, 2), 16)).ToArray()), ");
					Writer.WriteComment(param2);
					Writer.WriteComment(")");
				}
				else
				{
					if (!Settings.POCOIteratorSettings.Using)
					{
						Writer.Write("Microsoft.SqlServer.Types.");
					}

					Writer.WriteUserType(isGeography ? "SqlGeography" : "SqlGeometry");
					Writer.Write(".");
					Writer.Write(methodName);
					Writer.Write("(");
					Writer.WriteKeyword("new");
					Writer.Write(" System.Data.SqlTypes.");
					Writer.WriteUserType("SqlBytes");
					Writer.Write("(System.Linq.");
					Writer.WriteUserType("Enumerable");
					Writer.Write(".Range(0, ");
					Writer.Write((param1.Length / 2).ToString());
					Writer.Write(").Select(x => ");
					Writer.WriteUserType("Convert");
					Writer.Write(".ToByte(");
					Writer.WriteString("\"");
					Writer.WriteString(param1.Replace("\\", "\\\\").Replace("\"", "\\\""));
					Writer.WriteString("\"");
					Writer.Write(".Substring(x * 2, 2), 16)).ToArray()), ");
					Writer.Write(param2);
					Writer.Write(")");
				}

				WriteColumnDefaultConstructorInitializationEnd(isComment);
			}
		}

		protected static readonly Regex regexHierarchyId = new(@"\(\[(?i:hierarchyid)]:+(?i:Parse)\(+'(?<param1>.*?)'\s*\)+", RegexOptions.Compiled);

		protected virtual void WriteColumnDefaultConstructorInitializationSqlHierarchyId(string columnDefault, ITableColumn column, string namespaceOffset)
		{
			Match match = regexHierarchyId.Match(columnDefault);

			string param1 = columnDefault;
			if (match.Success)
			{
				param1 = match.Groups["param1"].Value;
			}

			bool isComment = false;
			try
			{
				SqlHierarchyId.Parse(new SqlString(param1));
			}
			catch
			{
				isComment = true;
			}

			WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset, isComment);

			if (isComment)
			{
				if (!Settings.POCOIteratorSettings.Using)
				{
					Writer.WriteComment("Microsoft.SqlServer.Types.");
				}

				Writer.WriteComment("SqlHierarchyId");
				Writer.WriteComment(".Parse(");
				Writer.WriteComment("new");
				Writer.WriteComment(" System.Data.SqlTypes.");
				Writer.WriteComment("SqlString");
				Writer.WriteComment("(");
				Writer.WriteComment("\"");
				Writer.WriteComment(param1.Replace("\\", "\\\\").Replace("\"", "\\\""));
				Writer.WriteComment("\"");
				Writer.WriteComment("))");
			}
			else
			{
				if (!Settings.POCOIteratorSettings.Using)
				{
					Writer.Write("Microsoft.SqlServer.Types.");
				}

				Writer.WriteUserType("SqlHierarchyId");
				Writer.Write(".Parse(");
				Writer.WriteKeyword("new");
				Writer.Write(" System.Data.SqlTypes.");
				Writer.WriteUserType("SqlString");
				Writer.Write("(");
				Writer.WriteString("\"");
				Writer.WriteString(param1.Replace("\\", "\\\\").Replace("\"", "\\\""));
				Writer.WriteString("\"");
				Writer.Write("))");
			}

			WriteColumnDefaultConstructorInitializationEnd(isComment);
		}

		protected override bool IsEFAttributeMaxLength(string dataTypeName)
		{
			return dataTypeName is "binary" or "char" or "nchar" or "nvarchar" or "varbinary" or "varchar";
		}

		protected override bool IsEFAttributeStringLength(string dataTypeName)
		{
			return dataTypeName is "binary" or "char" or "nchar" or "nvarchar" or "varbinary" or "varchar";
		}

		protected override bool IsEFAttributeTimestamp(string dataTypeName)
		{
			return dataTypeName == "timestamp";
		}

		protected override bool IsEFAttributeConcurrencyCheck(string dataTypeName)
		{
			return dataTypeName is "timestamp" or "rowversion";
		}

		protected override void WriteDbColumnDataType(IColumn column)
		{
			switch ((column.DataTypeDisplay ?? String.Empty).ToLower())
			{
				case "bit": WriteColumnBool(column.IsNullable); break;

				case "tinyint": WriteColumnByte(column.IsNullable); break;

				case "binary":
				case "filestream":
				case "image":
				case "rowversion":
				case "timestamp":
				case "varbinary": WriteColumnByteArray(); break;

				case "date":
				case "datetime":
				case "datetime2":
				case "smalldatetime": WriteColumnDateTime(column.IsNullable); break;

				case "datetimeoffset": WriteColumnDateTimeOffset(column.IsNullable); break;

				case "decimal":
				case "money":
				case "numeric":
				case "smallmoney": WriteColumnDecimal(column.IsNullable); break;

				case "float": WriteColumnDouble(column.IsNullable); break;

				case "real": WriteColumnFloat(column.IsNullable); break;

				case "uniqueidentifier": WriteColumnGuid(column.IsNullable); break;

				case "int": WriteColumnInt(column.IsNullable); break;

				case "bigint": WriteColumnLong(column.IsNullable); break;

				case "geography": WriteColumnSqlGeography(); break;
				case "geometry": WriteColumnSqlGeometry(); break;
				case "hierarchyid": WriteColumnSqlHierarchyId(); break;

				case "smallint": WriteColumnShort(column.IsNullable); break;

				case "char":
				case "nchar":
				case "ntext":
				case "nvarchar":
				case "text":
				case "varchar":
				case "xml": WriteColumnString(); break;

				case "time": WriteColumnTimeSpan(column.IsNullable); break;

				case "sql_variant":
				default: WriteColumnObject(); break;
			}
		}

		protected virtual void WriteColumnSqlGeography()
		{
			if (!Settings.POCOIteratorSettings.Using)
			{
				Writer.Write("Microsoft.SqlServer.Types.");
			}

			Writer.WriteUserType("SqlGeography");
		}

		protected virtual void WriteColumnSqlGeometry()
		{
			if (!Settings.POCOIteratorSettings.Using)
			{
				Writer.Write("Microsoft.SqlServer.Types.");
			}

			Writer.WriteUserType("SqlGeometry");
		}

		protected virtual void WriteColumnSqlHierarchyId()
		{
			if (!Settings.POCOIteratorSettings.Using)
			{
				Writer.Write("Microsoft.SqlServer.Types.");
			}

			Writer.WriteUserType("SqlHierarchyId");
		}
	}
}
