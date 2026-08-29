namespace LocalFileSorter.Ui.Theme;

public enum PartState
{
    Normal,
    Hover,
    Active,
    Disabled,
}

public readonly record struct PartKey(UiPart Part, PartState State);
