extends Node

class_name ComboHandler

export var combo_list : Array

signal recieved_input(combo_compare)

var cur_inputs : Array
var combo_compare : Array

func _ready():
	for c in combo_list:
		combo_compare.append(0)

func accept_input(input : String) -> void:
	for i in combo_list.size():
		if combo_compare[i] < combo_list[i].size() and combo_list[i].inputs[combo_compare[i]] == input:
			combo_compare[i] += 1
		elif combo_list[i].inputs[0] == input:
			combo_compare[i] = 1
		else:
			combo_compare[i] = 0
	emit_signal("recieved_input", combo_compare)

func confirm_combo() -> Combo:
	var combo : Combo = Combo.new()
	for i in combo_list.size():
		if combo_list[i].size() == combo_compare[i]:
			combo = combo_list[i]
		combo_compare[i] = 0
	emit_signal("recieved_input", combo_compare)
	return combo

# TODO For testing, delegate this to the player
func _process(_delta):
	if Input.is_action_just_pressed("ui_up"):
		accept_input("W")
	if Input.is_action_just_pressed("ui_down"):
		accept_input("S")
	if Input.is_action_just_pressed("ui_left"):
		accept_input("A")
	if Input.is_action_just_pressed("ui_right"):
		accept_input("D")
	if Input.is_action_just_pressed("ui_accept"):
		print(confirm_combo().inputs)
