extends Light2D

export var starting_color = Color(0xc7,0,0,255)
export var starting_energy = 2
export var energy_det_rate = 0.01
export var scale_inc_rate = 0.01
export var rot_rate = 0.01
export var blue_shift_rate = 0.005

# Called when the node enters the scene tree for the first time.
func _ready():
	self.scale = Vector2(0,0)
	self.rotation = rand_range(0,360)
	self.energy = starting_energy
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	self.color.r -= blue_shift_rate
	self.color.b += blue_shift_rate
	self.scale.x += scale_inc_rate
	self.scale.y += scale_inc_rate
	self.rotation += rot_rate
	self.energy -= scale_inc_rate*scale_inc_rate
	if self.energy <= 0:
		self.queue_free()
	pass
