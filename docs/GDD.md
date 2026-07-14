# GAME DESIGN DOCUMENT (GDD): ASYMMETRIC INVERSION

## 1. GRID GEOMETRY (Symmetric Circular Structure)
The board is a unified, continuous panel consisting of 6 vertical columns layout arranged in a vertical mirror reflection. It features an asymmetric node distribution across 6 rows, totaling 26 buttons: 3, 5, 6, 6, 5, 3.

- **Row A (Top):** 3 buttons. Centered horizontally relative to Row B.
- **Row B:** 5 buttons. Nodes 2, 3, and 4 sit directly underneath Nodes 1, 2, and 3 of Row A. Nodes 1 and 5 extend outward to the left and right (hanging nodes). No vertical alignment with Row C.
- **Row C:** 6 buttons. Node 1 of Row B sits halfway between Nodes 1 and 2 of Row C. Node 2 of Row B sits between Nodes 2 and 3 of Row C.
- **Row D:** 6 buttons. Perfectly aligned 1:1 vertically with Row C.
- **Row E:** 5 buttons. Identical mirror image of Row B.
- **Row F (Bottom):** 3 buttons. Identical mirror image of Row A.

---

## 2. CORE MECHANICS & STATE SYSTEM
- **Binary States:** Buttons have two states: White ($\bigcirc$, solved/cleared) and Black ($\mathbf{\bullet}$, active/shuffled).
- **Interaction Constraint:** The player can ONLY interact with (click) buttons currently in the Black ($\mathbf{\bullet}$) state. Clicks on White buttons are ignored.
- **Adjacency Inversion (Domino Effect):** Clicking a valid Black button toggles its state to White and triggers a wave that inverts the state of all its valid neighbors (White turns Black, Black turns White).
- **Win Condition:** The level is solved when all 26 buttons are simultaneously in the White ($\bigcirc$) state.

---

## 3. INVERSE GENERATION ENGINE & ADJACENCY LOGIC
To guarantee solvability and parameterize progression, the game skips traditional random shuffling. It utilizes a Breadth-First Search (BFS) algorithm operating in reverse from the solved root state, building a strict state tree.

### A) Adjacency Logic & Difficulty Modes

#### Easy Mode
- **Adjacency:** Row-bound only. Clicking a node only toggles its direct Left and Right neighbors on the same row. No vertical propagation.
- **Lose Condition:** Strict 300-second countdown timer.

#### Hard Mode
- **Adjacency:** Full cross-shaped propagation (Left, Right, Up, Down). Diagonals are strictly ignored.
- **Segmented Vertical Blocks Rule:** To optimize the data matrix around the physical row shifts, vertical interaction is locked into 3 isolated zones:
  - **Top Zone (Rows A & B):** Vertical propagation only occurs between rows A and B. Neighbors match 1:1 using the centered nodes. Rows B's hanging nodes (1 and 5) have no upper vertical neighbors (Option A). Interaction NEVER crosses down into Row C.
  - **Center Zone (Rows C & D):** Direct 1:1 vertical interaction between Row C and Row D. Completely isolated from Rows B and E.
  - **Bottom Zone (Rows E & F):** Mirror of the Top Zone. Vertical interaction locked between E and F. Isolated from Row D.
- **Lose Condition:** Strict 180-second countdown timer.

#### Campaign Mode (Story Mode)
- **Adjacency:** Toggles dynamically between Easy and Hard rules depending on the level configuration.
- **Lose Condition:** Strict move limit (e.g., "Solve in 10 moves"). Unlimited time. Running out of moves triggers a Game Over.

### B) Parameterization via State Depth (BFS Engine)
The generation engine receives `(minMoves, maxMoves, currentMode)`. The algorithm runs backwards from the solved state (all-white board):
- **Campaign Mode:** The BFS targets a state node at the exact depth layer requested (e.g., depth 6). This guarantees a puzzle that requires a minimum of 6 optimal moves to solve.
- **Easy Mode:** Selects a target state within a low depth threshold (5 to 7 inverse steps).
- **Hard Mode:** Selects a target state within a high complexity depth threshold (11 to 15 inverse steps).

---

## 4. VISUAL FEEDBACK & UX (Solvability Guarantee)
To mitigate player frustration, levels do not instantly snap into a scrambled state upon loading.
- **The Scramble Animation:** A rapid 3-second sequence displays the board in its fully solved state before breaking apart in real-time.
- **Visual Pathing:** The exact reverse steps computed by the BFS flash sequentially across the board using bright light bursts and color swaps. 
- **Psychological Goal:** Proves to the player visually that the layout is 100% solvable, completely eliminating any "broken game" perception.
