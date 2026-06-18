# My Weird Magic Academy Life

**My Weird Magic Academy Life** is a 2D pixel-art magic academy life simulation RPG developed in Unity.

The player takes the role of a student in a strange magical academy. The main goal is to study magical subjects, manage limited daily actions, take subject-based tests, survive the consequences of failure, and graduate successfully.

---

## Final Submission Status

This project is submitted as a **playable vertical slice** for coursework.

The current version includes:

* Academy exploration
* Player movement
* Camera follow
* Scene transitions
* Dorm and sleep system
* Day and semester progression
* Daily action limit system
* Player stats
* Teacher interaction menus
* Study activities
* Test activities
* Potion minigame
* Rune minigame
* Arcane minigame
* Failure penalty system
* Resurrection Hall scene
* Graduation logic
* Success and failure endings
* Title screen
* Background music
* UI feedback

This is not a full commercial-length game. It is a focused vertical slice showing the main gameplay loop and core systems.

---

## Game Concept

The player lives through a strange magic academy life and studies three magical subjects:

* **Arcane**
* **Potion**
* **Rune**

Each subject has its own stat and test minigame.

Studying is the safer option and increases the related stat by **5 points**.

Passing a test is more rewarding and increases the related stat by **10 points**.

Failing a test causes a **3-day penalty** and sends the player to the **Resurrection Hall**.

The final objective is to improve the required stats and graduate successfully.

---

## Core Gameplay Loop

The main gameplay loop is:

1. Explore academy areas.
2. Talk to teachers.
3. Choose to study or take a test.
4. Spend limited daily actions.
5. Improve Arcane, Potion, or Rune stats.
6. Return to the dorm.
7. Sleep to advance to the next day.
8. Progress through semesters.
9. Try to graduate successfully.

---

## Minigames

### Potion Minigame

The Potion test is a memory-card brewing minigame.

The player memorises ingredient card positions, then clicks ingredients in the correct recipe order.

Difficulty increases by semester:

* Semester 1: 3x3 grid
* Semester 2: 4x4 grid
* Semester 3: 5x5 grid

Higher Potion stat gives the player more memorisation time.

---

### Rune Minigame

The Rune test is a timing-based smithing minigame.

The player presses **Space** when the moving hammer marker is inside the hit zone.

Difficulty increases by semester through:

* Smaller hit zone
* Faster hammer movement
* More required successful hits

Higher Rune stat slows the hammer marker.

---

### Arcane Minigame

The Arcane test is a magical glyph-tracing minigame.

The player memorises a random node order, then clicks the nodes in the correct sequence after the preview numbers disappear.

Difficulty increases by semester through:

* More nodes
* Shorter base time

Higher Arcane stat gives the player more time.

---

## Controls

| Input             | Action                                          |
| ----------------- | ----------------------------------------------- |
| WASD / Arrow Keys | Move player                                     |
| E                 | Interact with teachers, doors, bed, and objects |
| Mouse Left Click  | Use UI buttons and interact with minigames      |
| Space             | Strike hammer in Rune minigame                  |

---

## How to Run the Playable Build

1. Download or unzip the submitted build file.
2. Open the extracted build folder.
3. Run the executable file for **My Weird Magic Academy Life**.
4. Start playing from the game’s starting scene.
5. Use the controls listed above.

---

## How to Open the Unity Project

1. Open Unity Hub.
2. Select **Open Project**.
3. Choose this project folder.
4. Use Unity version **2022.3.62f3c1** or a compatible Unity 2022.3 LTS version.
5. Open the project.
6. Open the starting scene from the Scenes folder.
7. Press Play to test the game inside Unity.

---

## Main Systems

The project includes the following main systems and scripts:

* **PlayerMovement** — controls top-down player movement.
* **CameraFollow** — follows the player during gameplay.
* **SceneTransition** — handles scene changes.
* **SpawnManager** — places the player at the correct spawn point.
* **PlayerStats** — stores Arcane, Potion, and Rune stats.
* **StatsUI** — displays player stats.
* **DayManager** — tracks current day and semester.
* **DailyActionManager** — controls daily study and test limits.
* **SleepTrigger** — advances to the next day.
* **AcademyTeacherMenuController** — controls teacher menu interactions.
* **PotionGameManager** — controls the Potion minigame.
* **PotionCard** — controls individual Potion cards.
* **RuneGameManager** — controls the Rune minigame.
* **ArcaneGameManager** — controls the Arcane minigame.
* **ArcaneNode** — controls each Arcane node.
* **GraduationManager** — checks graduation success or failure.
* **BGMManager** — manages background music.

---

## Built With

* Unity
* C#
* GitHub
* TextMeshPro
* Unity 2D physics
* Unity UI system

---

## External Assets and AI Use

Some visual assets were created with AI assistance using ChatGPT image generation and Doubao AI.

The player character asset is from:

**Roleworld Cleric Package**
Creator: **Pixel Sazy**
Source: https://pixel-sazy.itch.io/roleworld-cleric-package

AI assistance was also used for:

* Game design planning
* Debugging support
* Code explanation
* Unity workflow guidance
* English writing support
* Visual asset generation prompts

All AI-assisted and external resources are declared in the final report and professionalism portfolio.

---

## Known Limitations

This project is a coursework vertical slice, so some features are limited or postponed.

Known limitations include:

* No save/load system.
* Limited NPC dialogue.
* No relationship system yet.
* No part-time job system.
* Limited animation.
* Limited sound effects.
* Some visual assets are AI-assisted or placeholder-like.
* UI is best viewed at the intended build resolution.

These limitations were managed by focusing on the most important playable systems: exploration, study, tests, stats, daily actions, minigames, failure penalties, and graduation logic.

---

## Future Development

Future improvements could include:

* More NPC dialogue
* Relationship system
* Part-time jobs
* Inventory and item system
* Save/load system
* Random events
* More story content
* More sound effects
* More animations
* More polished pixel-art assets
* More endings

---

## Developer Notes

This project began as a larger magic academy life simulator idea. During development, the scope was reduced into a stable playable vertical slice suitable for coursework submission.

The final version focuses on the core academy progression loop:

**Explore → Study → Test → Improve Stats → Manage Time → Graduate**
