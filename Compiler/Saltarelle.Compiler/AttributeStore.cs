using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.Utils;

namespace Saltarelle.Compiler {
	public interface IAttributeStore {
		AttributeList AttributesFor(IAssembly assembly);
		AttributeList AttributesFor(IEntity entity);
	}

	public class AttributeStore : IAttributeStore {
		private readonly Dictionary<IAssembly, AttributeList> _assemblyStore;
		private readonly Dictionary<IEntity, AttributeList> _entityStore;

		public AttributeStore(ICompilation compilation) {
			_assemblyStore = new Dictionary<IAssembly, AttributeList>();
			_entityStore = new Dictionary<IEntity, AttributeList>();

			var assemblyTransformers = new List<Tuple<IAssembly, PluginAttributeBase>>();
			var entityTransformers = new List<Tuple<IEntity, PluginAttributeBase>>();

			foreach (var a in compilation.Assemblies) {
				ReadAssemblyAttributes(a, assemblyTransformers);
			}

			foreach (var t in compilation.Assemblies.SelectMany(a => TreeTraversal.PostOrder(a.TopLevelTypeDefinitions, t => t.NestedTypes))) {
				foreach (var m in t.Methods) {
					ReadEntityAttributes(m, entityTransformers);
				}
				foreach (var p in t.Properties) {
					if (p.CanGet)
						ReadEntityAttributes(p.Getter, entityTransformers);
					if (p.CanSet)
						ReadEntityAttributes(p.Setter, entityTransformers);
					ReadEntityAttributes(p, entityTransformers);
				}
				foreach (var f in t.Fields) {
					ReadEntityAttributes(f, entityTransformers);
				}
				foreach (var e in t.Events) {
					if (e.CanAdd)
						ReadEntityAttributes(e.AddAccessor, entityTransformers);
					if (e.CanRemove)
						ReadEntityAttributes(e.RemoveAccessor, entityTransformers);
					ReadEntityAttributes(e, entityTransformers);
				}
				ReadEntityAttributes(t, entityTransformers);
			}

			foreach (var t in entityTransformers)
				t.Item2.ApplyTo(t.Item1);

			foreach (var t in assemblyTransformers)
				t.Item2.ApplyTo(t.Item1);
		}

		public AttributeList AttributesFor(IAssembly assembly) {
			return _assemblyStore[assembly];
		}

		public AttributeList AttributesFor(IEntity entity) {
			AttributeList result;
			if (!_entityStore.TryGetValue(entity, out result)) {
				_entityStore[entity] = result = new AttributeList();
			}
			return result;
		}

		private void ReadAssemblyAttributes(IAssembly assembly, List<Tuple<IAssembly, PluginAttributeBase>> transformers) {
			_assemblyStore[assembly] = ReadAttributes(assembly, assembly.AssemblyAttributes, transformers);
		}

		private void ReadEntityAttributes(IEntity entity, List<Tuple<IEntity, PluginAttributeBase>> transformers) {
			_entityStore[entity] = ReadAttributes(entity, entity.Attributes, transformers);
		}

		private AttributeList ReadAttributes<T>(T t, IEnumerable<IAttribute> attributes, List<Tuple<T, PluginAttributeBase>> transformers) {
			var l = new AttributeList();
			foreach (var a in attributes) {
				var type = FindType(a.AttributeType);
				if (type != null) {
					var attr = ReadAttribute(a, type);
					var pab = attr as PluginAttributeBase;
					if (pab != null) {
						transformers.Add(Tuple.Create(t, pab));
					}
					else {
						l.Add(attr);
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

		private static string FindTypeName(IType type) {
			var def = type as ITypeDefinition;
			if (def == null || def.Attributes.Count == 0)
				return type.FullName;
			var attr = def.Attributes.FirstOrDefault(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.PluginNameAttribute");
			if (attr == null)
				return type.FullName;
			return (string)attr.PositionalArguments[0].ConstantValue;
		}

		private static Type FindType(IType type) {
			Type result;
			if (_typeCache.TryGetValue(type.FullName, out result))
				return result;

			string typeName = FindTypeName(type);

			result = Type.GetType(typeName);	// First search mscorlib
			if (result == null) {
				result = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(typeName)).SingleOrDefault(t => t != null);
			}
			_typeCache[type.FullName] = result;
			return result;
		}

		public static Attribute ReadAttribute(IAttribute attr, Type attributeType)  {
			var ctorArgTypes = new Type[attr.PositionalArguments.Count];
			var ctorArgs = new object[attr.PositionalArguments.Count];
			for (int i = 0; i < attr.PositionalArguments.Count; i++) {
				var arg = attr.PositionalArguments[i];
				ctorArgTypes[i] = FindType(arg.Type);
				ctorArgs[i]     = ChangeType(arg.ConstantValue, ctorArgTypes[i]);
			}
			var ctor = attributeType.GetConstructor(ctorArgTypes);
			var result = (Attribute)ctor.Invoke(ctorArgs);

			foreach (var arg in attr.NamedArguments) {
				var value = ChangeType(arg.Value.ConstantValue, FindType(arg.Value.Type));
				if (arg.Key is IField) {
					var fld = attributeType.GetField(arg.Key.Name);
					fld.SetValue(result, value);
				}
				else if (arg.Key is IProperty) {
					var prop = attributeType.GetProperty(arg.Key.Name);
					prop.SetValue(result, value, null);
				}
			}

			return result;
		}
	}
}
