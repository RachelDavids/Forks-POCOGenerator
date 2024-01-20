using System;
using System.Collections.Generic;
using System.Drawing;
using POCOGenerator;
using POCOGeneratorUI.TypesMapping;

namespace POCOGeneratorUI
{
	public sealed partial class POCOGeneratorForm
	{
		#region Type Mapping

		private TypesMappingForm SQLServerTypesMapping;
		private TypesMappingForm MySQLTypesMapping;

		private void btnTypeMapping_Click(object sender, EventArgs e)
		{
			if (_rdbms is RDBMS.SQLServer or RDBMS.None)
			{
				SQLServerTypesMapping ??= new TypesMappingForm("SQL Server", GetSQLServerTypesMapping(), 465, 645);
				SQLServerTypesMapping.ShowDialog(this);
			}
			else if (_rdbms == RDBMS.MySQL)
			{
				MySQLTypesMapping ??= new TypesMappingForm("MySQL", GetMySQLTypesMapping(), 576, 797);
				MySQLTypesMapping.ShowDialog(this);
			}
		}

		private static readonly Color textColor = Color.FromArgb(0, 0, 0);
		private static readonly Color keywordColor = Color.FromArgb(0, 0, 255);
		private static readonly Color userTypeColor = Color.FromArgb(43, 145, 175);

		private List<TypeMapping> GetSQLServerTypesMapping()
		{
			TypeMappingPart[] byteArray = new TypeMappingPart[]
										  {
											  new("byte", keywordColor),
											  new("[]", textColor)
										  };
			return
			[
				new TypeMapping("bigint", keywordColor, "long", keywordColor),
				new TypeMapping("binary", keywordColor, byteArray),
				new TypeMapping("bit", keywordColor, "bool", keywordColor),
				new TypeMapping("char", keywordColor, "string", keywordColor),
				new TypeMapping("date", keywordColor, "DateTime", userTypeColor),
				new TypeMapping("datetime", keywordColor, "DateTime", userTypeColor),
				new TypeMapping("datetime2", keywordColor, "DateTime", userTypeColor),
				new TypeMapping("datetimeoffset", keywordColor, "DateTimeOffset", userTypeColor),
				new TypeMapping("decimal", keywordColor, "decimal", keywordColor),
				new TypeMapping("filestream", keywordColor, byteArray),
				new TypeMapping("float", keywordColor, "double", keywordColor),
				new TypeMapping(
								"geography", keywordColor,
								new TypeMappingPart("Microsoft.SqlServer.Types.", textColor),
								new TypeMappingPart("SqlGeography", userTypeColor)
							   ),
				new TypeMapping(
								"geometry", keywordColor,
								new TypeMappingPart("Microsoft.SqlServer.Types.", textColor),
								new TypeMappingPart("SqlGeometry", userTypeColor)
							   ),
				new TypeMapping(
								"hierarchyid", keywordColor,
								new TypeMappingPart("Microsoft.SqlServer.Types.", textColor),
								new TypeMappingPart("SqlHierarchyId", userTypeColor)
							   ),
				new TypeMapping("image", keywordColor, byteArray),
				new TypeMapping("int", keywordColor, "int", keywordColor),
				new TypeMapping("money", keywordColor, "decimal", keywordColor),
				new TypeMapping("nchar", keywordColor, "string", keywordColor),
				new TypeMapping("ntext", keywordColor, "string", keywordColor),
				new TypeMapping("numeric", keywordColor, "decimal", keywordColor),
				new TypeMapping("nvarchar", keywordColor, "string", keywordColor),
				new TypeMapping("real", keywordColor, "float", keywordColor),
				new TypeMapping("rowversion", keywordColor, byteArray),
				new TypeMapping("smalldatetime", keywordColor, "DateTime", userTypeColor),
				new TypeMapping("smallint", keywordColor, "short", keywordColor),
				new TypeMapping("smallmoney", keywordColor, "decimal", keywordColor),
				new TypeMapping("sql_variant", keywordColor, "object", keywordColor),
				new TypeMapping("text", keywordColor, "string", keywordColor),
				new TypeMapping("time", keywordColor, "TimeSpan", userTypeColor),
				new TypeMapping("timestamp", keywordColor, byteArray),
				new TypeMapping("tinyint", keywordColor, "byte", keywordColor),
				new TypeMapping("uniqueidentifier", keywordColor, "Guid", userTypeColor),
				new TypeMapping("varbinary", keywordColor, byteArray),
				new TypeMapping("varchar", keywordColor, "string", keywordColor),
				new TypeMapping("xml", keywordColor, "string", keywordColor),
				new TypeMapping("else", textColor, "object", keywordColor)
			];
		}

		private List<TypeMapping> GetMySQLTypesMapping()
		{
			TypeMappingPart[] byteArray = new TypeMappingPart[]
										  {
											  new("byte", keywordColor),
											  new("[]", textColor)
										  };
			return
			[
				new TypeMapping("bigint", keywordColor, "long", keywordColor),
				new TypeMapping(
								new TypeMappingPart[]
								{
									new("bigint unsigned", keywordColor),
									new("/", textColor),
									new("serial", keywordColor)
								},
								new TypeMappingPart[] { new("decimal", keywordColor) }
							   ),
				new TypeMapping(
								new TypeMappingPart[]
								{
									new("binary", keywordColor),
									new("/", textColor),
									new("char byte", keywordColor)
								},
								byteArray
							   ),
				new TypeMapping("bit", keywordColor, "bool", keywordColor),
				new TypeMapping("blob", keywordColor, byteArray),
				new TypeMapping(
								new TypeMappingPart[]
								{
									new("char", keywordColor),
									new("/", textColor),
									new("character", keywordColor)
								},
								new TypeMappingPart[] { new("string", keywordColor) }
							   ),
				new TypeMapping("date", keywordColor, "DateTime", userTypeColor),
				new TypeMapping("datetime", keywordColor, "DateTime", userTypeColor),
				new TypeMapping(
								new TypeMappingPart[]
								{
									new("decimal", keywordColor),
									new("/", textColor),
									new("numeric", keywordColor),
									new("/", textColor),
									new("dec", keywordColor),
									new("/", textColor),
									new("fixed", keywordColor)
								},
								new TypeMappingPart[] { new("decimal", keywordColor) }
							   ),
				new TypeMapping("double", keywordColor, "double", keywordColor),
				new TypeMapping("double unsigned", keywordColor, "decimal", keywordColor),
				new TypeMapping("enum", keywordColor, "string", keywordColor),
				new TypeMapping("float", keywordColor, "float", keywordColor),
				new TypeMapping("float unsigned", keywordColor, "decimal", keywordColor),
				new TypeMapping(
								new TypeMappingPart[]
								{
									new("geometry", keywordColor),
									new("/", textColor),
									new("geometrycollection", keywordColor),
									new("/", textColor),
									new("geomcollection", keywordColor)
								},
								new TypeMappingPart[]
								{
									new("System.Data.Spatial.", textColor),
									new("DbGeometry", userTypeColor)
								}
							   ),
				new TypeMapping(
								new TypeMappingPart[]
								{
									new("int", keywordColor),
									new("/", textColor),
									new("integer", keywordColor)
								},
								new TypeMappingPart[] { new("int", keywordColor) }
							   ),
				new TypeMapping(
								new TypeMappingPart[]
								{
									new("int", keywordColor),
									new("/", textColor),
									new("integer unsigned", keywordColor)
								},
								new TypeMappingPart[] { new("long", keywordColor) }
							   ),
				new TypeMapping("json", keywordColor, "string", keywordColor),
				new TypeMapping(
								new TypeMappingPart[]
								{
									new("linestring", keywordColor),
									new("/", textColor),
									new("multilinestring", keywordColor)
								},
								new TypeMappingPart[]
								{
									new("System.Data.Spatial.", textColor),
									new("DbGeometry", userTypeColor)
								}
							   ),
				new TypeMapping("longblob", keywordColor, byteArray),
				new TypeMapping("longtext", keywordColor, "string", keywordColor),
				new TypeMapping("mediumblob", keywordColor, byteArray),
				new TypeMapping("mediumint", keywordColor, "int", keywordColor),
				new TypeMapping("mediumint unsigned", keywordColor, "int", keywordColor),
				new TypeMapping("mediumtext", keywordColor, "string", keywordColor),
				new TypeMapping(
								new TypeMappingPart[]
								{
									new("nchar", keywordColor),
									new("/", textColor),
									new("national char", keywordColor)
								},
								new TypeMappingPart[] { new("string", keywordColor) }
							   ),
				new TypeMapping(
								new TypeMappingPart[]
								{
									new("nvarchar", keywordColor),
									new("/", textColor),
									new("national varchar", keywordColor)
								},
								new TypeMappingPart[] { new("string", keywordColor) }
							   ),
				new TypeMapping(
								new TypeMappingPart[]
								{
									new("point", keywordColor),
									new("/", textColor),
									new("multipoint", keywordColor)
								},
								new TypeMappingPart[]
								{
									new("System.Data.Spatial.", textColor),
									new("DbGeometry", userTypeColor)
								}
							   ),
				new TypeMapping(
								new TypeMappingPart[]
								{
									new("polygon", keywordColor),
									new("/", textColor),
									new("multipolygon", keywordColor)
								},
								new TypeMappingPart[]
								{
									new("System.Data.Spatial.", textColor),
									new("DbGeometry", userTypeColor)
								}
							   ),
				new TypeMapping(
								new TypeMappingPart[]
								{
									new("real", keywordColor),
									new(" (REAL_AS_FLOAT off)", textColor)
								},
								new TypeMappingPart[] { new("double", keywordColor) }
							   ),
				new TypeMapping(
								new TypeMappingPart[]
								{
									new("real", keywordColor),
									new(" (REAL_AS_FLOAT on)", textColor)
								},
								new TypeMappingPart[] { new("float", keywordColor) }
							   ),
				new TypeMapping("set", keywordColor, "string", keywordColor),
				new TypeMapping("smallint", keywordColor, "short", keywordColor),
				new TypeMapping("smallint unsigned", keywordColor, "int", keywordColor),
				new TypeMapping("text", keywordColor, "string", keywordColor),
				new TypeMapping("time", keywordColor, "TimeSpan", userTypeColor),
				new TypeMapping("timestamp", keywordColor, "DateTime", userTypeColor),
				new TypeMapping("tinyblob", keywordColor, byteArray),
				new TypeMapping(
								new TypeMappingPart[]
								{
									new("tinyint(1)", keywordColor),
									new("/", textColor),
									new("bool", keywordColor),
									new("/", textColor),
									new("boolean", keywordColor)
								},
								new TypeMappingPart[] { new("bool", keywordColor) }
							   ),
				new TypeMapping("tinyint", keywordColor, "sbyte", keywordColor),
				new TypeMapping("tinyint unsigned", keywordColor, "byte", keywordColor),
				new TypeMapping("tinytext", keywordColor, "string", keywordColor),
				new TypeMapping("varbinary", keywordColor, byteArray),
				new TypeMapping(
								new TypeMappingPart[]
								{
									new("varchar", keywordColor),
									new("/", textColor),
									new("character varying", keywordColor)
								},
								new TypeMappingPart[] { new("string", keywordColor) }
							   ),
				new TypeMapping("year", keywordColor, "short", keywordColor),
				new TypeMapping("else", textColor, "object", keywordColor)
			];
		}

		#endregion
	}
}
