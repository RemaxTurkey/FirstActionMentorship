namespace Data.Entities;

public enum AttributeDataType
{
    String = 0,
    Integer = 1,
    Decimal = 2,
    Boolean = 3,
    DateTime = 4
}

public class ComponentTypeAttribute : Entity
{
    public string Name { get; set; }
    
    public AttributeDataType DataType { get; set; } = AttributeDataType.String;
    
    public ICollection<ComponentTypeAttributeAssoc> ComponentTypeAttributeAssocs { get; set; }
}