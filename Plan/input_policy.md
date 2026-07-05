# Input Policy

## Current Prototype

The current playable prototype supports keyboard only.

Keyboard controls:
- Move left/right: `A` / `D` or arrow keys
- Jump: `Space`
- Dash: `Left Shift` or `Right Shift`
- Shoot: `J` or `Z`

## Reserved For Later

Gamepad support is intentionally reserved, but not enabled yet.

When the keyboard prototype feels stable, add gamepad support through a dedicated input abstraction instead of mixing every device directly into gameplay code. A later pass should introduce one of these:

- `PlayerInputReader` backed by Unity Input System actions.
- Device-specific input providers such as `KeyboardInputProvider` and `GamepadInputProvider`.
- Rebindable action maps for keyboard and gamepad.

## Reason

Keeping the first prototype keyboard-only makes movement, jump, dash, and shoot testing easier to reason about. It also avoids hidden gamepad state affecting early gameplay tuning.
