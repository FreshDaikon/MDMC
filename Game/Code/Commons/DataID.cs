using Godot;
using Godot.Collections;

[Tool][GlobalClass]
public partial class DataID : Resource
{
    private int id = -1;

    [Export]
    public int Id {
        get { return id; }
        set {
            id = value;
        } 
    }   

    public DataID ()
    {
        if(id == -1)
        {
            id = (int)ResourceUid.CreateId();
        }        
    }

    public override void _ValidateProperty(Dictionary property)
    {
        if (property["name"].AsStringName() == PropertyName.Id )
        {
            var usage = property["usage"].As<PropertyUsageFlags>() | PropertyUsageFlags.ReadOnly;
            property["usage"] = (int)usage;
        }
    }
}