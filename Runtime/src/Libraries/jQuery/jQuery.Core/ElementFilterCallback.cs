// ElementFilterCallback.cs
// Script#/Libraries/jQuery/Core
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System;
using System.Html;
using System.Runtime.CompilerServices;

namespace jQueryApi {

    /// <summary>
    /// A callback to be invoked for each element in a jQueryObject being filtered.
    /// </summary>
    /// <param name="index">The index of the element in the matching set.</param>
    /// <returns>true if the element should be included; false otherwise.</returns>
    public delegate bool ElementFilterCallback(int index);

    /// <summary>
    /// A callback to be invoked for each element in a jQueryObject being filtered, with the Javascript 'this' context promoted to a parameter.
    /// </summary>
    /// <param name="element">Element in the matching set ('this' in script).</param>
    /// <param name="index">The index of the element in the matching set.</param>
    /// <returns>true if the element should be included; false otherwise.</returns>
    [BindThisToFirstParameter]
    public delegate bool ElementFilterCallbackWithContext(Element element, int index);
}
