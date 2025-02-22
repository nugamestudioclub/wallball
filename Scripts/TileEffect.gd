extends TileMap

var pulseeffect = load("res://scenes/pulseeffecttiles.tscn")
#var dusteffect = 
export var rate = 0.0005
export var starting_color = Color(0.75,0.75,1,1)
export var ending_color = Color(1,1,1,1)
export var game_started = 0

var at_maximum = 0

export var cur_color = Color(0.75,0.75,1,1)

# Called when the node enters the scene tree for the first time.
func _ready():
	cur_color = starting_color
	game_started = 1
	#pulse_tiles(Vector2(1,1))
	pass # Replace with function body.
	
#resets the color of the pulse and particle effect
func reset():
	game_started = 0
	cur_color = starting_color
	pass
	
func game_over():
	var p = pulseeffect.instance()
	p.color = Color(1,0,0,1)
	p.starting_energy = 6
	p.scale_inc_rate = 0.15
	p.rot_rate = 0.00
	p.blue_shift_rate = 0.000
	p.position.x = 0 - self.position.x
	p.position.y = 0 - self.position.y
	add_child(p)

func pulse_tiles(position):
	var pef = pulseeffect.instance()
	pef.color = cur_color
	add_child(pef)
	#get_parent().add_child(pef)
	pef.position.x = position.x - self.position.x
	pef.position.y = position.y - self.position.y
	pass
	
func spawn_particles(position, direction):
	pass
	
	
# Called every frame. 'delta' is the elapsed time since the previous frame.
# Red shifts the default pulse color every frame
func _process(delta):
	if game_started != 0:
		if ending_color.r > cur_color.r:
			cur_color.r += rate
		elif ending_color.r <= cur_color.r:
			cur_color.r -= rate
		if ending_color.b > cur_color.b:
			cur_color.b += rate
		elif ending_color.b <= cur_color.b:
			cur_color.b -= rate
		if ending_color.g > cur_color.g:
			cur_color.g += rate
		elif ending_color.g <= cur_color.g:
			cur_color.g -= rate
	pass
