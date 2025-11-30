# Documentation of the task "04-Mandala"

## Author

Sara Alić-Ekinović

## Command line arguments

```
-w, --width
    required
    image width in pixels

-h, --height
    required
    image height in pixels

-o, --output
    required
    output file path for the generated mandala image

-s, --style,
    not required, default = Geometric
    mandala styles: Sand, Geometric, Hindu, Celtic, Lotus, Chakra, Tantric, Buddha

--seed
    not required
    random seed for mandala generation

--symmetry
    not required, default = 8
    rotational symmetry order

--detail
    not required, default = 0.5
    level of detail [0,1]
```

## Input data

`dotnet run -- --width 800 --height 800 --output mandala.png`

> out
> `Saved mandala.png`

`dotnet run -- --width 800 --height 800 --symmetry 8 --detail 0.4 --style Geometric --output geometric.png --seed 5000`

> out
> `Saved geometric.png`

`dotnet run -- --width 800 --height 800 --style Sand --symmetry 12 --detail 0.7 --seed 2 --output sand.png`

> out
> `Saved sand.png`

`dotnet run -- --width 800 --height 800 --style Hindu --output hindu.png --symmetry 12 --detail 0.8`

> out
> `Saved hindu.png`

`dotnet run -- --width 800 --height 800 --style Celtic --symmetry 4 --detail 0.2 --seed 5000 --output celtic.png`

> out
> `Saved celtic.png`

`dotnet run -- --width 800 --height 800 --style Lotus --symmetry 28 --detail 0.4 --output lotus.png`

> out
> `Saved lotus.png`

`dotnet run -- --width 800 --height 800 --style Chakra --output chakra.png --symmetry 10 --detail 0.2`

> out
> `Saved chakra.png`

`dotnet run -- --width 800 --height 800 --style Tantric --output tantric.png --symmetry 32 --detail 1.0`

> out
> `Saved tantric.png`

`dotnet run -- --width 800 --height 800 --style Buddha --symmetry 12 --detail 0.7 --seed 3498358 --output buddha.png`

> out
> `Saved buddha.png`

#### Personal favourite style: Sand

## Extra work / Bonuses

- accepts rectangular image aspect ratios
- strong parametrization and/or more (switchable) ideas in your solution: extra options mentioned above

## Use of AI

- Ideas of different approaches to implement each style
