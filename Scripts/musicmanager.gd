extends Node2D


# Declare member variables here. Examples:
# var a = 2
# var b = "text"

var base
var drums
var bass
var lead

export var master_max_gain = 0
export var master_min_gain = -80

export var drum_fade = 0.1
export var drum_max_gain = 0
export var bass_fade = 0.1
export var bass_max_gain = 0
export var lead_fade = 0.1
export var lead_max_gain = 0


# Called when the node enters the scene tree for the first time.
func _ready():
	base = get_node("Base")
	drums = get_node("Drums")
	bass = get_node("Bass")
	lead = get_node("Lead")
	base.play()
	drums.play()
	bass.play()
	lead.play()
	base.volume_db = master_max_gain
	drums.volume_db = -80
	bass.volume_db = -80
	lead.volume_db = -80
	pass # Replace with function body.
	
func game_start():
	pass

func menu_start():
	pass
	
func change_global_max_gain(newgain):
	master_max_gain = newgain
	if drum_fade != 0:
		drums.volume_db += drum_fade
	if drums.volume_db >= master_max_gain + drum_max_gain:
		drums.volume_db = master_max_gain + drum_max_gain
		drum_fade = 0
	elif drums.volume_db <= master_min_gain:
		drums.volume_db = master_min_gain
		drum_fade = 0	

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if drum_fade != 0:
		drums.volume_db += drum_fade
	if drums.volume_db >= master_max_gain + drum_max_gain:
		drums.volume_db = master_max_gain + drum_max_gain
		drum_fade = 0
	elif drums.volume_db <= master_min_gain:
		drums.volume_db = master_min_gain
		drum_fade = 0	
	
	if bass_fade != 0:
		bass.volume_db += bass_fade
	if bass.volume_db >= master_max_gain + drum_max_gain:
		bass.volume_db = master_max_gain + drum_max_gain
		drum_fade = 0
	elif bass.volume_db <= master_min_gain:
		bass.volume_db = master_min_gain
		drum_fade = 0	
		
	if lead_fade != 0:
		lead.volume_db += lead_fade
	if lead.volume_db >= master_max_gain + drum_max_gain:
		lead.volume_db = master_max_gain + drum_max_gain
		drum_fade = 0
	elif lead.volume_db <= master_min_gain:
		lead.volume_db = master_min_gain
		drum_fade = 0	
	pass
