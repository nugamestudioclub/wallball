extends Sprite


# Declare member variables here. Examples:
# var a = 2
# var b = "text"

export var decay_rate = 0.03
export var rotation_rate = 0.03
export var x_rate = 0.01
export var y_rate = 0.02


# Called when the node enters the scene tree for the first time.
func _ready():
	
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	self.modulate.a -= decay_rate
	self.rotation += rotation_rate
	self.position.x += x_rate
	self.position.y += y_rate
	if self.modulate.a <= 0:
		self.queue_free()
	pass
