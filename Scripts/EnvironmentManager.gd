extends Node2D


# Declare member variables here. Examples:
# var a = 2
# var b = "text"

export(String, FILE) var settings_file


var vol_slider
var music_manager
var blueshift
var redshift

var pulser

var score_display
var hi_score_display

var jumpintoball
var gameover

var infinitymirror

var basedust = load("res://scenes/dustevent.tscn")
var pulseeffect = load("res://scenes/pulseeffecttiles.tscn")

var aberration = 0
var ab_rate = 0.01

# Called when the node enters the scene tree for the first time.
func _ready():
	vol_slider = get_node("UI/VolSlider")
	music_manager = get_node("Music Manager")
	blueshift = get_node("Aberration Wall/Blue Shift")
	redshift = get_node("Aberration Wall/Red Shift")
	pulser = get_node("Main Wall")
	score_display = get_node("UI/Score Label")
	hi_score_display = get_node("UI/HiScore Label")
	jumpintoball = get_node("UI/JumpIntoTheBall")
	gameover = get_node("UI/GameOver")
	infinitymirror = get_node("Infinity Mirror")
	game_reset()
	pass # Replace with function body.
	
	
func update_score(score): 
	score_display.text = score
	pass
	
func update_hi_score(score): 
	hi_score_display.text = score
	pass

func play_wall_hit(index):
	music_manager.play_wall_hit(index)
	pass
	
func play_combo_hit(index):
	music_manager.play_combo_hit(index)
	pass
	
func hit_effect(position):
	var d = basedust.instance()
	d.position = position
	d.quantity = 4
	d.dust_color = pulser.cur_color
	add_child(d)
	pass
	
func game_reset():
	jumpintoball.modulate.a = 1
	gameover.modulate.a = 0
	infinitymirror.reset()
	music_manager.game_over()
	pulser.ending_color = Color("123967")
	pass
	
func game_start():
	jumpintoball.modulate.a = 0
	gameover.modulate.a = 0
	music_manager.game_start()
	infinitymirror.start()
	pulser.ending_color = Color("ff6600")
	pass
	
func game_over(score):
	jumpintoball.modulate.a = 0
	gameover.modulate.a = 1
	pulser.game_over()
	music_manager.game_over()
	hi_score_display.text = score
	pulser.ending_color = Color("123967")
	pass

func pulse_effect(position):
	pulser.pulse_tiles(position)
	pass
	
func spawn_particles(position, direction):
	var d = basedust.instance()
	d.position = position
	d.quantity = 6
	d.dust_color = pulser.cur_color
	if direction.x < 0:
		d.x_min_rate = -1
		d.x_max_rate = -0.25
	else:
		d.x_min_rate = 0.25
		d.x_max_rate = 1	
	
	if direction.y < 0:
		d.y_min_rate = -1
		d.y_max_rate = -0.25
	else:
		d.y_min_rate = 0.25
		d.y_max_rate = 1	
	add_child(d)
	pass
	

func begin_aberration():
	aberration = 1
func backpedal_aberration():
	aberration = -1

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if aberration == 1:
		blueshift.modulate.a += 0.01
		blueshift.position.x += 0.4
		blueshift.position.y += 0.1

		redshift.modulate.a += 0.01
		redshift.position.x -= 0.4
		redshift.position.y -= 0.1
		if blueshift.modulate.a >= 0.2:
			aberration = 0
	elif aberration == -1:
		blueshift.modulate.a -= 0.01
		blueshift.position.x -= 0.4
		blueshift.position.y -= 0.1
		
		redshift.position.x += 0.4
		redshift.position.y += 0.1
		redshift.modulate.a -= 0.01
		if blueshift.modulate.a <= 0:
			aberration = 0
	# TODO game plays audio before audio settings can load
	# currently fixed by having music manager start at no sound, might be worth looking into later
	music_manager.master_max_gain = linear2db(vol_slider.value) # Slider
	pass

