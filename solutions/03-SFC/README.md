# Documentation of the task "03-SFC"

## Author

Sara Alić-Ekinović

## Command line arguments

```
-o, --output
    not required, default = "output.svg"
    output file-name (SVG)

-w, --width
    not required, default = 400
    image width

-h, --height
    not required, default = 400
    image height

-c, --curve
    not required, default = "hilbert"
    curve type (name or numeric index)

-d, --depth
    not required, default = 4
    recursion depth / detail level

--color
    not required, default = "#00FFAA"
    base curve color in #RRGGBB

--background
    not required, default = "#000000"
    background color in #RRGGBB

--thickness
    not required, default = 1.5
    base line thickness in pixels
```

## Input data

#### Basic

`dotnet run -- -c hilbert -d 6 -w 800 -h 800 -o hilbert.svg`

> out
> `SVG saved to hilbert.svg (curve=hilbert, depth=6).`

`dotnet run -- -c morton -d 6 -w 800 -h 800 -o morton.svg`

> out
> `SVG saved to morton.svg (curve=morton, depth=6).`

`dotnet run -- -c dragon -d 14 -w 1000 -h 800 -o dragon.svg`

> out
> `SVG saved to dragon.svg (curve=dragon, depth=14).`

`$ dotnet run -- -c levy -d 12 -w 800 -h 600 -o levy.svg`

> out
> `SVG saved to levy.svg (curve=levy, depth=12).`

`dotnet run -- -c barnsley-fern -d 1 -w 600 -h 900 -o fern.svg`

> out
> `SVG saved to fern.svg (curve=barnsley-fern, depth=1).`

`dotnet run -- -c peano -d 4 -w 800 -h 800 -o peano.svg`

> out
> `SVG saved to peano.svg (curve=peano, depth=4).`

`dotnet run -- -c sierpinski -d 7 -w 800 -h 600 -o sierpinski.svg`

> out
> `SVG saved to sierpinski.svg (curve=sierpinski, depth=7).`

`dotnet run -- -c gosper -d 4 -w 1000 -h 800 -o gosper.svg`

> out
> `SVG saved to gosper.svg (curve=gosper, depth=4).`

`dotnet run -- -c newton -d 1 -w 800 -h 800 -o newton.svg`

> out
> `SVG saved to newton.svg (curve=newton, depth=1).`

#### Interesting / ones I find pretty :D

`dotnet run -- -c hilbert -d 6 -w 800 -h 800 -o hilbert.svg --color "#00FFE5" --background "#050014" --thickness 1.4`

` dotnet run -- -c hilbert -d 8 -w 1000 -h 1000 -o hilbert.svg --color "#39FF14" --background "#000000" --thickness 0.9`

`dotnet run -- -c morton -d 8 -w 800 -h 800 -o morton.svg --color "#FF9800" --background "#111111" --thickness 0.4`

`dotnet run -- -c peano -d 8 -w 1000 -h 1000 -o peano.svg --color "#FF4081" --background "#05050A" --thickness 0.9`

`dotnet run -- -c sierpinski -d 12 -w 1200 -h 800 -o sierpinski.svg --color "#BB86FC" --background "#000000" --thickness 0.8`

`dotnet run -- -c levy -d 15 -w 1000 -h 1000 -o levy.svg --color "#00E5FF" --background "#000000" --thickness 0.8`

`dotnet run -- -c dragon -d 24 -w 1200 -h 800 -o dragon.svg --color "#FF5722" --background "#050508" --thickness 0.4`

`dotnet run -- -c gosper -d 6 -w 1000 -h 1000 -o gosper.svg --color "#FFC107" --background "#0B0B10" --thickness 1.2`

`dotnet run -- -c barnsley-fern -d 2 -w 800 -h 1000 -o fern.svg --color "#2E7D32" --background "#FFFFFF" --thickness 0.2`

`dotnet run -- -c barnsley-fern -d 7 -w 1000 -h 1300 -o fern.svg --color "#4CAF50" --background "#000000" --thickness 0.5`

## Curve options

Curves (all wired through CurveRegistry and selectable via -c by name or index):

1. `hilbert`: recursive SFC on the square
2. `morton`: SFC via bit interleaving
3. `dragon`: Heighway dragon using turn sequence
4. `levy`– Lévy C-curve via recursive segment replacement
5. `barnsley-fern`: IFS/random iteration fractal
6. `peano`: Peano SFC via L-system
7. `sierpinski`: Sierpiński arrowhead curve (L-system, triangular)
8. `gosper`: Gosper flowsnake (L-system, hexagonal)
9. `newton`: Newton fractal (z^3 - 1 basins boundary approximation via polyline)

## Extra work / Bonuses

- interesting graphic design (colors, line thickness, gradients)
  - I have added two as (non mandatory) options, gradient is present overall, since I find patterns better visible this way
- more curve types
  - list of curve types above

## Use of AI

- Information on SFC and fractal curves in general
- Help with "formulation" of patterns mathematically and for code (many websites have tutorials for this, but, since I was doing as many of them as I was, it was easier to find everything in one place)
