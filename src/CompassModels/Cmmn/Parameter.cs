using System;
using RedArrow.Argo.Attributes;

namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Cmmn
{
    public interface Parameter : CmmnElement
    {
        string Name { get; set; }
    }

    /// <summary>
    /// The class CaseParameter is used to model the inputs and outputs of Cases and Tasks.
    /// </summary>
    public interface CaseParameter : Parameter
    {
        /// <summary>
        /// A reference to a CaseFileItem.
        /// </summary>
        CaseFileItem Binding { get; set; }

        /// <summary>
        /// An optional Expression to further refine the binding of the CaseParameter to the CaseFileItem, that it is referenced by the bindingRef of the CaseParameter.
        /// </summary>
        //public BindingRefinementExpression BindingRefinementExpression { get; set; }
    }

    [Model("case-parameter-input")]
    public class InputCaseParameter : CaseParameter
    {
        [Id]
        public Guid Id { get; set; }
        [Property]
        public string Name { get; set; }
        [HasOne]
        public CaseFileItem Binding { get; set; }
        [Property]
        public string BindingRefinementExpresion { get; set; }
    }

    [Model("case-parameter-output")]
    public class OutputCaseParameter : CaseParameter
    {
        [Id]
        public Guid Id { get; set; }
        [Property]
        public string Name { get; set; }
        [HasOne]
        public CaseFileItem Binding { get; set; }
        [Property]
        public string BindingRefinementExpresion { get; set; }
    }
}
