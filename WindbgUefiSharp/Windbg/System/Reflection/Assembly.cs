﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// Changes made by Joshua Wierenga.

using System.Configuration.Assemblies;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security;

namespace System.Reflection
{
    //TODO Add ISerializable
    public abstract partial class Assembly : ICustomAttributeProvider//, ISerializable
    {
        //TODO Add Dictionary<T> and List<T>
        //private static readonly Dictionary<string, Assembly> s_loadfile = new Dictionary<string, Assembly>();
        //private static readonly List<string> s_loadFromAssemblyList = new List<string>();
        //TODO Use
        //private static bool s_loadFromHandlerSet;
        //private static int s_cachedSerializationSwitch;

        protected Assembly() { }

        //TODO Add IEnumerable<T>, TypeInfo, GetTypes, Type.GetTypeInfo and SR.Format
        /*public virtual IEnumerable<TypeInfo> DefinedTypes
        {
            [RequiresUnreferencedCode("Types might be removed")]
            get
            {
                Type[] types = GetTypes();
                TypeInfo[] typeinfos = new TypeInfo[types.Length];
                for (int i = 0; i < types.Length; i++)
                {
                    TypeInfo typeinfo = types[i].GetTypeInfo();
                    if (typeinfo == null)
                        throw new NotSupportedException(SR.Format(SR.NotSupported_NoTypeInfo, types[i].FullName));

                    typeinfos[i] = typeinfo;
                }
                return typeinfos;
            }
        }*/

        //TODO Add Array.Copy
        /*[RequiresUnreferencedCode("Types might be removed")]
        public virtual Type[] GetTypes()
        {
            Module[] m = GetModules(false);
            if (m.Length == 1)
            {
                return m[0].GetTypes();
            }

            int finalLength = 0;
            Type[][] moduleTypes = new Type[m.Length][];

            for (int i = 0; i < moduleTypes.Length; i++)
            {
                moduleTypes[i] = m[i].GetTypes();
                finalLength += moduleTypes[i].Length;
            }

            int current = 0;
            Type[] ret = new Type[finalLength];
            for (int i = 0; i < moduleTypes.Length; i++)
            {
                int length = moduleTypes[i].Length;
                Array.Copy(moduleTypes[i], 0, ret, current, length);
                current += length;
            }

            return ret;
        }*/

        //TODO Add IEnumerable<T>
        /*public virtual IEnumerable<Type> ExportedTypes
        {
            [RequiresUnreferencedCode("Types might be removed")]
            get => GetExportedTypes();
        }*/

        [RequiresUnreferencedCode("Types might be removed")]
        public virtual Type[] GetExportedTypes() { throw NotImplemented.ByDesign; }
        [RequiresUnreferencedCode("Types might be removed")]
        public virtual Type[] GetForwardedTypes() { throw NotImplemented.ByDesign; }

        internal const string ThrowingMessageInRAF = "This member throws an exception for assemblies embedded in a single-file app";

        //TODO Add RequiresAssemblyFilesAttribute
        //[RequiresAssemblyFiles(ThrowingMessageInRAF)]
        public virtual string? CodeBase => throw NotImplemented.ByDesign;
        public virtual MethodInfo? EntryPoint => throw NotImplemented.ByDesign;
        public virtual string? FullName => throw NotImplemented.ByDesign;
        public virtual string ImageRuntimeVersion => throw NotImplemented.ByDesign;
        public virtual bool IsDynamic => false;
        public virtual string Location => throw NotImplemented.ByDesign;
        public virtual bool ReflectionOnly => throw NotImplemented.ByDesign;
        public virtual bool IsCollectible => true;

        public virtual ManifestResourceInfo? GetManifestResourceInfo(string resourceName) { throw NotImplemented.ByDesign; }
        public virtual string[] GetManifestResourceNames() { throw NotImplemented.ByDesign; }
        //TODO Add Stream
        /*public virtual Stream? GetManifestResourceStream(string name) { throw NotImplemented.ByDesign; }
        public virtual Stream? GetManifestResourceStream(Type type, string name) { throw NotImplemented.ByDesign; }*/

        public bool IsFullyTrusted => true;

        public virtual AssemblyName GetName() => GetName(copiedName: false);
        public virtual AssemblyName GetName(bool copiedName) { throw NotImplemented.ByDesign; }

        [RequiresUnreferencedCode("Types might be removed")]
        public virtual Type? GetType(string name) => GetType(name, throwOnError: false, ignoreCase: false);
        [RequiresUnreferencedCode("Types might be removed")]
        public virtual Type? GetType(string name, bool throwOnError) => GetType(name, throwOnError: throwOnError, ignoreCase: false);
        [RequiresUnreferencedCode("Types might be removed")]
        public virtual Type? GetType(string name, bool throwOnError, bool ignoreCase) { throw NotImplemented.ByDesign; }

        public virtual bool IsDefined(Type attributeType, bool inherit) { throw NotImplemented.ByDesign; }

        //TODO Add IEnumerable<T> and IList<T>
        /*public virtual IEnumerable<CustomAttributeData> CustomAttributes => GetCustomAttributesData();
        public virtual IList<CustomAttributeData> GetCustomAttributesData() { throw NotImplemented.ByDesign; }*/

        public virtual object[] GetCustomAttributes(bool inherit) { throw NotImplemented.ByDesign; }
        public virtual object[] GetCustomAttributes(Type attributeType, bool inherit) { throw NotImplemented.ByDesign; }

        //TODO Add RequiresAssemblyFilesAttribute and AssemblyName.EscapeCodeBase
        /*[RequiresAssemblyFiles(ThrowingMessageInRAF)]
        //public virtual string EscapedCodeBase => AssemblyName.EscapeCodeBase(CodeBase)*/

        //TODO Add Activator and ActivatorImplementation
        /*[RequiresUnreferencedCode("Assembly.CreateInstance is not supported with trimming. Use Type.GetType instead.")]
        public object? CreateInstance(string typeName) => CreateInstance(typeName, false, BindingFlags.Public | BindingFlags.Instance, binder: null, args: null, culture: null, activationAttributes: null);

        [RequiresUnreferencedCode("Assembly.CreateInstance is not supported with trimming. Use Type.GetType instead.")]
        public object? CreateInstance(string typeName, bool ignoreCase) => CreateInstance(typeName, ignoreCase, BindingFlags.Public | BindingFlags.Instance, binder: null, args: null, culture: null, activationAttributes: null);

        [RequiresUnreferencedCode("Assembly.CreateInstance is not supported with trimming. Use Type.GetType instead.")]
        public virtual object? CreateInstance(string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder? binder, object[]? args, CultureInfo? culture, object[]? activationAttributes)
        {
            Type? t = GetType(typeName, throwOnError: false, ignoreCase: ignoreCase);
            if (t == null)
                return null;

            return Activator.CreateInstance(t, bindingAttr, binder, args, culture, activationAttributes);
        }*/

        //TODO Add ModuleResolveEventHandler
        //TODO Add Nullable`1' and Support Delegate
        //public virtual event ModuleResolveEventHandler? ModuleResolve { add { throw NotImplemented.ByDesign; } remove { throw NotImplemented.ByDesign; } }

        public virtual Module ManifestModule => throw NotImplemented.ByDesign;
        public virtual Module? GetModule(string name) { throw NotImplemented.ByDesign; }


        public Module[] GetModules() => GetModules(getResourceModules: false);
        public virtual Module[] GetModules(bool getResourceModules) { throw NotImplemented.ByDesign; }

        //TODO Add IEnumerable<T>
        //public virtual IEnumerable<Module> Modules => GetLoadedModules(getResourceModules: true);
        public Module[] GetLoadedModules() => GetLoadedModules(getResourceModules: false);
        public virtual Module[] GetLoadedModules(bool getResourceModules) { throw NotImplemented.ByDesign; }

        [RequiresUnreferencedCode("Assembly references might be removed")]
        public virtual AssemblyName[] GetReferencedAssemblies() { throw NotImplemented.ByDesign; }

        public virtual Assembly GetSatelliteAssembly(CultureInfo culture) { throw NotImplemented.ByDesign; }
        public virtual Assembly GetSatelliteAssembly(CultureInfo culture, Version? version) { throw NotImplemented.ByDesign; }

        //TODO Add FileStream and RequiresAssemblyFilesAttribute
        /*[RequiresAssemblyFiles(ThrowingMessageInRAF)]
        public virtual FileStream? GetFile(string name) { throw NotImplemented.ByDesign; }
        [RequiresAssemblyFiles(ThrowingMessageInRAF)]
        public virtual FileStream[] GetFiles() => GetFiles(getResourceModules: false);
        /*[RequiresAssemblyFiles(ThrowingMessageInRAF)]
        public virtual FileStream[] GetFiles(bool getResourceModules) { throw NotImplemented.ByDesign; }*/

        //TODO Add SerializationInfo and StreamingContext
        //public virtual void GetObjectData(SerializationInfo info, StreamingContext context) { throw NotImplemented.ByDesign; }

        public override string ToString()
        {
            return FullName ?? base.ToString()!;
        }

        /*
          Returns true if the assembly was loaded from the global assembly cache.
        */
        [Obsolete(Obsoletions.GlobalAssemblyCacheMessage, DiagnosticId = Obsoletions.GlobalAssemblyCacheDiagId, UrlFormat = Obsoletions.SharedUrlFormat)]
        public virtual bool GlobalAssemblyCache => throw NotImplemented.ByDesign;
        public virtual long HostContext => throw NotImplemented.ByDesign;

        public override bool Equals(object? o) => base.Equals(o);
        public override int GetHashCode() => base.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Assembly? left, Assembly? right)
        {
            // Test "right" first to allow branch elimination when inlined for null checks (== null)
            // so it can become a simple test
            if (right is null)
            {
                // return true/false not the test result https://github.com/dotnet/runtime/issues/4207
                return (left is null) ? true : false;
            }

            // Try fast reference equality and opposite null check prior to calling the slower virtual Equals
            if ((object?)left == (object)right)
            {
                return true;
            }

            return (left is null) ? false : left.Equals(right);
        }

        public static bool operator !=(Assembly? left, Assembly? right) => !(left == right);

        public static string CreateQualifiedName(string? assemblyName, string? typeName) => typeName + ", " + assemblyName;

        //TODO Add Type.Module
        /*public static Assembly? GetAssembly(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            Module m = type.Module;
            if (m == null)
                return null;
            else
                return m.Assembly;
        }*/

        // internal test hook
        //TODO Use
        //private static bool s_forceNullEntryPoint;

        //TODO Add GetEntryAssemblyInternal
        /*public static Assembly? GetEntryAssembly()
        {
            if (s_forceNullEntryPoint)
                return null;

            return GetEntryAssemblyInternal();
        }*/

        //TODO Add SerializationInfo, AssemblyLoadContext and IndividualAssemblyLoadContext
        /*[RequiresUnreferencedCode("Types and members the loaded assembly depends on might be removed")]
        public static Assembly Load(byte[] rawAssembly) => Load(rawAssembly, rawSymbolStore: null);

        // Loads the assembly with a COFF based IMAGE containing
        // an emitted assembly. The assembly is loaded into a fully isolated ALC with resolution fully deferred to the AssemblyLoadContext.Default.
        // The second parameter is the raw bytes representing the symbol store that matches the assembly.
        [RequiresUnreferencedCode("Types and members the loaded assembly depends on might be removed")]
        public static Assembly Load(byte[] rawAssembly, byte[]? rawSymbolStore)
        {
            if (rawAssembly == null)
                throw new ArgumentNullException(nameof(rawAssembly));

            if (rawAssembly.Length == 0)
                throw new BadImageFormatException(SR.BadImageFormat_BadILFormat);

            SerializationInfo.ThrowIfDeserializationInProgress("AllowAssembliesFromByteArrays",
                ref s_cachedSerializationSwitch);

            AssemblyLoadContext alc = new IndividualAssemblyLoadContext("Assembly.Load(byte[], ...)");
            return alc.InternalLoad(rawAssembly, rawSymbolStore);
        }*/

        //TODO Add PathInternal, SR.Format, Path, s_loadfile, AssemblyLoadContext and IndividualAssemblyLoadContext
        /*[RequiresUnreferencedCode("Types and members the loaded assembly depends on might be removed")]
        public static Assembly LoadFile(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (PathInternal.IsPartiallyQualified(path))
            {
                throw new ArgumentException(SR.Format(SR.Argument_AbsolutePathRequired, path), nameof(path));
            }

            string normalizedPath = Path.GetFullPath(path);

            Assembly? result;
            lock (s_loadfile)
            {
                if (s_loadfile.TryGetValue(normalizedPath, out result))
                    return result;

                AssemblyLoadContext alc = new IndividualAssemblyLoadContext($"Assembly.LoadFile({normalizedPath})");
                result = alc.LoadFromAssemblyPath(normalizedPath);
                s_loadfile.Add(normalizedPath, result);
            }
            return result;
        }*/

        //TODO Add ResolveEventArgs, AssemblyLoadContext, Path and s_loadFromAssemblyList
        /*[RequiresUnreferencedCode("Types and members the loaded assembly depends on might be removed")]
        [UnconditionalSuppressMessage("SingleFile", "IL3000:Avoid accessing Assembly file path when publishing as a single file",
            Justification = "The assembly is loaded by specifying a path outside of the single-file bundle, the location of the path will not be empty if the path exist, otherwise it will be handled as null")]
        private static Assembly? LoadFromResolveHandler(object? sender, ResolveEventArgs args)
        {
            Assembly? requestingAssembly = args.RequestingAssembly;
            if (requestingAssembly == null)
            {
                return null;
            }

            // Requesting assembly for LoadFrom is always loaded in defaultContext - proceed only if that
            // is the case.
            if (AssemblyLoadContext.Default != AssemblyLoadContext.GetLoadContext(requestingAssembly))
                return null;

            // Get the path where requesting assembly lives and check if it is in the list
            // of assemblies for which LoadFrom was invoked.
            string requestorPath = Path.GetFullPath(requestingAssembly.Location);
            if (string.IsNullOrEmpty(requestorPath))
                return null;

            lock (s_loadFromAssemblyList)
            {
                // If the requestor assembly was not loaded using LoadFrom, exit.
                if (!s_loadFromAssemblyList.Contains(requestorPath))
                {
#if CORECLR
                    if (AssemblyLoadContext.IsTracingEnabled())
                    {
                        AssemblyLoadContext.TraceAssemblyLoadFromResolveHandlerInvoked(args.Name, false, requestorPath, null);
                    }
#endif // CORECLR
                    return null;
                }
            }

            // Requestor assembly was loaded using loadFrom, so look for its dependencies
            // in the same folder as it.
            // Form the name of the assembly using the path of the assembly that requested its load.
            AssemblyName requestedAssemblyName = new AssemblyName(args.Name!);
            string requestedAssemblyPath = Path.Combine(Path.GetDirectoryName(requestorPath)!, requestedAssemblyName.Name + ".dll");
#if CORECLR
            if (AssemblyLoadContext.IsTracingEnabled())
            {
                AssemblyLoadContext.TraceAssemblyLoadFromResolveHandlerInvoked(args.Name, true, requestorPath, requestedAssemblyPath);
            }
#endif // CORECLR
            try
            {
                // Load the dependency via LoadFrom so that it goes through the same path of being in the LoadFrom list.
                return Assembly.LoadFrom(requestedAssemblyPath);
            }
            catch (FileNotFoundException)
            {
                // Catch FileNotFoundException when attempting to resolve assemblies via this handler to account for missing assemblies.
                return null;
            }
        }*/

        //TODO Add Path, s_loadFromAssemblyList, AssemblyLoadContext and LoadFromResolveHandler
        /*[RequiresUnreferencedCode("Types and members the loaded assembly depends on might be removed")]
        public static Assembly LoadFrom(string assemblyFile)
        {
            if (assemblyFile == null)
                throw new ArgumentNullException(nameof(assemblyFile));

            string fullPath = Path.GetFullPath(assemblyFile);

            if (!s_loadFromHandlerSet)
            {
                lock (s_loadFromAssemblyList)
                {
                    if (!s_loadFromHandlerSet)
                    {
                        AssemblyLoadContext.AssemblyResolve += LoadFromResolveHandler!;
                        s_loadFromHandlerSet = true;
                    }
                }
            }

            // Add the path to the LoadFrom path list which we will consult
            // before handling the resolves in our handler.
            lock (s_loadFromAssemblyList)
            {
                if (!s_loadFromAssemblyList.Contains(fullPath))
                {
                    s_loadFromAssemblyList.Add(fullPath);
                }
            }

            return AssemblyLoadContext.Default.LoadFromAssemblyPath(fullPath);
        }*/

        [RequiresUnreferencedCode("Types and members the loaded assembly depends on might be removed")]
        public static Assembly LoadFrom(string assemblyFile, byte[]? hashValue, AssemblyHashAlgorithm hashAlgorithm)
        {
            throw new NotSupportedException(SR.NotSupported_AssemblyLoadFromHash);
        }

        //TODO Add LoadFrom(string)
        /*[RequiresUnreferencedCode("Types and members the loaded assembly depends on might be removed")]
        public static Assembly UnsafeLoadFrom(string assemblyFile) => LoadFrom(assemblyFile);*/

        [RequiresUnreferencedCode("Types and members the loaded module depends on might be removed")]
        public Module LoadModule(string moduleName, byte[]? rawModule) => LoadModule(moduleName, rawModule, null);
        [RequiresUnreferencedCode("Types and members the loaded module depends on might be removed")]
        public virtual Module LoadModule(string moduleName, byte[]? rawModule, byte[]? rawSymbolStore) { throw NotImplemented.ByDesign; }

        [Obsolete(Obsoletions.ReflectionOnlyLoadingMessage, DiagnosticId = Obsoletions.ReflectionOnlyLoadingDiagId, UrlFormat = Obsoletions.SharedUrlFormat)]
        [RequiresUnreferencedCode("Types and members the loaded assembly depends on might be removed")]
        public static Assembly ReflectionOnlyLoad(byte[] rawAssembly) { throw new PlatformNotSupportedException(SR.PlatformNotSupported_ReflectionOnly); }
        [Obsolete(Obsoletions.ReflectionOnlyLoadingMessage, DiagnosticId = Obsoletions.ReflectionOnlyLoadingDiagId, UrlFormat = Obsoletions.SharedUrlFormat)]
        [RequiresUnreferencedCode("Types and members the loaded assembly depends on might be removed")]
        public static Assembly ReflectionOnlyLoad(string assemblyString) { throw new PlatformNotSupportedException(SR.PlatformNotSupported_ReflectionOnly); }
        [Obsolete(Obsoletions.ReflectionOnlyLoadingMessage, DiagnosticId = Obsoletions.ReflectionOnlyLoadingDiagId, UrlFormat = Obsoletions.SharedUrlFormat)]
        [RequiresUnreferencedCode("Types and members the loaded assembly depends on might be removed")]
        public static Assembly ReflectionOnlyLoadFrom(string assemblyFile) { throw new PlatformNotSupportedException(SR.PlatformNotSupported_ReflectionOnly); }

        public virtual SecurityRuleSet SecurityRuleSet => SecurityRuleSet.None;
    }
}