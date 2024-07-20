extends Node2D


# Declare member variables here. Examples:
# var a = 2
# var b = "text"

var vol_slider
var music_manager
var blueshift
var redshift

var pulser


var aberration = 0
var ab_rate = 0.01

# Called when the node enters the scene tree for the first time.
func _ready():
	vol_slider = get_node("UI/VolSlider")
	music_manager = get_node("Music Manager")
	blueshift = get_node("Aberration Wall/Blue Shift")
	redshift = get_node("Aberration Wall/Red Shift")
	pulser = get_node("Main Wall")
	#begin_aberration()
	pass # Replace with function body.
	
func pulse_effect(position):
	pulser.pulse_tiles(position)
	pass
	
func spawn_particles(position, direction):
	pass
	

func begin_aberration():
	aberration = 1
func backpedal_aberration():
	aberration = -1

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if aberration == 1:
		blueshift.modulate.a += 0.01
		blueshift.position.x += 0.2
		blueshift.position.y += 0.1

		redshift.modulate.a += 0.01
		redshift.position.x -= 0.2
		redshift.position.y -= 0.1
		if blueshift.modulate.a >= 0.2:
			aberration = 0
	elif aberration == -1:
		blueshift.modulate.a -= 0.01
		blueshift.position.x -= 0.2
		blueshift.position.y -= 0.1
		redshift.position.x += 0.2
		redshift.position.y += 0.1
		redshift.modulate.a -= 0.01
		if blueshift.modulate.a <= 0:
			aberration = 0
	music_manager.master_max_gain = linear2db(vol_slider.value) # Slider
	pass
