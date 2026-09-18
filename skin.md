# arenji Skinning Guide

This document outlines all the skinnable elements available in arenji. Textures are loaded using `arenjiSkinManager.SkinTextures?.Get("path")`. 

Below is a breakdown of every skinnable element, ordered from the **top-most layer to the bottom-most layer**. Elements at the top of this list will render in front of elements at the bottom.

## Layer 1: Piano Keys (Top Layer)
The piano keys are rendered on top of all gameplay elements (bulbs, particles, and notes).

- **`skin/keyWhite`**
  - **Description**: The texture used for the white piano keys on the virtual keyboard.
- **`skin/keyDark`**
  - **Description**: The texture used for the black piano keys on the virtual keyboard.

## Layer 2: Light Bulbs
The light bulbs sit immediately beneath the piano keys, flashing when a key is hit.

- **`b`**
  - **Description**: The glow/bulb texture that flashes when a note is hit. It utilizes additive blending and is positioned relative to the top center of its corresponding piano key.

## Layer 3: Particles
Particles are emitted from the keys when notes are hit, flowing upwards. They are rendered behind the piano keys and light bulbs, but in front of the falling notes.

- **`skin/p`**
  - **Description**: The particle texture. The size, speed, and lifetime of these particles are controlled by the user's settings.

## Layer 4: MIDI Notes (Bottom Layer)
The notes fall from the top of the screen down towards the virtual keyboard. They are rendered behind everything else (except the background video/image).

Notes are constructed using three separate textures stacked vertically:
- **`Skins/noteHead`**
  - **Description**: The bottom cap of the falling note. This is the part that aligns with the piano key when it is time to hit.
- **`Skins/noteBody`**
  - **Description**: The middle, elongating section of the falling note. If the provided texture has a height of 128px or less, it will automatically tile (repeat) to fill the duration of the note. Otherwise, it will stretch.
- **`Skins/noteEnd`**
  - **Description**: The top cap of the falling note, representing the release point.

> [!NOTE]
> Within the note layer itself, black key notes are drawn *on top of* white key notes.
