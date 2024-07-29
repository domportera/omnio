using System;
using Godot;

namespace NodeGraphEditor.GraphNodeUI;

// an interface that represents a Control with modifiable text
// so that we can handle different types of controls that have text in a uniform way
public interface ITextDisplay
{
    public string Text { get; set; }
    string Name { get; set; }
    public void SetTextSilently(string text);

    public event Action<string>? TextChanged;
    public void Dispose();
}

public interface ITextDisplay<out T> : ITextDisplay where T : Control
{
    public T Control { get; }
    void ITextDisplay.Dispose() => Control.Dispose();

    string ITextDisplay.Name
    {
        get => Control.Name;
        set => Control.Name = value;
    }
}

public sealed class LabelDisplay(Label control) : ITextDisplay<Label>
{
    public Label Control { get; } = control;

    public void SetTextSilently(string text) => Control.Text = text;

    public string Text
    {
        get => Control.Text;
        set
        {
            Control.Text = value;
            TextChanged?.Invoke(value);
        }
    }

    public static implicit operator Label(LabelDisplay display) => display.Control;
    public event Action<string>? TextChanged;
}

public sealed class LineEditDisplay : ITextDisplay<LineEdit>
{
    public LineEditDisplay(LineEdit control)
    {
        Control = control;
        var callable = Callable.From<string>((args) => TextChanged?.Invoke(args));
        control.Connect(LineEdit.SignalName.TextChanged, callable);
    }

    public LineEdit Control { get; }

    public string Text
    {
        get => Control.Text;
        set
        {
            Control.Text = value;
            TextChanged?.Invoke(value);
        }
    }

    public void SetTextSilently(string text) => Control.Text = text;

    public event Action<string>? TextChanged;
    public static implicit operator LineEdit(LineEditDisplay display) => display.Control;
}

public sealed class TextEditDisplay : ITextDisplay<TextEdit>
{
    public TextEditDisplay(TextEdit control)
    {
        Control = control;
        var callable = Callable.From(() => TextChanged?.Invoke(control.Text));
        control.Connect(TextEdit.SignalName.TextChanged, callable);
    }

    public TextEdit Control { get; }

    public string Text
    {
        get => Control.Text;
        set
        {
            Control.Text = value;
            TextChanged?.Invoke(value);
        }
    }

    public void SetTextSilently(string text) => Control.Text = text;

    public event Action<string>? TextChanged;
    public static implicit operator TextEdit(TextEditDisplay display) => display.Control;
}