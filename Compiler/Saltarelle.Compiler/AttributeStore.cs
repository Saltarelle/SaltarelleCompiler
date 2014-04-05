using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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
				ReadAssemblyAttributes(a.Properties, _assemblyTransformers);
			}

			foreach (var t in compilation.Assemblies.SelectMany(a => TreeTraversal.PostOrder(a.TopLevelTypeDefinitions, t => t.NestedTypes))) {
				foreach (var m in t.Methods) {
					ReadEntityAttributes(m, _entityTransformers);
				}
				foreach (var p in t.Properties) {
					if (p.CanGet)
						ReadEntityAttributes(p.Getter, _entityTransformers);
					if (p.CanSet)
						ReadEntityAttributes(p.Setter, _entityTransformers);
					ReadEntityAttributes(p, _entityTransformers);
				}
				foreach (var f in t.Fields) {
					ReadEntityAttributes(f, _entityTransformers);
				}
				foreach (var e in t.Events) {
					if (e.CanAdd)
						ReadEntityAttributes(e.AddAccessor, _entityTransformers);
					if (e.CanRemove)
						ReadEntityAttributes(e.RemoveAccessor, _entityTransformers);
					ReadEntityAttributes(e, _entityTransformers);
				}
				ReadEntityAttributes(t, _entityTransformers);
			}
		}

		public void RunAttributeCode() {
			foreach (var t in _entityTransformers) {
				_errorReporter.Region = t.Item1.Locations[0];
				t.Item2.ApplyTo(t.Item1, this, _errorReporter);
			}

			_errorReporter.Region = null;
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
			var attr = type.GetAttributes().FirstOrDefault(a => a.AttributeClass.Name == "System.Runtime.CompilerServices.PluginNameAttribute";
			if (attr == null)
				return type.Name;
			return (string)attr.ConstructorArguments[0].Value;
		}

		private static Type FindType(ITypeSymbol type) {
			Type result;
			if (_typeCache.TryGetValue(type.Name, out result))
				return result;

			string typeName = FindTypeName(type);

			result = Type.GetType(typeName);	// First search mscorlib
			if (result == null) {
				result = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(typeName)).SingleOrDefault(t => t != null);
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
