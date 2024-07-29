using Godot;

namespace NodeGraphEditor.GraphNodeUI;

internal static class DefaultTextDisplay
{
    public static LabelDisplay CreateLabel(string text, HorizontalAlignment alignment)
    {
        var label = new Label();
        label.HorizontalAlignment = alignment;
        label.Text = text;
        label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        return new LabelDisplay(label);
    }
    
    public static LineEditDisplay CreateLineEdit(string placeholderText, HorizontalAlignment alignment)
    {
        var lineEdit = new LineEdit();
        
        lineEdit.PlaceholderText = placeholderText;
        lineEdit.Alignment = alignment;
        lineEdit.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        
        return new LineEditDisplay(lineEdit);
    }
}