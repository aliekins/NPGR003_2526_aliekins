# Documentation of the task "08-Fireworks"

## Author

Sara Alić-Ekinović

## Command line arguments

```
-w, --width
    not required, default = 800

-h, --height
    not required, default = 600

-p, --particles
    not required, default = 10000

-r, --rate
    not required, default = 2.0

-t, --texture
    not required, default = ":check:"
```

## Input data

Controls:

- Mouse
  - `left` mouse button + drag - camera rotation (trackball control)
  - `right` mouse button + drag - pan the camera
  - mouse `wheel` - zoom in/out

- Keyboard
  - `space` - fire rockets (hold for continuous fire)
  - `C` - toggle crackle effect
  - `S` - toggle strobe effect
  - `R` - reset simulation
  - `P` - pause/resume simulation (stops/continues spawning)
  - `F` - (un)freeze simulation (stupes/resumes aging of the particles - freeze frame)
  - `Up\Down` - make the rocket rate higher\lower resp. by 0.5
  - `Esc` - exit the app

## Examples

`dotnet run -- -w 1280 -h 800 -p 3000 -r 2.0`

> simple, crackle and strobe off

`dotnet run -- -w 1280 -h 800 -p 3000 -r 1.5 -t particle.png`

> calmer fireworks, optional crackle and/or strobe based on preference

`dotnet run -- --width 1600 --height 900 --particles 8000 --rate 4.0 --texture particle_star.png`

> "festival" mode, crackle and strobe ON

`dotnet run -- -w 1920 -h 1080 -p 20000 -r 10.0 -t particle_star.png`

> crazy, just press space and enjoy :)

## Extra work / Bonuses

- Multiple rocket/particle types
  - Implemented
    - Rocket – initial launch particle with fuse and upward velocity
    - Bomblet – intermediate explosion particles
    - Spark – primary explosion fragments
    - MicroSpark – secondary crackle particles

- Multi-stage explosions
  - Implemented
    1. rockets explode into bomblets after their fuse expires
    2. bomblets explode into sparks
    3. sparks may generate micro-sparks via the crackle effect

- Color/point-size changes during the life of a particle/rocket...
  - Implemented - particles change their visual appearance over their lifetime:
    - color evolves over time based on the particle’s normalized age, including subtle per-particle color drift
    - point size is defined per particle type (and randomized for some types) and can be adjusted via configuration parameters

- Visualization of rocket trajectories
  - I am not sure if I understood this one correctly - if You meant there is a trace following the rocket trajectory. But for this simulation the trajectory is seen, in a way, since moving rocket particles are continuously rendered. However, there we no trajectory "lines".

- Interactive fireworks control (mouse, keyboard). Launcher fire trigger
  - Implemented: keyboard input controls, mouse input controls, rocket firing is user-triggered

- Advanced shading effects, etc.
  - Per-particle coloring, optional textured point sprites

## Use of AI

- `.py` scripts for textures, debugging help

## Notes

Default "texture" is just plain square. However, files `make_particle.py` and `make_star_texture.py` in this directory, when run

> ```
> python3 make_particle.py
> python3 make_star_texture.py
> ```

produce `particle.png` and `particle_star.png` (respectively), which You can use as textures.
