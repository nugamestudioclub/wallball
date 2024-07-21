using Godot;
using System.Collections.Generic;

public class ComboConfig : Resource
{
    [Export]
    public List<string> names = new List<string>();
}
