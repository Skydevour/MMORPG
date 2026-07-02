# Third Person Player Implementation Plan

**Goal:** Build the first playable third-person character loop with runtime bootstrapping, map loading, player spawning, camera follow, and code-driven player states.

**Architecture:** Keep reusable state machine code in `Assets/Scripts/Framework/StateMachine`. Keep game-specific startup, player movement, animation, and camera code in `Assets/Scripts/Game`. Use `GameBootstrap` on a single scene root to instantiate the map and the Maria greatsword character prefab.

**Tasks:**
1. Add a small generic state machine framework.
2. Add player input, motor, animation driver, and player states.
3. Add camera follow and scene bootstrap scripts.
4. Wire `MainScene` to a single `GameRoot` that references the map and player prefabs.
5. Verify C# compilation for `Assembly-CSharp`.
