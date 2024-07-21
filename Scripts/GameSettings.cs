using Godot;
using System;

public class GameSettings : Resource
{
	[Export]
	public int highScore = 0;
	
	[Export]
	public float volume = 0.7f;
}
