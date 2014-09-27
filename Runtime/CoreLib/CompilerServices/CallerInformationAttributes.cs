using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Runtime.CompilerServices {
	[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
	[NonScriptable]
	public class CallerLineNumberAttribute : Attribute {
	}

	[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
	[NonScriptable]
	public class CallerFilePathAttribute : Attribute {
	}

	[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
	[NonScriptable]
	public class CallerMemberNameAttribute : Attribute {
	}
}
