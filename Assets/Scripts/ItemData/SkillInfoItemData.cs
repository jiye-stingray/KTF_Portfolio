using System.Collections.Generic;

public class SkillInfoItemData : ItemData
{
    public int _skillLevel;
    public int _activeLevel;
    public List<SkillDescriptionArg> _args;
    public bool IsOpen => _activeLevel >= _skillLevel;
}
