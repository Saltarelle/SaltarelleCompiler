using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Saltarelle.Compiler.JSModel;
using TypeScriptModel.Model;

namespace TypeScriptModel {
	public class OutputFormatter {
		public static string Format(Globals globals) {
			var cb = new CodeBuilder();
			Format(globals, cb);
			return cb.ToString();
		}

		public static void Format(Globals globals, CodeBuilder cb) {
			foreach (var m in globals.Members)
				FormatGlobalMember(m, "declare", cb);
			foreach (var i in globals.Interfaces)
				Format(i, cb);
			foreach (var m in globals.Modules)
				Format(m, cb);
		}

		public static void Format(Member member, CodeBuilder cb) {
			if (member is Constructor)
				Format((Constructor)member, cb);
			else if (member is Function)
				Format((Function)member, cb);
			else if (member is Indexer)
				Format((Indexer)member, cb);
			else if (member is Variable)
				Format((Variable)member, cb);
			else
				throw new ArgumentException("Unknown member type " + member.GetType().FullName, "member");
		}

		public static void Format(Constructor ctor, CodeBuilder cb) {
			cb.Append("new ");
			FormatParameterList(ctor.Parameters, cb);
			if (ctor.ReturnType != null) {
				cb.Append(": ");
				Format(ctor.ReturnType, cb);
			}
			cb.Append(";");
		}

		public static void Format(Function function, CodeBuilder cb) {
			cb.Append(function.Name);
			FormatParameterList(function.Parameters, cb);
			if (function.ReturnType != null) {
				cb.Append(": ");
				Format(function.ReturnType, cb);
			}
			cb.Append(";");
		}

		public static void Format(Indexer indexer, CodeBuilder cb) {
			cb.Append("[")
			  .Append(indexer.ParameterName);
			if (indexer.ParameterType != null) {
				cb.Append(": ");
				Format(indexer.ParameterType, cb);
			}
			cb.Append("]");
			if (indexer.ReturnType != null) {
				cb.Append(": ");
				Format(indexer.ReturnType, cb);
			}
			cb.Append(";");
		}

		public static void Format(Variable variable, CodeBuilder cb) {
			cb.Append(variable.Name);
			if (variable.Optional)
				cb.Append("?");
			if (variable.Type != null) {
				cb.Append(": ");
				Format(variable.Type, cb);
			}
			cb.Append(";");
		}

		public static void Format(Interface iface, CodeBuilder cb) {
			cb.Append("interface ").Append(iface.Name);
			for (int i = 0, n = iface.Extends.Count; i < n; i++) {
				cb.Append(i == 0 ? " extends " : ", ").Append(iface.Extends[i].Name);
			}
			cb.Append(" ");
			FormatMemberList(iface.Members, cb);
			cb.AppendLine();
		}

		private static void FormatGlobalMember(Member member, string prefix, CodeBuilder cb) {
			if (!string.IsNullOrEmpty(prefix))
				cb.Append(prefix).Append(" ");
			cb.Append(member is Function ? "function " : "var ");
			Format(member, cb);
			cb.AppendLine();
		}

		public static void Format(Module module, CodeBuilder cb) {
			cb.Append("declare module \"")
			  .Append(module.Name)
			  .AppendLine("\" {")
			  .Indent();

			foreach (var i in module.Imports) {
				cb.Append("import ")
				  .Append(i.Alias)
				  .Append(" = module(\"")
				  .Append(i.Module)
				  .AppendLine("\");");
			}

			foreach (var i in module.ExportedInterfaces) {
				cb.Append("export ");
				Format(i, cb);
			}

			foreach (var m in module.ExportedMembers) {
				FormatGlobalMember(m, "export", cb);
			}

			foreach (var i in module.Interfaces) {
				Format(i, cb);
			}

			foreach (var m in module.Members) {
				FormatGlobalMember(m, null, cb);
			}

			cb.Outdent()
			  .AppendLine("}");
		}

		public static void Format(TSType type, CodeBuilder cb) {
			if (type is ArrayType)
				Format((ArrayType)type, cb);
			else if (type is CompositeType)
				Format((CompositeType)type, cb);
			else if (type is FunctionType)
				Format((FunctionType)type, cb);
			else if (type is TypeReference)
				Format((TypeReference)type, cb);
			else
				throw new ArgumentException("invalid type " + type.GetType().FullName, "type");
		}

		public static void Format(ArrayType type, CodeBuilder cb) {
			Format(type.ElementType, cb);
			cb.Append("[]");
		}

		public static void Format(CompositeType type, CodeBuilder cb) {
			FormatMemberList(type.Members, cb);
		}

		private static void FormatMemberList(IEnumerable<Member> members, CodeBuilder sb) {
			sb.AppendLine("{")
			  .Indent();
			foreach (var m in members) {
				Format(m, sb);
				sb.AppendLine();
			}
			sb.Outdent()
			  .Append("}");
		}

		private static void FormatParameter(Parameter p, CodeBuilder sb) {
			if (p.ParamArray)
				sb.Append("...");
			sb.Append(p.Name);
			if (p.Optional)
				sb.Append("?");
			if (p.Type != null) {
				sb.Append(": ");
				Format(p.Type, sb);
			}
		}

		private static void FormatParameterList(IEnumerable<Parameter> parameters, CodeBuilder sb) {
			sb.Append("(");
			bool first = true;
			foreach (var p in parameters) {
				if (!first)
					sb.Append(", ");
				FormatParameter(p, sb);
				first = false;
			}
			sb.Append(")");
		}

		public static void Format(FunctionType type, CodeBuilder cb) {
			FormatParameterList(type.Parameters, cb);
			cb.Append(" => ");
			Format(type.ReturnType, cb);
		}

		public static void Format(TypeReference type, CodeBuilder cb) {
			cb.Append(type.Name);
		}
	}
}
