using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeScriptModel.Model;

namespace TypeScriptModel {
	public class OutputFormatter {
		public static string Format(Globals globals) {
			var sb = new StringBuilder();
			Format(globals, sb);
			return sb.ToString();
		}

		public static void Format(Globals globals, StringBuilder sb) {
			foreach (var m in globals.Members)
				Format(m, sb);
			foreach (var i in globals.Interfaces)
				Format(i, sb);
			foreach (var m in globals.Modules)
				Format(m, sb);
		}

		public static void Format(Member member, StringBuilder sb) {
			if (member is Constructor)
				Format((Constructor)member, sb);
			else if (member is Function)
				Format((Function)member, sb);
			else if (member is Indexer)
				Format((Indexer)member, sb);
			else if (member is Variable)
				Format((Variable)member, sb);
			else
				throw new ArgumentException("Unknown member type " + member.GetType().FullName, "member");
		}

		public static void Format(Constructor ctor, StringBuilder sb) {
			throw new NotImplementedException();
		}

		public static void Format(Function function, StringBuilder sb) {
			throw new NotImplementedException();
		}

		public static void Format(Indexer indexer, StringBuilder sb) {
			throw new NotImplementedException();
		}

		public static void Format(Variable variable, StringBuilder sb) {
			sb.Append("declare var ").Append(variable.Name);
			if (variable.Type != null) {
				sb.Append(": ");
				Format(variable.Type, sb);
			}
			sb.Append(";");
		}

		public static void Format(Interface iface, StringBuilder sb) {
			sb.Append("interface ").Append(iface.Name);
			if (iface.Extends.Count > 0)
				throw new NotImplementedException();
			sb.AppendLine(" {");
			foreach (var m in iface.Members)
				throw new NotImplementedException();
			sb.Append("}");
		}

		public static void Format(Module module, StringBuilder sb) {
			throw new NotImplementedException();
		}

		public static void Format(TSType type, StringBuilder sb) {
			if (type is ArrayType)
				Format((ArrayType)type, sb);
			else if (type is CompositeType)
				Format((CompositeType)type, sb);
			else if (type is FunctionType)
				Format((FunctionType)type, sb);
			else if (type is TypeReference)
				Format((TypeReference)type, sb);
			else
				throw new ArgumentException("invalid type " + type.GetType().FullName, "type");
		}

		public static void Format(ArrayType type, StringBuilder sb) {
			throw new NotImplementedException();
		}

		public static void Format(CompositeType type, StringBuilder sb) {
			throw new NotImplementedException();
		}

		public static void Format(FunctionType type, StringBuilder sb) {
			throw new NotImplementedException();
		}

		public static void Format(TypeReference type, StringBuilder sb) {
			sb.Append(type.Name);
		}
	}
}
