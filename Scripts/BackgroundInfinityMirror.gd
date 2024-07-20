extends Node2D


# Declare member variables here. Examples:
# var a = 2
# var b = "text"

var twelve
var eight
var four

export var w12 = 0.03
export var v12 = 0.001

var radius = 0
var counter = 0
var alpha_rate = 0.001

# Called when the node enters the scene tree for the first time.
func _ready():
	twelve = get_node("TileMap12")
	eight = get_node("TileMap8")
	four = get_node("TileMap4")
	self.modulate.a = 0
	pass # Replace with function body.

func reset():
	twelve.position.x = 0
	twelve.position.y = 0
	eight.position.x = 0
	eight.position.y = 0
	four.position.x = 0
	four.position.y = 0

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	twelve.position.x = radius*3*cos(w12*counter)
	twelve.position.y = radius*3*sin(w12*counter)
		
	eight.position.x = radius*2*cos(w12*counter+0.1)
	eight.position.y = radius*2*sin(w12*counter)
	
	four.position.x = radius*cos(w12*counter+0.3)
	four.position.y = radius*sin(w12*counter)
	
	radius = radius + v12
	if radius > 4:
		v12 = 0
		
	self.modulate.a += alpha_rate
	if self.modulate.a > 1:
		self.modulate.a = 1
		alpha_rate = 0
	counter = counter + 1
	pass
