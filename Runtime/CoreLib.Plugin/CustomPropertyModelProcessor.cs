using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.Decorators;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.ScriptSemantics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace CoreLib.Plugin
{
    public class CustomPropertyModelProcessor : MetadataImporterDecoratorBase, IJSTypeSystemRewriter, IRuntimeContext
    {
        private readonly IErrorReporter _errorReporter;
        private readonly IRuntimeLibrary _runtimeLibrary;
        private readonly ICompilation _compilation;
        private readonly INamer _namer;
        private readonly bool _minimizeNames;
        private readonly Dictionary<IProperty, string> _modelProperties = new Dictionary<IProperty, string>();

        public CustomPropertyModelProcessor(IMetadataImporter prev, IErrorReporter errorReporter, IRuntimeLibrary runtimeLibrary, ICompilation compilation, INamer namer, CompilerOptions options)
            : base(prev)
        {
            _errorReporter = errorReporter;
            _runtimeLibrary = runtimeLibrary;
            _compilation = compilation;
            _namer = namer;
            _minimizeNames = options.MinimizeScript;
        }

        void PrepareModelProperty(IProperty p, CustomPropertyModelAttribute attribute)
        {
            var preferredName = MetadataUtils.DeterminePreferredMemberName(p, _minimizeNames);
            string name = preferredName.Item2 ? preferredName.Item1 : MetadataUtils.GetUniqueName(preferredName.Item1, n => IsMemberNameAvailable(p.DeclaringTypeDefinition, n, false));
            base.ReserveMemberName(p.DeclaringTypeDefinition, name, false);

            var getter = attribute.GetGetter(name, p.Name);
            var getterSemantics = getter != null
                ? MethodScriptSemantics.InlineCode(getter)
                : MethodScriptSemantics.NormalMethod(name, generateCode: false);
            var setter = attribute.GetSetter(name, p.Name);
            var setterSemantics = setter != null
                ? MethodScriptSemantics.InlineCode(setter)
                : MethodScriptSemantics.NormalMethod(name, generateCode: false);
            base.SetPropertySemantics(p, PropertyScriptSemantics.GetAndSetMethods(getterSemantics, setterSemantics));
            _modelProperties[p] = attribute.GetInitializer(name, p.Name);
        }

        public override void Prepare(ITypeDefinition type)
        {
            var attribute = type.ReadAttributeEx<CustomPropertyModelAttribute>();
            if (attribute != null)
            {
                foreach (var p in type.Properties.Where(p => p.IsAutoProperty() != false && !p.IsStatic))
                {
                    PrepareModelProperty(p, attribute);
                }
            }

            base.Prepare(type);
        }

        JsType InitializeModelProperties(JsType type)
        {
            var c = type as JsClass;
            if (c == null)
                return type;

            return c.PrependInitializers(type.CSharpTypeDefinition.Properties.Select(p =>
            {
                string code;
                if (!_modelProperties.TryGetValue(p, out code) || code == null)
                    return null;
                return JsStatement.Expression(c.Compile(code, _runtimeLibrary, _compilation, this, _errorReporter));
            }));
        }

        public IEnumerable<JsType> Rewrite(IEnumerable<JsType> types)
        {
            return types.Select(InitializeModelProperties);
        }

        public JsExpression ResolveTypeParameter(ITypeParameter tp)
        {
            return JsExpression.Identifier(_namer.GetTypeParameterName(tp));
        }

        public JsExpression EnsureCanBeEvaluatedMultipleTimes(JsExpression expression, IList<JsExpression> expressionsThatMustBeEvaluatedBefore)
        {
            throw new NotSupportedException();
        }
    }
}
