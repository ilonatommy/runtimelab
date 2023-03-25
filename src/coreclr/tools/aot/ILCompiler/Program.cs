// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Parsing;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using Internal.IL;
using Internal.TypeSystem;
using Internal.TypeSystem.Ecma;

using ILCompiler.Dataflow;
using ILLink.Shared;

using Debug = System.Diagnostics.Debug;
using InstructionSet = Internal.JitInterface.InstructionSet;

namespace ILCompiler
{
    internal sealed class Program
    {
        private readonly ILCompilerRootCommand _command;

        public Program(ILCompilerRootCommand command)
        {
            _command = command;

<<<<<<< HEAD
            if (Get(command.WaitForDebugger))
=======
        private void Help(string helpText)
        {
            Console.WriteLine();
            Console.Write(".NET Native IL Compiler");
            Console.Write(" ");
            Console.Write(typeof(Program).GetTypeInfo().Assembly.GetName().Version);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(helpText);
        }

        public static void ComputeDefaultOptions(out TargetOS os, out TargetArchitecture arch)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                os = TargetOS.Windows;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                os = TargetOS.Linux;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                os = TargetOS.OSX;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
                os = TargetOS.FreeBSD;
            else
                throw new NotImplementedException();

            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.X86:
                    arch = TargetArchitecture.X86;
                    break;
                case Architecture.X64:
                    arch = TargetArchitecture.X64;
                    break;
                case Architecture.Arm:
                    arch = TargetArchitecture.ARM;
                    break;
                case Architecture.Arm64:
                    arch = TargetArchitecture.ARM64;
                    break;
                default:
                    throw new NotImplementedException();
            }

        }

        private void InitializeDefaultOptions()
        {
            ComputeDefaultOptions(out _targetOS, out _targetArchitecture);
        }

        private ArgumentSyntax ParseCommandLine(string[] args)
        {
            var validReflectionDataOptions = new string[] { "all", "none" };

            IReadOnlyList<string> inputFiles = Array.Empty<string>();
            IReadOnlyList<string> referenceFiles = Array.Empty<string>();

            bool optimize = false;
            bool optimizeSpace = false;
            bool optimizeTime = false;

            bool waitForDebugger = false;
            AssemblyName name = typeof(Program).GetTypeInfo().Assembly.GetName();
            ArgumentSyntax argSyntax = ArgumentSyntax.Parse(args, syntax =>
            {
                syntax.ApplicationName = name.Name.ToString();

                // HandleHelp writes to error, fails fast with crash dialog and lacks custom formatting.
                syntax.HandleHelp = false;
                syntax.HandleErrors = true;

                syntax.DefineOption("h|help", ref _help, "Help message for ILC");
                syntax.DefineOptionList("r|reference", ref referenceFiles, "Reference file(s) for compilation");
                syntax.DefineOption("o|out", ref _outputFilePath, "Output file path");
                syntax.DefineOption("O", ref optimize, "Enable optimizations");
                syntax.DefineOption("Os", ref optimizeSpace, "Enable optimizations, favor code space");
                syntax.DefineOption("Ot", ref optimizeTime, "Enable optimizations, favor code speed");
                syntax.DefineOptionList("m|mibc", ref _mibcFilePaths, "Mibc file(s) for profile guided optimization"); ;
                syntax.DefineOption("g", ref _enableDebugInfo, "Emit debugging information");
                syntax.DefineOption("wasm", ref _isLlvmCodegen, "Compile for Web Assembly code-generation");
                syntax.DefineOption("llvm", ref _isLlvmCodegen, "Compile for LLVM code-generation");
                syntax.DefineOption("gdwarf-5", ref _useDwarf5, "Generate source-level debug information with dwarf version 5");
                syntax.DefineOption("nativelib", ref _nativeLib, "Compile as static or shared library");
                syntax.DefineOption("exportsfile", ref _exportsFile, "File to write exported method definitions");
                syntax.DefineOption("dgmllog", ref _dgmlLogFileName, "Save result of dependency analysis as DGML");
                syntax.DefineOption("fulllog", ref _generateFullDgmlLog, "Save detailed log of dependency analysis");
                syntax.DefineOption("scandgmllog", ref _scanDgmlLogFileName, "Save result of scanner dependency analysis as DGML");
                syntax.DefineOption("scanfulllog", ref _generateFullScanDgmlLog, "Save detailed log of scanner dependency analysis");
                syntax.DefineOption("verbose", ref _isVerbose, "Enable verbose logging");
                syntax.DefineOption("systemmodule", ref _systemModuleName, "System module name (default: System.Private.CoreLib)");
                syntax.DefineOption("multifile", ref _multiFile, "Compile only input files (do not compile referenced assemblies)");
                syntax.DefineOption("waitfordebugger", ref waitForDebugger, "Pause to give opportunity to attach debugger");
                syntax.DefineOption("resilient", ref _resilient, "Ignore unresolved types, methods, and assemblies. Defaults to false");
                syntax.DefineOptionList("codegenopt", ref _codegenOptions, "Define a codegen option");
                syntax.DefineOptionList("rdxml", ref _rdXmlFilePaths, "RD.XML file(s) for compilation");
                syntax.DefineOptionList("descriptor", ref _linkTrimFilePaths, "ILLinkTrim.Descriptor file(s) for compilation");
                syntax.DefineOption("map", ref _mapFileName, "Generate a map file");
                syntax.DefineOption("mstat", ref _mstatFileName, "Generate an mstat file");
                syntax.DefineOption("metadatalog", ref _metadataLogFileName, "Generate a metadata log file");
                syntax.DefineOption("nometadatablocking", ref _noMetadataBlocking, "Ignore metadata blocking for internal implementation details");
                syntax.DefineOption("completetypemetadata", ref _completeTypesMetadata, "Generate complete metadata for types");
                syntax.DefineOption("reflectiondata", ref _reflectionData, $"Reflection data to generate (one of: {string.Join(", ", validReflectionDataOptions)})");
                syntax.DefineOption("scanreflection", ref _scanReflection, "Scan IL for reflection patterns");
                syntax.DefineOption("scan", ref _useScanner, "Use IL scanner to generate optimized code (implied by -O)");
                syntax.DefineOption("noscan", ref _noScanner, "Do not use IL scanner to generate optimized code");
                syntax.DefineOption("ildump", ref _ilDump, "Dump IL assembly listing for compiler-generated IL");
                syntax.DefineOption("stacktracedata", ref _emitStackTraceData, "Emit data to support generating stack trace strings at runtime");
                syntax.DefineOption("methodbodyfolding", ref _methodBodyFolding, "Fold identical method bodies");
                syntax.DefineOptionList("initassembly", ref _initAssemblies, "Assembly(ies) with a library initializer");
                syntax.DefineOptionList("appcontextswitch", ref _appContextSwitches, "System.AppContext switches to set (format: 'Key=Value')");
                syntax.DefineOptionList("feature", ref _featureSwitches, "Feature switches to apply (format: 'Namespace.Name=[true|false]'");
                syntax.DefineOptionList("runtimeopt", ref _runtimeOptions, "Runtime options to set");
                syntax.DefineOption("parallelism", ref _parallelism, "Maximum number of threads to use during compilation");
                syntax.DefineOption("instruction-set", ref _instructionSet, "Instruction set to allow or disallow");
                syntax.DefineOption("guard", ref _guard, "Enable mitigations. Options: 'cf': CFG (Control Flow Guard, Windows only)");
                syntax.DefineOption("preinitstatics", ref _preinitStatics, "Interpret static constructors at compile time if possible (implied by -O)");
                syntax.DefineOption("nopreinitstatics", ref _noPreinitStatics, "Do not interpret static constructors at compile time");
                syntax.DefineOptionList("nowarn", ref _suppressedWarnings, "Disable specific warning messages");
                syntax.DefineOption("singlewarn", ref _singleWarn, "Generate single AOT/trimming warning per assembly");
                syntax.DefineOption("notrimwarn", ref _noTrimWarn, "Disable warnings related to trimming");
                syntax.DefineOption("noaotwarn", ref _noAotWarn, "Disable warnings related to AOT");
                syntax.DefineOptionList("singlewarnassembly", ref _singleWarnEnabledAssemblies, "Generate single AOT/trimming warning for given assembly");
                syntax.DefineOptionList("nosinglewarnassembly", ref _singleWarnDisabledAssemblies, "Expand AOT/trimming warnings for given assembly");
                syntax.DefineOptionList("directpinvoke", ref _directPInvokes, "PInvoke to call directly");
                syntax.DefineOptionList("directpinvokelist", ref _directPInvokeLists, "File with list of PInvokes to call directly");
                syntax.DefineOptionList("wasmimport", ref _wasmImports, "WebAssembly import module names for PInvoke functions");
                syntax.DefineOptionList("wasmimportlist", ref _wasmImportsLists, "File with list of WebAssembly import module names for PInvoke functions");
                syntax.DefineOption("maxgenericcycle", ref _maxGenericCycle, "Max depth of generic cycle");
                syntax.DefineOptionList("root", ref _rootedAssemblies, "Fully generate given assembly");
                syntax.DefineOptionList("conditionalroot", ref _conditionallyRootedAssemblies, "Fully generate given assembly if it's used");
                syntax.DefineOptionList("trim", ref _trimmedAssemblies, "Trim the specified assembly");
                syntax.DefineOption("defaultrooting", ref _rootDefaultAssemblies, "Root assemblies that are not marked [IsTrimmable]");

                syntax.DefineOption("targetarch", ref _targetArchitectureStr, "Target architecture for cross compilation");
                syntax.DefineOption("targetos", ref _targetOSStr, "Target OS for cross compilation");
                syntax.DefineOption("jitpath", ref _jitPath, "Path to JIT compiler library");

                syntax.DefineOption("singlemethodtypename", ref _singleMethodTypeName, "Single method compilation: assembly-qualified name of the owning type");
                syntax.DefineOption("singlemethodname", ref _singleMethodName, "Single method compilation: name of the method");
                syntax.DefineOptionList("singlemethodgenericarg", ref _singleMethodGenericArgs, "Single method compilation: generic arguments to the method");

                syntax.DefineOption("make-repro-path", ref _makeReproPath, "Path where to place a repro package");

                syntax.DefineParameterList("in", ref inputFiles, "Input file(s) to compile");
            });

            if (_help)
            {
                List<string> extraHelp = new List<string>();

                extraHelp.Add("Options may be passed on the command line, or via response file. On the command line switch values may be specified by passing " +
                    "the option followed by a space followed by the value of the option, or by specifying a : between option and switch value. A response file " +
                    "is specified by passing the @ symbol before the response file name. In a response file all options must be specified on their own lines, and " +
                    "only the : syntax for switches is supported.");

                extraHelp.Add("");

                extraHelp.Add("Use the '--' option to disambiguate between input files that have begin with -- and options. After a '--' option, all arguments are " +
                    "considered to be input files. If no input files begin with '--' then this option is not necessary.");

                extraHelp.Add("");

                string[] ValidArchitectures = new string[] { "arm", "arm64", "x86", "x64" };
                string[] ValidOS = new string[] { "windows", "linux", "osx" };

                Program.ComputeDefaultOptions(out TargetOS defaultOs, out TargetArchitecture defaultArch);

                extraHelp.Add(String.Format("Valid switches for {0} are: '{1}'. The default value is '{2}'", "--targetos", String.Join("', '", ValidOS), defaultOs.ToString().ToLowerInvariant()));

                extraHelp.Add("");

                extraHelp.Add(String.Format("Valid switches for {0} are: '{1}'. The default value is '{2}'", "--targetarch", String.Join("', '", ValidArchitectures), defaultArch.ToString().ToLowerInvariant()));

                extraHelp.Add("");

                extraHelp.Add("The allowable values for the --instruction-set option are described in the table below. Each architecture has a different set of valid " +
                    "instruction sets, and multiple instruction sets may be specified by separating the instructions sets by a ','. For example 'avx2,bmi,lzcnt'");

                foreach (string arch in ValidArchitectures)
                {
                    StringBuilder archString = new StringBuilder();

                    archString.Append(arch);
                    archString.Append(": ");

                    TargetArchitecture targetArch = GetTargetArchitectureFromArg(arch);
                    bool first = true;
                    foreach (var instructionSet in Internal.JitInterface.InstructionSetFlags.ArchitectureToValidInstructionSets(targetArch))
                    {
                        // Only instruction sets with are specifiable should be printed to the help text
                        if (instructionSet.Specifiable)
                        {
                            if (first)
                            {
                                first = false;
                            }
                            else
                            {
                                archString.Append(", ");
                            }
                            archString.Append(instructionSet.Name);
                        }
                    }

                    extraHelp.Add(archString.ToString());
                }

                extraHelp.Add("");
                extraHelp.Add("The following CPU names are predefined groups of instruction sets and can be used in --instruction-set too:");
                extraHelp.Add(string.Join(", ", Internal.JitInterface.InstructionSetFlags.AllCpuNames));

                argSyntax.ExtraHelpParagraphs = extraHelp;
            }

            if (waitForDebugger)
>>>>>>> origin/feature/NativeAOT-LLVM
            {
                Console.WriteLine("Waiting for debugger to attach. Press ENTER to continue");
                Console.ReadLine();
            }
        }

        private IReadOnlyCollection<MethodDesc> CreateInitializerList(CompilerTypeSystemContext context)
        {
            List<ModuleDesc> assembliesWithInitializers = new List<ModuleDesc>();

            // Build a list of assemblies that have an initializer that needs to run before
            // any user code runs.
            foreach (string initAssemblyName in Get(_command.InitAssemblies))
            {
                ModuleDesc assembly = context.ResolveAssembly(new AssemblyName(initAssemblyName), throwIfNotFound: true);
                assembliesWithInitializers.Add(assembly);
            }

            var libraryInitializers = new LibraryInitializers(context, assembliesWithInitializers);

            List<MethodDesc> initializerList = new List<MethodDesc>(libraryInitializers.LibraryInitializerMethods);

            // If there are any AppContext switches the user wishes to enable, generate code that sets them.
            string[] appContextSwitches = Get(_command.AppContextSwitches);
            if (appContextSwitches.Length > 0)
            {
                MethodDesc appContextInitMethod = new Internal.IL.Stubs.StartupCode.AppContextInitializerMethod(
                    context.GeneratedAssembly.GetGlobalModuleType(), appContextSwitches);
                initializerList.Add(appContextInitMethod);
            }

            return initializerList;
        }

        public int Run()
        {
            string outputFilePath = Get(_command.OutputFilePath);
            if (outputFilePath == null)
                throw new CommandLineException("Output filename must be specified (/out <file>)");

            TargetArchitecture targetArchitecture = Get(_command.TargetArchitecture);
            TargetOS targetOS = Get(_command.TargetOS);
            InstructionSetSupport instructionSetSupport = Helpers.ConfigureInstructionSetSupport(Get(_command.InstructionSet), targetArchitecture, targetOS,
                "Unrecognized instruction set {0}", "Unsupported combination of instruction sets: {0}/{1}");

            string systemModuleName = Get(_command.SystemModuleName);
            string reflectionData = Get(_command.ReflectionData);
            bool supportsReflection = reflectionData != "none" && systemModuleName == Helpers.DefaultSystemModule;

            //
            // Initialize type system context
            //

            SharedGenericsMode genericsMode = SharedGenericsMode.CanonicalReferenceTypes;

            var simdVectorLength = instructionSetSupport.GetVectorTSimdVector();
            var targetAbi = TargetAbi.NativeAot;
            var targetDetails = new TargetDetails(targetArchitecture, targetOS, targetAbi, simdVectorLength);
            CompilerTypeSystemContext typeSystemContext =
                new CompilerTypeSystemContext(targetDetails, genericsMode, supportsReflection ? DelegateFeature.All : 0, Get(_command.MaxGenericCycle));

            //
            // TODO: To support our pre-compiled test tree, allow input files that aren't managed assemblies since
            // some tests contain a mixture of both managed and native binaries.
            //
            // See: https://github.com/dotnet/corert/issues/2785
            //
            // When we undo this hack, replace the foreach with
            //  typeSystemContext.InputFilePaths = _command.Result.GetValueForArgument(inputFilePaths);
            //
            Dictionary<string, string> inputFilePaths = new Dictionary<string, string>();
            foreach (var inputFile in _command.Result.GetValueForArgument(_command.InputFilePaths))
            {
                try
                {
                    var module = typeSystemContext.GetModuleFromPath(inputFile.Value);
                    inputFilePaths.Add(inputFile.Key, inputFile.Value);
                }
                catch (TypeSystemException.BadImageFormatException)
                {
                    // Keep calm and carry on.
                }
            }

            typeSystemContext.InputFilePaths = inputFilePaths;
            typeSystemContext.ReferenceFilePaths = Get(_command.ReferenceFiles);
            if (!typeSystemContext.InputFilePaths.ContainsKey(systemModuleName)
                && !typeSystemContext.ReferenceFilePaths.ContainsKey(systemModuleName))
                throw new CommandLineException($"System module {systemModuleName} does not exists. Make sure that you specify --systemmodule");

            typeSystemContext.SetSystemModule(typeSystemContext.GetModuleForSimpleName(systemModuleName));

            if (typeSystemContext.InputFilePaths.Count == 0)
                throw new CommandLineException("No input files specified");

            SecurityMitigationOptions securityMitigationOptions = 0;
            string guard = Get(_command.Guard);
            if (StringComparer.OrdinalIgnoreCase.Equals(guard, "cf"))
            {
                if (targetOS != TargetOS.Windows)
                {
                    throw new CommandLineException($"Control flow guard only available on Windows");
                }

                securityMitigationOptions = SecurityMitigationOptions.ControlFlowGuardAnnotations;
            }
            else if (!string.IsNullOrEmpty(guard))
            {
                throw new CommandLineException($"Unrecognized mitigation option '{guard}'");
            }

            //
            // Initialize compilation group and compilation roots
            //

            // Single method mode?
            MethodDesc singleMethod = CheckAndParseSingleMethodModeArguments(typeSystemContext);

            CompilationModuleGroup compilationGroup;
            List<ICompilationRootProvider> compilationRoots = new List<ICompilationRootProvider>();
            bool multiFile = Get(_command.MultiFile);
            if (singleMethod != null)
            {
                // Compiling just a single method
                compilationGroup = new SingleMethodCompilationModuleGroup(singleMethod);
                compilationRoots.Add(new SingleMethodRootProvider(singleMethod));
            }
            else
            {
                // Either single file, or multifile library, or multifile consumption.
                EcmaModule entrypointModule = null;
                bool systemModuleIsInputModule = false;
                foreach (var inputFile in typeSystemContext.InputFilePaths)
                {
                    EcmaModule module = typeSystemContext.GetModuleFromPath(inputFile.Value);

                    if (module.PEReader.PEHeaders.IsExe)
                    {
                        if (entrypointModule != null)
                            throw new Exception("Multiple EXE modules");
                        entrypointModule = module;
                    }

                    if (module == typeSystemContext.SystemModule)
                        systemModuleIsInputModule = true;

                    compilationRoots.Add(new ExportedMethodsRootProvider(module));
                }

                string[] runtimeOptions = Get(_command.RuntimeOptions);
                if (entrypointModule != null)
                {
                    compilationRoots.Add(new MainMethodRootProvider(entrypointModule, CreateInitializerList(typeSystemContext)));
                    compilationRoots.Add(new RuntimeConfigurationRootProvider(runtimeOptions));
                    compilationRoots.Add(new ExpectedIsaFeaturesRootProvider(instructionSetSupport));
                }

                bool nativeLib = Get(_command.NativeLib);
                if (multiFile)
                {
                    List<EcmaModule> inputModules = new List<EcmaModule>();

                    foreach (var inputFile in typeSystemContext.InputFilePaths)
                    {
                        EcmaModule module = typeSystemContext.GetModuleFromPath(inputFile.Value);

                        if (entrypointModule == null)
                        {
                            // This is a multifile production build - we need to root all methods
                            compilationRoots.Add(new LibraryRootProvider(module));
                        }
                        inputModules.Add(module);
                    }

                    compilationGroup = new MultiFileSharedCompilationModuleGroup(typeSystemContext, inputModules);
                }
                else
                {
                    if (entrypointModule == null && !nativeLib)
                        throw new Exception("No entrypoint module");

                    if (!systemModuleIsInputModule)
                        compilationRoots.Add(new ExportedMethodsRootProvider((EcmaModule)typeSystemContext.SystemModule));
                    compilationGroup = new SingleFileCompilationModuleGroup();
                }

                if (nativeLib)
                {
                    // Set owning module of generated native library startup method to compiler generated module,
                    // to ensure the startup method is included in the object file during multimodule mode build
                    compilationRoots.Add(new NativeLibraryInitializerRootProvider(typeSystemContext.GeneratedAssembly, CreateInitializerList(typeSystemContext)));
                    compilationRoots.Add(new RuntimeConfigurationRootProvider(runtimeOptions));
                    compilationRoots.Add(new ExpectedIsaFeaturesRootProvider(instructionSetSupport));
                }

                foreach (var rdXmlFilePath in Get(_command.RdXmlFilePaths))
                {
                    compilationRoots.Add(new RdXmlRootProvider(typeSystemContext, rdXmlFilePath));
                }

                foreach (var linkTrimFilePath in Get(_command.LinkTrimFilePaths))
                {
                    if (!File.Exists(linkTrimFilePath))
                        throw new CommandLineException($"'{linkTrimFilePath}' doesn't exist");
                    compilationRoots.Add(new ILCompiler.DependencyAnalysis.TrimmingDescriptorNode(linkTrimFilePath));
                }
            }

            // Root whatever assemblies were specified on the command line
            string[] rootedAssemblies = Get(_command.RootedAssemblies);
            foreach (var rootedAssembly in rootedAssemblies)
            {
                // For compatibility with IL Linker, the parameter could be a file name or an assembly name.
                // This is the logic IL Linker uses to decide how to interpret the string. Really.
                EcmaModule module = File.Exists(rootedAssembly)
                    ? typeSystemContext.GetModuleFromPath(rootedAssembly)
                    : typeSystemContext.GetModuleForSimpleName(rootedAssembly);

                // We only root the module type. The rest will fall out because we treat rootedAssemblies
                // same as conditionally rooted ones and here we're fulfilling the condition ("something is used").
                compilationRoots.Add(
                    new GenericRootProvider<ModuleDesc>(module,
                    (ModuleDesc module, IRootingServiceProvider rooter) => rooter.AddCompilationRoot(module.GetGlobalModuleType(), "Command line root")));
            }

            //
            // Compile
            //
            CompilationBuilder builder;
            bool isLlvmCodegen = targetArchitecture == TargetArchitecture.Wasm32 ||
                                 targetArchitecture == TargetArchitecture.Wasm64;
            if (isLlvmCodegen)
            {
                builder = new LLVMCodegenCompilationBuilder(typeSystemContext, compilationGroup);
            }
            else
            {
                builder = new RyuJitCompilationBuilder(typeSystemContext, compilationGroup);
            }

            string compilationUnitPrefix = multiFile ? Path.GetFileNameWithoutExtension(outputFilePath) : "";
            builder.UseCompilationUnitPrefix(compilationUnitPrefix);

            string[] mibcFilePaths = Get(_command.MibcFilePaths);
            if (mibcFilePaths.Length > 0)
                ((RyuJitCompilationBuilder)builder).UseProfileData(mibcFilePaths);

            string jitPath = Get(_command.JitPath);
            if (!string.IsNullOrEmpty(jitPath))
                ((RyuJitCompilationBuilder)builder).UseJitPath(jitPath);

            PInvokeILEmitterConfiguration pinvokePolicy;
            if (isLlvmCodegen)
            {
                pinvokePolicy = new DirectPInvokePolicy();
            }
            else
            {
                pinvokePolicy = new ConfigurablePInvokePolicy(typeSystemContext.Target,
                    Get(_command.DirectPInvokes), Get(_command.DirectPInvokeLists));
            }
            ConfigurableWasmImportPolicy wasmImportPolicy = new ConfigurableWasmImportPolicy(Get(_command.WasmImport), Get(_command.WasmImportList));

            ILProvider ilProvider = new NativeAotILProvider();

            List<KeyValuePair<string, bool>> featureSwitches = new List<KeyValuePair<string, bool>>();
            foreach (var switchPair in Get(_command.FeatureSwitches))
            {
                string[] switchAndValue = switchPair.Split('=');
                if (switchAndValue.Length != 2
                    || !bool.TryParse(switchAndValue[1], out bool switchValue))
                    throw new CommandLineException($"Unexpected feature switch pair '{switchPair}'");
                featureSwitches.Add(new KeyValuePair<string, bool>(switchAndValue[0], switchValue));
            }
            ilProvider = new FeatureSwitchManager(ilProvider, featureSwitches);

            var suppressedWarningCategories = new List<string>();
            if (Get(_command.NoTrimWarn))
                suppressedWarningCategories.Add(MessageSubCategory.TrimAnalysis);
            if (Get(_command.NoAotWarn))
                suppressedWarningCategories.Add(MessageSubCategory.AotAnalysis);

            var logger = new Logger(Console.Out, ilProvider, Get(_command.IsVerbose), ProcessWarningCodes(Get(_command.SuppressedWarnings)),
                Get(_command.SingleWarn), Get(_command.SingleWarnEnabledAssemblies), Get(_command.SingleWarnDisabledAssemblies), suppressedWarningCategories);
            CompilerGeneratedState compilerGeneratedState = new CompilerGeneratedState(ilProvider, logger);

            var stackTracePolicy = Get(_command.EmitStackTraceData) ?
                (StackTraceEmissionPolicy)new EcmaMethodStackTraceEmissionPolicy() : new NoStackTraceEmissionPolicy();

            MetadataBlockingPolicy mdBlockingPolicy;
            ManifestResourceBlockingPolicy resBlockingPolicy;
            UsageBasedMetadataGenerationOptions metadataGenerationOptions = default;
            if (supportsReflection)
            {
                mdBlockingPolicy = Get(_command.NoMetadataBlocking) ?
                    new NoMetadataBlockingPolicy() : new BlockedInternalsBlockingPolicy(typeSystemContext);

                resBlockingPolicy = new ManifestResourceBlockingPolicy(featureSwitches);

                metadataGenerationOptions |= UsageBasedMetadataGenerationOptions.AnonymousTypeHeuristic;
                if (Get(_command.CompleteTypesMetadata))
                    metadataGenerationOptions |= UsageBasedMetadataGenerationOptions.CompleteTypesOnly;
                if (Get(_command.ScanReflection))
                    metadataGenerationOptions |= UsageBasedMetadataGenerationOptions.ReflectionILScanning;
                if (reflectionData == "all")
                    metadataGenerationOptions |= UsageBasedMetadataGenerationOptions.CreateReflectableArtifacts;
                if (Get(_command.RootDefaultAssemblies))
                    metadataGenerationOptions |= UsageBasedMetadataGenerationOptions.RootDefaultAssemblies;
            }
            else
            {
                mdBlockingPolicy = new FullyBlockedMetadataBlockingPolicy();
                resBlockingPolicy = new FullyBlockedManifestResourceBlockingPolicy();
            }

            DynamicInvokeThunkGenerationPolicy invokeThunkGenerationPolicy = new DefaultDynamicInvokeThunkGenerationPolicy();

            var flowAnnotations = new ILLink.Shared.TrimAnalysis.FlowAnnotations(logger, ilProvider, compilerGeneratedState);

            MetadataManager metadataManager = new UsageBasedMetadataManager(
                    compilationGroup,
                    typeSystemContext,
                    mdBlockingPolicy,
                    resBlockingPolicy,
                    Get(_command.MetadataLogFileName),
                    stackTracePolicy,
                    invokeThunkGenerationPolicy,
                    flowAnnotations,
                    metadataGenerationOptions,
                    logger,
                    featureSwitches,
                    Get(_command.ConditionallyRootedAssemblies),
                    rootedAssemblies,
                    Get(_command.TrimmedAssemblies));

            InteropStateManager interopStateManager = new InteropStateManager(typeSystemContext.GeneratedAssembly);
            InteropStubManager interopStubManager = new UsageBasedInteropStubManager(interopStateManager, pinvokePolicy, logger);

            // Unless explicitly opted in at the command line, we enable scanner for retail builds by default.
            // We also don't do this for multifile because scanner doesn't simulate inlining (this would be
            // fixable by using a CompilationGroup for the scanner that has a bigger worldview, but
            // let's cross that bridge when we get there).
<<<<<<< HEAD
            // For LLVM the scanner is always on to enable precomputed vtable slots
            bool useScanner = Get(_command.UseScanner) || isLlvmCodegen ||
                (_command.OptimizationMode != OptimizationMode.None && !multiFile);
=======
            bool useScanner = _useScanner ||
                (_optimizationMode != OptimizationMode.None && !_multiFile);
>>>>>>> origin/feature/NativeAOT-LLVM

            useScanner &= !Get(_command.NoScanner);

            // Enable static data preinitialization in optimized builds.
            bool preinitStatics = Get(_command.PreinitStatics) ||
                (_command.OptimizationMode != OptimizationMode.None && !multiFile);
            preinitStatics &= !Get(_command.NoPreinitStatics);

            var preinitManager = new PreinitializationManager(typeSystemContext, compilationGroup, ilProvider, preinitStatics);
            builder
                .UseILProvider(ilProvider)
                .UsePreinitializationManager(preinitManager);

#if DEBUG
            List<TypeDesc> scannerConstructedTypes = null;
            List<MethodDesc> scannerCompiledMethods = null;
#endif

            int parallelism = Get(_command.Parallelism);
            if (useScanner)
            {
                // Run the scanner in a separate stack frame so that there's no dangling references to
                // it once we're done with it and it can be garbage collected.
                RunScanner();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            void RunScanner()
            {
                ILScannerBuilder scannerBuilder = builder.GetILScannerBuilder()
                    .UseCompilationRoots(compilationRoots)
                    .UseMetadataManager(metadataManager)
                    .UseParallelism(parallelism)
                    .UseInteropStubManager(interopStubManager)
                    .UseLogger(logger);

                string scanDgmlLogFileName = Get(_command.ScanDgmlLogFileName);
                if (scanDgmlLogFileName != null)
                    scannerBuilder.UseDependencyTracking(Get(_command.GenerateFullScanDgmlLog) ?
                            DependencyTrackingLevel.All : DependencyTrackingLevel.First);

                IILScanner scanner = scannerBuilder.ToILScanner();

                ILScanResults scanResults = scanner.Scan();

#if DEBUG
                scannerCompiledMethods = new List<MethodDesc>(scanResults.CompiledMethodBodies);
                scannerConstructedTypes = new List<TypeDesc>(scanResults.ConstructedEETypes);
#endif

                if (scanDgmlLogFileName != null)
                    scanResults.WriteDependencyLog(scanDgmlLogFileName);

                metadataManager = ((UsageBasedMetadataManager)metadataManager).ToAnalysisBasedMetadataManager();

                interopStubManager = scanResults.GetInteropStubManager(interopStateManager, pinvokePolicy);

                // If we have a scanner, feed the vtable analysis results to the compilation.
                // This could be a command line switch if we really wanted to.
                builder.UseVTableSliceProvider(scanResults.GetVTableLayoutInfo());

                // If we have a scanner, feed the generic dictionary results to the compilation.
                // This could be a command line switch if we really wanted to.
                builder.UseGenericDictionaryLayoutProvider(scanResults.GetDictionaryLayoutInfo());

                // If we have a scanner, we can drive devirtualization using the information
                // we collected at scanning time (effectively sealing unsealed types if possible).
                // This could be a command line switch if we really wanted to.
                builder.UseDevirtualizationManager(scanResults.GetDevirtualizationManager());

                // If we use the scanner's result, we need to consult it to drive inlining.
                // This prevents e.g. devirtualizing and inlining methods on types that were
                // never actually allocated.
                builder.UseInliningPolicy(scanResults.GetInliningPolicy());

                // Use an error provider that prevents us from re-importing methods that failed
                // to import with an exception during scanning phase. We would see the same failure during
                // compilation, but before RyuJIT gets there, it might ask questions that we don't
                // have answers for because we didn't scan the entire method.
                builder.UseMethodImportationErrorProvider(scanResults.GetMethodImportationErrorProvider());
            }

            string ilDump = Get(_command.IlDump);
            DebugInformationProvider debugInfoProvider = Get(_command.EnableDebugInfo) ?
                (ilDump == null ? new DebugInformationProvider() : new ILAssemblyGeneratingMethodDebugInfoProvider(ilDump, new EcmaOnlyDebugInformationProvider())) :
                new NullDebugInformationProvider();

            string dgmlLogFileName = Get(_command.DgmlLogFileName);
            DependencyTrackingLevel trackingLevel = dgmlLogFileName == null ?
                DependencyTrackingLevel.None : (Get(_command.GenerateFullDgmlLog) ?
                    DependencyTrackingLevel.All : DependencyTrackingLevel.First);

            compilationRoots.Add(metadataManager);
            compilationRoots.Add(interopStubManager);

            builder
                .UseInstructionSetSupport(instructionSetSupport)
                .UseBackendOptions(Get(_command.CodegenOptions))
                .UseMethodBodyFolding(enable: Get(_command.MethodBodyFolding))
                .UseParallelism(parallelism)
                .UseMetadataManager(metadataManager)
                .UseInteropStubManager(interopStubManager)
                .UseLogger(logger)
                .UseDependencyTracking(trackingLevel)
                .UseCompilationRoots(compilationRoots)
                .UseOptimizationMode(_command.OptimizationMode)
                .UseSecurityMitigationOptions(securityMitigationOptions)
                .UseDebugInfoProvider(debugInfoProvider)
                .UseWasmImportPolicy(wasmImportPolicy)
                .UseDwarf5(Get(_command.UseDwarf5));

            builder.UseResilience(Get(_command.Resilient));

            ICompilation compilation = builder.ToCompilation();

            string mapFileName = Get(_command.MapFileName);
            string mstatFileName = Get(_command.MstatFileName);

            List<ObjectDumper> dumpers = new List<ObjectDumper>();

            if (mapFileName != null)
                dumpers.Add(new XmlObjectDumper(mapFileName));

            if (mstatFileName != null)
                dumpers.Add(new MstatObjectDumper(mstatFileName, typeSystemContext));

            CompilationResults compilationResults = compilation.Compile(outputFilePath, ObjectDumper.Compose(dumpers));
            string exportsFile = Get(_command.ExportsFile);
            if (exportsFile != null)
            {
                ExportsFileWriter defFileWriter = new ExportsFileWriter(typeSystemContext, exportsFile);
                foreach (var compilationRoot in compilationRoots)
                {
                    if (compilationRoot is ExportedMethodsRootProvider provider)
                        defFileWriter.AddExportedMethods(provider.ExportedMethods);
                }

                defFileWriter.EmitExportedMethods();
            }

            typeSystemContext.LogWarnings(logger);

            if (dgmlLogFileName != null)
                compilationResults.WriteDependencyLog(dgmlLogFileName);

#if DEBUG
            if (scannerConstructedTypes != null)
            {
                // If the scanner and compiler don't agree on what to compile, the outputs of the scanner might not actually be usable.
                // We are going to check this two ways:
                // 1. The methods and types generated during compilation are a subset of method and types scanned
                // 2. The methods and types scanned are a subset of methods and types compiled (this has a chance to hold for unoptimized builds only).

                // Check that methods and types generated during compilation are a subset of method and types scanned
                bool scanningFail = false;
                DiffCompilationResults(ref scanningFail, compilationResults.CompiledMethodBodies, scannerCompiledMethods,
                    "Methods", "compiled", "scanned", method => !(method.GetTypicalMethodDefinition() is EcmaMethod) || IsRelatedToInvalidInput(method));
                DiffCompilationResults(ref scanningFail, compilationResults.ConstructedEETypes, scannerConstructedTypes,
                    "EETypes", "compiled", "scanned", type => !(type.GetTypeDefinition() is EcmaType));

                static bool IsRelatedToInvalidInput(MethodDesc method)
                {
                    // RyuJIT is more sensitive to invalid input and might detect cases that the scanner didn't have trouble with.
                    // If we find logic related to compiling fallback method bodies (methods that just throw) that got compiled
                    // but not scanned, it's usually fine. If it wasn't fine, we would probably crash before getting here.
                    return method.OwningType is MetadataType mdType
                        && mdType.Module == method.Context.SystemModule
                        && (mdType.Name.EndsWith("Exception") || mdType.Namespace.StartsWith("Internal.Runtime"));
                }

                // If optimizations are enabled, the results will for sure not match in the other direction due to inlining, etc.
                // But there's at least some value in checking the scanner doesn't expand the universe too much in debug.
                if (_command.OptimizationMode == OptimizationMode.None)
                {
                    // Check that methods and types scanned are a subset of methods and types compiled

                    // If we find diffs here, they're not critical, but still might be causing a Size on Disk regression.
                    bool dummy = false;

                    // We additionally skip methods in SIMD module because there's just too many intrisics to handle and IL scanner
                    // doesn't expand them. They would show up as noisy diffs.
                    DiffCompilationResults(ref dummy, scannerCompiledMethods, compilationResults.CompiledMethodBodies,
                    "Methods", "scanned", "compiled", method => !(method.GetTypicalMethodDefinition() is EcmaMethod) || method.OwningType.IsIntrinsic);
                    DiffCompilationResults(ref dummy, scannerConstructedTypes, compilationResults.ConstructedEETypes,
                        "EETypes", "scanned", "compiled", type => !(type.GetTypeDefinition() is EcmaType));
                }

                if (scanningFail)
                    throw new Exception("Scanning failure");
            }
#endif

            if (debugInfoProvider is IDisposable)
                ((IDisposable)debugInfoProvider).Dispose();

            preinitManager.LogStatistics(logger);

            return 0;
        }

        private static void DiffCompilationResults<T>(ref bool result, IEnumerable<T> set1, IEnumerable<T> set2, string prefix,
            string set1name, string set2name, Predicate<T> filter)
        {
            HashSet<T> diff = new HashSet<T>(set1);
            diff.ExceptWith(set2);

            // TODO: move ownership of compiler-generated entities to CompilerTypeSystemContext.
            // https://github.com/dotnet/corert/issues/3873
            diff.RemoveWhere(filter);

            if (diff.Count > 0)
            {
                result = true;

                Console.WriteLine($"*** {prefix} {set1name} but not {set2name}:");

                foreach (var d in diff)
                {
                    Console.WriteLine(d.ToString());
                }
            }
        }

        private static TypeDesc FindType(CompilerTypeSystemContext context, string typeName)
        {
            ModuleDesc systemModule = context.SystemModule;

            TypeDesc foundType = systemModule.GetTypeByCustomAttributeTypeName(typeName, false, (typeDefName, module, throwIfNotFound) =>
            {
                return (MetadataType)context.GetCanonType(typeDefName)
                    ?? CustomAttributeTypeNameParser.ResolveCustomAttributeTypeDefinitionName(typeDefName, module, throwIfNotFound);
            });
            if (foundType == null)
                throw new CommandLineException($"Type '{typeName}' not found");

            return foundType;
        }

        private MethodDesc CheckAndParseSingleMethodModeArguments(CompilerTypeSystemContext context)
        {
            string singleMethodName = Get(_command.SingleMethodName);
            string singleMethodTypeName = Get(_command.SingleMethodTypeName);
            string[] singleMethodGenericArgs = Get(_command.SingleMethodGenericArgs);

            if (singleMethodName == null && singleMethodTypeName == null && singleMethodGenericArgs.Length == 0)
                return null;

            if (singleMethodName == null || singleMethodTypeName == null)
                throw new CommandLineException("Both method name and type name are required parameters for single method mode");

            TypeDesc owningType = FindType(context, singleMethodTypeName);

            // TODO: allow specifying signature to distinguish overloads
            MethodDesc method = owningType.GetMethod(singleMethodName, null);
            if (method == null)
                throw new CommandLineException($"Method '{singleMethodName}' not found in '{singleMethodTypeName}'");

            if (method.HasInstantiation != (singleMethodGenericArgs != null) ||
                (method.HasInstantiation && (method.Instantiation.Length != singleMethodGenericArgs.Length)))
            {
                throw new CommandLineException(
                    $"Expected {method.Instantiation.Length} generic arguments for method '{singleMethodName}' on type '{singleMethodTypeName}'");
            }

            if (method.HasInstantiation)
            {
                List<TypeDesc> genericArguments = new List<TypeDesc>();
                foreach (var argString in singleMethodGenericArgs)
                    genericArguments.Add(FindType(context, argString));
                method = method.MakeInstantiatedMethod(genericArguments.ToArray());
            }

            return method;
        }

        private static IEnumerable<int> ProcessWarningCodes(IEnumerable<string> warningCodes)
        {
            foreach (string value in warningCodes)
            {
                string[] values = value.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string id in values)
                {
                    if (!id.StartsWith("IL", StringComparison.Ordinal) || !ushort.TryParse(id.AsSpan(2), out ushort code))
                        continue;

                    yield return code;
                }
            }
        }

        private T Get<T>(Option<T> option) => _command.Result.GetValueForOption(option);

        private static int Main(string[] args) =>
            new CommandLineBuilder(new ILCompilerRootCommand(args))
                .UseTokenReplacer(Helpers.TryReadResponseFile)
                .UseVersionOption("-v")
                .UseHelp(context => context.HelpBuilder.CustomizeLayout(ILCompilerRootCommand.GetExtendedHelp))
                .UseParseErrorReporting()
                .Build()
                .Invoke(args);
    }
}
