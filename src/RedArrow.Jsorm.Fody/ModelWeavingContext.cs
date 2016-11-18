using Mono.Cecil;
using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RedArrow.Jsorm
{
    public class ModelWeavingContext
    {
        public PropertyDefinition IdPropDef { get; private set; }

        public IEnumerable<PropertyDefinition> MappedAttributes { get; }
        public IEnumerable<PropertyDefinition> MappedHasOnes { get; }
        public IEnumerable<PropertyDefinition> MappedHasMany { get; }

        private TypeDefinition ModelTypeDef { get; }
        public TypeReference ModelTypeRef => ModelTypeDef;

        public Collection<FieldDefinition> Fields => ModelTypeDef.Fields;
        public Collection<MethodDefinition> Methods => ModelTypeDef.Methods;
        public Collection<PropertyDefinition> Properties => ModelTypeDef.Properties;

        public TypeReference SessionTypeRef { get; private set; }
        public FieldDefinition SessionField { get; private set; }

        public ModelWeavingContext(TypeDefinition modelTypeDef)
        {
            ModelTypeDef = modelTypeDef;

            GetMappedIdProperty();

            MappedAttributes = GetMappedProperties(Constants.Attributes.Property);
            MappedHasOnes = GetMappedProperties(Constants.Attributes.HasOne);
            MappedHasMany = GetMappedProperties(Constants.Attributes.HasMany);
        }

        public void AddSessionField(TypeDefinition sessionTypeDef)
        {
            if (SessionField != null) return;

            SessionTypeRef = ModelTypeDef.Module.ImportReference(sessionTypeDef);

            SessionField = new FieldDefinition(
                    "__jsorm__generated_session",
                    FieldAttributes.Private | FieldAttributes.NotSerialized,
                    SessionTypeRef);

            Fields.Add(SessionField);
        }

        private void GetMappedIdProperty()
        {
            IdPropDef = Properties.SingleOrDefault(x => x.CustomAttributes.ContainsAttribute(Constants.Attributes.Id));

            if (IdPropDef == null)
            {
                throw new Exception($"{ModelTypeDef.FullName} does not have an id property mapped");
            }

            if (IdPropDef.GetMethod?.ReturnType.FullName != "System.Guid")
            {
                throw new Exception($"{ModelTypeDef} id property must have a System.Guid getter");
            }
        }

        private IEnumerable<PropertyDefinition> GetMappedProperties(string attrFullName)
        {
            return ModelTypeDef.Properties
                .Where(x => x.HasCustomAttributes)
                .Where(p => p.CustomAttributes.ContainsAttribute(attrFullName))
                .ToArray();
        }

        public TypeReference ImportReference(TypeDefinition typeDef)
        {
            return ModelTypeDef.Module.ImportReference(typeDef);
        }

        public TypeReference ImportReference(TypeReference typeRef)
        {
            return ModelTypeDef.Module.ImportReference(typeRef);
        }

        public MethodReference ImportReference(MethodDefinition methDef)
        {
            return ModelTypeDef.Module.ImportReference(methDef);
        }

        public MethodReference ImportReference(GenericInstanceMethod genMethDef)
        {
            return ModelTypeDef.Module.ImportReference(genMethDef);
        }
    }
}