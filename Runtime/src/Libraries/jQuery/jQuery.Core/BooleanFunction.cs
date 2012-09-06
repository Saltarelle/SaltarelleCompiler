// BooleanFunction.cs
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
    public delegate bool BooleanFunction(int index);

    /// <summary>
    /// A callback that returns a value for the element at the specified index, with the Javascript 'this' context promoted to a parameter.
    /// </summary>
    /// <param name="element">Element in the set ('this' in script).</param>
    /// <param name="index">The index of the element in the set.</param>
    [BindThisToFirstParameter]
    public delegate bool BooleanFunctionWithContext(Element element, int index);
}
