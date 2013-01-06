using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace PaperJsGenerator
{
    class Program
    {
        public static string StripHtml(string source)
        {
            var array = new char[source.Length];
            int arrayIndex = 0;
            bool inside = false;

            foreach (char c in source)
                if (c == '<')
                    inside = true;
                else if (c == '>')
                    inside = false;
                else if (!inside)
                    array[arrayIndex++] = c;

            return new string(array, 0, arrayIndex);
        }

        private static string logText = "";

        static void Log(string format = null, params object[] args)
        {
            var formatted = format == null ? "" : args.Length == 0 ? format : String.Format(format, args);
            Console.WriteLine(formatted);
            logText += formatted + "\n";
        }

        private static string errorText = "";

        static void ErrorLog(string format = null, params object[] args)
        {
            var formatted = format == null ? "" : args.Length == 0 ? format : String.Format(format, args);
            
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(formatted);
            Console.ForegroundColor = oldColor;

            errorText += formatted + "\n";
        }

        class Downloader
        {
            private string CacheDir { get; set; }
            readonly WebClient wc = new WebClient();

            public Downloader(string cacheDir)
            {
                CacheDir = cacheDir;
            }

            public string Get(string urlPart)
            {
                var cacheFn = urlPart.Replace("/", "_").Replace("?", "_");
                if (cacheFn.EndsWith("_"))
                    cacheFn = cacheFn.Substring(0, cacheFn.Length - 1);
                cacheFn = CacheDir + cacheFn + ".html";

                if (File.Exists(cacheFn))
                    return File.ReadAllText(cacheFn);
                
                try
                {
                    var content = wc.DownloadString("http://paperjs.org/" + urlPart);
                    File.WriteAllText(cacheFn, content);
                    return content;
                }
                catch
                {
                    return "";
                }
            }
        }

        private static Downloader downloader;

        static ClassData[] DownloadClassDatas(string prgPath)
        {
            var refPath = Utils.ProvidePath(prgPath + @"\doc\reference\");
            var docStaticPath = Utils.ProvidePath(prgPath + @"\doc\static\");

            downloader = new Downloader(Utils.ProvidePath(prgPath + @"\cache\"));
            Func<string, string> download = urlPart => downloader.Get(urlPart);

            Log("Downloading Reference page...");
            var classDatas = Regex.Matches(download("reference"), @"<a href=""/reference/([^""]+)"">([^<]+)").ToArray().
                Select(x => new ClassData { UrlName = x[1], Name = x[2].Replace(" ", "") }).ToArray();

            // save to ref path
            foreach (var classData in classDatas)
            {
                var outFn = refPath + classData.Name + ".html";
                if (File.Exists(outFn))
                {
                    classData.HtmlContent = File.ReadAllText(outFn);
                    continue;                    
                }

                Log("Downloading and saving class page: {0}...", classData.Name);
                var content = download(classData.DownloadName);

                var staticContentUrls = Regex.Matches(content, @"(?:href|src)=""/(static/[^""]+)").ToSingleMatchArray();
                foreach (var scUrl in staticContentUrls)
                {
                    var scName = Regex.Match(scUrl, @"([^/?]+)(\?.*)?$").Groups[1].Value;
                    var scFn = docStaticPath + scName;
                    if (!File.Exists(scFn))
                        File.WriteAllText(scFn, download(scUrl));

                    content = content.Replace("/" + scUrl, "../static/" + scName);
                }

                content = Regex.Replace(content, @"(?<="")/reference/([^""]+)", @"$1.html");

                File.WriteAllText(outFn, content);
                classData.HtmlContent = content;
            }

            return classDatas;
        }

        static string RemoveNewLines(string text)
        {
            return text == null ? null : text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n').Select(x => x.Trim()).Join("\n").Replace("\n", " ").Trim().Replace("  ", " ");
        }

        static void ClassDataJsProcess(ClassData classData)
        {
            const RegexOptions defOptions = RegexOptions.Singleline | RegexOptions.IgnoreCase;
            Func<string, string, string> quickMatch = (c, pattern) =>
            {
                var g = Regex.Match(c, pattern, defOptions).Groups[1];
                return g.Success ? g.Value : null;
            };
            //Func<string, string, string[]> sepMatch = (c, separator) => Regex.Matches(c, String.Format(@"{0}(.*?)(?={0}|$)", separator), defOptions).ToSingleMatchArray();
            Func<string, string, string, string[]> sepMatchTwo = (c, sep1, sep2) => Regex.Matches(c, String.Format(@"{0}(.*?)(?={1}|$)", sep1, sep2), defOptions).ToSingleMatchArray();
            Func<string, string, string[][]> matchAll = (c, pattern) => Regex.Matches(c, pattern, defOptions).ToArray();

            var content = classData.HtmlContent;

            content = quickMatch(content, "id=.content(.*?)class=.reference-end");
            classData.NameOnPage = quickMatch(content, "<h1>(.*?)</h1>");
            classData.Description = quickMatch(content, "reference-class.>(.*?)</div>");
            
            var descMatch = matchAll(classData.Description, "<p> Extends (.*?)</p>(.*)");
            if (descMatch.Length > 0)
            {
                classData.Description = descMatch[0][2];
                classData.Extends = matchAll(descMatch[0][1], "<tt>(.*?)</tt>").Select(x => x[1]).ToArray();                
            }

            classData.Members = sepMatchTwo(content, @"class=.member-header", @"class=""member(-header)?""").Select(x => new ClassMember { Class = classData, HtmlContent = x }).ToArray();
            foreach (var member in classData.Members)
            {
                member.JsName = quickMatch(member.HtmlContent, "<tt><b>([^<]+)");
                member.ParameterList = quickMatch(member.HtmlContent, @"<tt><b>[^<]+</b>\(([^)]*)");
                member.OperandTypeJs = quickMatch(member.HtmlContent, @"<tt><b>[^<]+</b> ([^<]+)");

                if (member.OperandTypeJs != null)
                {
                    member.Type = MemberTypes.Operator;
                    member.Operator = OperatorsList.FirstOrDefault(op => op.Symbol == member.JsName);
                }
                else if (member.ParameterList != null)
                    if (member.JsName == member.Class.Name)
                        member.Type = MemberTypes.Constructor;
                    else
                    {
                        if (member.JsName.StartsWith(member.Class.Name + "."))
                        {
                            member.JsName = member.JsName.Substring(member.Class.Name.Length + 1);
                            member.Type = MemberTypes.StaticMethod;
                            member.CallWithNew = member.JsName.Length >= 1 && Char.IsUpper(member.JsName[0]);
                        }
                        else
                            member.Type = MemberTypes.Method;
                    }
                else
                    member.Type = MemberTypes.Property;

                if (member.ParameterList != null)
                    member.Parameters = member.ParameterList.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).
                                               Select((x, i) => new MethodParameter { Name = x, Idx = i }).ToArray();

                member.Description = quickMatch(member.HtmlContent, @"<div class=.member-text.>\s*<p>(.*?)</p>\s+<ul");

                var paramsHtml = quickMatch(member.HtmlContent, @"<ul><b>Parameters:</b>(.*?)</ul>");
                if (paramsHtml != null)
                {
                    var paramsData = Regex.Matches(paramsHtml, @"<li>\s*(.*?)</li>", defOptions).ToSingleMatchArray();
                    member.Parameters2 = paramsData.Select((paramData, i) =>
                    {

                        var m = matchAll(paramData, @"<tt>([^:<]+):</tt>(.*?)\s*(?:&mdash;(?:&nbsp;)?(.*)|$)")[0];
                        var jsTypes = m[2] == null ? new string[]{ null } : matchAll(m[2], "<tt>([^<]+)</tt>").Select(y => y[1]).ToArray();
                        if(jsTypes.Length == 0)
                            jsTypes = new string[]{ null };

                        var mp = new MethodParameter { Idx = i, Name = m[1], JsTypes = jsTypes, Description = (m[3] ?? "").Trim(), 
                            IsArrayType = m[2] != null && m[2].ToLower().Contains("array of"), };
                        mp.IsOptional = mp.Description.ToLower().Contains("optional");
                        return mp;
                    }).ToArray();
                }

                if ((member.Type != MemberTypes.Operator && member.Type != MemberTypes.Property) && member.Parameters2 == null)
                    member.Parameters2 = new MethodParameter[0];

                var returnMatch = matchAll(member.HtmlContent, @"<ul><b>Returns:</b>.*?<tt>(.*?)<tt>([^<]+)</tt>.*?</tt>(?:&mdash;|&nbsp;)*(.*?)</li>");
                if (returnMatch.Length != 0)
                {
                    member.ReturnArrayType = returnMatch[0][1].ToLower().Contains("array of");
                    member.ReturnJsType = returnMatch[0][2];
                    member.ReturnDescription = returnMatch[0][3];
                }

                var propertyMatch = matchAll(member.HtmlContent, @"<ul><b>Type:</b>(.*?)<tt>(.*?)</tt>");
                if (propertyMatch.Length != 0)
                {
                    member.PropertyArrayType = propertyMatch[0][1].ToLower().Contains("array of");
                    member.PropertyJsType = propertyMatch[0][2];                    
                }
            }
        }

        static void FixErrors(ClassData classData)
        {
            foreach (var member in classData.Members)
            {
                member.OperandTypeJs = RemoveNewLines(member.OperandTypeJs);
                member.ReturnJsType = RemoveNewLines(member.ReturnJsType);
                member.PropertyJsType = RemoveNewLines(member.PropertyJsType);

                if (member.Parameters2 != null)
                {
                    foreach (var mp in member.Parameters2.Where(mp => mp.Name == "object"))
                        mp.Name = "_object";

                    foreach (var mp in member.Parameters2.Where(mp => String.IsNullOrWhiteSpace(mp.JsType)))
                    {
                        mp.JsType = NameToTypeDict.GetValueOrDefault(mp.Name);
                        if (mp.JsType == null)
                        {
                            mp.JsType = "Object";
                            ErrorLog("Could not derive type from parameter name: {0}.{1} -> {2}", classData, member.JsName, mp.Name);
                        }
                    }                    
                }
            }
        }

        private static string UpFirst(string s)
        {
            return string.IsNullOrEmpty(s) ? "" : char.ToUpper(s[0]) + s.Substring(1);
        }

        static void FillCsData(ClassMember member)
        {
            Func<string, string> typeConvert = jsType => jsType == null ? null : TypeConvertDict.GetValueOrDefault(jsType, jsType);

            member.CsName = UpFirst(member.JsName);

            member.OperandTypeCs = typeConvert(member.OperandTypeJs);

            if ((member.Type == MemberTypes.Method || member.Type == MemberTypes.StaticMethod) && member.ReturnJsType == null)
                member.ReturnCsType = "void";
            else
                member.ReturnCsType = typeConvert(member.ReturnJsType) + (member.ReturnArrayType ? "[]" : "");

            if (member.PropertyJsType == "Function")
            {
                member.PropertyCsType = "Action<" + (member.Description.Contains("KeyEvent") ? "KeyEvent" :
                    member.Description.Contains("ToolEvent") ? "ToolEvent" : "Event") + ">";
            }
            else
                member.PropertyCsType = typeConvert(member.PropertyJsType) + (member.PropertyArrayType ? "[]" : "");

            if (member.Parameters2 != null)
                foreach (var mp in member.Parameters2)
                    mp.CsType = typeConvert(mp.JsType) + (mp.IsArrayType ? "[]" : "");
        }

        static string IndentFix(string content)
        {
            Func<string, int> firstCharCol = str =>
            {
                var startColumn = 0;
                for (; startColumn < str.Length; startColumn++)
                    if (str[startColumn] != ' ' && str[startColumn] != '\t')
                        break;
                return startColumn;
            };

            while (content.StartsWith("\r") || content.StartsWith("\n"))
                content = content.Substring(1);

            content = content.Replace("\t", "    ");

            var lines = content.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None).
                            Select(line => new { line, firstCharCol = firstCharCol(line) }).ToArray();

            if (lines.Length == 0) return "";

            var defaultIndent = firstCharCol(lines[0].line);

            var lastIndent = defaultIndent;
            var newLines = lines.Select((x, i) =>
            {
                var res = new
                {
                    newIndent = Math.Max((x.firstCharCol == 0 ? lastIndent : x.firstCharCol - defaultIndent), 0),
                    unindentedLine = x.line.Trim()
                };
                lastIndent = res.newIndent;
                return res;
            }).ToArray();

            var newContent = newLines.Select(x => new string(' ', x.newIndent) + x.unindentedLine).Join(Environment.NewLine);
            return newContent;            
        }

        private const string FileHeaders =
            @"using System;
              using System.Html;
              using System.Runtime.CompilerServices;

              namespace PaperJs";

        static string GenerateSaltarelleCode(ClassData classData)
        {
            Func<string, string> lowerFirst = s => string.IsNullOrEmpty(s) ? "" : char.ToLower(s[0]) + s.Substring(1);

            Func<string, string> convertNewLines = str => str == null ? null : StripHtml(RemoveNewLines(str));
            Func<ClassMember, string> getComment = m =>
                "/// <summary>\n" +
                "/// " + convertNewLines(m.Description) + "\n" +
                "/// </summary>" +
                (m.Parameters2 == null || m.Parameters2.Length == 0 ? "" : "\n" + m.Parameters2.Select(p => @"/// <param name=""" + p.Name + @""">" + convertNewLines(p.Description) + @"</param>").Join("\n")) +
                (m.ReturnDescription == null ? "" : "\n" + @"/// <returns>" + convertNewLines(m.ReturnDescription) + "</returns>");

            var properties = classData.Members.Where(x => x.Type == MemberTypes.Property).ToArray();
            var constructors = classData.Members.Where(x => x.Type == MemberTypes.Constructor).ToArray();
            var operators = classData.Members.Where(x => x.Type == MemberTypes.Operator).ToArray();
            var methods = classData.Members.Where(x => x.Type == MemberTypes.Method).ToArray();
            var staticMethods = classData.Members.Where(x => x.Type == MemberTypes.StaticMethod).ToArray();

            var lowClassName = lowerFirst(classData.Name);

            Func<MethodParameter, string> genParamStr = p => p.CsType + " " + p.Name/* + (p.IsOptional ? " = default(" + p.CsType + ")" : "")*/;
            Func<ClassMember, string> genParamList = m => "(" + m.Parameters2.Select(genParamStr).Join(", ") + ")";
            Func<ClassMember, string> genBody = m => " " + ((m.ReturnCsType == "void" || m.ReturnCsType == null) ? "{ }" : "{ return default(" + m.ReturnCsType + @"); }");

            var saltarelleMetadata = @"
                " + FileHeaders + @"
                {
                    /// <summary>
                    /// " + convertNewLines(classData.Description) + @"
                    /// </summary>
                    [Imported, IgnoreNamespace]
                    public partial class " + classData.Name + (classData.InheritParent == null ? "" : " : " + classData.InheritParent.Name) + @"
                    {   " + (properties.Length == 0 ? "" : @"
                        #region Properties

                        " + properties.Select(x => @"
                        " + getComment(x) + @"
                        public " + x.PropertyCsType + " " + x.CsName + ";").Join("\n") + @"
                        
                        #endregion") + (constructors.Length == 0 ? "" : @"

                        #region Constructors" + (constructors.Any(x => x.Parameters2.Length == 0) ? "" : @"
        
                        /// <summary>
                        /// Constructor for enable inheritance
                        /// </summary>
                        protected " + classData.Name + @"(){ }") + @"

                        " + constructors.Select(m => getComment(m) + @"                        
                        [ScriptName("""")] 
                        public " + m.CsName + genParamList(m) + "{ }").Join("\n\n") + @" 
        
                        #endregion") + (operators.Length == 0 ? "" : @"

                        #region Operators

                        " + operators.Select(m => 
                        getComment(m) + @"
                        public " + m.ReturnCsType + @" " + m.Operator.Type + "(" + m.OperandTypeCs + " operand)" + genBody(m) +
                            (m.Operator.Type == OperatorTypes.Equals ? "" : @"

                        " + getComment(m) + @"
                        [InlineCode(""{" + lowClassName + @"}." + m.Operator.JsName + @"({operand})"")]
                        static public " + m.ReturnCsType + @" operator " + m.Operator.Symbol +
                            @"(" + classData.Name + @" " + lowClassName + @", " + m.OperandTypeCs + @" operand)" + genBody(m))).Join("\n\n") + @"
        
                        #endregion") + (methods.Length == 0 ? "" : @"

                        #region Methods

                        " + methods.Where(m => m.CsName != "ToString").Select(m =>
                        getComment(m) + @"
                        public " + m.ReturnCsType + @" " + m.CsName + genParamList(m) + genBody(m)).Join("\n\n") + @"

                        #endregion
                        ") + (staticMethods.Length == 0 ? "" : @"
                        #region Static Methods

                        " + staticMethods.Select(m =>
                        getComment(m) + (!m.CallWithNew ? "" : @"
                        [ScriptAlias(""new (" + classData.Name + "." + m.CsName + @")"")]") + @"
                        public static " + m.ReturnCsType + @" " + (m.CallWithNew ? "Create" : "") + m.CsName + genParamList(m) + genBody(m)).Join("\n\n") + @"

                        #endregion") + @"
                    }
                }";

            foreach (var rr in RegexReplaces)
                saltarelleMetadata = Regex.Replace(saltarelleMetadata, rr.Key, rr.Value);

            saltarelleMetadata = IndentFix(saltarelleMetadata);
            return saltarelleMetadata;
        }

        static void FillInheritance(ClassData[] classDatas)
        {
            var classDatasDict = classDatas.ToDictionary(x => x.Name);
            var alreadyFilled = new HashSet<ClassData>();

            Action<ClassData> provideInheritParent = null;
            provideInheritParent = cd =>
            {
                if (alreadyFilled.Contains(cd))
                    return;

                if (cd.Extends != null)
                {
                    var extends = cd.Extends.Select(x => classDatasDict.GetValueOrDefault(x)).Where(x => x != null).ToArray();
                    foreach (var extend in extends)
                        provideInheritParent(extend);

                    var chains = extends.Select(x => x.GetInheritChain()).ToArray();
                    var trueBaseClass = chains.Where(chain => !chains.Except(new[] { chain }).Any(c => c.Contains(chain.First()))).Select(x => x[0]).ToArray();

                    if (trueBaseClass.Length > 1)
                        ErrorLog("Multiple base class found for class {0}: {1}", cd.Name, trueBaseClass.Select(x => x.Name).Join(", "));
                    else if (trueBaseClass.Length == 1)
                        cd.InheritParent = trueBaseClass[0];                    
                }

                alreadyFilled.Add(cd);
            };

            foreach (var classData in classDatas)
            {
                provideInheritParent(classData);
            }
        }

        static IEnumerable<int[]> Permutation(int[] maxValues)
        {
            if (maxValues.Length == 0)
            {
                yield return new int[0];
                yield break;
            }

            var divArr = maxValues.Select((val, i) => new { val, div = maxValues.Skip(i + 1).Aggregate(1, (x, s) => x * s) }).ToArray();
            var all = divArr[0].div * divArr[0].val;
            for (int i = 0; i < all; i++)
            {
                var j = i;
                var idxs = divArr.Select(x =>
                {
                    var res = j / x.div;
                    j %= x.div;
                    return res;
                }).ToArray();

                yield return idxs;
            }            
        }

        static void Main(string[] args)
        {
            //foreach (var idxs in Permutation(new int[] { }))
            //    Console.WriteLine(idxs.Select(x => x.ToString()).Join(", "));

            var prgPath = AppDomain.CurrentDomain.BaseDirectory;
            var srcDir = Utils.ProvidePath(prgPath + @"\..\..\..\..\src\Libraries\PaperJS\Generated\");

            var classDatas = DownloadClassDatas(prgPath);

            // process class
            foreach (var classData in classDatas)
            {
                Log();
                Log("==================================");
                Log("Processing class page: {0}...", classData.Name);
                Log("==================================");
                Log();
                ClassDataJsProcess(classData);
                SplitByParameters(classData);
                FixErrors(classData);

                //Log(String.Join(Environment.NewLine, classData.Members.Select(x => "[" + x.Type + "] " + x.ToString()).ToArray()));

                if (classData.Extends != null)
                    Log("Extends: {0}", classData.Extends.Join(", "));
            }

            foreach (var classData in classDatas)
            {
                var parameterMismatch = classData.Members.Where(m => (m.Parameters == null ? 0 : 1) != (m.Parameters2 == null ? 0 : 1) || (m.Parameters != null && m.Parameters.Length != m.Parameters2.Length)).ToArray();
                if (parameterMismatch.Length != 0)
                    ErrorLog("Parameter mismatch: " + String.Join(", ", parameterMismatch.Select(x => x.JsName)));
            }

            FillInheritance(classDatas);

            foreach (var classData in classDatas)
                foreach (var member in classData.Members)
                    FillCsData(member);

            var baseMembersAlreadyRemoved = new HashSet<ClassData>();
            Action<ClassData> removeBaseMembers = null;
            removeBaseMembers = cd =>
            {
                if (baseMembersAlreadyRemoved.Contains(cd)) return;

                if (cd.InheritParent != null)
                {
                    removeBaseMembers(cd.InheritParent);
                    var parentMembers = cd.InheritParent.GetInheritChain().SelectMany(x => x.Members);
                    cd.Members = cd.Members.Where(m => !parentMembers.Any(pm => pm.ToStringCs(false, false) == m.ToStringCs(false, false))).ToArray();
                }

                baseMembersAlreadyRemoved.Add(cd);
            };

            foreach (var classData in classDatas)
            {
                Log();
                Log("==================================");
                Log("Class members: {0}...", classData.Name);
                Log("==================================");
                Log();
                Log(String.Join(Environment.NewLine, classData.Members.Select(x => "[" + x.Type + "] " + x.ToStringCs(false, true)).ToArray()));
            }

            foreach (var classData in classDatas)
            {
                Log("Classes: {0} -> {1}", classData.Name, classData.InheritParent == null ? "" : classData.InheritParent.Name);
                removeBaseMembers(classData);
            }

            foreach (var classData in classDatas)
            {
                var members = new List<ClassMember>();
                foreach (var member in classData.Members)
                {
                    members.Add(member);

                    var newMember = member;
                    if (newMember.Parameters2 != null)
                    {
                        while (true)
                        {
                            var p = newMember.Parameters2.LastOrDefault();
                            if (p == null || !p.IsOptional)
                                break;

                            newMember = newMember.Clone();
                            newMember.Parameters2 = newMember.Parameters2.Take(newMember.Parameters2.Length - 1).ToArray();
                            members.Add(newMember);
                        }
                    }
                }
                classData.Members = members.GroupBy(x => x.ToString()).Select(x => x.First()).ToArray();
            }

            Func<KeyValuePair<string, ClassMember[]>[]> getUsedTypes = () => 
                classDatas.SelectMany(classData => classData.Members.SelectMany(x => new[] { new[] { Tuple.Create(x.PropertyCsType, x), Tuple.Create(x.ReturnCsType, x), Tuple.Create(x.OperandTypeCs, x) }, 
                    (x.Parameters2 ?? new MethodParameter[0]).Select(y => Tuple.Create(y.CsType, x)) }.SelectMany(y => y))).GroupBy(x => (x.Item1 ?? "").Replace("[]", ""), x => x.Item2).Where(x => x.Key != "").
                    ToDictionary(x => x.Key, x => x.ToArray()).OrderBy(x => x.Key).ToArray();

            var usedTypes = getUsedTypes();
            var enumTypes = usedTypes.Where(x => x.Key.StartsWith("String(") && x.Value.All(y => y.Type == MemberTypes.Property)).ToArray();

            var enumNames = new List<string>();
            foreach (var enumDesc in enumTypes)
            {
                var name = (enumDesc.Value.Length == 1 ? enumDesc.Value[0].Class.Name : "") + enumDesc.Value[0].CsName;
                var values = enumDesc.Key.Replace("String(", "").Replace(")", "").Replace("'", "").Split(new[]{ ", " }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var prop in enumDesc.Value)
                    prop.PropertyCsType = name;

                enumNames.Add(name);

                var enums = values.Select(jsName =>
                {
                    var csName = EnumWords.Aggregate(jsName, (current, enumWord) => current.Replace(enumWord, enumWord + "-"));
                    csName = csName.Split('-').Select(UpFirst).Join("");
                    return new { jsName, csName };
                }).ToArray();

                var enumFileContent = IndentFix(@"
                    " + FileHeaders + @"
                    {
                        /// <summary>
                        /// 
                        /// </summary>
                        [NamedValues]
                        public enum " + name + @"
                        {
                            " + enums.Select(x => @"
                            /// <summary>
                            /// Javascript value: '" + x.jsName + @"'
                            /// </summary>
                            [ScriptName(""" + x.jsName + @""")] " + x.csName + ",").Join("\n") + @"
                        }
                    }");

                File.WriteAllText(srcDir + name + ".cs", enumFileContent);
            }

            usedTypes = getUsedTypes();
            
            var knownTypes = classDatas.Select(x => x.Name).Union(new[] { "bool", "Array", "string", "double", "Action<ToolEvent>", "Action<KeyEvent>", "Action<Event>", "void", "CanvasElement" }).Union(enumNames).ToArray();

            var unknownTypes = usedTypes.Where(x => !knownTypes.Contains(x.Key)).ToArray();

            ErrorLog();
            foreach (var unknownType in unknownTypes)
                ErrorLog("Unknown type: {0}\n{1}\n", unknownType.Key, unknownType.Value.Select(x => "  " + x.ToString(true)).Join("\n"));


            foreach (var classData in classDatas)
            {
                var classOutFn = srcDir + classData.Name + ".cs";

                var saltarelleMetadata = GenerateSaltarelleCode(classData);
                File.WriteAllText(classOutFn, saltarelleMetadata);
            }

            Log("Done.");
            File.WriteAllText("log.txt", logText);
            File.WriteAllText("error.txt", errorText);

            Console.ReadLine();
        }

        private static void SplitByParameters(ClassData classData)
        {
            var members = new List<ClassMember>();
            foreach (var member in classData.Members)
            {
                if(member.Parameters2 == null)
                    members.Add(member);
                else
                {
                    foreach (var paramPerm in Permutation(member.Parameters2.Select(x => x.JsTypes.Length).ToArray()))
                    {
                        var newMember = member.Clone();
                        for (int i = 0; i < paramPerm.Length; i++)
                            newMember.Parameters2[i].JsType = newMember.Parameters2[i].JsTypes[paramPerm[i]];
                        members.Add(newMember);
                    }
                }
            }
            classData.Members = members.ToArray();            
        }

        public static Dictionary<string, string> TypeConvertDict = new Dictionary<string, string>
        {
            { "Number",  "double" },
            { "Boolean", "bool"   },
            { "array",   "Array"  },
            { "Object",  "object" },
            { "String",  "string" },
            { "true",    "bool" },
            { "item",    "Item" },
            { "Index",   "int" },
            { "HTMLImageELement", "ImageElement" },
            { "HTMLImageElement", "ImageElement" },
			{ "HTMLCanvasElement", "CanvasElement" },
        };

        public static Dictionary<string, string> NameToTypeDict = new Dictionary<string, string>
        {
            { "x", "double" },
            { "y", "double" },
            { "point", "Point" },
            { "color", "Color" },
            { "center", "Point" },
            { "position", "Point" },
        };

        public static Dictionary<string, string> RegexReplaces = new Dictionary<string, string>
        {
            { "object Modifiers;", "EventModifiers Modifiers;" },
            { "&mdash;&nbsp;", "- " },
            { "double(?= (index|Index|num|id|Id))", "int" },
            { "public RgbColor ", "public Color " },
            { "(&nbsp;|&mdash;)", "" },
            { "CanvasRenderingContext2D", "System.Html.Media.Graphics.CanvasContext2D" },
            { "ImageData", "System.Html.Media.Graphics.ImageData" },
            { "public Context Context;", "public System.Html.Media.Graphics.CanvasContext2D Context;" },
            { "(?<=[ (])Canvas(?=[ )])", "CanvasElement" },
        };

        public static OperatorData[] OperatorsList = new[]
        {
            new OperatorData(OperatorTypes.Add, "+"),
            new OperatorData(OperatorTypes.Subtract, "-"),
            new OperatorData(OperatorTypes.Multiply, "*"),
            new OperatorData(OperatorTypes.Divide, "/"),
            new OperatorData(OperatorTypes.Modulo, "%"),
            new OperatorData(OperatorTypes.Equals, "=="),
        };

        public static string[] EnumWords = new[] { "key", "mouse" };
    }

    public static class Utils
    {
        public static string ProvidePath(string path)
        {
            var di = new DirectoryInfo(path);
            di.Create();
            return path;
        }
    }

    public class MethodParameter
    {
        public int Idx;
        public string Name;
        public string JsType;
        public string[] JsTypes;
        public string CsType;
        public string Description;
        public bool IsOptional;
        public bool IsArrayType;

        public MethodParameter Clone()
        {
            return new MethodParameter()
            {
                Idx = Idx,
                Name = Name,
                JsType = JsType,
                JsTypes = JsTypes,
                CsType = CsType,
                Description = Description,
                IsOptional = IsOptional,
                IsArrayType = IsArrayType
            };
        }
    }

    public enum MemberTypes { Constructor, Operator, Property, Method, StaticMethod };
    public enum OperatorTypes { Add, Subtract, Multiply, Divide, Modulo, Equals };

    public class OperatorData
    {
        public OperatorTypes Type;
        public string Symbol;
        public string JsName;
        public string CsName;

        public OperatorData(OperatorTypes type, string symbol, string csName = null, string jsName = null)
        {
            Type = type;
            Symbol = symbol;
            CsName = csName ?? Type.ToString();
            JsName = jsName ?? CsName.ToLower();
        }
    }

    public class ClassMember
    {
        public ClassData Class;

        public string HtmlContent;

        public string JsName;
        public string CsName;
        public string ParameterList;

        public string OperandTypeJs;
        public string OperandTypeCs;

        public OperatorData Operator;

        public MemberTypes Type;

        public MethodParameter[] Parameters;
        public MethodParameter[] Parameters2;

        public string Description;

        public string PropertyJsType;
        public string PropertyCsType;
        public bool PropertyArrayType;

        public string ReturnJsType;
        public string ReturnCsType;
        public bool ReturnArrayType;

        public string ReturnDescription;
        public bool CallWithNew;

        public string ToString(bool showClassName = false)
        {
            return ((ReturnJsType ?? PropertyJsType) != null ? (ReturnJsType ?? PropertyJsType) + " " : "") + (showClassName ? Class.Name + "." : "") + 
                JsName + (OperandTypeJs != null ? "(" + OperandTypeJs + ")" : "") +
                (Parameters2 != null ? "(" + String.Join(", ", Parameters2.Select(x => x.JsType + " " + x.Name)) + ")" : "");            
        }

        public string ToStringCs(bool showClassName = false, bool showParamNames = true)
        {
            return ReturnCsType + PropertyCsType + " " + (showClassName ? Class.Name + "." : "") +
                (Operator != null && Operator.Type == OperatorTypes.Equals ? "Equals" : CsName) + (OperandTypeCs != null ? "(" + OperandTypeCs + ")" : "") +
                (Parameters2 != null ? "(" + String.Join(", ", Parameters2.Select(x => x.CsType + (showParamNames ? " " + x.Name : ""))) + ")" : "");
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public ClassMember Clone()
        {
            return new ClassMember
            {
                Class = Class,
                HtmlContent = HtmlContent,
                JsName = JsName,
                CsName = CsName,
                ParameterList = ParameterList,
                OperandTypeJs = OperandTypeJs,
                OperandTypeCs = OperandTypeCs,
                Operator = Operator,
                Type = Type,
                Parameters = Parameters == null ? null : Parameters.Select(x => x.Clone()).ToArray(),
                Parameters2 = Parameters2 == null ? null : Parameters2.Select(x => x.Clone()).ToArray(),
                Description = Description,
                PropertyJsType = PropertyJsType,
                PropertyCsType = PropertyCsType,
                PropertyArrayType = PropertyArrayType,
                ReturnJsType = ReturnJsType,
                ReturnCsType = ReturnCsType,
                ReturnArrayType = ReturnArrayType,
                ReturnDescription = ReturnDescription,
                CallWithNew = CallWithNew
            };
        }
    }

    public class ClassData
    {
        public string UrlName;
        public string DownloadName { get { return "reference/" + UrlName; } }

        public string[] Extends { get; set; }

        public string HtmlContent;

        public string Name;
        public string NameOnPage;
        public string Description;

        public ClassMember[] Members;

        public ClassData InheritParent;

        public override string ToString()
        {
            return Name;
        }

        public ClassData[] GetInheritChain()
        {
            var list = new List<ClassData>();
            var cd = this;
            while (cd != null)
            {
                list.Add(cd);
                cd = cd.InheritParent;
            }
            return list.ToArray();
        }
    }

    public static class ExtensionMethods
    {
        public static string[] ToSingleMatchArray(this MatchCollection matches)
        {
            return matches.OfType<Match>().Select(x => x.Success ? x.Groups[1].Value : null).ToArray();
        }

        public static string[][] ToArray(this MatchCollection matches)
        {
            return matches.OfType<Match>().Select(x => x.Groups.OfType<Group>().Select(y => y.Success ? y.Value : null).ToArray()).ToArray();
        }

        public static string Join(this IEnumerable<string> list, string separator)
        {
            return String.Join(separator, list);
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            if (!dict.TryGetValue(key, out value))
                value = defaultValue;
            return value;
        }
    }
}
