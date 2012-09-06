// AjaxCallback.cs
// Script#/Libraries/jQuery/Core
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System;
using System.Html;
using System.Net;
using System.Runtime.CompilerServices;

namespace jQueryApi {
    /// <summary>
    /// An action that is invoked with an element being the Javascript 'this' context.
    /// </summary>
    /// <param name="elem">The element context ('this' in script).</param>
    [BindThisToFirstParameter]
    public delegate void ActionWithContext(Element elem);
}
