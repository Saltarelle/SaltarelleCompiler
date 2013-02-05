using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Options;

namespace EmbedAssemblies {
	class Program {
		static readonly string _programName = Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location);

		const string OptionsText =
@"    -out     Specifies the output assembly (short: -o).
               Default is to overwrite the input assembly.
    -assembly  Specifies an assembly to merge.
               Can be specified multiple times (short: -a)
    -debug     Also merge .pdb files (short: -d)
    -help      Show this message (short: -?)";

		static void ShowHelp() {
			Console.WriteLine("EmbedAssemblies");
			Console.WriteLine("Usage: " + _programName + " [options] assembly");
			Console.WriteLine();
			Console.WriteLine("Options:");
			Console.WriteLine(OptionsText);
			Console.WriteLine();
			Console.WriteLine("Options can be of the form -option or /option");
		}

		static IEnumerable<string> ExpandWildcards(string wildcard) {
			var substitutedArg = System.Environment.ExpandEnvironmentVariables(wildcard);
			
			var dirPart = Path.GetDirectoryName(substitutedArg);
			if (dirPart.Length == 0)
				dirPart = ".";
			
			var filePart = Path.GetFileName(substitutedArg);
			
			return Directory.GetFiles(dirPart, filePart);
		}

		static bool EmbedFile(ModuleDefinition container, string file) {
			byte[] content;
			try {
				content = File.ReadAllBytes(file);
			}
			catch (IOException ex) {
				Console.WriteLine("Error reading file " + file + ": " + ex.Message);
				return false;
			}

			container.Resources.Add(new EmbeddedResource(Path.GetFileName(file), ManifestResourceAttributes.Public, content));
			return true;
		}

		static bool EmbedAssemblies(ModuleDefinition container, IEnumerable<string> toEmbed, bool debug) {
			foreach (var asmPath in toEmbed) {
				if (!EmbedFile(container, asmPath))
					return false;

				if (debug) {
					string pdbPath = Path.ChangeExtension(asmPath, ".pdb");
					if (File.Exists(pdbPath)) {
						if (!EmbedFile(container, pdbPath))
							return false;
					}
				}
			}
			return true;
		}

		static Instruction GetInstruction(MethodBody oldBody, MethodBody newBody, Instruction i) {
			int pos = oldBody.Instructions.IndexOf(i);
			if (pos > -1 && pos < newBody.Instructions.Count)
				return newBody.Instructions[pos];
			return null /*newBody.Instructions.Outside*/;
		}

		static MethodReference Import(ModuleDefinition container, MethodReference method) {
			var result = new MethodReference(method.Name, Import(container, method.ReturnType), Import(container, method.DeclaringType)) {
				HasThis = method.HasThis
			};
			foreach (var p in method.Parameters)
				result.Parameters.Add(new ParameterDefinition(p.Name, p.Attributes, Import(container, p.ParameterType)));
			return result;
		}
		
		static TypeReference Import(ModuleDefinition container, TypeReference type) {
			return container.Types.FirstOrDefault(t => t.FullName == type.FullName) ?? container.Import(type);
		}

		static FieldReference Import(ModuleDefinition container, FieldReference field) {
			return new FieldReference(field.Name, Import(container, field.FieldType), Import(container, field.DeclaringType));
		}

		static MethodDefinition ImportMethod(ModuleDefinition container, MethodDefinition source) {
			var target = new MethodDefinition(source.Name, source.Attributes, container.Import(source.ReturnType));

			foreach (var p in source.Parameters)
				target.Parameters.Add(new ParameterDefinition(p.Name, p.Attributes, container.Import(p.ParameterType)));

			target.Body.MaxStackSize  = source.Body.MaxStackSize;
			target.Body.InitLocals    = source.Body.InitLocals;
			target.Body.LocalVarToken = source.Body.LocalVarToken;

			foreach (var var in source.Body.Variables)
				target.Body.Variables.Add(new VariableDefinition(var.Name, container.Import(var.VariableType)));

			foreach (var instr in source.Body.Instructions) {
				Instruction ni;

				if (instr.OpCode.Code == Code.Calli) {
					ni = Instruction.Create(instr.OpCode, (Mono.Cecil.CallSite)instr.Operand);
				}
				else {
					switch (instr.OpCode.OperandType) {
						case OperandType.InlineArg:
						case OperandType.ShortInlineArg:
							if (instr.Operand == source.Body.ThisParameter) {
								ni = Instruction.Create(instr.OpCode, target.Body.ThisParameter);
							}
							else {
								int param = source.Parameters.IndexOf((ParameterDefinition)instr.Operand);
								ni = Instruction.Create(instr.OpCode, target.Parameters[param]);
							}
							break;
						case OperandType.InlineVar:
						case OperandType.ShortInlineVar:
							int var = source.Body.Variables.IndexOf((VariableDefinition)instr.Operand);
							ni = Instruction.Create(instr.OpCode, target.Body.Variables[var]);
							break;
						case OperandType.InlineField:
							ni = Instruction.Create(instr.OpCode, Import(container, (FieldReference)instr.Operand));
							break;
							throw new InvalidOperationException();
						case OperandType.InlineMethod:
							ni = Instruction.Create(instr.OpCode, Import(container, (MethodReference)instr.Operand));
							break;
						case OperandType.InlineType:
							ni = Instruction.Create(instr.OpCode, Import(container, (TypeReference)instr.Operand));
							break;
						case OperandType.InlineTok:
							if (instr.Operand is TypeReference)
								ni = Instruction.Create(instr.OpCode, Import(container, (TypeReference)instr.Operand));
							else if (instr.Operand is FieldReference)
								ni = Instruction.Create(instr.OpCode, Import(container, (FieldReference)instr.Operand));
							else if (instr.Operand is MethodReference)
								ni = Instruction.Create(instr.OpCode, Import(container, (MethodReference)instr.Operand));
							else
								throw new InvalidOperationException();
							break;
						case OperandType.ShortInlineBrTarget:
						case OperandType.InlineBrTarget:
							ni = Instruction.Create(instr.OpCode, (Instruction)instr.Operand);
							break;
						case OperandType.InlineSwitch:
							ni = Instruction.Create(instr.OpCode, (Instruction[])instr.Operand);
							break;
						case OperandType.InlineR:
							ni = Instruction.Create(instr.OpCode, (double)instr.Operand);
							break;
						case OperandType.ShortInlineR:
							ni = Instruction.Create(instr.OpCode, (float)instr.Operand);
							break;
						case OperandType.InlineNone:
							ni = Instruction.Create(instr.OpCode);
							break;
						case OperandType.InlineString:
							ni = Instruction.Create(instr.OpCode, (string)instr.Operand);
							break;
						case OperandType.ShortInlineI:
							if (instr.OpCode == OpCodes.Ldc_I4_S)
								ni = Instruction.Create(instr.OpCode, (sbyte)instr.Operand);
							else
								ni = Instruction.Create(instr.OpCode, (byte)instr.Operand);
							break;
						case OperandType.InlineI8:
							ni = Instruction.Create(instr.OpCode, (long)instr.Operand);
							break;
						case OperandType.InlineI:
							ni = Instruction.Create(instr.OpCode, (int)instr.Operand);
							break;
						default:
							throw new InvalidOperationException();
					}
				}
				ni.SequencePoint = instr.SequencePoint;
				target.Body.Instructions.Add(ni);
			}

			for (int i = 0; i < target.Body.Instructions.Count; i++) {
				Instruction instr = target.Body.Instructions[i];
				if (instr.OpCode.OperandType != OperandType.ShortInlineBrTarget &&
					instr.OpCode.OperandType != OperandType.InlineBrTarget)
					continue;
				
				instr.Operand = GetInstruction(source.Body, target.Body, (Instruction)source.Body.Instructions[i].Operand);
			}

			foreach (ExceptionHandler eh in source.Body.ExceptionHandlers) {
				ExceptionHandler neh = new ExceptionHandler(eh.HandlerType) {
					TryStart     = GetInstruction(source.Body, target.Body, eh.TryStart),
					TryEnd       = GetInstruction(source.Body, target.Body, eh.TryEnd),
					HandlerStart = GetInstruction(source.Body, target.Body, eh.HandlerStart),
					HandlerEnd   = GetInstruction(source.Body, target.Body, eh.HandlerEnd)
				};

				switch (eh.HandlerType) {
					case ExceptionHandlerType.Catch:
						neh.CatchType = container.Import(eh.CatchType);
						break;
					case ExceptionHandlerType.Filter:
						neh.FilterStart = GetInstruction(source.Body, target.Body, eh.FilterStart);
						break;
				}
			
				target.Body.ExceptionHandlers.Add(neh);
			}

			return target;
		}

		static void InsertAssemblyLoader(ModuleDefinition container) {
			var loader = container.Types.FirstOrDefault(t => t.FullName == typeof(EmbeddedAssemblyLoader).FullName);
			if (loader == null) {
				var current        = AssemblyDefinition.ReadAssembly(System.Reflection.Assembly.GetExecutingAssembly().Location);
				var existingLoader = current.MainModule.GetType(typeof(EmbeddedAssemblyLoader).FullName);
				loader = new TypeDefinition(typeof(EmbeddedAssemblyLoader).Namespace, typeof(EmbeddedAssemblyLoader).Name, existingLoader.Attributes, container.TypeSystem.Object);
				container.Types.Add(loader);
				foreach (var f in existingLoader.Fields) {
					loader.Fields.Add(new FieldDefinition(f.Name, f.Attributes, Import(container, f.FieldType)));
				}
				foreach (var m in existingLoader.Methods) {
					loader.Methods.Add(ImportMethod(container, m));
				}
			}

			var module = container.GetType("<Module>");

			var moduleInitializer = new MethodDefinition( ".cctor", MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, container.TypeSystem.Void);
			moduleInitializer.Body.Instructions.Add(Instruction.Create(OpCodes.Call, loader.Methods.Single(m => m.Name == "Register")));
			moduleInitializer.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
			module.Methods.Add(moduleInitializer);
		}

		static int Main(string[] args) {
			if (args.Length == 0) {
				ShowHelp();
				return 1;
			}

			bool showHelp = false, debug = false;
			string inputFile = null, outputFile = null;
			var assemblies = new List<string>();

			var opts = new OptionSet {
				{ "?|help", v => showHelp = true },
				{ "a|assembly=", v => assemblies.AddRange(ExpandWildcards(v)) },
				{ "d|debug=", v => debug = true },
				{ "o|out=", v => outputFile = v },
			};

			try {
				var extra = opts.Parse(args);
				if (extra.Count != 1 || showHelp) {
					ShowHelp();
					return 1;
				}
				if (assemblies.Count == 0) {
					Console.WriteLine("No assemblies to merge");
					return 1;
				}
				inputFile = extra[0];
				outputFile = outputFile ?? inputFile;

				assemblies = assemblies.Select(a => Path.GetFullPath(a)).Distinct().ToList();
				assemblies.RemoveAll(a => Path.GetFileName(a) == Path.GetFileName(inputFile));
				var dup = (  from a in assemblies
				            group a by Path.GetFileName(a) into g
				            where g.Count() > 1
				           select g.Key)
				          .FirstOrDefault();

				if (dup != null) {
					Console.WriteLine("Duplicate assembly " + dup);
					return 1;
				}

				AssemblyDefinition asm;
				try {
					asm = AssemblyDefinition.ReadAssembly(inputFile);
				}
				catch (Exception ex) {
					Console.WriteLine("Error reading assembly " + inputFile + ": " + ex.Message);
					return 1;
				}

				if (!EmbedAssemblies(asm.MainModule, assemblies, debug))
					return 1;

				InsertAssemblyLoader(asm.MainModule);

				try {
					asm.Write(outputFile);
				}
				catch (Exception ex) {
					Console.WriteLine("Error writing to file " + outputFile + ": " + ex.Message);
					return 1;
				}

				return 0;
			}
			catch (OptionException ex) {
				Console.WriteLine(ex.Message);
				return 1;
			}
		}
	}
}
