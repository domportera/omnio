using System;
using System.Collections.Generic;
using Godot;
using Utilities;

namespace NodeGraphEditor.UI;

public partial class TypeInSearch : LineEdit
{
    [Export] private ItemList _itemList;
    [Export] private bool _grabFocusOnVisibility = true;

    private bool _needsFocus = false;
    public event EventHandler<string>? ItemSelected;

    private Func<IEnumerable<string>>? _getAllPossibleItems;


    public void SetItems(Func<IEnumerable<string>> items)
    {
        _itemList.Clear();
        _getAllPossibleItems = items;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        TextChanged += UpdateSearch;
        _itemList.ItemSelected += OnItemSelected;
        VisibilityChanged += OnVisibilityChanged;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (_needsFocus)
        {
            if (Visible) // just in case we get hidden before we can grab focus
            {
                GrabFocus();
            }

            _needsFocus = false;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (!Visible)
            return;
        
        switch (@event)
        {
            case InputEventKey {Pressed: true} keyEvent:
            {
                if (keyEvent.Keycode == Key.Escape)
                {
                    Visible = false;
                }

                break;
            }
            
            case InputEventMouseButton {Pressed: true} mouseEvent:
            {
                var globalMousePos = mouseEvent.GlobalPosition;
                if (!GetGlobalRect().HasPoint(globalMousePos) && !_itemList.GetGlobalRect().HasPoint(globalMousePos))
                {
                    Visible = false;
                }

                break;
            }
        }
    }

    private void OnVisibilityChanged()
    {
        var visible = Visible;
        Editable = visible;
        UpdateSearch("");
        if (_grabFocusOnVisibility && visible)
        {
            // godot requires a frame to pass before we can grab focus
            _needsFocus = true;
        }
    }

    private void OnItemSelected(long index)
    {
        var itemName = _itemList.GetItemText((int)index);
        ItemSelected?.Invoke(this, itemName);
    }

    private unsafe void UpdateSearch(string text)
    {
        if (_getAllPossibleItems == null)
            return;

        _itemList.Clear();
        var searchLength = text.Length + 2;
        var search = stackalloc char[searchLength];

        search[0] = '*';
        search[searchLength - 1] = '*';

        for (int i = 0; i < text.Length; i++)
        {
            search[i + 1] = text[i];
        }

        var searchSpan = new ReadOnlySpan<char>(search, searchLength);

        foreach (var itemName in _getAllPossibleItems())
        {
            if (StringUtils.MatchesSearchFilter(itemName, searchSpan, true))
            {
                _itemList.AddItem(itemName);
            }
        }
    }
}