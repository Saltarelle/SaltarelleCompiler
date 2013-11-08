﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace CoreLib.Tests.RuntimeLibraryTests {
	[TestFixture]
	public class ScriptTypeTests {
		[Test]
		public void DelegateTypeAppearsAsFunctionAsGenericArgument() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
class G<T> {}
class C {
	public void M() {
		// BEGIN
		var f = new G<Func<int, string>>();
		// END
	}
}
",
@"			var f = new (ss.makeGenericType($$G$1, [Function]))();
");
		}

		[Test]
		public void DelegateTypeAppearsAsFunctionAsTypeArgumentInInlineCode() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
class C {
	[System.Runtime.CompilerServices.InlineCode(""{T}"")] public object F<T>() {}
	public void M() {
		// BEGIN
		var x = F<Func<int, string>>();
		// END
	}
}
",
@"			var x = Function;
");
		}

		[Test]
		public void TypeOfDelegateTypeReturnsFunction() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
class C {
	public void M() {
		// BEGIN
		var t = typeof(Func<int, string>);
		// END
	}
}
",
@"			var t = Function;
");
		}

		[Test]
		public void CastingToDelegateTypeCastsToFunction() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
class C {
	public void M() {
		object o = null;
		// BEGIN
		var t = (Func<int, string>)o;
		// END
	}
}
",
@"			var t = ss.cast(o, Function);
");
		}

		[Test]
		public void ArrayTypeAppearsAsArrayAsGenericArgument() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
class G<T> {}
class C {
	public void M() {
		// BEGIN
		var f = new G<int[]>();
		// END
	}
}
",
@"			var f = new (ss.makeGenericType($$G$1, [Array]))();
");
		}

		[Test]
		public void ArrayTypeAppearsAsArrayAsTypeArgumentInInlineCode() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
class C {
	[System.Runtime.CompilerServices.InlineCode(""{T}"")] public object F<T>() {}
	public void M() {
		// BEGIN
		var x = F<int[]>();
		// END
	}
}
",
@"			var x = Array;
");
		}

		[Test]
		public void TypeOfArrayTypeReturnsArray() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
class C {
	public void M() {
		// BEGIN
		var t = typeof(int[]);
		// END
	}
}
",
@"			var t = Array;
");
		}

		[Test]
		public void CastingToArrayTypeCastsToArray() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
class C {
	public void M() {
		object o = null;
		// BEGIN
		var t = (int[])o;
		// END
	}
}
",
@"			var t = ss.cast(o, Array);
");
		}

		[Test]
		public void TypeOfOpenGenericTypeWorks() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
public class G<T1, T2> {}
class C {
	public void M() {
		// BEGIN
		var t = typeof(G<,>);
		// END
	}
}
",
@"			var t = $G$2;
");
		}

		[Test]
		public void EnumAsGenericArgumentWorks() {
			SourceVerifier.AssertSourceCorrect(@"
public enum E {}
public class G<T> {}
class C {
	public void M() {
		// BEGIN
		var f = new G<E>();
		// END
	}
}
",
@"			var f = new (ss.makeGenericType($G$1, [$E]))();
");
		}

		[Test]
		public void EnumAsTypeArgumentInInlineCodeWorks() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
public enum E {}
class C {
	[System.Runtime.CompilerServices.InlineCode(""{T}"")] public object F<T>() {}
	public void M() {
		// BEGIN
		var x = F<E>();
		// END
	}
}
",
@"			var x = $E;
");
		}

		[Test]
		public void CastToEnumIsCastToInt() {
			SourceVerifier.AssertSourceCorrect(@"
public enum E {}
class C {
	public void M() {
		object o = null;
		// BEGIN
		var f = (E)o;
		// END
	}
}
",
@"			var f = ss.cast(o, ss.Int32);
");
		}

		[Test]
		public void TypeOfEnumIsTheEnum() {
			SourceVerifier.AssertSourceCorrect(@"
public enum E {}
class C {
	public void M() {
		object o = null;
		// BEGIN
		var t = typeof(E);
		// END
	}
}
",
@"			var t = $E;
");
		}

		[Test]
		public void ImportedEnumIsObjectAsTypeArgument() {
			SourceVerifier.AssertSourceCorrect(@"
[System.Runtime.CompilerServices.Imported] public enum E {}
public class G<T> {}
class C {
	public void M() {
		// BEGIN
		var f = new G<E>();
		// END
	}
}
",
@"			var f = new (ss.makeGenericType($G$1, [Object]))();
");
		}

		[Test]
		public void ImportedEnumIsObjectAsTypeArgumentInInlineCode() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
[System.Runtime.CompilerServices.Imported] public enum E {}
class C {
	[System.Runtime.CompilerServices.InlineCode(""{T}"")] public object F<T>() {}
	public void M() {
		// BEGIN
		var x = F<E>();
		// END
	}
}
",
@"			var x = Object;
");
		}

		[Test]
		public void ImportedEnumThatObeysTypeSystemIsItselfAsTypeArgument() {
			SourceVerifier.AssertSourceCorrect(@"
[System.Runtime.CompilerServices.Imported(ObeysTypeSystem = true)] public enum E {}
public class G<T> {}
class C {
	public void M() {
		// BEGIN
		var f = new G<E>();
		// END
	}
}
",
@"			var f = new (ss.makeGenericType($G$1, [E]))();
");
		}

		[Test]
		public void ParameterizedTypeAppearsAsItselfWhenUsedAsGenericArgument() {
			SourceVerifier.AssertSourceCorrect(@"
public class G<T1> {}
class C {
	public void M() {
		// BEGIN
		var f = new G<G<int>>();
		// END
	}
}
",
@"			var f = new (ss.makeGenericType($G$1, [ss.makeGenericType($G$1, [ss.Int32])]))();
");
		}

		[Test]
		public void ParameterizedTypeAppearsAsItselfWhenUsedAsInlineCodeGenericArgument() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
public class G<T1> {}
class C {
	[System.Runtime.CompilerServices.InlineCode(""{T}"")] public object F<T>() {}
	public void M() {
		// BEGIN
		var x = F<G<int>>();
		// END
	}
}
",
@"			var x = ss.makeGenericType($G$1, [ss.Int32]);
");
		}

		[Test]
		public void ParameterizedTypeWorksWhenUsedWithTypeOf() {
			SourceVerifier.AssertSourceCorrect(@"
public class G<T1, T2> {}
class C {
	public void M() {
		// BEGIN
		var t = typeof(G<object, string>);
		// END
	}
}
",
@"			var t = ss.makeGenericType($G$2, [Object, String]);
");
		}

		[Test]
		public void CanUseStaticMemberOfParameterizedType() {
			SourceVerifier.AssertSourceCorrect(@"
public class G<T1, T2> { public static void M() {} }
class C {
	public void M() {
		// BEGIN
		G<object, string>.M();
		// END
	}
}
",
@"			ss.makeGenericType($G$2, [Object, String]).m();
");
		}

		[Test]
		public void CastingToParameterizedTypeWorks() {
			SourceVerifier.AssertSourceCorrect(@"
public class G<T1, T2> { public void M() {} }
class C {
	public void M() {
		object o = null;
		// BEGIN
		var g = (G<object, string>)o;
		// END
	}
}
",
@"			var g = ss.cast(o, ss.makeGenericType($G$2, [Object, String]));
");
		}

		[Test]
		public void ParameterizedTypeWithIgnoreGenericArgumentsAppearsAsItselfWhenUsedAsGenericArgument() {
			SourceVerifier.AssertSourceCorrect(@"
public class G<T1> {}
[System.Runtime.CompilerServices.IncludeGenericArguments(false)] public class G2<T1> {}
class C {
	public void M() {
		// BEGIN
		var f = new G<G2<int>>();
		// END
	}
}
",
@"			var f = new (ss.makeGenericType($G$1, [$G2]))();
");
		}

		[Test]
		public void ParameterizedTypeWithIgnoreGenericArgumentsAppearsAsItselfWhenUsedAsInlineCodeGenericArgument() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
[System.Runtime.CompilerServices.IncludeGenericArguments(false)] public class G<T1> {}
class C {
	[System.Runtime.CompilerServices.InlineCode(""{T}"")] public object F<T>() {}
	public void M() {
		// BEGIN
		var x = F<G<int>>();
		// END
	}
}
",
@"			var x = $G;
");
		}

		[Test]
		public void ParameterizedTypeWithIgnoreGenericArgumentsWorksWhenUsedWithTypeOf() {
			SourceVerifier.AssertSourceCorrect(@"
[System.Runtime.CompilerServices.IncludeGenericArguments(false)] public class G<T1, T2> {}
class C {
	public void M() {
		// BEGIN
		var t = typeof(G<object, string>);
		// END
	}
}
",
@"			var t = $G;
");
		}

		[Test]
		public void CanUseStaticMemberOfParameterizedTypeWithIgnoreGenericArguments() {
			SourceVerifier.AssertSourceCorrect(@"
[System.Runtime.CompilerServices.IncludeGenericArguments(false)] public class G<T1, T2> { public static void M() {} }
class C {
	public void M() {
		// BEGIN
		G<object, string>.M();
		// END
	}
}
",
@"			$G.m();
");
		}

		[Test]
		public void CastingToParameterizedTypeWithIgnoreGenericArgumentsWorks() {
			SourceVerifier.AssertSourceCorrect(@"
[System.Runtime.CompilerServices.IncludeGenericArguments(false)] public class G<T1, T2> { public void M() {} }
class C {
	public void M() {
		object o = null;
		// BEGIN
		var g = (G<object, string>)o;
		// END
	}
}
",
@"			var g = ss.cast(o, $G);
");
		}

		[Test]
		public void ImportedParameterizedTypeAppearsAsObjectWhenUsedAsGenericArgument() {
			SourceVerifier.AssertSourceCorrect(@"
public class G<T1> {}
[System.Runtime.CompilerServices.Imported] public class G2<T1> {}
class C {
	public void M() {
		// BEGIN
		var f = new G<G2<int>>();
		// END
	}
}
",
@"			var f = new (ss.makeGenericType($G$1, [Object]))();
");
		}

		[Test]
		public void ImportedParameterizedTypeAppearsAsObjectWhenUsedAsAsInlineCodeGenericArgument() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
[System.Runtime.CompilerServices.Imported] public class G<T1> {}
class C {
	[System.Runtime.CompilerServices.InlineCode(""{T}"")] public object F<T>() {}
	public void M() {
		// BEGIN
		var x = F<G<int>>();
		// END
	}
}
",
@"			var x = Object;
");
		}

		[Test]
		public void ImportedParameterizedTypeWorksWhenUsedWithTypeOf() {
			SourceVerifier.AssertSourceCorrect(@"
[System.Runtime.CompilerServices.Imported] public class G<T1, T2> {}
class C {
	public void M() {
		// BEGIN
		var t = typeof(G<object, string>);
		// END
	}
}
",
@"			var t = G;
");
		}

		[Test]
		public void CanUseStaticMemberOfImportedParameterizedType() {
			SourceVerifier.AssertSourceCorrect(@"
[System.Runtime.CompilerServices.Imported] public class G<T1, T2> { public static void M() {} }
class C {
	public void M() {
		// BEGIN
		G<object, string>.M();
		// END
	}
}
",
@"			G.m();
");
		}

		[Test]
		public void CastingToImportedParameterizedTypeIsANoOp() {
			SourceVerifier.AssertSourceCorrect(@"
[System.Runtime.CompilerServices.Imported] public class G<T1, T2> { public void M() {} }
class C {
	public void M() {
		object o = null;
		// BEGIN
		var g = (G<object, string>)o;
		// END
	}
}
",
@"			var g = o;
");
		}

		[Test]
		public void SerializableParameterizedTypeAppearsAsItselfWhenUsedAsGenericArgument() {
			SourceVerifier.AssertSourceCorrect(@"
public class G<T1> {}
[System.Serializable] public class G2<T1> {}
class C {
	public void M() {
		// BEGIN
		var f = new G<G2<int>>();
		// END
	}
}
",
@"			var f = new (ss.makeGenericType($G$1, [ss.makeGenericType($G2$1, [ss.Int32])]))();
");
		}

		[Test]
		public void SerializableParameterizedTypeAppearsAsItselfWhenUsedAsGenericArgumentInInlineCode() {
			SourceVerifier.AssertSourceCorrect(@"
[System.Serializable] public class G<T1> {}
class C {
	[System.Runtime.CompilerServices.InlineCode(""{T}"")] public object F<T>() {}
	public void M() {
		// BEGIN
		var x = F<G<int>>();
		// END
	}
}
",
@"			var x = ss.makeGenericType($G$1, [ss.Int32]);
");
		}

		[Test]
		public void SerializableParameterizedTypeWorksWhenUsedWithTypeOf() {
			SourceVerifier.AssertSourceCorrect(@"
[System.Serializable] public class G<T1, T2> {}
class C {
	public void M() {
		// BEGIN
		var t = typeof(G<object, string>);
		// END
	}
}
",
@"			var t = ss.makeGenericType($G$2, [Object, String]);
");
		}

		[Test]
		public void CanUseStaticMemberOfSerializableParameterizedType() {
			SourceVerifier.AssertSourceCorrect(@"
[System.Serializable] public class G<T1, T2> { public static void M() {} }
class C {
	public void M() {
		// BEGIN
		G<object, string>.M();
		// END
	}
}
",
@"			ss.makeGenericType($G$2, [Object, String]).m();
");
		}

		[Test]
		public void CastingToSerializableParameterizedTypeIsANoOp() {
			SourceVerifier.AssertSourceCorrect(@"
[System.Serializable] public class G<T1, T2> { public void M() {} }
class C {
	public void M() {
		object o = null;
		// BEGIN
		var g = (G<object, string>)o;
		// END
	}
}
",
@"			var g = o;
");
		}

		[Test]
		public void SerializableParameterizedTypeWithIgnoreGenericArgumentsAppearsAsItselfWhenUsedAsGenericArgument() {
			SourceVerifier.AssertSourceCorrect(@"
public class G<T1> {}
[System.Serializable, System.Runtime.CompilerServices.IncludeGenericArguments(false)] public class G2<T1> {}
class C {
	public void M() {
		// BEGIN
		var f = new G<G2<int>>();
		// END
	}
}
",
@"			var f = new (ss.makeGenericType($G$1, [$G2]))();
");
		}

		[Test]
		public void SerializableParameterizedTypeWithIgnoreGenericArgumentsAppearsAsItselfWhenUsedAsInlineCodeGenericArgument() {
			SourceVerifier.AssertSourceCorrect(@"
[System.Serializable, System.Runtime.CompilerServices.IncludeGenericArguments(false)] public class G2<T1> {}
class C {
	[System.Runtime.CompilerServices.InlineCode(""{T}"")] public object F<T>() {}
	public void M() {
		// BEGIN
		var x = F<G2<int>>();
		// END
	}
}
",
@"			var x = $G2;
");
		}

		[Test]
		public void SerializableParameterizedTypeWithIgnoreGenericArgumentsWorksWhenUsedWithTypeOf() {
			SourceVerifier.AssertSourceCorrect(@"
[System.Serializable, System.Runtime.CompilerServices.IncludeGenericArguments(false)] public class G<T1, T2> {}
class C {
	public void M() {
		// BEGIN
		var t = typeof(G<object, string>);
		// END
	}
}
",
@"			var t = $G;
");
		}

		[Test]
		public void CanUseStaticMemberOfSerializableParameterizedTypeWithIgnoreGenericArguments() {
			SourceVerifier.AssertSourceCorrect(@"
[System.Serializable, System.Runtime.CompilerServices.IncludeGenericArguments(false)] public class G<T1, T2> { public static void M() {} }
class C {
	public void M() {
		// BEGIN
		G<object, string>.M();
		// END
	}
}
",
@"			$G.m();
");
		}

		[Test]
		public void CastingToSerializableParameterizedTypeWithIgnoreGenericArgumentsIsANoOp() {
			SourceVerifier.AssertSourceCorrect(@"
[System.Serializable, System.Runtime.CompilerServices.IncludeGenericArguments(false)] public class G<T1, T2> { public void M() {} }
class C {
	public void M() {
		object o = null;
		// BEGIN
		var g = (G<object, string>)o;
		// END
	}
}
",
@"			var g = o;
");
		}

		[Test]
		public void TypeDefinitionAppearsAsItselfWhenUsedAsGenericArgument() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
public class G<T> {}
public class X {}
class C {
	public void M() {
		// BEGIN
		var f = new G<X>();
		// END
	}
}
",
@"			var f = new (ss.makeGenericType($G$1, [$X]))();
");
		}

		[Test]
		public void TypeDefinitionAppearsAsItselfWhenUsedAsGenericArgumentInInlineCode() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
public class X {}
class C {
	[System.Runtime.CompilerServices.InlineCode(""{T}"")] public object F<T>() {}
	public void M() {
		// BEGIN
		var x = F<X>();
		// END
	}
}
",
@"			var x = $X;
");
		}

		[Test]
		public void ImportedTypeDefinitionAppearsAsObjectWhenUsedAsGenericArgument() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
public class G<T> {}
[System.Runtime.CompilerServices.Imported] public class X {}
class C {
	public void M() {
		// BEGIN
		var f = new G<X>();
		// END
	}
}
",
@"			var f = new (ss.makeGenericType($G$1, [Object]))();
");
		}

		[Test]
		public void ImportedTypeDefinitionAppearsAsObjectWhenUsedAsGenericArgumentInInlineCode() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
[System.Runtime.CompilerServices.Imported] public class X {}
class C {
	[System.Runtime.CompilerServices.InlineCode(""{T}"")] public object F<T>() {}
	public void M() {
		// BEGIN
		var f = F<X>();
		// END
	}
}
",
@"			var f = Object;
");
		}

		[Test]
		public void ImportedTypeDefinitionThatObeysTheTypeSystemAppearsAsItselfWhenUsedAsGenericArgument() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
public class G<T> {}
[System.Runtime.CompilerServices.Imported(ObeysTypeSystem=true)] public class X {}
class C {
	public void M() {
		// BEGIN
		var f = new G<X>();
		// END
	}
}
",
@"			var f = new (ss.makeGenericType($G$1, [X]))();
");
		}

		[Test]
		public void ImportedTypeDefinitionThatObeysTheTypeSystemAppearsAsItselfWhenUsedAsGenericArgumentInInlineCode() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
public class G<T> {}
[System.Runtime.CompilerServices.Imported(ObeysTypeSystem=true)] public class X {}
class C {
	[System.Runtime.CompilerServices.InlineCode(""{T}"")] public object F<T>() {}
	public void M() {
		// BEGIN
		var x = F<X>();
		// END
	}
}
",
@"			var x = X;
");
		}

		[Test]
		public void SerializableTypeDefinitionAppearsAsItselfWhenUsedAsGenericArgument() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
public class G<T> {}
[System.Serializable] public class X {}
class C {
	public void M() {
		// BEGIN
		var f = new G<X>();
		// END
	}
}
",
@"			var f = new (ss.makeGenericType($G$1, [$X]))();
");
		}

		[Test]
		public void SerializableTypeDefinitionAppearsAsItselfWhenUsedAsGenericArgumentInInlineCode() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
[System.Serializable] public class X {}
class C {
	[System.Runtime.CompilerServices.InlineCode(""{T}"")] public object F<T>() {}
	public void M() {
		// BEGIN
		var f = F<X>();
		// END
	}
}
",
@"			var f = $X;
");
		}

		[Test]
		public void TypeOfTypeDefinitionWorks() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
public class X {}
class C {
	public void M() {
		// BEGIN
		var t = typeof(X);
		// END
	}
}
",
@"			var t = $X;
");
		}

		[Test]
		public void TypeOfImportedTypeDefinitionReturnsItself() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
[System.Runtime.CompilerServices.Imported] public class X {}
class C {
	public void M() {
		// BEGIN
		var t = typeof(X);
		// END
	}
}
",
@"			var t = X;
");
		}

		[Test]
		public void TypeOfImportedTypeDefinitionThatObeysTheTypeSystemReturnsItself() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
[System.Runtime.CompilerServices.Imported(ObeysTypeSystem=true)] public class X {}
class C {
	public void M() {
		// BEGIN
		var t = typeof(X);
		// END
	}
}
",
@"			var t = X;
");
		}

		[Test]
		public void TypeOfSerializableTypeDefinitionWorks() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
[System.Serializable] public class X {}
class C {
	public void M() {
		// BEGIN
		var f = typeof(X);
		// END
	}
}
",
@"			var f = $X;
");
		}

		[Test]
		public void InvokingStaticMethodOfTypeDefinitionWorks() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
public class X { public static void M() {} }
class C {
	public void M() {
		// BEGIN
		X.M();
		// END
	}
}
",
@"			$X.m();
");
		}

		[Test]
		public void InvokingStaticMethodOfImportedTypeDefinitionWorks() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
[System.Runtime.CompilerServices.Imported] public class X { public static void M() {} }
class C {
	public void M() {
		// BEGIN
		X.M();
		// END
	}
}
",
@"			X.m();
");
		}

		[Test]
		public void InvokingStaticMethodOfImportedTypeDefinitionThatObeysTheTypeSystemWorks() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
[System.Runtime.CompilerServices.Imported(ObeysTypeSystem=true)] public class X { public static void M() {} }
class C {
	public void M() {
		// BEGIN
		X.M();
		// END
	}
}
",
@"			X.m();
");
		}

		[Test]
		public void InvokingStaticMethodOfSerializableTypeDefinitionWorks() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
[System.Serializable] public class X { public static void M() {} }
class C {
	public void M() {
		// BEGIN
		X.M();
		// END
	}
}
",
@"			$X.m();
");
		}

		[Test]
		public void CastingToTypeDefinitionWorks() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
public class X {}
class C {
	public void M() {
		object o;
		// BEGIN
		var x = (X)o;
		// END
	}
}
",
@"			var x = ss.cast(o, $X);
");
		}

		[Test]
		public void CastingToImportedTypeDefinitionIsANoOp() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
[System.Runtime.CompilerServices.Imported] public class X {}
class C {
	public void M() {
		object o;
		// BEGIN
		var x = (X)o;
		// END
	}
}
",
@"			var x = o;
");
		}

		[Test]
		public void CastingToImportedTypeDefinitionThatObeysTheTypeSystemWorks() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
[System.Runtime.CompilerServices.Imported(ObeysTypeSystem=true)] public class X { public static void M() {} }
class C {
	public void M() {
		object o;
		// BEGIN
		var x = (X)o;
		// END
	}
}
",
@"			var x = ss.cast(o, X);
");
		}

		[Test]
		public void CastingToSerializableTypeDefinitionIsANoOp() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
[System.Serializable] public class X { public static void M() {} }
class C {
	public void M() {
		object o;
		// BEGIN
		var x = (X)o;
		// END
	}
}
",
@"			var x = o;
");
		}

		[Test]
		public void CastingFromImportedInterfaceVerifiesTheType() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
[System.Runtime.CompilerServices.Imported] public interface I {}
public class X : I {}
class C {
	public void M() {
		I i = null;
		// BEGIN
		var x = (X)i;
		// END
	}
}
",
@"			var x = ss.cast(i, $X);
");
		}

		[Test]
		public void CastingBetweenTwoTypesWithTheSameScriptNameIsANoOp() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
[System.Runtime.CompilerServices.ScriptName(""Something"")] public interface I1 {}
[System.Runtime.CompilerServices.ScriptName(""Something"")] public interface I2 {}
class C {
	public void M() {
		I1 i = null;
		// BEGIN
		var i2 = (I2)i;
		// END
	}
}
",
@"			var i2 = i;
");
		}

		[Test]
		public void CastingBetweenTypesWithTheSameNameInDifferentAssembliesVerifiesTheType() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
[System.Runtime.CompilerServices.ScriptName(""IDisposable"")] public interface I2 {}
class C {
	public void M() {
		IDisposable i;
		// BEGIN
		var i2 = (I2)i;
		// END
	}
}
",
@"			var i2 = ss.cast(i, $IDisposable);
");
		}

		[Test]
		public void DynamicAppearsAsObjectAsGenericArgument() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
class G<T> {}
class C {
	public void M() {
		// BEGIN
		var f = new G<dynamic>();
		// END
	}
}
",
@"			var f = new (ss.makeGenericType($$G$1, [Object]))();
");
		}

		[Test]
		public void DynamicAppearsAsObjectAsTypeArgumentInInlineCode() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
class C {
	[System.Runtime.CompilerServices.InlineCode(""{T}"")] public object F<T>() {}
	public void M() {
		// BEGIN
		var x = F<dynamic>();
		// END
	}
}
",
@"			var x = Object;
");
		}

		[Test]
		public void TypeOfDynamicReturnsObject() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
class C {
	public void M() {
		// BEGIN
		var t = typeof(dynamic);
		// END
	}
}
",
@"			var t = Object;
");
		}

		[Test]
		public void CastingToDynamicIsANoOp() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
class C {
	public void M() {
		object o = null;
		// BEGIN
		var t = (dynamic)o;
		// END
	}
}
",
@"			var t = o;
");
		}

		[Test]
		public void AnonymousTypeAppearsAsObjectAsGenericArgument() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
class G<T> {}
public class C {
	public void G<T>(T x) {}
	public void M() {
		var o = new { Item = 1 };
		// BEGIN
		G(o);
		// END
	}
}
",
@"			this.g(Object).call(this, o);
");
		}

		[Test]
		public void AnonymousTypeAppearsAsObjectAsGenericArgumentInInlineCode() {
			SourceVerifier.AssertSourceCorrect(@"
using System;
class C {
	[System.Runtime.CompilerServices.InlineCode(""{T}"")] public object F<T>(T x) {}
	public void M() {
		var o = new { Item = 1 };
		// BEGIN
		var x = F(o);
		// END
	}
}
",
@"			var x = Object;
");
		}

		[Test]
		public void UseOfTypeIsWithNonScriptableTypeIsAnError() {
			var result = SourceVerifier.Compile(@"
[System.Runtime.CompilerServices.NonScriptable] class X1 {}

public class C {
	public static void M(object o) {
		var x = o is X1;
	}
}", expectErrors: true);
			Assert.That(result.Item2.AllMessages.Any(m => m.Code == 7522 && m.FormattedMessage.Contains("X1")));

			result = SourceVerifier.Compile(@"
class G<T> {}
[System.Runtime.CompilerServices.NonScriptable] class X1 {}

public class C {
	public static void M(object o) {
		var x = o is G<X1>;
	}
}", expectErrors: true);
			Assert.That(result.Item2.AllMessages.Any(m => m.Code == 7522 && m.FormattedMessage.Contains("X1")));
		}

		[Test]
		public void UseOfTypeAsWithNonScriptableTypeIsAnError() {
			var result = SourceVerifier.Compile(@"
[System.Runtime.CompilerServices.NonScriptable] class X1 {}

public class C {
	public static void M(object o) {
		var x = o as X1;
	}
}", expectErrors: true);
			Assert.That(result.Item2.AllMessages.Any(m => m.Code == 7522 && m.FormattedMessage.Contains("X1")));

			result = SourceVerifier.Compile(@"
class G<T> {}
[System.Runtime.CompilerServices.NonScriptable] class X1 {}

public class C {
	public static void M(object o) {
		var x = o as G<X1>;
	}
}", expectErrors: true);
			Assert.That(result.Item2.AllMessages.Any(m => m.Code == 7522 && m.FormattedMessage.Contains("X1")));
		}

		[Test]
		public void UseOfCastWithNonScriptableTypeIsAnError() {
			var result = SourceVerifier.Compile(@"
[System.Runtime.CompilerServices.NonScriptable] class X1 {}

public class C {
	public static void M(object o) {
		var x = (X1)o;
	}
}", expectErrors: true);
			Assert.That(result.Item2.AllMessages.Any(m => m.Code == 7522 && m.FormattedMessage.Contains("X1")));

			result = SourceVerifier.Compile(@"
class G<T> {}
[System.Runtime.CompilerServices.NonScriptable] class X1 {}

public class C {
	public static void M(object o) {
		var x = (G<X1>)o;
	}
}", expectErrors: true);
			Assert.That(result.Item2.AllMessages.Any(m => m.Code == 7522 && m.FormattedMessage.Contains("X1")));
		}
	}
}
