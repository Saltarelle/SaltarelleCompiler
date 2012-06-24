using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using Mono.Cecil;

namespace Saltarelle.Compiler.MetadataWriteBackEngine {
	public class CecilMetadataWriteBackEngine : IMetadataWriteBackEngine {
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

		private class SimpleAttributeCollection : ICollection<IAttribute> {
			private readonly IList<IAttribute> _attributes;
			public bool IsDirty { get; private set; }
			public ICustomAttributeProvider Entity { get; private set; }

			public SimpleAttributeCollection(ICompilation compilation, ICustomAttributeProvider entity) {
				Entity      = entity;
				_attributes = ConvertAttributes(compilation, entity.CustomAttributes);
				IsDirty     = false;
			}

			public IEnumerator<IAttribute> GetEnumerator() {
				return _attributes.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return GetEnumerator();
			}

			public void Add(IAttribute item) {
				_attributes.Add(item);
				IsDirty = true;
			}

			public void Clear() {
				_attributes.Clear();
				IsDirty = true;
			}

			public bool Contains(IAttribute item) {
				return _attributes.Contains(item);
			}

			public void CopyTo(IAttribute[] array, int arrayIndex) {
				_attributes.CopyTo(array, arrayIndex);
			}

			public bool Remove(IAttribute item) {
				bool result = _attributes.Remove(item);
				IsDirty = true;
				return result;
			}

			public int Count {
				get { return _attributes.Count; }
			}

			public bool IsReadOnly {
				get { return false; }
			}

			private List<IAttribute> ConvertAttributes(ICompilation compilation, IEnumerable<CustomAttribute> src) {
				var result = new List<IAttribute>();
				foreach (var a in src) {
					var attrType = ReflectionHelper.ParseReflectionName(a.AttributeType.FullName).Resolve(compilation);
					var constructor = attrType.GetConstructors().Single(c => AreParameterListsEqual(a.Constructor.Parameters, c.Parameters, compilation));
					var positionalArgs = a.ConstructorArguments.Select((arg, i) => (ResolveResult)new ConstantResolveResult(constructor.Parameters[i].Type, arg.Value)).ToList();
					
					var namedArgs = (         from pv in a.Properties
					                           let p = attrType.GetProperties().Single(p => p.Name == pv.Name) 
					                        select new KeyValuePair<IMember, ResolveResult>(p, new ConstantResolveResult(p.ReturnType, pv.Argument.Value)))
					                .Concat(  from fv in a.Fields
					                           let f = attrType.GetFields().Single(f => f.Name == fv.Name)
					                        select new KeyValuePair<IMember, ResolveResult>(f, new ConstantResolveResult(f.ReturnType, fv.Argument.Value)))
					                .ToList();

					result.Add(new SimpleAttribute(attrType, constructor, positionalArgs.AsReadOnly(), namedArgs.AsReadOnly()));
				}
				return result;
			}
		}

		private readonly AssemblyDefinition _assembly;
		private readonly ICompilation _compilation;
		private readonly Dictionary<string, TypeDefinition> _allTypes;
		private readonly Dictionary<ITypeDefinition, SimpleAttributeCollection> _typeAttributes;
		private readonly Dictionary<IMember, SimpleAttributeCollection> _memberAttributes;
		private readonly CSharpConversions _conversions;
		private readonly CSharpResolver _resolver;

		public CecilMetadataWriteBackEngine(AssemblyDefinition assembly, ICompilation compilation) {
			_assembly         = assembly;
			_compilation      = compilation;
			_conversions      = CSharpConversions.Get(_compilation);
			_resolver         = new CSharpResolver(_compilation);
			_allTypes         = assembly.Modules.SelectMany(m => m.GetTypes()).ToDictionary(t => t.FullName);
			_typeAttributes   = new Dictionary<ITypeDefinition, SimpleAttributeCollection>();
			_memberAttributes = new Dictionary<IMember, SimpleAttributeCollection>();
		}

		public ICollection<IAttribute> GetAttributes(ITypeDefinition type) {
			SimpleAttributeCollection result;
			if (!_typeAttributes.TryGetValue(type, out result))
				_typeAttributes[type] = result = new SimpleAttributeCollection(_compilation, _allTypes[type.ReflectionName]);
			return result;
		}

		public ICollection<IAttribute> GetAttributes(IMember member) {
			SimpleAttributeCollection result;
			if (!_memberAttributes.TryGetValue(member, out result)) {
				var type = _allTypes[member.DeclaringTypeDefinition.ReflectionName];
				var cecilMember = FindMember(type, member);
				_memberAttributes[member] = result = new SimpleAttributeCollection(_compilation, cecilMember);
			}
			return result;
		}

		public IAttribute CreateAttribute(IAssembly attributeAssembly, string attributeTypeName, IList<Tuple<IType, object>> positionalArguments, IList<Tuple<string, object>> namedArguments) {
			var attrType = attributeAssembly.GetAllTypeDefinitions().SingleOrDefault(t => t.ReflectionName == attributeTypeName);
			if (attrType == null)
				throw new ArgumentException("Could not find the type " + attributeTypeName + " in the assembly " + attributeAssembly.AssemblyName + ".");

			var posArgWithError = (positionalArguments != null ? positionalArguments.FirstOrDefault(pa => pa.Item2 != null && _compilation.FindType(pa.Item2.GetType()) != pa.Item1) : null);
			if (posArgWithError != null)
				throw new ArgumentException("The value " + posArgWithError.Item2 + " is not of the type " + posArgWithError.Item1.FullName);

			var or = new OverloadResolution(_compilation, positionalArguments != null ? positionalArguments.Select(a => new ConstantResolveResult(a.Item1, a.Item2)).ToArray<ResolveResult>() : new ResolveResult[0], conversions: _conversions);
			foreach (var c in attrType.GetConstructors())
				or.AddCandidate(c);
			if (!or.FoundApplicableCandidate)
				throw new ArgumentException("Could not find a constructor for the attribute which can be invoked with the suplied positional arguments");
			var actualPosArgs = or.CreateResolveResult(null).Arguments.ToList();

			var actualNamedArgs = new List<KeyValuePair<IMember, ResolveResult>>();
			if (namedArguments != null) {
				foreach (var a in namedArguments) {
					var m = (IMember)attrType.Properties.SingleOrDefault(p => p.Name == a.Item1) ?? attrType.Fields.SingleOrDefault(f => f.Name == a.Item1);
					if (m == null)
						throw new ArgumentException("Could not find member " + a.Item1);
					var sourceType = (a.Item2 != null ? _compilation.FindType(a.Item2.GetType()) : m.ReturnType);
					var conv = _conversions.StandardImplicitConversion(sourceType, m.ReturnType);
					if (!conv.IsValid)
						throw new ArgumentException("Could not convert type " + sourceType.FullName + " to " + m.ReturnType.FullName + " for named argument " + a.Item1);
					actualNamedArgs.Add(new KeyValuePair<IMember, ResolveResult>(m, _resolver.ResolveCast(m.ReturnType, new ConstantResolveResult(sourceType, a.Item2))));
				}
			}
			
			return new SimpleAttribute(attrType, (IMethod)or.BestCandidate, actualPosArgs, actualNamedArgs);
		}

		private string GetTypeNameForExplicitImplementation(IType type) {
			if (type is ParameterizedType) {
				var pt = (ParameterizedType)type;
				return GetTypeNameForExplicitImplementation(pt.GetDefinition()) + "<" + string.Join(",", pt.TypeArguments.Select(GetTypeNameForExplicitImplementation)) + ">";
			}
			else {
				return type.FullName;
			}
		}

		private static bool AreTypesEqual(TypeReference t1, IType t2, ICompilation compilation) {
			if (t1.IsGenericParameter) {
				if (t2.Kind == TypeKind.TypeParameter)
					return t1.Name == t2.Name;
				else
					return false;
			}
			else {
				if (t2.Kind == TypeKind.TypeParameter)
					return false;
				else
					return ReflectionHelper.ParseReflectionName(t1.FullName).Resolve(compilation) == t2;
			}
		}

		private static bool AreParameterListsEqual(IEnumerable<ParameterDefinition> l1, IEnumerable<IParameter> l2, ICompilation compilation) {
			var e1 = l1.GetEnumerator();
			var e2 = l2.GetEnumerator();
			for (;;) {
				bool b1 = e1.MoveNext();
				bool b2 = e2.MoveNext();
				if (b1 != b2)
					return false;
				if (!b1)
					return true;
				if (!AreTypesEqual(e1.Current.ParameterType, e2.Current.Type, compilation))
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
						name = GetTypeNameForExplicitImplementation(member.ImplementedInterfaceMembers[0].DeclaringType) + "." + member.ImplementedInterfaceMembers[0].Name;
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
					else if (accessor.AccessorOwner is IEvent) {
						var e = (IEvent)accessor.AccessorOwner;
						if (member == e.AddAccessor)
							return ((EventDefinition)cecilOwner).AddMethod;
						else if (member == e.RemoveAccessor)
							return ((EventDefinition)cecilOwner).RemoveMethod;
						else
							throw new Exception("The accessor " + member.DeclaringType.FullName + "." + member.Name + " is neither the adder nor the remover of the owning event.");
					}
					else {
						throw new Exception("The owner of the accessor " + member.DeclaringType.FullName + "." + member.Name + " is neither a property nor an event.");
					}
				}

				case EntityType.Indexer: {
					string name;
					if (member.IsExplicitInterfaceImplementation) {
						if (member.ImplementedInterfaceMembers.Count > 1)
							throw new NotSupportedException(type.FullName + "." + member.Name + " implements more than one member explicitly.");
						name = GetTypeNameForExplicitImplementation(member.ImplementedInterfaceMembers[0].DeclaringType) + "." + member.ImplementedInterfaceMembers[0].Name;
					}
					else {
						name = member.Name;
					}
					var result = type.Properties.SingleOrDefault(p => p.Name == name && AreParameterListsEqual(p.Parameters, ((IParameterizedMember)member).Parameters, _compilation));
					if (result == null)
						throw new Exception("Could not find indexer " + name + ".");
					return result;
				}

				case EntityType.Event: {
					string name;
					if (member.IsExplicitInterfaceImplementation) {
						if (member.ImplementedInterfaceMembers.Count > 1)
							throw new NotSupportedException(type.FullName + "." + member.Name + " implements more than one member explicitly.");
						name = GetTypeNameForExplicitImplementation(member.ImplementedInterfaceMembers[0].DeclaringType) + "." + member.ImplementedInterfaceMembers[0].Name;
					}
					else {
						name = member.Name;
					}
					var result = type.Events.SingleOrDefault(p => p.Name == name);
					if (result == null)
						throw new Exception("Could not find event " + name + ".");
					return result;
				}

				case EntityType.Method:
				case EntityType.Operator: {
					string name;
					if (member.IsExplicitInterfaceImplementation) {
						if (member.ImplementedInterfaceMembers.Count > 1)
							throw new NotSupportedException(type.FullName + "." + member.Name + " implements more than one member explicitly.");
						name = GetTypeNameForExplicitImplementation(member.ImplementedInterfaceMembers[0].DeclaringType) + "." + member.ImplementedInterfaceMembers[0].Name;
					}
					else {
						name = member.Name;
					}
					var result = type.Methods.SingleOrDefault(m => !m.IsConstructor && m.Name == name && AreParameterListsEqual(m.Parameters, ((IParameterizedMember)member).Parameters, _compilation) && AreTypesEqual(m.ReturnType, member.ReturnType, _compilation));
					if (result == null)
						throw new Exception("Could not find method " + name + ".");
					return result;
				}

				case EntityType.Constructor: {
					var result = type.Methods.SingleOrDefault(m => m.IsConstructor && AreParameterListsEqual(m.Parameters, ((IParameterizedMember)member).Parameters, _compilation));
					if (result == null)
						throw new Exception("Could not find constructor.");
					return result;
				}

				default:
					throw new NotSupportedException("Entity type " + member.EntityType + " is not supported.");
			}
		}

		private TypeReference CreateTypeReference(ModuleDefinition module, IType type) {
			if (type == _compilation.FindType(KnownTypeCode.Object))
				return module.TypeSystem.Object;
			if (type == _compilation.FindType(KnownTypeCode.Void))
				return module.TypeSystem.Void;
			if (type == _compilation.FindType(KnownTypeCode.Boolean))
				return module.TypeSystem.Boolean;
			if (type == _compilation.FindType(KnownTypeCode.Char))
				return module.TypeSystem.Char;
			if (type == _compilation.FindType(KnownTypeCode.SByte))
				return module.TypeSystem.SByte;
			if (type == _compilation.FindType(KnownTypeCode.Byte))
				return module.TypeSystem.Byte;
			if (type == _compilation.FindType(KnownTypeCode.Int16))
				return module.TypeSystem.Int16;
			if (type == _compilation.FindType(KnownTypeCode.UInt16))
				return module.TypeSystem.UInt16;
			if (type == _compilation.FindType(KnownTypeCode.Int32))
				return module.TypeSystem.Int32;
			if (type == _compilation.FindType(KnownTypeCode.UInt32))
				return module.TypeSystem.UInt32;
			if (type == _compilation.FindType(KnownTypeCode.Int64))
				return module.TypeSystem.Int64;
			if (type == _compilation.FindType(KnownTypeCode.UInt64))
				return module.TypeSystem.UInt64;
			if (type == _compilation.FindType(KnownTypeCode.Single))
				return module.TypeSystem.Single;
			if (type == _compilation.FindType(KnownTypeCode.Double))
				return module.TypeSystem.Double;
			if (type == _compilation.FindType(KnownTypeCode.IntPtr))
				return module.TypeSystem.IntPtr;
			if (type == _compilation.FindType(KnownTypeCode.UIntPtr))
				return module.TypeSystem.UIntPtr;
			if (type == _compilation.FindType(KnownTypeCode.String))
				return module.TypeSystem.String;

			var name = type.GetDefinition().ParentAssembly.AssemblyName;
			if (module.Assembly.Name.Name == name)
				return new TypeReference(type.Namespace, type.Name, module, module);
			var asm = module.AssemblyReferences.Where(n => n.Name == name).OrderByDescending(n => n.Version).FirstOrDefault();	// We can have two different references to mscorlib, and unfortunately NRefactory cannot distinguish them.
			if (asm == null)
				throw new InvalidOperationException("The processed module does not reference the assembly " + name);
			return new TypeReference(type.Namespace, type.Name, module, asm);
		}

		private CustomAttribute ConvertAttribute(MemberReference entity, IAttribute a) {
			var attrType = CreateTypeReference(entity.Module, a.AttributeType);
			var ctor = new MethodReference(".ctor", entity.Module.TypeSystem.Void, attrType);
			for (int i = 0; i < a.PositionalArguments.Count; i++) {
				ctor.Parameters.Add(new ParameterDefinition(a.Constructor.Parameters[i].Name, ParameterAttributes.None, CreateTypeReference(entity.Module, a.Constructor.Parameters[i].Type)));
			}
			var result = new CustomAttribute(ctor);
			foreach (var arg in a.PositionalArguments) {
				result.ConstructorArguments.Add(new CustomAttributeArgument(CreateTypeReference(entity.Module, arg.Type), arg.ConstantValue));
			}

			foreach (var arg in a.NamedArguments) {
				var na = new CustomAttributeNamedArgument(arg.Key.Name, new CustomAttributeArgument(CreateTypeReference(entity.Module, arg.Value.Type), arg.Value.ConstantValue));
				if (arg.Key is IField)
					result.Fields.Add(na);
				else if (arg.Key is IProperty)
					result.Properties.Add(na);
				else
					throw new Exception("Invalid named argument target " + arg.Key);
			}

			return result;
		}

		private void ApplyAttributes(ICustomAttributeProvider entity, ICollection<IAttribute> newAttributes) {
			entity.CustomAttributes.Clear();
			if (newAttributes != null) {
				foreach (var a in newAttributes)
					entity.CustomAttributes.Add(ConvertAttribute((MemberReference)entity, a));
			}
		}

		public void Apply() {
			foreach (var v in _typeAttributes.Values.Concat(_memberAttributes.Values).Where(v => v.IsDirty))
				ApplyAttributes(v.Entity, v);
		}
	}
}
