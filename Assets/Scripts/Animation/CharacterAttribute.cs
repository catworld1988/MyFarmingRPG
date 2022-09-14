
[System.Serializable]

//角色属性 结构体
public struct CharacterAttribute  //角色特性
{
    public CharacterPartAnimator characterPart;
    public PartVariantColour partVariantColour;
    public PartVariantType partVariantType;

    public CharacterAttribute(CharacterPartAnimator characterPart, PartVariantColour partVariantColour, PartVariantType partVariantType)
    {
        this.characterPart = characterPart;
        this.partVariantColour = partVariantColour;
        this.partVariantType = partVariantType;
    }
}
