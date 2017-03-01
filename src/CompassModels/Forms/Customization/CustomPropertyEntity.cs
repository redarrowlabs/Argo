namespace RedArrow.Compass.CareTeam.CaseManagement.Model.Forms.Customization
{
    /// <summary>
    /// Emulates a typed property on a domain model.
    /// </summary>
    public class CustomPropertyEntity
    {
        public string PropertyName { get; set; }
        public FieldEntity Field { get; set; }
    }
}
