extends TileMap

var pulseeffect = load("res://Scenes/pulseeffecttiles.tscn")
#var dusteffect = 
export var red_shift_rate = 0.0005
export var starting_color = Color(0.75,0.75,1,1)
export var game_started = 0

var at_maximum = 0

var cur_color = starting_color

# Called when the node enters the scene tree for the first time.
func _ready():
	cur_color = starting_color
	#pulse_tiles(Vector2(1,1))
	pass # Replace with function body.
	
#resets the color of the pulse and particle effect
func reset():
	game_started = 0
	cur_color = starting_color
	pass

func pulse_tiles(position):
	var pef = pulseeffect.instance()
	pef.color = cur_color
	add_child(pef)
	#get_parent().add_child(pef)
	pef.position.x = position.x
	pef.position.y = position.y
	pass
	
func spawn_particles(position, direction):
	pass
	
	
# Called every frame. 'delta' is the elapsed time since the previous frame.
# Red shifts the default pulse color every frame
func _process(delta):
	if game_started != 0:
		if at_maximum == 0:
			cur_color.b -= red_shift_rate
			cur_color.r += red_shift_rate
			if cur_color.r >= 1:
				at_maximum = 1
	pass
