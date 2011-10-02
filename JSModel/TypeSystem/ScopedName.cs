using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saltarelle.Compiler.JSModel.TypeSystem {
    public sealed class ScopedName {
        public enum ScopeType {
            NestedType,
            Global
        }

        public ScopeType Scope { get; private set; }

        private ScopedName _containingType;

        /// <summary>
        /// Gets the containing type for a nested type.
        /// </summary>
        public ScopedName ContainingType {
            get {
                if (Scope != ScopeType.NestedType)
                    throw new InvalidOperationException();
                return _containingType;
            }
        }

        private string _nmspace;

        /// <summary>
        /// Gets the namespace for a non-nested type.
        /// </summary>
        public string Namespace {
            get {
                if (Scope != ScopeType.Global)
                    throw new InvalidOperationException();
                return _nmspace;
            }
        }

        /// <summary>
        /// Gets all namespace nesting at the same time (Nmspace1.Nmspace2 => [ Nmspace1, Nmspace2 ]). Returns an empty sequence if there is no namespace.
        /// </summary>
        public IEnumerable<string> NamespaceParts {
            get {
                return Namespace != null ? Namespace.Split('.') : new string[0];
            }
        }

        /// <summary>
        /// Gets the unqualified name.
        /// </summary>
        public string UnqualifiedName { get; private set; }

        /// <summary>
        /// Creates a global name
        /// </summary>
        /// <param name="nmspace">Namespace of the type (sub-namespaces are separated by '.')</param>
        /// <param name="unqualifiedName">Unqualified name.</param>
        /// <returns></returns>
        public static ScopedName Global(string nmspace, string unqualifiedName) {
            Require.ValidJavaScriptNestedIdentifier(nmspace, "nmspace", allowNull: true);
            Require.NotNull(unqualifiedName, "unqualifiedName");

            return new ScopedName { Scope = ScopeType.Global, _nmspace = nmspace, UnqualifiedName = unqualifiedName };
        }

        /// <summary>
        /// Creates a nested name.
        /// </summary>
        public static ScopedName Nested(ScopedName containingType, string unqualifiedName) {
            Require.NotNull(containingType, "containingType");
            Require.ValidJavaScriptIdentifier(unqualifiedName, "unqualifiedName");

            return new ScopedName { Scope = ScopeType.NestedType, _containingType = containingType, UnqualifiedName = unqualifiedName };
        }

        public bool Equals(ScopedName other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._containingType, _containingType) && Equals(other._nmspace, _nmspace) && Equals(other.Scope, Scope) && Equals(other.UnqualifiedName, UnqualifiedName);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(ScopedName)) return false;
            return Equals((ScopedName)obj);
        }

        public override int GetHashCode() {
            unchecked {
                int result = (_containingType != null ? _containingType.GetHashCode() : 0);
                result = (result*397) ^ (_nmspace != null ? _nmspace.GetHashCode() : 0);
                result = (result*397) ^ Scope.GetHashCode();
                result = (result*397) ^ (UnqualifiedName != null ? UnqualifiedName.GetHashCode() : 0);
                return result;
            }
        }

        public override string ToString() {
            switch (Scope) {
                case ScopeType.Global:
                    return (Namespace != null ? Namespace + "." : "") + UnqualifiedName;
                case ScopeType.NestedType:
                    return ContainingType.ToString() + "+" + UnqualifiedName;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
