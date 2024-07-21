extends Node2D


# Declare member variables here. Examples:
# var a = 2
# var b = "text"

var animplayer
var sprite

# Called when the node enters the scene tree for the first time.
func _ready():
	animplayer = get_node("AnimationPlayer")
	animplayer.play("IDLE")
	#anim_walk_left()
	pass # Replace with function body.

func anim_walk_left():
	self.scale.x = -1
	animplayer.play("WALK")
	pass
	
func anim_walk_right():
	animplayer.play("WALK")
	pass
	
func anim_idle():
	animplayer.play("IDLE")
	pass
	
func jump():
	animplayer.play("JUMP")
	pass

func anim_combo_w():
	animplayer.play("COMBO_W")
	pass
	
func anim_combo_a():
	animplayer.play("COMBO_A")
	pass
	
func anim_combo_s():
	animplayer.play("COMBO_S")
	pass
	
func anim_combo_d():
	animplayer.play("COMBO_D")
	pass


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass
