extends Node


# Declare member variables here. Examples:
# var a = 2
# var b = "text"
export var density = 24
export var scale_max = 0.6
export var scale_min = 0.2
export var decay_rate = 0.02
export var x_min_rate = -0.75
export var x_max_rate = 0.75
export var y_min_rate = -0.75
export var y_max_rate = 0.75
export var dust_color = Color(1,1,1,1)

var basedust = load("res://Scenes/basedust.tscn")

var frame_counter = 0

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	frame_counter += 1
	if frame_counter % (120/density) == 0:
		var d = basedust.instance()
		var scale_val = rand_range(scale_min,scale_max)
		d.decay_rate = decay_rate
		d.scale.x = scale_val
		d.scale.y = scale_val
		d.modulate = dust_color
		d.modulate.a = rand_range(0.25, 1)
		d.rotation_rate = rand_range(-0.3,0.3)
		d.x_rate = rand_range(-0.75,0.75)
		d.y_rate = rand_range(-0.75,0.75)
		add_child(d)
	pass
