extends Node

class_name ComboHandler

signal recieved_input(combo_compare)
signal scored_points(points)

var cur_inputs : Array

export(Resource) var data

func _ready():
	assert(data is ComboListData)

func accept_input(input : String) -> void:
	for i in data.combo_list.size():
		if data.completion_list[i] < data.combo_list[i].size() and data.combo_list[i].inputs[data.completion_list[i]] == input:
			data.completion_list[i] += 1
		elif data.combo_list[i].inputs[0] == input:
			data.completion_list[i] = 1
		else:
			data.completion_list[i] = 0
	emit_signal("recieved_input", data)

func confirm_combo() -> Combo:
	var combo : Combo = Combo.new()
	for i in data.combo_list.size():
		if data.combo_list[i].size() == data.completion_list[i] and data.combo_list[i].points > combo.points:
			combo = data.combo_list[i]
		data.completion_list[i] = 0
	emit_signal("recieved_input", data)
	emit_signal("scored_points", combo.points)
	return combo
