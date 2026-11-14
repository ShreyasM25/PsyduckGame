# PsyDuck Hunt

PsyDuck Hunt is a 3D third-person capture game built in Unity using C#. Explore an interactive environment, track down roaming NPCs, and capture them using physics-based throwable objects before time runs out.

The project combines player movement, AI-driven NPC behavior, physics interactions, animation systems, and dynamic audio and UI to create an interactive gameplay experience.

## Gameplay Features

- **Third-Person Movement:** WASD-based strafing movement with running, jumping, and character animations.
- **Dynamic Camera System:** Third-person follow camera with adjustable distance and automatic obstacle avoidance.
- **NPC AI and Navigation:** NPCs independently explore the environment, avoid obstacles, periodically idle, and flee when they detect the player.
- **Physics-Based Capture System:** Throw objects at NPCs to initiate capture sequences featuring object animations, particle effects, and randomized escape chances.
- **Multiple Capture Items:** Switch between unlimited regular Pokéballs and three Master Balls, which guarantee successful captures.
- **Audio Systems:** Surface-dependent footsteps, spatial NPC audio, ambient sounds, and layered throwing sound effects.
- **Dynamic Game UI:** Countdown timer, remaining NPC counter, start and end screens, and end-of-game statistics.

## Additional Systems

- **High Score System:** Tracks player performance and makes high scores accessible from the main menu.
- **Dynamic Countdown Effects:** Timer changes color and size as time runs out, accompanied by sound effects during the final five seconds and tense background music during the final ten seconds.
- **Environmental Boundaries:** Physical fences replace invisible boundaries to create a more integrated game environment.
- **Interactive Menu UI:** Return to Menu button features a hover-state visual change and an interaction sound effect.

## Technical Implementation

| Technology | Application |
|---|---|
| C# | Gameplay logic, player controls, NPC AI, and game systems |
| Unity | 3D environment and game development |
| Unity Animator | Character and capture animations |
| Unity Physics | Collision detection and throwable object mechanics |
| Unity Audio | Spatial audio, environmental sound, and sound effects |
| Unity UI | Menus, timers, gameplay information, and high scores |

## Controls

| Input | Action |
|---|---|
| W / A / S / D | Move |
| Left Shift | Run |
| Space | Jump |
| Throw Button | Throw capture object |
| Camera Controls | Adjust camera distance |

## Project Highlights

PsyDuck Hunt brings together several interconnected gameplay systems, requiring coordination between player input, NPC state and behavior, physics interactions, animation events, audio, and UI updates.

The project provided practical experience building interactive 3D environments and implementing gameplay mechanics using Unity and C#.
