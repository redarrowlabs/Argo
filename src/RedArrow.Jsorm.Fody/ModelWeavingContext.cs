using Mono.Cecil;
using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using RedArrow.Jsorm.Extensions;

namespace RedArrow.Jsorm
{
    public class ModelWeavingContext
    {
        public PropertyDefinition IdPropDef { get; private set; }

        public IEnumerable<PropertyDefinition> MappedAttributes { get; }
        public IEnumerable<PropertyDefinition> MappedHasOnes { get; }
        public IEnumerable<PropertyDefinition> MappedHasManys { get; }

        private TypeDefinition ModelTypeDef { get; }
        public TypeReference ModelTypeRef => ModelTypeDef;

        public Collection<FieldDefinition> Fields => ModelTypeDef.Fields;
        public Collection<MethodDefinition> Methods => ModelTypeDef.Methods;
        public Collection<PropertyDefinition> Properties => ModelTypeDef.Properties;
        
        public FieldDefinition SessionField { get; set; }

        public ModelWeavingContext(TypeDefinition modelTypeDef)
        {
            ModelTypeDef = modelTypeDef;

            GetMappedIdProperty();

            MappedAttributes = GetMappedProperties(Constants.Attributes.Property);
            MappedHasOnes = GetMappedProperties(Constants.Attributes.HasOne);
            MappedHasManys = GetMappedProperties(Constants.Attributes.HasMany);
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
		
        public TypeReference ImportReference(TypeReference typeRef)
        {
            return ModelTypeDef.Module.ImportReference(typeRef);
        }

        public MethodReference ImportReference(MethodReference methRef)
        {
            return ModelTypeDef.Module.ImportReference(methRef);
		}

		public MethodReference ImportReference(MethodReference methRef, IGenericParameterProvider context)
		{
			return ModelTypeDef.Module.ImportReference(methRef, context);
		}
	}
}