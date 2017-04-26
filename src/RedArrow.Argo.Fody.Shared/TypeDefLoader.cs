using System;
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

        private TypeDefinition _hasOneAttributeTypeDef;
        private TypeDefinition _hasManyAttributeTypeDef;
        private TypeDefinition _loadStrategyTypeDef;

        private TypeDefinition _sessionTypeDef;
        private TypeDefinition _resourceIdentifierTypeDef;

        private MethodDefinition _session_DisposedGetter;
        private MethodDefinition _session_GetId;
        private MethodDefinition _session_GetAttribute;
        private MethodDefinition _session_GetGenericEnumerable;
        private MethodDefinition _session_SetGenericEnumerable;
        private MethodDefinition _session_GetGenericCollection;
        private MethodDefinition _session_SetGenericCollection;
        
        private int _stringComparison_ordinal;
        private MethodDefinition _string_equals;
        private TypeDefinition _equalityComparerTypeDef;
        private MethodDefinition _object_equals;

        private void LoadTypeDefinitions()
        {
            var argoAssemblyDef = AssemblyResolver.Resolve(AssemblyNameReference.Parse("RedArrow.Argo"));

            _hasOneAttributeTypeDef = argoAssemblyDef.MainModule.GetType("RedArrow.Argo.Attributes.HasOneAttribute");
            _hasManyAttributeTypeDef = argoAssemblyDef.MainModule.GetType("RedArrow.Argo.Attributes.HasManyAttribute");
            _loadStrategyTypeDef = argoAssemblyDef.MainModule.GetType("RedArrow.Argo.Attributes.LoadStrategy");

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
            _session_GetId = _sessionTypeDef
                .Methods
                .Single(x => x.Name == "GetId");
            _session_GetAttribute = _sessionTypeDef
                .Methods
                .Single(x => x.Name == "GetAttribute");
            _session_GetGenericEnumerable = _sessionTypeDef
                .Methods
                .Single(x => x.Name == "GetGenericEnumerable");
            _session_SetGenericEnumerable = _sessionTypeDef
                .Methods
                .Single(x => x.Name == "SetGenericEnumerable");
            _session_GetGenericCollection = _sessionTypeDef
                .Methods
                .Single(x => x.Name == "GetGenericCollection");
            _session_SetGenericCollection = _sessionTypeDef
                .Methods
                .Single(x => x.Name == "SetGenericCollection");
            
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