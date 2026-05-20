using System;

[Serializable]
public class IngredientData : GameDataBase
{
    public string Name;
    public string Description;
    public string IconPath;
    public string Grade;           // "¿œπ›", "¡¡¿Ω", "»Ò±Õ", "Ω≈»≠"
    public int MaxStackCount;
}