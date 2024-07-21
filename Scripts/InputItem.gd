# Make sure to change inheritence if using buttons instead of labels
extends Label

var input_val : String setget set_input_val
var pressed : bool setget set_pressed

func _ready():
	modulate = Color("#b3b0ff")

# Change set methods if using buttons instead of labels
func set_input_val(val : String):
	input_val = val
	text = val

func set_pressed(val : bool):
	pressed = val
	if val:
		modulate = Color("#e7f37f")
	else:
		modulate = Color("#b3b0ff")

