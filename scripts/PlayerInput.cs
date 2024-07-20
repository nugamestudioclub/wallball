using System.Collections;

class PlayerInput {

	private const int LEFT   = 0,
					  RIGHT  = 1,
					  UP     = 2,
					  DOWN   = 3,
					  JUMP   = 4,
					  FINISH = JUMP;

	private readonly BitArray _bits = new BitArray(5);

	public bool Left {
		get => _bits[LEFT];
		set => _bits[LEFT] = value;
	}
	public bool Right {
		get => _bits[RIGHT];
		set => _bits[RIGHT] = value;
	}
	public bool Up {
		get => _bits[UP];
		set => _bits[UP] = value;
	}
	public bool Down {
		get => _bits[DOWN];
		set => _bits[DOWN] = value;
	}
	public bool Jump {
		get => _bits[JUMP];
		set => _bits[JUMP] = value;
	}
	public bool Finish {
		get => _bits[FINISH];
		set => _bits[FINISH] = value;
	}
}