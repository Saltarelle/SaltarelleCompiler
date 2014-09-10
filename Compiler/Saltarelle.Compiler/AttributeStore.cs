using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Saltarelle.Compiler.Roslyn;

namespace Saltarelle.Compiler {
	public interface IAttributeStore {
		AttributeList AttributesFor(ISymbol symbol);
	}

	public class AttributeStore : IAttributeStore {
		private readonly IErrorReporter _errorReporter;
		private readonly Dictionary<ISymbol, AttributeList> _store;
		private readonly List<Tuple<ISymbol, PluginAttributeBase>> _assemblyTransformers;
		private readonly List<Tuple<ISymbol, PluginAttributeBase>> _entityTransformers;

		public AttributeStore(CSharpCompilation compilation, IErrorReporter errorReporter) {
			_errorReporter = errorReporter;
			_store = new Dictionary<ISymbol, AttributeList>();
			_assemblyTransformers = new List<Tuple<ISymbol, PluginAttributeBase>>();
			_entityTransformers = new List<Tuple<ISymbol, PluginAttributeBase>>();

			ReadAssemblyAttributes(compilation.Assembly, _assemblyTransformers);
			foreach (var a in compilation.References) {
				ReadAssemblyAttributes(compilation.GetAssemblyOrModuleSymbol(a), _assemblyTransformers);
			}

			foreach (var t in compilation.GetAllTypes()) {
				foreach (var m in t.GetNonAccessorNonTypeMembers()) {
					ReadEntityAttributes(m, _entityTransformers);

					var p = m as IPropertySymbol;
					if (p != null) {
						if (p.GetMethod != null)
							ReadEntityAttributes(p.GetMethod, _entityTransformers);
						if (p.SetMethod != null)
							ReadEntityAttributes(p.SetMethod, _entityTransformers);
					}

					var e = m as IEventSymbol;
					if (e != null) {
						if (e.AddMethod != null)
							ReadEntityAttributes(e.AddMethod, _entityTransformers);
						if (e.RemoveMethod != null)
							ReadEntityAttributes(e.RemoveMethod, _entityTransformers);
					}
				}

				ReadEntityAttributes(t, _entityTransformers);
			}
		}

		public void RunAttributeCode() {
			foreach (var t in _entityTransformers) {
				_errorReporter.Location = t.Item1.Locations[0];
				t.Item2.ApplyTo(t.Item1, this, _errorReporter);
			}

			_errorReporter.Location = null;
			foreach (var t in _assemblyTransformers) {
				t.Item2.ApplyTo(t.Item1, this, _errorReporter);
			}

			_entityTransformers.Clear();
			_assemblyTransformers.Clear();
		}

		public AttributeList AttributesFor(ISymbol symbol) {
			AttributeList result;
			if (!_store.TryGetValue(symbol, out result)) {
				_store[symbol] = result = new AttributeList();
			}
			return result;
		}

		private void ReadAssemblyAttributes(ISymbol assembly, List<Tuple<ISymbol, PluginAttributeBase>> transformers) {
			_store[assembly] = ReadAttributes(assembly, assembly.GetAttributes(), transformers);
		}

		private void ReadEntityAttributes(ISymbol entity, List<Tuple<ISymbol, PluginAttributeBase>> transformers) {
			_store[entity] = ReadAttributes(entity, entity.GetAttributes(), transformers);
		}

		private AttributeList ReadAttributes<T>(T t, IEnumerable<AttributeData> attributes, List<Tuple<T, PluginAttributeBase>> transformers) {
			var l = new AttributeList();
			foreach (var a in attributes) {
				var type = FindType(a.AttributeClass);
				if (type != null) {
					var attr = ReadAttribute(a, type);
					var pab = attr as PluginAttributeBase;
					l.Add(attr);
					if (pab != null) {
						transformers.Add(Tuple.Create(t, pab));
					}
				}
			}
			return l;
		}

		private static object ChangeType(object source, Type type) {
			if (type.IsArray) {
				var arr = (Array)source;
				var elemType = type.GetElementType();
				var result = Array.CreateInstance(elemType, arr.Length);
				for (int i = 0; i < arr.Length; i++) {
					result.SetValue(ChangeType(arr.GetValue(i), elemType), i);
				}
				return result;
			}
			else if (type.IsEnum) {
				return Enum.ToObject(type, source);
			}
			else {
				return Convert.ChangeType(source, type);
			}
		}

		private static readonly Dictionary<string, Type> _typeCache = new Dictionary<string, Type>();

		private static string FindTypeName(ITypeSymbol type) {
			var attr = type.GetAttributes().FirstOrDefault(a => a.AttributeClass.FullyQualifiedName() == "System.Runtime.CompilerServices.PluginNameAttribute");
			if (attr == null)
				return type.FullyQualifiedName();
			return (string)attr.ConstructorArguments[0].Value;
		}

		private static Type FindType(ITypeSymbol type) {
			Type result;
			if (_typeCache.TryGetValue(type.Name, out result))
				return result;

			string typeName = FindTypeName(type);

			result = Type.GetType(typeName);	// First search mscorlib
			if (result == null) {
				result = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(typeName)).Where(t => t != null).Distinct().SingleOrDefault();
			}
			_typeCache[type.Name] = result;
			return result;
		}

		public static Attribute ReadAttribute(AttributeData attr, Type attributeType)  {
			var ctorArgTypes = new Type[attr.ConstructorArguments.Length];
			var ctorArgs = new object[attr.ConstructorArguments.Length];
			for (int i = 0; i < attr.ConstructorArguments.Length; i++) {
				var arg = attr.ConstructorArguments[i];
				ctorArgTypes[i] = FindType(arg.Type);
				ctorArgs[i]     = ChangeType(arg.Value, ctorArgTypes[i]);
			}
			var ctor = attributeType.GetConstructor(ctorArgTypes);
			var result = (Attribute)ctor.Invoke(ctorArgs);

			foreach (var arg in attr.NamedArguments) {
				var value = ChangeType(arg.Value.Value, FindType(arg.Value.Type));
				var member = attributeType.GetMember(arg.Key)[0];
				if (member is FieldInfo) {
					((FieldInfo)member).SetValue(result, value);
				}
				else if (member is PropertyInfo) {
					((PropertyInfo)member).SetValue(result, value, null);
				}
			}

			return result;
		}
	}
}
