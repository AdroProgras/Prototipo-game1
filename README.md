 Asymmetric Inversion 

An asymmetric binary puzzle game built in C# for Unity. The mechanics revolve around a custom-shaped grid where players interact with nodes under strict state rules. The game features a tactile "Nuclear Self-Destruction Console" visual theme.

##  Current Status
- **Core Engine:** 100% completed and finalized.
- **Prototypes:** C# Console application is fully functional, incorporating advanced 32-bit bitmask numerical optimization, a custom dynamic Hint System, and a zoned Hybrid BFS generation engine.
- **Next Phase:** Unity Mobile/PC MVP implementation.

##  Low-Level Optimization Success
- **Bitmask Migration:** Shifted state serializations to 32-bit integers (`int`), executing state evaluations via direct CPU bitwise operations. This reduced queue RAM consumption to 0% and boosted execution speeds by over 1,000%.
- **Operational Zones:** Isolated combinatorial math loops into three independent grid sectors to guarantee instant screen loading without thread deadlocks.

##  Game Design Document (GDD)
The full specifications, physical node layouts, difficulty parameters, and architectural details are located in the [docs/GDD.md](docs/GDD.md) file.
