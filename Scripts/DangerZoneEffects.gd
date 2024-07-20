extends Node2D


# Declare member variables here. Examples:
# var a = 2
# var b = "text"

export var radius = 50

var dusteffect = load("res://Scenes/dustemitter.tscn")

# Called when the node enters the scene tree for the first time.
func _ready():
	var lightfield = get_node("DangerZoneL")
	var pem = dusteffect.instance()
	pem.density = 12
	pem.x_min_rate = -0.1
	pem.x_max_rate = 0.1
	pem.y_min_rate = -0.01
	pem.y_max_rate = -0.2
	pem.x_pos_start_min = -1*radius
	pem.x_pos_start_max = radius
	pem.dust_color = lightfield.color
	pem.dust_color.r -= 0.5
	pem.dust_color.g += 0.1
	pem.dust_color.b += 0.3
	pem.modulate.a = 0.6
	
	add_child(pem)
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	
	pass
