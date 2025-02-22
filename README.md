# Unity 2D Ninja Rope & Movement System

This project implements **Liero-style ninja rope mechanics** and **N+ style movement physics** for a **2D physics-based platformer**. The system is designed to provide a fluid and responsive experience, combining rope physics, player momentum, and wall-climbing mechanics to create dynamic traversal gameplay.

## Features

- **Liero-Style Ninja Rope** - Fully dynamic rope with tension, swinging mechanics, and adjustable length.
- **N+ Inspired Movement** - Smooth acceleration, deacceleration, wall sliding, and wall jumping.
- **Momentum-Based Physics** - Realistic player momentum with preserved movement energy.
- **Dynamic Rope Length** - Increase or decrease rope length in real-time for advanced maneuvering.
- **Attachable Surfaces** - Rope can latch onto valid surfaces for physics-based traversal.
- **Wall Climbing & Sliding** - Players can climb walls, slide down surfaces, and jump off walls.
- **Physics-Based Movement** - Handles slopes, bounce effects, and external forces.

## System Overview

This system is structured around **modular components** that interact seamlessly:

- **GravityPoint.cs** - Manages the ninja rope system, including shooting, attachment, and physics interactions.
- **PlayerMomentum.cs** - Handles **momentum preservation** and smooth movement transitions.
- **PlayerMovement.cs** - Implements advanced movement physics, including acceleration, jumping, wall interactions, and air control.

## Core Mechanics

### Ninja Rope System (GravityPoint.cs)
- **Firing the Rope** - The player shoots a rope projectile that latches onto surfaces.
- **Dynamic Length Adjustment** - Players can shorten or extend the rope in real-time.
- **Swinging Mechanics** - Applies **tension forces** to maintain realistic swinging physics.
- **Rope Detachment** - Players can release the rope at any time to conserve momentum.

### Player Movement System (PlayerMovement.cs)
- **Grounded Movement** - Acceleration, deacceleration, and maximum movement speed control.
- **Jumping & Wall Interaction** - Supports double jumping, wall sliding, and wall jumping.
- **Slope Handling** - Allows smooth movement across sloped surfaces.
- **Bounce & Shockwave Effects** - Players can be pushed or launched by external forces.

### Momentum System (PlayerMomentum.cs)
- **Velocity Tracking** - Captures player velocity frame-by-frame for momentum-based movement.
- **Energy Conservation** - Ensures movement feels fluid and realistic by maintaining inertia.

## How to Use

1. **Attach `PlayerMovement.cs`** to the player GameObject to enable N+ style movement.
2. **Attach `GravityPoint.cs`** to the player GameObject to enable ninja rope functionality.
3. **Configure rope settings** (speed, length, attachment behavior) via the Unity Inspector.
4. **Assign input controls** for movement, jumping, and rope usage.
5. **Test and refine** physics settings to achieve the desired feel.

## Future Enhancements
- **Grapple Boost Mechanics** - Propel the player when detaching from a swing.
- **Advanced Wall Running** - Add seamless parkour-style movement.
- **Multiple Rope Types** - Introduce different ropes with varying elasticity and behavior.
- **Multiplayer Support** - Implement networked physics for competitive play.

## About the Developer
This project showcases expertise in **2D physics simulations, advanced player movement, and grappling mechanics**. The combination of **N+ inspired movement and Liero-style ninja rope** creates a dynamic traversal system for any fast-paced 2D action game.

**Looking to hire a Unity Developer?** Feel free to reach out!

---

**Built for 2D Physics | Optimized for Fluid Movement | Designed for Advanced Gameplay**

