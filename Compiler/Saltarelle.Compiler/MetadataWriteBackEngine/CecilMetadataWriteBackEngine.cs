using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Mono.Cecil;

namespace Saltarelle.Compiler.MetadataWriteBackEngine {
	internal class CecilBackedAttributeCollection : ICollection<IAttribute> {
		private readonly ICustomAttributeProvider _owner;

		public CecilBackedAttributeCollection(ICustomAttributeProvider owner) {
			_owner = owner;
		}

		public IEnumerator<IAttribute> GetEnumerator() {
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public void Add(IAttribute item) {
			throw new NotImplementedException();
		}

		public void Clear() {
			throw new NotImplementedException();
		}

		public bool Contains(IAttribute item) {
			throw new NotImplementedException();
		}

		public void CopyTo(IAttribute[] array, int arrayIndex) {
			int i = arrayIndex;
			foreach (var a in this) {
				array[i] = a;
				i++;
			}
		}

		public bool Remove(IAttribute item) {
			throw new NotImplementedException();
		}

		public int Count {
			get { return _owner.CustomAttributes.Count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}
	}

	public class CecilMetadataWriteBackEngine : IMetadataWriteBackEngine {
		private AssemblyDefinition _assembly;
		private Dictionary<string, TypeDefinition> _allTypes;
		private Dictionary<ITypeDefinition, ICollection<IAttribute>> _typeAttributes;
		private Dictionary<IMember, ICollection<IAttribute>> _memberAttributes;

		public CecilMetadataWriteBackEngine(AssemblyDefinition assembly) {
			_assembly         = assembly;
			_allTypes         = assembly.Modules.SelectMany(m => m.GetTypes()).ToDictionary(t => t.FullName);
			_typeAttributes   = new Dictionary<ITypeDefinition, ICollection<IAttribute>>();
			_memberAttributes = new Dictionary<IMember, ICollection<IAttribute>>();
		}

		public ICollection<IAttribute> GetAttributes(ITypeDefinition type) {
			ICollection<IAttribute> result;
			if (_typeAttributes.TryGetValue(type, out result))
				_typeAttributes[type] = result = new CecilBackedAttributeCollection(_allTypes[type.FullName]);
			return result;
		}

		public ICollection<IAttribute> GetAttributes(IMember member) {
			ICollection<IAttribute> result;
			if (_memberAttributes.TryGetValue(member, out result)) {
				var type = _allTypes[member.DeclaringTypeDefinition.FullName];
				var cecilMember = FindMember(type, member);
				_memberAttributes[member] = result = new CecilBackedAttributeCollection(cecilMember);
			}
			return result;
		}

		public IAttribute CreateAttribute(IAssembly attributeAssembly, string attributeTypeName, IEnumerable<object> positionalArguments, IEnumerable<object> namedArguments) {
			throw new NotImplementedException();
		}

		private IMemberDefinition FindMember(TypeDefinition type, IMember member) {
			switch (member.EntityType) {
				case EntityType.Field:
					return type.Fields.Single(f => f.Name == member.Name);

				case EntityType.Property:
					// TODO: What about explicits
					return type.Properties.Single(p => p.Name == member.Name);

				case EntityType.Accessor: {
					var owner = FindMember(type, ((IMethod)member).AccessorOwner);
					//if (owner
					break;
				}

				case EntityType.Indexer:
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
