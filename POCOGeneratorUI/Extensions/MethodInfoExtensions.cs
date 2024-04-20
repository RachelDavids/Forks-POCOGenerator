using System.Collections.Generic;
using System.Text;

namespace System.Reflection
{
	// https://gist.github.com/jamietre/4476307

	public static partial class MethodInfoExtensions
	{
		/// <summary>
		/// Return the method signature as a string.
		/// </summary>
		///
		/// <param name="property">
		/// The property to act on.
		/// </param>
		///
		/// <returns>
		/// Method signature.
		/// </returns>
		public static string GetSignature(this PropertyInfo property)
		{
			MethodInfo getter = property.GetGetMethod();
			MethodInfo setter = property.GetSetMethod();

			StringBuilder sigBuilder = new();
			MethodInfo primaryDef = LeastRestrictiveVisibility(getter, setter);


			BuildReturnSignature(sigBuilder, primaryDef);
			sigBuilder.Append(" { ");
			if (getter != null)
			{
				if (primaryDef != getter)
				{
					sigBuilder.Append(Visibility(getter) + " ");
				}
				sigBuilder.Append("get; ");
			}
			if (setter != null)
			{
				if (primaryDef != setter)
				{
					sigBuilder.Append(Visibility(setter) + " ");
				}
				sigBuilder.Append("set; ");
			}
			sigBuilder.Append("}");
			return sigBuilder.ToString();

		}

		/// <summary>
		/// Return the method signature as a string.
		/// </summary>
		///
		/// <param name="method">
		/// The Method.
		/// </param>
		/// <param name="callable">
		/// Return as an callable string(public void a(string b) would return a(b))
		/// </param>
		///
		/// <returns>
		/// Method signature.
		/// </returns>
		public static string GetSignature(this MethodInfo method, bool callable = false)
		{

			StringBuilder sigBuilder = new();

			BuildReturnSignature(sigBuilder, method, callable);

			sigBuilder.Append("(");
			bool firstParam = true;
			bool secondParam = false;

			ParameterInfo[] parameters = method.GetParameters();

			foreach (ParameterInfo param in parameters)
			{
				if (firstParam)
				{
					firstParam = false;
					if (method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false))
					{
						if (callable)
						{
							secondParam = true;
							continue;
						}
						sigBuilder.Append("this ");
					}
				}
				else if (secondParam)
				{
					secondParam = false;
				}
				else
				{
					sigBuilder.Append(", ");
				}
				if (param.IsOut)
				{
					sigBuilder.Append("out ");
				}
				else if (param.ParameterType.IsByRef)
				{
					sigBuilder.Append("ref ");
				}
				if (IsParamArray(param))
				{
					sigBuilder.Append("params ");
				}

				if (!callable)
				{
					sigBuilder.Append(TypeName(param.ParameterType));
					sigBuilder.Append(' ');
				}


				sigBuilder.Append(param.Name);

				if (param.IsOptional)
				{
					sigBuilder.Append(" = " +
						(param.DefaultValue ?? "null")
					);
				}
			}
			sigBuilder.Append(")");

			// generic constraints


			foreach (Type arg in method.GetGenericArguments())
			{
				List<string> constraints = [];
				foreach (Type constraint in arg.GetGenericParameterConstraints())
				{
					constraints.Add(TypeName(constraint));
				}

				GenericParameterAttributes attrs = arg.GenericParameterAttributes;

				if (attrs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
				{
					constraints.Add("class");
				}
				if (attrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
				{
					constraints.Add("struct");
				}
				if (attrs.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
				{
					constraints.Add("new()");
				}
				if (constraints.Count > 0)
				{
					sigBuilder.Append(" where " + TypeName(arg) + ": " + String.Join(", ", constraints));
				}
			}




			return sigBuilder.ToString();
		}

		/// <summary>
		/// Get full type name with full namespace names.
		/// </summary>
		///
		/// <param name="type">
		/// Type. May be generic or nullable.
		/// </param>
		///
		/// <returns>
		/// Full type name, fully qualified namespaces.
		/// </returns>
		public static string TypeName(Type type)
		{
			Type nullableType = Nullable.GetUnderlyingType(type);
			if (nullableType != null)
			{
				return TypeName(nullableType) + "?";
			}

			string typeName = type.Name;
			if (typeName.EndsWith("&"))
			{
				typeName = typeName.TrimEnd('&');
			}
			if (!type.IsGenericType)
			{
				if (type.IsArray)
				{
					return TypeName(type.GetElementType()) + "[]";
				}

				switch (typeName)
				{
					case "Char":
					case "System.Char": return "char";
					case "String":
					case "System.String": return "string";
					case "Boolean":
					case "System.Boolean": return "bool";
					case "Byte":
					case "System.Byte": return "byte";
					case "SByte":
					case "System.SByte": return "sbyte";
					case "Int16":
					case "System.Int16": return "short";
					case "UInt16":
					case "System.UInt16": return "ushort";
					case "Int32":
					case "System.Int32": return "int";
					case "UInt32":
					case "System.UInt32": return "uint";
					case "Int64":
					case "System.Int64": return "long";
					case "UInt64":
					case "System.UInt64": return "ulong";
					case "Decimal":
					case "System.Decimal": return "decimal";
					case "Double":
					case "System.Double": return "double";
					case "Object":
					case "System.Object": return "object";
					case "Void": return "void";
					default:
						{
							return String.IsNullOrWhiteSpace(type.FullName) ? typeName : type.FullName.TrimEnd('&');
						}
				}
			}

			StringBuilder sb = new(typeName[..typeName.IndexOf('`')]
			);

			sb.Append('<');
			bool first = true;
			foreach (Type t in type.GetGenericArguments())
			{
				if (!first)
				{
					sb.Append(',');
				}
				sb.Append(TypeName(t));
				first = false;
			}
			sb.Append('>');
			return sb.ToString();
		}

		private static void BuildReturnSignature(StringBuilder sigBuilder, MethodInfo method, bool callable = false)
		{
			bool firstParam = true;
			if (!callable)
			{
				sigBuilder.Append(Visibility(method) + " ");

				if (method.IsStatic)
				{
					sigBuilder.Append("static ");
				}
				sigBuilder.Append(TypeName(method.ReturnType));
				sigBuilder.Append(' ');
			}
			sigBuilder.Append(method.Name);

			// Add method generics
			if (method.IsGenericMethod)
			{
				sigBuilder.Append("<");
				foreach (Type g in method.GetGenericArguments())
				{
					if (firstParam)
					{
						firstParam = false;
					}
					else
					{
						sigBuilder.Append(", ");
					}
					sigBuilder.Append(TypeName(g));
				}
				sigBuilder.Append(">");
			}

		}

		private static string Visibility(MethodInfo method)
		{
			if (method.IsPublic)
			{
				return "public";
			}
			else if (method.IsPrivate)
			{
				return "private";
			}
			else if (method.IsAssembly)
			{
				return "internal";
			}
			else
			{
				return method.IsFamily ? "protected" : throw new Exception("I wasn't able to parse the visibility of this method.");
			}
		}

		private static MethodInfo LeastRestrictiveVisibility(MethodInfo member1, MethodInfo member2)
		{
			if (member1 != null && member2 == null)
			{
				return member1;
			}
			else if (member2 != null && member1 == null)
			{
				return member2;
			}

			int vis1 = VisibilityValue(member1);
			int vis2 = VisibilityValue(member2);
			return vis1 < vis2 ? member1 : member2;
		}

		private static int VisibilityValue(MethodInfo method)
		{
			if (method.IsPublic)
			{
				return 1;
			}
			else if (method.IsFamily)
			{
				return 2;
			}
			else if (method.IsAssembly)
			{
				return 3;
			}
			else
			{
				return method.IsPrivate ? 4 : throw new Exception("I wasn't able to parse the visibility of this method.");
			}
		}

		private static bool IsParamArray(ParameterInfo info)
		{
			return info.GetCustomAttribute(typeof(ParamArrayAttribute), true) != null;
		}
	}
}
