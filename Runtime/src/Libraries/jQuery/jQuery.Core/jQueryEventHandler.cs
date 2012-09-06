// jQueryEventHandler.cs
// Script#/Libraries/jQuery/Core
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System;
using System.Html;
using System.Runtime.CompilerServices;

namespace jQueryApi {

    /// <summary>
    /// Handles a jQuery event.
    /// </summary>
    public delegate void jQueryEventHandler(jQueryEvent e);

	/// <summary>
	/// Handles a jQuery event, and promotes the 'this' in Javascript to a parameter.
	/// </summary>
	[BindThisToFirstParameter]
	public delegate void jQueryEventHandlerWithContext(Element elem, jQueryEvent e);
}
