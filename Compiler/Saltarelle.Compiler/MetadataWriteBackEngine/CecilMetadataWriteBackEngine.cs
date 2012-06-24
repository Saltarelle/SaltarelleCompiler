using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using Mono.Cecil;

namespace Saltarelle.Compiler.MetadataWriteBackEngine {
	internal class CecilBackedAttributeCollection : ICollection<IAttribute> {
		private class SimpleAttribute : IAttribute {
			private readonly IType _attributeType;
			private readonly IMethod _constructor;
			private readonly IList<ResolveResult> _positionalArguments;
			private readonly IList<KeyValuePair<IMember, ResolveResult>> _namedArguments;

			public SimpleAttribute(IType attributeType, IMethod constructor, IList<ResolveResult> positionalArguments, IList<KeyValuePair<IMember, ResolveResult>> namedArguments) {
				_attributeType       = attributeType;
				_constructor         = constructor;
				_positionalArguments = positionalArguments;
				_namedArguments      = namedArguments;
			}

			public DomRegion Region { get { return DomRegion.Empty; } }

			public IType AttributeType {
				get { return _attributeType; }
			}

			public IMethod Constructor {
				get { return _constructor; }
			}

			public IList<ResolveResult> PositionalArguments {
				get { return _positionalArguments; }
			}

			public IList<KeyValuePair<IMember, ResolveResult>> NamedArguments {
				get { return _namedArguments; }
			}
		}

		private readonly ICustomAttributeProvider _owner;
		private readonly IList<IAttribute> _attributes;
		private bool _isDirty;

		public CecilBackedAttributeCollection(ICompilation compilation, ICustomAttributeProvider owner) {
			_owner      = owner;
			_attributes = ConvertAttributes(compilation, owner.CustomAttributes);
			_isDirty    = false;
		}

		public IEnumerator<IAttribute> GetEnumerator() {
			return _attributes.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public void Add(IAttribute item) {
			_attributes.Add(item);
			_isDirty = true;
		}

		public void Clear() {
			_attributes.Clear();
			_isDirty = true;
		}

		public bool Contains(IAttribute item) {
			return _attributes.Contains(item);
		}

		public void CopyTo(IAttribute[] array, int arrayIndex) {
			_attributes.CopyTo(array, arrayIndex);
		}

		public bool Remove(IAttribute item) {
			bool result = _attributes.Remove(item);
			_isDirty = true;
			return result;
		}

		public int Count {
			get { return _attributes.Count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		private List<IAttribute> ConvertAttributes(ICompilation compilation, IEnumerable<CustomAttribute> src) {
/*
			MethodReference ctor = attribute.Constructor;
			ITypeReference attributeType = ReadTypeReference(attribute.AttributeType);
			IList<ITypeReference> ctorParameterTypes = null;
			if (ctor.HasParameters) {
				ctorParameterTypes = new ITypeReference[ctor.Parameters.Count];
				for (int i = 0; i < ctorParameterTypes.Count; i++) {
					ctorParameterTypes[i] = ReadTypeReference(ctor.Parameters[i].ParameterType);
				}
			}
			if (this.InterningProvider != null) {
				attributeType = this.InterningProvider.Intern(attributeType);
				ctorParameterTypes = this.InterningProvider.InternList(ctorParameterTypes);
			}
			return new CecilUnresolvedAttribute(attributeType, ctorParameterTypes ?? EmptyList<ITypeReference>.Instance, attribute.GetBlob());
*/
			var result = new List<IAttribute>();
			foreach (var a in src) {
				var attrType = ReflectionHelper.ParseReflectionName(a.AttributeType.FullName).Resolve(compilation);
				IList<IType> ctorParameterTypes = a.Constructor.Parameters.Select(p => ReflectionHelper.ParseReflectionName(p.ParameterType.FullName).Resolve(compilation)).ToList();
				var constructor = attrType.GetConstructors().Single(c => c.Parameters.Select(p => p.Type).SequenceEqual(ctorParameterTypes));
				var positionalArgs = a.ConstructorArguments.Select((arg, i) => (ResolveResult)new ConstantResolveResult(ctorParameterTypes[i], arg.Value)).ToList();
				var namedArgs = new List<KeyValuePair<IMember, ResolveResult>>();

				result.Add(new SimpleAttribute(attrType, constructor, positionalArgs.AsReadOnly(), namedArgs.AsReadOnly()));
			}
			return result;
		}
	}

	public class CecilMetadataWriteBackEngine : IMetadataWriteBackEngine {
		private readonly AssemblyDefinition _assembly;
		private readonly ICompilation _compilation;
		private readonly Dictionary<string, TypeDefinition> _allTypes;
		private readonly Dictionary<ITypeDefinition, ICollection<IAttribute>> _typeAttributes;
		private readonly Dictionary<IMember, ICollection<IAttribute>> _memberAttributes;

		public CecilMetadataWriteBackEngine(AssemblyDefinition assembly, ICompilation compilation) {
			_assembly         = assembly;
			_compilation      = compilation;
			_allTypes         = assembly.Modules.SelectMany(m => m.GetTypes()).ToDictionary(t => t.FullName);
			_typeAttributes   = new Dictionary<ITypeDefinition, ICollection<IAttribute>>();
			_memberAttributes = new Dictionary<IMember, ICollection<IAttribute>>();
		}

		public ICollection<IAttribute> GetAttributes(ITypeDefinition type) {
			ICollection<IAttribute> result;
			if (!_typeAttributes.TryGetValue(type, out result))
				_typeAttributes[type] = result = new CecilBackedAttributeCollection(_compilation, _allTypes[type.FullName]);
			return result;
		}

		public ICollection<IAttribute> GetAttributes(IMember member) {
			ICollection<IAttribute> result;
			if (!_memberAttributes.TryGetValue(member, out result)) {
				var type = _allTypes[member.DeclaringTypeDefinition.FullName];
				var cecilMember = FindMember(type, member);
				_memberAttributes[member] = result = new CecilBackedAttributeCollection(_compilation, cecilMember);
			}
			return result;
		}

		public IAttribute CreateAttribute(IAssembly attributeAssembly, string attributeTypeName, IEnumerable<object> positionalArguments, IEnumerable<object> namedArguments) {
			throw new NotImplementedException();
		}

		private string GetTypeName(IType type) {
			if (type is ParameterizedType) {
				var pt = (ParameterizedType)type;
				return GetTypeName(pt.GetDefinition()) + "<" + string.Join(",", pt.TypeArguments.Select(GetTypeName)) + ">";
			}
			else {
				return type.FullName;
			}
		}

		private bool AreParameterListsEqual(IEnumerable<ParameterDefinition> l1, IEnumerable<IParameter> l2) {
			var e1 = l1.GetEnumerator();
			var e2 = l2.GetEnumerator();
			for (;;) {
				bool b1 = e1.MoveNext();
				bool b2 = e2.MoveNext();
				if (b1 != b2)
					return false;
				if (!b1)
					return true;
				if (ReflectionHelper.ParseReflectionName(e1.Current.ParameterType.FullName).Resolve(_compilation) != e2.Current.Type)
					return false;
			}
		}

		private IMemberDefinition FindMember(TypeDefinition type, IMember member) {
			switch (member.EntityType) {
				case EntityType.Field:
					return type.Fields.Single(f => f.Name == member.Name);

				case EntityType.Property: {
					string name;
					if (member.IsExplicitInterfaceImplementation) {
						if (member.ImplementedInterfaceMembers.Count > 1)
							throw new NotSupportedException(type.FullName + "." + member.Name + " implements more than one member explicitly.");
						name = GetTypeName(member.ImplementedInterfaceMembers[0].DeclaringType) + "." + member.ImplementedInterfaceMembers[0].Name;
					}
					else {
						name = member.Name;
					}
					var result = type.Properties.SingleOrDefault(p => p.Name == name);
					if (result == null)
						throw new Exception("Could not find property " + name + ".");
					return result;
				}

				case EntityType.Accessor: {
					var accessor   = (IMethod)member;
					var cecilOwner = FindMember(type, accessor.AccessorOwner);
					if (accessor.AccessorOwner is IProperty) {
						var p = (IProperty)accessor.AccessorOwner;
						if (member == p.Getter)
							return ((PropertyDefinition)cecilOwner).GetMethod;
						else if (member == p.Setter)
							return ((PropertyDefinition)cecilOwner).SetMethod;
						else
							throw new Exception("The accessor " + member.DeclaringType.FullName + "." + member.Name + " is neither the getter nor the setter of the owning property.");
					}
					break;
				}

				case EntityType.Indexer: {
					string name;
					if (member.IsExplicitInterfaceImplementation) {
						throw new NotImplementedException("TODO...");
						if (member.ImplementedInterfaceMembers.Count > 1)
							throw new NotSupportedException(type.FullName + "." + member.Name + " implements more than one member explicitly.");
						name = GetTypeName(member.ImplementedInterfaceMembers[0].DeclaringType) + "." + member.ImplementedInterfaceMembers[0].Name;
					}
					else {
						name = member.Name;
					}
					var result = type.Properties.SingleOrDefault(p => AreParameterListsEqual(p.Parameters, ((IParameterizedMember)member).Parameters));
					if (result == null)
						throw new Exception("Could not find indexer " + name + ".");
					return result;
				}

				case EntityType.Event:
				case EntityType.Method:
				case EntityType.Operator:
				case EntityType.Constructor:
					break;

				default:
					throw new NotSupportedException("Entity type " + member.EntityType + " is not supported.");
			}
			throw new NotImplementedException();
		}
	}
}
