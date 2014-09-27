using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Saltarelle.Compiler.Roslyn;

namespace Saltarelle.Compiler {
	public class AttributeStore : IAttributeStore {
		internal const string ScriptSerializableAttribute = "System.Runtime.CompilerServices.Internal.ScriptSerializableAttribute";

		private readonly Dictionary<ISymbol, AttributeList> _store;

		public AttributeStore(Compilation compilation, IErrorReporter errorReporter, IEnumerable<IAutomaticMetadataAttributeApplier> automaticMetadataAppliers) {
			_store = new Dictionary<ISymbol, AttributeList>();
			var assemblyTransformers = new List<Tuple<ISymbol, PluginAttributeBase>>();
			var entityTransformers = new List<Tuple<ISymbol, PluginAttributeBase>>();
			
			var scriptSerializableAttribute = compilation.GetTypeByMetadataName(ScriptSerializableAttribute);

			ReadAssemblyAttributes(compilation.Assembly, assemblyTransformers);
			foreach (var a in compilation.References) {
				ReadAssemblyAttributes(compilation.GetAssemblyOrModuleSymbol(a), null);
			}

			foreach (var t in compilation.GetAllTypes()) {
				var currentEntityTransformers = Equals(t.ContainingAssembly, compilation.Assembly) ? entityTransformers : null;

				foreach (var m in t.GetNonAccessorNonTypeMembers()) {
					ReadEntityAttributes(m, currentEntityTransformers);

					var p = m as IPropertySymbol;
					if (p != null) {
						if (p.GetMethod != null)
							ReadEntityAttributes(p.GetMethod, currentEntityTransformers);
						if (p.SetMethod != null)
							ReadEntityAttributes(p.SetMethod, currentEntityTransformers);
					}

					var e = m as IEventSymbol;
					if (e != null) {
						if (e.AddMethod != null)
							ReadEntityAttributes(e.AddMethod, currentEntityTransformers);
						if (e.RemoveMethod != null)
							ReadEntityAttributes(e.RemoveMethod, currentEntityTransformers);
					}
				}

				ReadEntityAttributes(t, currentEntityTransformers);
				ReadSerializableAttribute(t, scriptSerializableAttribute);
			}

			ApplyTransformers(compilation.Assembly, entityTransformers.Concat(assemblyTransformers), automaticMetadataAppliers, errorReporter);
		}

		private void ApplyTransformers(IAssemblySymbol assembly, IEnumerable<Tuple<ISymbol, PluginAttributeBase>> transformers, IEnumerable<IAutomaticMetadataAttributeApplier> automaticMetadataAppliers, IErrorReporter errorReporter) {
			foreach (var applier in automaticMetadataAppliers) {
				foreach (var t in assembly.GetAllTypes())
					applier.Process(t, this);
				applier.Process(assembly, this);
			}

			foreach (var t in transformers) {
				errorReporter.Location = t.Item1 is IAssemblySymbol ? null : t.Item1.Locations.FirstOrDefault();
				t.Item2.ApplyTo(t.Item1, this, errorReporter);
			}
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

		private void ReadSerializableAttribute(INamedTypeSymbol type, INamedTypeSymbol scriptSerializableAttribute) {
			var serializableAttr = type.GetAttributes().SingleOrDefault(a => a.AttributeClass.Name == typeof(SerializableAttribute).Name && a.AttributeClass.ContainingNamespace.FullyQualifiedName() == typeof(SerializableAttribute).Namespace);
			if (serializableAttr != null) {
				var attrType = FindType(scriptSerializableAttribute);
				var typeCheckCode = serializableAttr.NamedArguments.SingleOrDefault(a => a.Key == "TypeCheckCode").Value.Value;
				_store[type].Add((Attribute)attrType.GetConstructor(new[] { typeof(string) }).Invoke(new object[] { typeCheckCode }));
			}
		}

		private AttributeList ReadAttributes<T>(T t, IEnumerable<AttributeData> attributes, List<Tuple<T, PluginAttributeBase>> transformers) {
			var l = new AttributeList();
			foreach (var a in attributes.Where(a => a.AttributeClass.Name != typeof(SerializableAttribute).Name || a.AttributeClass.ContainingNamespace.FullyQualifiedName() != typeof(SerializableAttribute).Namespace)) {
				var type = FindType(a.AttributeClass);
				if (type != null) {
					var attr = ReadAttribute(a, type);
					l.Add(attr);
					if (transformers != null) {
						var pab = attr as PluginAttributeBase;
						if (pab != null) {
							transformers.Add(Tuple.Create(t, pab));
						}
					}
				}
			}
			return l;
		}

		private static readonly Dictionary<ITypeSymbol, Type> _typeCache = new Dictionary<ITypeSymbol, Type>();

		private static Type FindType(ITypeSymbol type) {
			Type result;
			if (_typeCache.TryGetValue(type, out result))
				return result;

			if (type is IArrayTypeSymbol) {
				var at = (IArrayTypeSymbol)type;
				result = FindType(at.ElementType).MakeArrayType(at.Rank);
			}
			else if (type is INamedTypeSymbol) {
				var ns = type.ContainingNamespace.FullyQualifiedName();
				var typeName = (!string.IsNullOrEmpty(ns) ? ns + "." : "") + type.MetadataName;
				result = Type.GetType(typeName);	// First search mscorlib
				if (result == null) {
					result = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(typeName)).Where(t => t != null).Distinct().SingleOrDefault();
				}

				var namedType = (INamedTypeSymbol)type;
				if (namedType.TypeArguments.Length > 0 && !namedType.IsUnboundGenericType) {
					result = result.MakeGenericType(namedType.TypeArguments.Select(FindType).ToArray());
				}
			}
			else {
				throw new ArgumentException("Invalid type in attribute: " + type);
			}
			_typeCache[type] = result;
			return result;
		}

		private static object ConvertArgument(TypedConstant c) {
			switch (c.Kind) {
				case TypedConstantKind.Array:
					var elementType = FindType(((IArrayTypeSymbol)c.Type).ElementType);
					var result = Array.CreateInstance(elementType, c.Values.Length);
					for (int i = 0; i < result.Length; i++)
						result.SetValue(ConvertArgument(c.Values[i]), i);
					return result;

				case TypedConstantKind.Enum:
					return Enum.ToObject(FindType(c.Type), c.Value);

				case TypedConstantKind.Primitive:
					return c.Value;

				case TypedConstantKind.Type:
					return FindType((ITypeSymbol)c.Value) ?? typeof(object);

				default:
					throw new Exception("Invalid attribute constant " + c);
			}
		}

		public static Attribute ReadAttribute(AttributeData attr, Type attributeType)  {
			var ctorArgTypes = new Type[attr.ConstructorArguments.Length];
			var ctorArgs = new object[attr.ConstructorArguments.Length];
			for (int i = 0; i < attr.ConstructorArguments.Length; i++) {
				var arg = attr.ConstructorArguments[i];
				ctorArgTypes[i] = FindType(arg.Type);
				ctorArgs[i]     = ConvertArgument(arg);
			}
			var ctor = attributeType.GetConstructor(ctorArgTypes);
			var result = (Attribute)ctor.Invoke(ctorArgs);

			foreach (var arg in attr.NamedArguments) {
				var value = ConvertArgument(arg.Value);
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
