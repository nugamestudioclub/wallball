extends HBoxContainer

class_name ComboItem

export(PackedScene) var input_item_file : PackedScene

var combo setget set_combo
var combo_progress : int = 0 setget set_combo_progress

var input_items : Array

func set_combo(new_combo : Combo):
	combo = new_combo
	for n in get_children():
		remove_child(n)
		n.queue_free()
	for input in combo.inputs:
		var input_item = input_item_file.instance()
		input_items.append(input_item)
		add_child(input_item)
		input_item.input_val = input

func set_combo_progress(val : int):
	combo_progress = val
	for i in input_items.size():
		if i < val:
			input_items[i].pressed = true
		else:
			input_items[i].pressed = false

