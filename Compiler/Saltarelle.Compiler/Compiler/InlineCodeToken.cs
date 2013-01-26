using System;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler.Compiler {
	public class InlineCodeToken {
		public enum TokenType {
			Text,
			This,
			Parameter,
			TypeParameter,
			TypeRef,
			LiteralStringParameterToUseAsIdentifier,
			ExpandedParamArrayParameter,
		}

		public TokenType Type { get; private set; }
		private readonly string _text;

		public string Text {
			get {
				if (Type != TokenType.Text && Type != TokenType.TypeRef)
					throw new InvalidOperationException();
				return _text;
			}
		}

		private readonly int _index;
		public int Index {
			get {
				if (Type != TokenType.Parameter && Type != TokenType.TypeParameter && Type != TokenType.LiteralStringParameterToUseAsIdentifier && Type != TokenType.ExpandedParamArrayParameter)
					throw new InvalidOperationException();
				return _index;
			}
		}

		private readonly EntityType _ownerType;
		public EntityType OwnerType {
			get {
				if (Type != TokenType.TypeParameter)
					throw new InvalidOperationException();
				return _ownerType;
			}
		}

		public InlineCodeToken(TokenType type, string text = null, int index = -1, EntityType ownerType = default(EntityType)) {
			Type   = type;
			_text  = text;
			_index = index;
			_ownerType = ownerType;
		}

		public bool Equals(InlineCodeToken other) {
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other._text, _text) && other._index == _index && Equals(other.Type, Type) && _ownerType == other._ownerType;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (InlineCodeToken)) return false;
			return Equals((InlineCodeToken) obj);
		}

		public override int GetHashCode() {
			unchecked {
				int result = (_text != null ? _text.GetHashCode() : 0);
				result = (result*397) ^ _index;
				result = (result*397) ^ Type.GetHashCode();
				result = (result*397) ^ _ownerType.GetHashCode();
				return result;
			}
		}

		public override string ToString() {
			return string.Format("Text: {0}, Index: {1}, Type: {2}, OwnerType: {3}", _text, _index, Type, _ownerType);
		}
	}
}