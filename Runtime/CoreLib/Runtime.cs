// Runtime.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace System {

	[AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
	[NonScriptable]
	public sealed class FlagsAttribute : Attribute {
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public abstract class MarshalByRefObject {
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Imported]
	[IgnoreNamespace]
	[ScriptName("Object")]
	public abstract class ValueType {
		[ScriptSkip]
		protected ValueType() {
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public struct IntPtr {
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public struct UIntPtr {
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public struct RuntimeTypeHandle {
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public struct RuntimeFieldHandle {
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public struct RuntimeMethodHandle {
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public sealed class ParamArrayAttribute : Attribute {
	}

	[AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Interface | AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class, Inherited = false)]
	[NonScriptable]
	public sealed class ObsoleteAttribute : Attribute {

		private bool _error;
		private string _message;

		public ObsoleteAttribute() {
		}

		public ObsoleteAttribute(string message) {
			_message = message;
		}

		public ObsoleteAttribute(string message, bool error) {
			_message = message;
			_error = error;
		}

		public bool IsError {
			get {
				return _error;
			}
		}

		public string Message {
			get {
				return _message;
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
	[NonScriptable]
	public sealed class CLSCompliantAttribute : Attribute {

		private bool _isCompliant;

		public CLSCompliantAttribute(bool isCompliant) {
			_isCompliant = isCompliant;
		}

		public bool IsCompliant {
			get {
				return _isCompliant;
			}
		}
	}
}

namespace System.CodeDom.Compiler {
	[EditorBrowsable(EditorBrowsableState.Never)]
	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
	[NonScriptable]
	public sealed class GeneratedCodeAttribute : Attribute {

		private string _tool;
		private string _version;

		public GeneratedCodeAttribute(string tool, string version) {
			_tool = tool;
			_version = version;
		}

		public string Tool {
			get {
				return _tool;
			}
		}

		public string Version {
			get {
				return _version;
			}
		}
	}
}

namespace System.ComponentModel {
	/// <summary>
	/// This attribute marks a field, property, event or method as
	/// "browsable", i.e. present in the type descriptor associated with
	/// the type.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	[NonScriptable]
	public sealed class BrowsableAttribute : Attribute {
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Delegate | AttributeTargets.Interface)]
	[NonScriptable]
	public sealed class EditorBrowsableAttribute : Attribute {

		private EditorBrowsableState _browsableState;

		public EditorBrowsableAttribute(EditorBrowsableState state) {
			_browsableState = state;
		}

		public EditorBrowsableState State {
			get {
				return _browsableState;
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public enum EditorBrowsableState {
		Always = 0,
		Never = 1,
		Advanced = 2
	}
}

namespace System.Reflection {
	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public sealed class DefaultMemberAttribute {

		private string _memberName;

		public DefaultMemberAttribute(string memberName) {
			_memberName = memberName;
		}

		public string MemberName {
			get {
				return _memberName;
			}
		}
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[NonScriptable]
	public sealed class AssemblyCopyrightAttribute : Attribute {
		private string _copyright;

		public AssemblyCopyrightAttribute(string copyright) {
			_copyright = copyright;
		}

		public string Copyright {
			get {
				return _copyright;
			}
		}
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[NonScriptable]
	public sealed class AssemblyTrademarkAttribute : Attribute {
		private string _trademark;

		public AssemblyTrademarkAttribute(string trademark) {
			_trademark = trademark;
		}

		public string Trademark {
			get {
				return _trademark;
			}
		}
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[NonScriptable]
	public sealed class AssemblyProductAttribute : Attribute {
		private string _product;

		public AssemblyProductAttribute(string product) {
			_product = product;
		}

		public string Product {
			get {
				return _product;
			}
		}
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[NonScriptable]
	public sealed class AssemblyCompanyAttribute : Attribute {
		private string _company;

		public AssemblyCompanyAttribute(string company) {
			_company = company;
		}

		public string Company {
			get {
				return _company;
			}
		}
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[NonScriptable]
	public sealed class AssemblyDescriptionAttribute : Attribute {
		private string _description;

		public AssemblyDescriptionAttribute(string description) {
			_description = description;
		}

		public string Description {
			get {
				return _description;
			}
		}
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[NonScriptable]
	public sealed class AssemblyTitleAttribute : Attribute {
		private string _title;

		public AssemblyTitleAttribute(string title) {
			_title = title;
		}

		public string Title {
			get {
				return _title;
			}
		}
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[NonScriptable]
	public sealed class AssemblyConfigurationAttribute : Attribute {
		private string _configuration;

		public AssemblyConfigurationAttribute(string configuration) {
			_configuration = configuration;
		}

		public string Configuration {
			get {
				return _configuration;
			}
		}
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[NonScriptable]
	public sealed class AssemblyFileVersionAttribute : Attribute {
		private string _version;

		public AssemblyFileVersionAttribute(string version) {
			_version = version;
		}

		public string Version {
			get {
				return _version;
			}
		}
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[NonScriptable]
	public sealed class AssemblyInformationalVersionAttribute : Attribute {
		private string _informationalVersion;

		public AssemblyInformationalVersionAttribute(string informationalVersion) {
			_informationalVersion = informationalVersion;
		}

		public string InformationalVersion {
			get {
				return _informationalVersion;
			}
		}
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[NonScriptable]
	public sealed class AssemblyCultureAttribute : Attribute {
		private string _culture;

		public AssemblyCultureAttribute(string culture) {
			_culture = culture;
		}

		public string Culture {
			get {
				return _culture;
			}
		}
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[NonScriptable]
	public sealed class AssemblyVersionAttribute : Attribute {
		private string _version;

		public AssemblyVersionAttribute(string version) {
			_version = version;
		}

		public string Version {
			get {
				return _version;
			}
		}
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[NonScriptable]
	public sealed class AssemblyKeyFileAttribute : Attribute {
		private string _keyFile;

		public AssemblyKeyFileAttribute(string keyFile) {
			_keyFile = keyFile;
		}

		public string KeyFile {
			get {
				return _keyFile;
			}
		}
	}

	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[NonScriptable]
	public sealed class AssemblyDelaySignAttribute : Attribute {
		private bool _delaySign;

		public AssemblyDelaySignAttribute(bool delaySign) {
			_delaySign = delaySign;
		}

		public bool DelaySign {
			get {
				return _delaySign;
			}
		}
	}
}

namespace System.Runtime.CompilerServices {
	[AttributeUsage(AttributeTargets.All, Inherited = true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public sealed class CompilerGeneratedAttribute : Attribute {
	}

	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field, Inherited = false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public sealed class DecimalConstantAttribute : Attribute {
		public DecimalConstantAttribute(byte scale, byte sign, int hi, int mid, int low) {
		}

		public DecimalConstantAttribute(byte scale, byte sign, uint hi, uint mid, uint low) {
		}

		public decimal Value {
			get {
				return 0m;
			}
		}
	}

	[AttributeUsage(AttributeTargets.Assembly|AttributeTargets.Class|AttributeTargets.Method)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public sealed class ExtensionAttribute : Attribute {
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public sealed class DynamicAttribute : Attribute {
		public IList<bool> TransformFlags { get { return null; } }
		public DynamicAttribute() {}
		public DynamicAttribute(bool[] transformFlags) {}
	}

	[AttributeUsage(AttributeTargets.Field, Inherited = false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public sealed class FixedBufferAttribute : Attribute {
		public Type ElementType { get { return null; } }
		public int Length { get { return 0; } }
		public FixedBufferAttribute(Type elementType, int length) {}
	}

	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public sealed class RuntimeCompatibilityAttribute : Attribute {
		public bool WrapNonExceptionThrows { get; set; }
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public class CallSite {
		public CallSiteBinder Binder { get { return null; } }

		public static CallSite Create(Type delegateType, CallSiteBinder binder) {
			return null;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public sealed class CallSite<T> : CallSite where T : class {
		public T Update { get { return null; } }
		public T Target;

		public static CallSite<T> Create(CallSiteBinder binder) {
			return null;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public abstract class CallSiteBinder {
		public static LabelTarget UpdateLabel { get { return null; } }

		public virtual T BindDelegate<T>(CallSite<T> site, object[] args) where T : class {
			return null;
		}
	}

	[NonScriptable]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[AttributeUsage(AttributeTargets.Property)]
	public class IndexerNameAttribute : Attribute {
		public IndexerNameAttribute(string indexerName) {
			this.Value = indexerName;
		}
		public string Value { get; private set; }
	}

	[NonScriptable]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public struct AsyncVoidMethodBuilder {
		public static AsyncVoidMethodBuilder Create(){
			return default(AsyncVoidMethodBuilder);
		}

		public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine {
		}

		public void SetStateMachine(IAsyncStateMachine stateMachine) {
		}

		public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine {
		}

		public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine {
		}

		public void SetResult() {
		}

		public void SetException(Exception exception) {
		}
	}

	[NonScriptable]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public struct AsyncTaskMethodBuilder {
		public Task Task { get { return null; } }

		public static AsyncTaskMethodBuilder Create() {
			return default(AsyncTaskMethodBuilder);
		}

		public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine {
		}

		public void SetStateMachine(IAsyncStateMachine stateMachine) {
		}

		public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine {
		}

		public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine {
		}

		public void SetResult() {
		}

		public void SetException(Exception exception) {
		}
	}

	[NonScriptable]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public struct AsyncTaskMethodBuilder<TResult> {
		public Task<TResult> Task { get { return null; } }

		public static AsyncTaskMethodBuilder<TResult> Create() {
			return default(AsyncTaskMethodBuilder<TResult>);
		}

		public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine {
		}

		public void SetStateMachine(IAsyncStateMachine stateMachine) {
		}

		public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine {
		}

		public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine {
		}

		public void SetResult(TResult result) {
		}

		public void SetException(Exception exception) {
		}
	}
  
	[NonScriptable]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IAsyncStateMachine {
		void MoveNext();
		void SetStateMachine(IAsyncStateMachine stateMachine);
	}

	[NonScriptable]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface INotifyCompletion {
		void OnCompleted(Action continuation);
	}

	[NonScriptable]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface ICriticalNotifyCompletion : INotifyCompletion {
		void UnsafeOnCompleted(Action continuation);
	}
}

namespace System.Runtime.InteropServices {
	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public class OutAttribute {
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public sealed class StructLayoutAttribute : Attribute {
		public int Pack;
		public int Size;
		public CharSet CharSet;

		public LayoutKind Value { get { return LayoutKind.Auto; } }

		public StructLayoutAttribute(LayoutKind layoutKind) {}
		public StructLayoutAttribute(short layoutKind) {}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public enum CharSet {
		None = 1,
		Ansi = 2,
		Unicode = 3,
		Auto = 4,
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public enum LayoutKind {
		Sequential = 0,
		Explicit = 2,
		Auto = 3,
	}
}

namespace System.Runtime.Versioning {
	[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
	[NonScriptable]
	public sealed class TargetFrameworkAttribute : Attribute {
		private string _frameworkName;
		private string _frameworkDisplayName;

		public TargetFrameworkAttribute(string frameworkName) {
			_frameworkName = frameworkName;
		}

		public string FrameworkDisplayName {
			get {
				return _frameworkDisplayName;
			}
			set {
				_frameworkDisplayName = value;
			}
		}

		public string FrameworkName {
			get {
				return _frameworkName;
			}
		}
	}
}

namespace System.Threading {
	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public static class Interlocked {
		public static int CompareExchange(ref int location1, int value, int comparand) {
			return 0;
		}

		public static T CompareExchange<T>(ref T location1, T value, T comparand) where T : class {
			return null;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public static class Monitor {
		public static void Enter(object obj) {
		}

		public static void Enter(object obj, ref bool b) {
		}

		public static void Exit(object obj) {
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public class Thread {
		public int ManagedThreadId { get { return 0; } }
		public static Thread CurrentThread { get { return null; } }
	}
}

namespace System.Security.Permissions {
	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public enum SecurityAction {
		Demand = 2,
		Assert = 3,
		Deny = 4,
		PermitOnly = 5,
		LinkDemand = 6,
		InheritanceDemand = 7,
		RequestMinimum = 8,
		RequestOptional = 9,
		RequestRefuse = 10,
	}
}

namespace Microsoft.CSharp.RuntimeBinder {
	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public static class Binder {
		public static CallSiteBinder BinaryOperation(CSharpBinderFlags flags, ExpressionType operation, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo) {
			return null;
		}

		public static CallSiteBinder Convert(CSharpBinderFlags flags, Type type, Type context) {
			return null;
		}

		public static CallSiteBinder GetIndex(CSharpBinderFlags flags, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo) {
			return null;
		}

		public static CallSiteBinder GetMember(CSharpBinderFlags flags, string name, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo) {
			return null;
		}

		public static CallSiteBinder Invoke(CSharpBinderFlags flags, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo) {
			return null;
		}

		public static CallSiteBinder InvokeMember(CSharpBinderFlags flags, string name, IEnumerable<Type> typeArguments, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo) {
			return null;
		}

		public static CallSiteBinder InvokeConstructor(CSharpBinderFlags flags, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo) {
			return null;
		}

		public static CallSiteBinder IsEvent(CSharpBinderFlags flags, string name, Type context) {
			return null;
		}

		public static CallSiteBinder SetIndex(CSharpBinderFlags flags, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo) {
			return null;
		}

		public static CallSiteBinder SetMember(CSharpBinderFlags flags, string name, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo) {
			return null;
		}

		public static CallSiteBinder UnaryOperation(CSharpBinderFlags flags, ExpressionType operation, Type context, IEnumerable<CSharpArgumentInfo> argumentInfo) {
			return null;
		}
	}

	[Flags]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public enum CSharpBinderFlags {
		None = 0,
		CheckedContext = 1,
		InvokeSimpleName = 2,
		InvokeSpecialName = 4,
		BinaryOperationLogical = 8,
		ConvertExplicit = 16,
		ConvertArrayIndex = 32,
		ResultIndexed = 64,
		ValueFromCompoundAssignment = 128,
		ResultDiscarded = 256,
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public sealed class CSharpArgumentInfo {
		public static CSharpArgumentInfo Create(CSharpArgumentInfoFlags flags, string name) {
			return null;
		}
	}

	[Flags]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[NonScriptable]
	public enum CSharpArgumentInfoFlags {
		None = 0,
		UseCompileTimeType = 1,
		Constant = 2,
		NamedArgument = 4,
		IsRef = 8,
		IsOut = 16,
		IsStaticType = 32,
	}
}

namespace System.Diagnostics {
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, Inherited = false)]
	[NonScriptable]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public sealed class DebuggerStepThroughAttribute : Attribute {
	}
}