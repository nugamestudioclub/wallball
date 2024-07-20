extends Resource

class_name ComboListData

export var combo_list : Array setget set_combo_list
var completion_list : Array

func set_combo_list(input : Array):
	combo_list = input
	completion_list.clear()
	for c in input:
		completion_list.append(0)
