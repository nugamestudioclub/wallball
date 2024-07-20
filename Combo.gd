extends Resource

class_name Combo

export var points : int
export var inputs : Array

func size() -> int:
	return inputs.size()

func empty() -> bool:
	return size() == 0
