extends Node2D


# Declare member variables here. Examples:
# var a = 2
# var b = "text"
export var quantity = 6
export var scale_max = 0.6
export var scale_min = 0.2
export var decay_rate = 0.02
export var x_min_rate = -0.75
export var x_max_rate = 0.75
export var y_min_rate = -0.75
export var y_max_rate = 0.75
export var dust_color = Color(1,1,1,1)

var basedust = load("res://Scenes/basedust.tscn")

# Called when the node enters the scene tree for the first time.
func _ready():
	for n in quantity:
		var d = basedust.instance()
		var scale_val = rand_range(scale_min,scale_max)
		d.decay_rate = decay_rate
		d.scale.x = scale_val
		d.scale.y = scale_val
		d.modulate = dust_color
		d.modulate.a = rand_range(0.25, 1)
		d.rotation_rate = rand_range(-0.3,0.3)
		d.x_rate = rand_range(x_min_rate,x_max_rate)
		d.y_rate = rand_range(y_min_rate,y_max_rate)
		add_child(d)
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if get_child_count() == 0:
		self.queue_free()
	pass
