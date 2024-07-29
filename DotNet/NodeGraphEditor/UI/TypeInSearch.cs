using Godot;
using System;
using System.Collections.Generic;
using OperatorCore;

public partial class TypeInSearch : LineEdit
{
	[Export] private ItemList _itemList;
	
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
	}

	private void OnItemSelected(long index)
	{
		var itemName = _itemList.GetItemText((int)index);
		ItemSelected?.Invoke(this, itemName);
	}

	private unsafe void UpdateSearch(string text)
	{
		if(_getAllPossibleItems == null)
			return;
		
		_itemList.Clear();
		var searchLength = text.Length + 2;
		var search = stackalloc char[searchLength];
		
		search[0] = '*';
		search[searchLength - 1] = '*';
		
		for(int i = 0; i < text.Length; i++)
		{
			search[i + 1] = text[i];
		}
		
		var searchSpan = new ReadOnlySpan<char>(search, searchLength);

		foreach(var itemName in _getAllPossibleItems())
		{
			if (StringUtils.MatchesSearchFilter(itemName, searchSpan, true))
			{
				_itemList.AddItem(itemName);
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
