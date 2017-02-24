﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Mono.Cecil;

namespace RedArrow.Argo
{
    public partial class ModuleWeaver
    {
        private MethodDefinition _compilerGeneratedAttribute;
        private MethodDefinition _debuggerBrowsableAttribute;
        private TypeDefinition _debuggerBrowsableStateTypeDef;

        private TypeDefinition _sessionTypeDef;
        private TypeDefinition _resourceIdentifierTypeDef;

        private MethodDefinition _session_DisposedGetter;
        private MethodDefinition _session_GetGenericEnumerable;
        private MethodDefinition _session_SetGenericEnumerable;
        private MethodDefinition _session_GetGenericCollection;
        private MethodDefinition _session_SetGenericCollection;
        
        //private TypeDefinition _genericIEnumerableTypeDef;
        //private TypeDefinition _genericICollectionTypeDef;

        private int _stringComparison_ordinal;
        private MethodDefinition _string_equals;
        private TypeDefinition _equalityComparerTypeDef;
        private MethodDefinition _object_equals;

        private void LoadTypeDefinitions()
        {
            var argoAssemblyDef = AssemblyResolver.Resolve("RedArrow.Argo");
            _sessionTypeDef = argoAssemblyDef.MainModule.GetType("RedArrow.Argo.Session.IModelSession");
            _resourceIdentifierTypeDef = argoAssemblyDef.MainModule.GetType("RedArrow.Argo.Model.IResourceIdentifier");

            _compilerGeneratedAttribute = argoAssemblyDef.MainModule
                .ImportReference(typeof(CompilerGeneratedAttribute).GetConstructor(new Type[0]))
                .Resolve();
            _debuggerBrowsableAttribute = argoAssemblyDef.MainModule
                .ImportReference(typeof(DebuggerBrowsableAttribute).GetConstructor(new [] {typeof(DebuggerBrowsableState) }))
                .Resolve();
            _debuggerBrowsableStateTypeDef = argoAssemblyDef.MainModule
                .ImportReference(typeof(DebuggerBrowsableState))
                .Resolve();

            _session_DisposedGetter = _sessionTypeDef
                .Properties
                .Single(x => x.Name == "Disposed")
                .GetMethod;
            _session_GetGenericEnumerable = _sessionTypeDef
                .Methods
                .SingleOrDefault(x => x.Name == "GetGenericEnumerable");
            _session_SetGenericEnumerable = _sessionTypeDef
                .Methods
                .SingleOrDefault(x => x.Name == "SetGenericEnumerable");
            _session_GetGenericCollection = _sessionTypeDef
                .Methods
                .SingleOrDefault(x => x.Name == "GetGenericCollection");
            _session_SetGenericCollection = _sessionTypeDef
                .Methods
                .SingleOrDefault(x => x.Name == "SetGenericCollection");
            
            //_genericIEnumerableTypeDef = argoAssemblyDef.MainModule
            //    .ImportReference(typeof(IEnumerable<>))
            //    .Resolve();
            //_genericICollectionTypeDef = argoAssemblyDef.MainModule
            //    .ImportReference(typeof(ICollection<>))
            //    .Resolve();

            _string_equals = ModuleDefinition
                .TypeSystem
                .String
                .Resolve()
                .Methods
                .First(x => x.IsStatic &&
                            x.Name == "Equals" &&
                            x.Parameters.Count == 3 &&
                            x.Parameters[0].ParameterType.Name == "String" &&
                            x.Parameters[1].ParameterType.Name == "String" &&
                            x.Parameters[2].ParameterType.Name == "StringComparison");

            _stringComparison_ordinal = (int)argoAssemblyDef.MainModule
                .ImportReference(typeof(StringComparison))
                .Resolve()
                .Fields
                .First(x => x.Name == "Ordinal")
                .Constant;

            _equalityComparerTypeDef = argoAssemblyDef.MainModule
                .ImportReference(typeof(EqualityComparer<>))
                .Resolve();

            _object_equals = ModuleDefinition
                .TypeSystem
                .Object
                .Resolve()
                .Methods
                .First(x => x.Name == "Equals" && x.Parameters.Count == 2);
        }
    }
}