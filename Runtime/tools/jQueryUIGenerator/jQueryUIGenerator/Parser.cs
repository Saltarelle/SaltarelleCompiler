// Parser.cs
// jQueryUIGenerator
//
// Copyright 2012 Ivaylo Gochkov
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

using ScriptSharp.Tools.jQueryUIGenerator.Model;
using System.Linq;

namespace ScriptSharp.Tools.jQueryUIGenerator {
    /// <summary>
    /// jQueryUI API documentation parser.
    /// </summary>
    public class Parser {
        private string SourcePath;
        private TextWriter Messages;

        /// <summary>
        /// Creates a parser of jQueryUI API documentation.
        /// </summary>
        /// <param name="sourcePath">The path to the jQueryUI API documentation.</param>
        /// <param name="messageStream">A message stream.</param>
        public Parser(string sourcePath, TextWriter messageStream = null) {
            Debug.Assert(!string.IsNullOrEmpty(sourcePath), "The path to the jQueryUI API documentation is not specified.");

            SourcePath = sourcePath;
            Messages = messageStream ?? TextWriter.Null;
        }

		private void ResolveIncludes(XmlDocument doc, string basePath) {
			var nsm = new XmlNamespaceManager(new NameTable());
			nsm.AddNamespace("xi", "http://www.w3.org/2003/XInclude");
			foreach (XmlNode n in doc.SelectNodes("//xi:include", nsm)) {
				XmlAttribute href = n.Attributes["href"];
				if (href == null || string.IsNullOrEmpty(href.Value))
					throw new ArgumentException("Missing 'href' in include");
				var innerDoc = new XmlDocument();
				string includeFile = Path.Combine(basePath, href.Value);
				innerDoc.Load(includeFile);
				ResolveIncludes(innerDoc, Path.GetDirectoryName(includeFile));
				n.ParentNode.ReplaceChild(doc.ImportNode(innerDoc.DocumentElement, true), n);
			}
		}

        private Entry ParseFile(string file) {
            Messages.Write("Parsing " + file + " ...");

            XmlDocument document = new XmlDocument();

            try {
                document.Load(file);
            } catch {
                Messages.WriteLine("Failed");
                return null;
            }

			ResolveIncludes(document, Path.GetDirectoryName(file));

            XmlNode xmlEntry = document.SelectSingleNode("//entry");

            if (xmlEntry == null) {
                Messages.WriteLine("Failed");
            }

            Entry entry = ParseEntry(xmlEntry);

            Messages.WriteLine("Ok");

            return entry;
        }
        
        /// <summary>
        /// Parses all XML files found in the source directory including sub-directories
        /// </summary>
        /// <returns>List with jQueryUI entries</returns>
        public IList<Entry> Parse() {
            DirectoryInfo source = new DirectoryInfo(SourcePath);
            FileInfo[] files = source.GetFiles("*.xml", SearchOption.AllDirectories);

	        return files.Select(file => ParseFile(file.FullName)).Where(entry => entry != null).ToList();
        }

        private Entry ParseEntry(XmlNode xmlEntry) {
            if (xmlEntry == null) {
                return null;
            }

            Entry entry = new Entry();

            entry.Name = GetAttributeStringValue(xmlEntry, "name");
            entry.Type = GetAttributeStringValue(xmlEntry, "type");

			entry.Description = GetNodeInnerXml(xmlEntry, "desc", entry.Name);
            entry.LongDescription = GetNodeInnerXml(xmlEntry, "longdesc", entry.Name);
            entry.Created = GetNodeInnerXml(xmlEntry, "created", entry.Name);

			if (entry.Type == "effect") {
				entry.Options = new[] { new Option { Name = "easing", Description = "The easing to use for the effect", Type = "string" } }
				                .Concat(ParseArguments(GetNodeList(xmlEntry, "arguments/argument")).Select(a => new Option { Name = a.Name, Description = a.Description, Type = a.Type }))
								.ToList();
			}
			else if (entry.Type == "method") {
				entry.Options = ParseArguments(GetNodeList(xmlEntry, "signature/argument[@name='options']/property")).Select(a => new Option { Name = a.Name, Description = a.Description, Type = a.Type }).ToList();
			}
			else {
				entry.Options = ParseOptions(GetNodeList(xmlEntry, "//options//option"));
			}

            entry.Events = ParseEvents(GetNodeList(xmlEntry, "//events//event"));
            entry.Methods = ParseMethods(GetNodeList(xmlEntry, "//methods/*"));

            XmlNode xmlExample = xmlEntry.SelectSingleNode("//example");
            if (xmlExample != null) {
                entry.Example = ParseExample(xmlExample);
            }

            entry.Categories = ParseCategories(GetNodeList(xmlEntry, "//category"));

            return entry;
        }

        private IList<Option> ParseOptions(XmlNodeList xmlOptions) {
            IList<Option> options = new List<Option>();

            if (xmlOptions == null) {
                return options;
            }

            for (int i = 0; i < xmlOptions.Count; i++) {
                XmlNode xmlOption = xmlOptions[i];

                Option option = new Option();
                option.Name = GetAttributeStringValue(xmlOption, "name");
                option.Default = GetAttributeStringValue(xmlOption, "default");
                option.Description = GetNodeInnerXml(xmlOption, "desc", null);

                // Type appears not only as attribute but also as an XML node.
                // see comment from jzaefferer: https://github.com/jquery/api.jqueryui.com/pull/1#issuecomment-6151386
                option.Type = GetAttributeStringValue(xmlOption, "type");
                if (string.IsNullOrEmpty(option.Type)) {
                    XmlNodeList xmlTypes = GetNodeList(xmlOption, "type");
                    if (xmlTypes != null) {
                        option.Type = ParseTypes(xmlTypes);
                    }
                }

                options.Add(option);
            }

            return options;
        }

        private IList<Event> ParseEvents(XmlNodeList xmlEvents) {
            IList<Event> events = new List<Event>();

            if (xmlEvents == null) {
                return events;
            }

            for (int i = 0; i < xmlEvents.Count; i++) {
                XmlNode xmlEvent = xmlEvents[i];

                Event @event = new Event();
                @event.Name = GetAttributeStringValue(xmlEvent, "name");
                @event.Description = GetNodeInnerXml(xmlEvent, "desc", null);
                @event.Arguments = ParseArguments(GetNodeList(xmlEvent, "argument"));

                events.Add(@event);
            }

            return events;
        }

        private IList<Method> ParseMethods(XmlNodeList xmlMethods) {
            IList<Method> methods = new List<Method>();

            if (xmlMethods == null) {
                return methods;
            }

            for (int i = 0; i < xmlMethods.Count; i++) {
                XmlNode xmlMethod = xmlMethods[i];
                if (xmlMethod.Name == "method") {
					XmlNodeList signature = GetNodeList(xmlMethod, "signature");

					string name = GetAttributeStringValue(xmlMethod, "name");
					foreach (XmlNode n in (signature.Count == 0 ? new[] { xmlMethod } : signature.Cast<XmlNode>())) {
						methods.Add(new Method {
							Name = name,
							ReturnType = GetAttributeStringValue(n, "return"),
							Description = GetNodeInnerXml(n, "desc", null),
							Arguments = ParseArguments(GetNodeList(n, "argument"))
						});
					}
                }
            }

            return methods;
        }

        private IList<Argument> ParseArguments(XmlNodeList xmlArguments) {
            IList<Argument> arguments = new List<Argument>();

            if (xmlArguments == null) {
                return arguments;
            }

            for (int i = 0; i < xmlArguments.Count; i++) {
                XmlNode xmlArgument = xmlArguments[i];

                Argument argument = new Argument();
                argument.Name = GetAttributeStringValue(xmlArgument, "name");
				var typeNodes = GetNodeList(xmlArgument, "type");
				if (typeNodes.Count != 0)
					argument.Type = string.Join(" or ", typeNodes.OfType<XmlNode>().Select(n => n.Attributes["name"].Value));
				else
					argument.Type = GetAttributeStringValue(xmlArgument, "type");
					
                argument.Optional = GetAttributeBoolValue(xmlArgument, "optional");
                argument.Description = GetNodeInnerXml(xmlArgument, "desc", null);
                argument.Properties = ParseProperties(GetNodeList(xmlArgument, "property"));

                arguments.Add(argument);
            }

            return arguments;
        }

        private IList<Property> ParseProperties(XmlNodeList xmlProperties) {
            IList<Property> properties = new List<Property>();

            if (xmlProperties == null) {
                return properties;
            }

            for (int i = 0; i < xmlProperties.Count; i++) {
                XmlNode xmlProperty = xmlProperties[i];

                Property property = new Property();
                property.Name = GetAttributeStringValue(xmlProperty, "name");
                property.Type = GetAttributeStringValue(xmlProperty, "type");
                property.Description = GetNodeInnerXml(xmlProperty, "desc", null);

                properties.Add(property);
            }

            return properties;
        }

        private Example ParseExample(XmlNode xmlExample) {
            if (xmlExample == null) {
                return null;
            }

            Example example = new Example();

            example.Description = (xmlExample.SelectSingleNode("desc") != null)
                                ? xmlExample.SelectSingleNode("desc").InnerXml
                                : string.Empty;

            example.Code = (xmlExample.SelectSingleNode("code") != null)
                         ? xmlExample.SelectSingleNode("code").InnerXml
                         : string.Empty;

            example.Html = (xmlExample.SelectSingleNode("html") != null)
                         ? xmlExample.SelectSingleNode("html").InnerXml
                         : string.Empty;

            return example;
        }

        private string[] ParseCategories(XmlNodeList xmlCategories) {
			return xmlCategories.Cast<XmlNode>().Select(n => GetAttributeStringValue(n, "slug")).ToArray();
        }

        private string ParseTypes(XmlNodeList xmlTypes) {
            Debug.Assert(xmlTypes != null);

            List<string> types = new List<string>();

            for (int i = 0; i < xmlTypes.Count; i++) {
                XmlNode xmlType = xmlTypes[i];
                types.Add(GetAttributeStringValue(xmlType, "name"));
            }

            return string.Join(",", types);
        }

        private string GetAttributeStringValue(XmlNode xmlNode, string attributeName) {
            Debug.Assert(xmlNode != null, "XmlNode is null.");
            Debug.Assert(!string.IsNullOrEmpty(attributeName), "Attribute name is not specified.");

            return (xmlNode.Attributes[attributeName] != null)
                  ? xmlNode.Attributes[attributeName].Value
                  : string.Empty;
        }

        private bool GetAttributeBoolValue(XmlNode xmlNode, string attributeName) {
            Debug.Assert(xmlNode != null, "XmlNode is null.");
            Debug.Assert(!string.IsNullOrEmpty(attributeName), "Attribute name is not specified.");

            string value = (xmlNode.Attributes[attributeName] != null)
                         ? xmlNode.Attributes[attributeName].Value
                         : string.Empty;

            bool optional;

            if (bool.TryParse(value, out optional)) {
                return optional;
            } else {
                return false;
            }
        }

        private string GetNodeInnerXml(XmlNode xmlNode, string nodeName, string placeholderNameValue) {
            Debug.Assert(xmlNode != null, "XmlNode is null.");
            Debug.Assert(!string.IsNullOrEmpty(nodeName), "Node name is not specified.");

			var resultNode = xmlNode.SelectSingleNode(nodeName);
			if (resultNode == null)
				return string.Empty;

			foreach (XmlNode n in resultNode.SelectNodes("//placeholder[@name='name' or @name='widget-element']")) {
				if (placeholderNameValue == null)
					throw new ArgumentException("Need a value for the 'name' placeholder");
				n.ParentNode.ReplaceChild(n.OwnerDocument.CreateTextNode(placeholderNameValue), n);
			}
/*
			foreach (XmlNode n in resultNode.SelectNodes("//placeholder")) {
				throw new ArgumentException("Unexpected placeholder " + n.Attributes["name"].Value);
			}
			*/
#warning TODO
            return resultNode.InnerXml;
        }

        private XmlNodeList GetNodeList(XmlNode xmlNode, string nodeName) {
            Debug.Assert(xmlNode != null, "XmlNode is null.");
            Debug.Assert(!string.IsNullOrEmpty(nodeName), "Node name is not specified.");

            return xmlNode.SelectNodes(nodeName);
        }
    }
}