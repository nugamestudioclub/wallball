extends VBoxContainer

export(PackedScene) var combo_item_file : PackedScene
export(NodePath) var combo_handler_path
onready var combo_handler : ComboHandler = get_node(combo_handler_path)

var combo_items : Array

# Called when the node enters the scene tree for the first time.
func _ready():
	combo_handler.connect("recieved_input", self, "_update")
	for n in get_children():
		remove_child(n)
		n.queue_free()
	for combo in combo_handler.data.combo_list:
		var combo_item = combo_item_file.instance()
		combo_items.append(combo_item)
		add_child(combo_item)
		combo_item.combo = combo

func _update(combo_data : ComboListData) -> void:
	for i in combo_data.completion_list.size():
		combo_items[i].combo_progress = combo_data.completion_list[i]
