// TypeOption.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System
{
    /// <summary>
    /// Use this type when interacting with external libraries that accept arguments that can be of different types.
    /// </summary>
	[Imported, IgnoreNamespace, ScriptName("Object"), IgnoreGenericArguments]
	public sealed class TypeOption<T1, T2> {
        [ScriptSkip] public static implicit operator TypeOption<T1, T2>(T1 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2>(T2 t) { return null; }

        [ScriptSkip] public static explicit operator T1(TypeOption<T1, T2> o) { return default(T1); }
        [ScriptSkip] public static explicit operator T2(TypeOption<T1, T2> o) { return default(T2); }
    }

    /// <summary>
    /// Use this type when interacting with external libraries that accept arguments that can be of different types.
    /// </summary>
	[Imported, IgnoreNamespace, ScriptName("Object"), IgnoreGenericArguments]
	public sealed class TypeOption<T1, T2, T3> {
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3>(T1 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3>(T2 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3>(T3 t) { return null; }

        [ScriptSkip] public static explicit operator T1(TypeOption<T1, T2, T3> o) { return default(T1); }
        [ScriptSkip] public static explicit operator T2(TypeOption<T1, T2, T3> o) { return default(T2); }
        [ScriptSkip] public static explicit operator T3(TypeOption<T1, T2, T3> o) { return default(T3); }
    }

    /// <summary>
    /// Use this type when interacting with external libraries that accept arguments that can be of different types.
    /// </summary>
	[Imported, IgnoreNamespace, ScriptName("Object"), IgnoreGenericArguments]
	public sealed class TypeOption<T1, T2, T3, T4> {
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4>(T1 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4>(T2 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4>(T3 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4>(T4 t) { return null; }

        [ScriptSkip] public static explicit operator T1(TypeOption<T1, T2, T3, T4> o) { return default(T1); }
        [ScriptSkip] public static explicit operator T2(TypeOption<T1, T2, T3, T4> o) { return default(T2); }
        [ScriptSkip] public static explicit operator T3(TypeOption<T1, T2, T3, T4> o) { return default(T3); }
        [ScriptSkip] public static explicit operator T4(TypeOption<T1, T2, T3, T4> o) { return default(T4); }
    }

    /// <summary>
    /// Use this type when interacting with external libraries that accept arguments that can be of different types.
    /// </summary>
	[Imported, IgnoreNamespace, ScriptName("Object"), IgnoreGenericArguments]
	public sealed class TypeOption<T1, T2, T3, T4, T5> {
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4, T5>(T1 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4, T5>(T2 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4, T5>(T3 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4, T5>(T4 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4, T5>(T5 t) { return null; }

        [ScriptSkip] public static explicit operator T1(TypeOption<T1, T2, T3, T4, T5> o) { return default(T1); }
        [ScriptSkip] public static explicit operator T2(TypeOption<T1, T2, T3, T4, T5> o) { return default(T2); }
        [ScriptSkip] public static explicit operator T3(TypeOption<T1, T2, T3, T4, T5> o) { return default(T3); }
        [ScriptSkip] public static explicit operator T4(TypeOption<T1, T2, T3, T4, T5> o) { return default(T4); }
        [ScriptSkip] public static explicit operator T5(TypeOption<T1, T2, T3, T4, T5> o) { return default(T5); }
    }

    /// <summary>
    /// Use this type when interacting with external libraries that accept arguments that can be of different types.
    /// </summary>
	[Imported, IgnoreNamespace, ScriptName("Object"), IgnoreGenericArguments]
	public sealed class TypeOption<T1, T2, T3, T4, T5, T6> {
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4, T5, T6>(T1 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4, T5, T6>(T2 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4, T5, T6>(T3 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4, T5, T6>(T4 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4, T5, T6>(T5 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4, T5, T6>(T6 t) { return null; }

        [ScriptSkip] public static explicit operator T1(TypeOption<T1, T2, T3, T4, T5, T6> o) { return default(T1); }
        [ScriptSkip] public static explicit operator T2(TypeOption<T1, T2, T3, T4, T5, T6> o) { return default(T2); }
        [ScriptSkip] public static explicit operator T3(TypeOption<T1, T2, T3, T4, T5, T6> o) { return default(T3); }
        [ScriptSkip] public static explicit operator T4(TypeOption<T1, T2, T3, T4, T5, T6> o) { return default(T4); }
        [ScriptSkip] public static explicit operator T5(TypeOption<T1, T2, T3, T4, T5, T6> o) { return default(T5); }
        [ScriptSkip] public static explicit operator T6(TypeOption<T1, T2, T3, T4, T5, T6> o) { return default(T6); }
    }

    /// <summary>
    /// Use this type when interacting with external libraries that accept arguments that can be of different types.
    /// </summary>
	[Imported, IgnoreNamespace, ScriptName("Object"), IgnoreGenericArguments]
	public sealed class TypeOption<T1, T2, T3, T4, T5, T6, T7> {
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4, T5, T6, T7>(T1 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4, T5, T6, T7>(T2 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4, T5, T6, T7>(T3 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4, T5, T6, T7>(T4 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4, T5, T6, T7>(T5 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4, T5, T6, T7>(T6 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4, T5, T6, T7>(T7 t) { return null; }

        [ScriptSkip] public static explicit operator T1(TypeOption<T1, T2, T3, T4, T5, T6, T7> o) { return default(T1); }
        [ScriptSkip] public static explicit operator T2(TypeOption<T1, T2, T3, T4, T5, T6, T7> o) { return default(T2); }
        [ScriptSkip] public static explicit operator T3(TypeOption<T1, T2, T3, T4, T5, T6, T7> o) { return default(T3); }
        [ScriptSkip] public static explicit operator T4(TypeOption<T1, T2, T3, T4, T5, T6, T7> o) { return default(T4); }
        [ScriptSkip] public static explicit operator T5(TypeOption<T1, T2, T3, T4, T5, T6, T7> o) { return default(T5); }
        [ScriptSkip] public static explicit operator T6(TypeOption<T1, T2, T3, T4, T5, T6, T7> o) { return default(T6); }
        [ScriptSkip] public static explicit operator T7(TypeOption<T1, T2, T3, T4, T5, T6, T7> o) { return default(T7); }
    }

    /// <summary>
    /// Use this type when interacting with external libraries that accept arguments that can be of different types.
    /// </summary>
	[Imported, IgnoreNamespace, ScriptName("Object"), IgnoreGenericArguments]
	public sealed class TypeOption<T1, T2, T3, T4, T5, T6, T7, T8> {
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4, T5, T6, T7, T8>(T1 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4, T5, T6, T7, T8>(T2 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4, T5, T6, T7, T8>(T3 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4, T5, T6, T7, T8>(T4 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4, T5, T6, T7, T8>(T5 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4, T5, T6, T7, T8>(T6 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4, T5, T6, T7, T8>(T7 t) { return null; }
        [ScriptSkip] public static implicit operator TypeOption<T1, T2, T3, T4, T5, T6, T7, T8>(T8 t) { return null; }

        [ScriptSkip] public static explicit operator T1(TypeOption<T1, T2, T3, T4, T5, T6, T7, T8> o) { return default(T1); }
        [ScriptSkip] public static explicit operator T2(TypeOption<T1, T2, T3, T4, T5, T6, T7, T8> o) { return default(T2); }
        [ScriptSkip] public static explicit operator T3(TypeOption<T1, T2, T3, T4, T5, T6, T7, T8> o) { return default(T3); }
        [ScriptSkip] public static explicit operator T4(TypeOption<T1, T2, T3, T4, T5, T6, T7, T8> o) { return default(T4); }
        [ScriptSkip] public static explicit operator T5(TypeOption<T1, T2, T3, T4, T5, T6, T7, T8> o) { return default(T5); }
        [ScriptSkip] public static explicit operator T6(TypeOption<T1, T2, T3, T4, T5, T6, T7, T8> o) { return default(T6); }
        [ScriptSkip] public static explicit operator T7(TypeOption<T1, T2, T3, T4, T5, T6, T7, T8> o) { return default(T7); }
        [ScriptSkip] public static explicit operator T8(TypeOption<T1, T2, T3, T4, T5, T6, T7, T8> o) { return default(T8); }
    }
}
