extends Node2D


# Declare member variables here. Examples:
# var a = 2
# var b = "text"

var base
var drums
var bass
var lead
onready var SFX1 = $SFX1
onready var SFX2 = $SFX2
onready var SFX3 = $SFX3
onready var SFX4 = $SFX4
onready var SFX5 = $SFX5
onready var SFX6 = $SFX6

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
	drum_fade = 0.1
	bass_fade = 0.1
	lead_fade = 0.1
	pass

func game_over():
	drum_fade = -0.5
	bass_fade = -0.5
	lead_fade = -0.5
	pass

	
func play_wall_hit(index):
	if index % 3 == 1:
		SFX1.play()
	elif index % 3 == 2:
		SFX2.play()
	else:
		SFX3.play()
	pass
	
func play_combo_hit(index):
	if index % 3 == 1:
		SFX4.play()
	elif index % 3 == 2:
		SFX5.play()
	else:
		SFX6.play()
	pass
	
func change_global_max_gain(newgain):
	base.volume_db = master_max_gain
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
	base.volume_db = master_max_gain
	
	SFX1.volume_db = master_max_gain - 15
	SFX2.volume_db = master_max_gain - 15
	SFX3.volume_db = master_max_gain - 15
	SFX4.volume_db = master_max_gain - 15
	SFX5.volume_db = master_max_gain - 15
	SFX6.volume_db = master_max_gain - 15
	
	drums.volume_db += drum_fade
	if drums.volume_db >= master_max_gain + drum_max_gain:
		drums.volume_db = master_max_gain + drum_max_gain
	elif drums.volume_db <= master_min_gain:
		drums.volume_db = master_min_gain
	
	bass.volume_db += bass_fade
	if bass.volume_db >= master_max_gain + drum_max_gain:
		bass.volume_db = master_max_gain + drum_max_gain
	elif bass.volume_db <= master_min_gain:
		bass.volume_db = master_min_gain
		
	lead.volume_db += lead_fade
	if lead.volume_db >= master_max_gain + drum_max_gain:
		lead.volume_db = master_max_gain + drum_max_gain
	elif lead.volume_db <= master_min_gain:
		lead.volume_db = master_min_gain
	pass
