# Make sure to change inheritence if using buttons instead of labels
extends Label

var input_val : String setget set_input_val
var pressed : bool setget set_pressed

# Change set methods if using buttons instead of labels
func set_input_val(val : String):
	input_val = val
	text = val

func set_pressed(val : bool):
	pressed = val
	if val:
		modulate = Color.red
	else:
		modulate = Color.white
