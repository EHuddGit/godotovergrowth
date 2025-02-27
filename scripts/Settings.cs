using Godot;
using System;
using System.Collections.Generic;

public partial class Settings : Control
{
	// Called when the node enters the scene tree for the first time.

	private string busName = "ping sound";
	private int busIndex;
	private PackedScene Inputbutton = ResourceLoader.Load<PackedScene>("res://scenes/HotKeyButton.tscn");
	private PackedScene menu = ResourceLoader.Load<PackedScene>("res://scenes/menu.tscn");
	VBoxContainer ActionList;
	private bool isRemapping = false;
	private KeyValuePair<string,string> actionToRemap = new KeyValuePair<string, string>();
	private Node remappingButton = new Node();

	private Dictionary<string,string> actionNames = new Dictionary<string, string>()
	{
		{"left","Left"},
		{"right","Right"},
		{"shoot","Shoot"},
		{"command","Command"},
		{"gather","Call"}
	};

	
	public override void _Ready()
	{
		busIndex = AudioServer.GetBusIndex(busName);
		var slider = GetNode<HSlider>("volumeSlider");
		var backButton = GetNode<Button>("Back");
		slider.DragEnded += emitSound;
		backButton.Pressed += Back;
		ActionList = GetNode<VBoxContainer>("ScrollContainer/ActionList");
		createActionList();
	}

	public void Back()
	{
		GetTree().ChangeSceneToPacked(menu);
		var scene_instance = menu.Instantiate();
		GetParent().AddChild(scene_instance);
		this.QueueFree();
	}

	public void createActionList()
	{
		InputMap.LoadFromProjectSettings();
		var sep = new VSeparator();
		sep.AddThemeConstantOverride("Seperation",30);

		foreach(var item in ActionList.GetChildren())
		{
			item.QueueFree();
		}
		ActionList.AddChild(sep);
		foreach(var action in actionNames)
		{
			GD.Print(action);
			var button = Inputbutton.Instantiate();
			Label actionLabel = (Label)button.FindChild("ActionLabel");
			Button inputButton = (Button)button.FindChild("ActionButton");

			actionLabel.Text = action.Value;
			var events = InputMap.ActionGetEvents(action.Key);

			if(events.Count > 0)
				inputButton.Text = events[0].AsText().TrimSuffix(" (Physical)");
			else
				inputButton.Text = "";

			inputButton.Pressed += () => inputRecord(button, action);
			ActionList.AddChild(button);
		}
		//ActionList.AddChild(sep);
		
	}
	public void inputRecord(Node Button,KeyValuePair<string,string> action)
	{
		if(!isRemapping)
		{
			isRemapping = true;
			actionToRemap = action;
			remappingButton = Button;
			((Button)Button.FindChild("ActionButton")).Text = "Press key to bind...";
		}
	}

	public override void _Input(InputEvent @event)
	{
		if(isRemapping)
		{
			if(@event is InputEventKey eventKey || (@event is InputEventMouseButton && @event.IsPressed()))
			{
				InputMap.ActionEraseEvents(actionToRemap.Key);
				InputMap.ActionAddEvent(actionToRemap.Key,@event);
				updateActionList(remappingButton, @event);

				isRemapping = false;
			}
		}

	}

	public void updateActionList(Node button,InputEvent @event)
	{
		((Button)remappingButton.FindChild("ActionButton")).Text = @event.AsText().TrimSuffix(" (Physical)");
	}
	public void emitSound(bool valueChanged)
	{
		var slider = GetNode<HSlider>("volumeSlider");
		AudioServer.SetBusVolumeDb(busIndex,(float)Mathf.LinearToDb(slider.Value));
		GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D").Play();

	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
