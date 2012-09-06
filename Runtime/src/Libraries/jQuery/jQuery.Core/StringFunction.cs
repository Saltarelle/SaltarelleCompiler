// StringFunction.cs
// Script#/Libraries/jQuery/Core
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System;
using System.Html;
using System.Runtime.CompilerServices;

namespace jQueryApi {

    /// <summary>
    /// A callback that returns a value for the element at the specified index.
    /// </summary>
    /// <param name="index">The index of the element in the set.</param>
    public delegate string StringFunction(int index);

    /// <summary>
    /// A callback that returns a value for the element at the specified index.
    /// </summary>
    /// <param name="element">Element for which the function is being invoked, in script represented as 'this'.</param>
    /// <param name="index">The index of the element in the set.</param>
    [BindThisToFirstParameter]
    public delegate string StringFunctionWithContext(Element element, int index);

    /// <summary>
    /// A callback that returns a value for the element at the specified index.
    /// </summary>
    /// <param name="index">The index of the element in the set.</param>
    /// <param name="currentValue">The current value.</param>
    public delegate string StringReplaceFunction(int index, string currentValue);

    /// <summary>
    /// A callback that returns a value for the element at the specified index.
    /// </summary>
    /// <param name="element">Element for which the function is being invoked, in script represented as 'this'.</param>
    /// <param name="index">The index of the element in the set.</param>
    /// <param name="currentValue">The current value.</param>
    [BindThisToFirstParameter]
	public delegate string StringReplaceFunctionWithContext(Element element, int index, string currentValue);
}
