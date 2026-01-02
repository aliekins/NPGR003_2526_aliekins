# Documentation of the task "05-Animation"

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

-p, --fps
    not required, default = 30.0
    frames per second (used when encoding video with ffmpeg)

-f, --frames
    not required, default = 60
    total number of frames in the animation

-o, --output
    not required, default = "anim/out{0:0000}.png"
    output filename mask for frames (use C# format, e.g. anim/out{0:0000}.png")

-s, --style
    not required, default = MandalaStyleKind.Celtic
    mandala style: Sand, Geometric, Hindu, Celtic, Lotus, Chakra, Tantric, Buddhav

--seed
    not required = false
    random seed for mandala generation

--symmetry
    not required, default = 8
    rotational symmetry order (e.g. 5-12)

--detail
    not required, default = 0.5
    level of detail [0,1]
```

## Input data

`dotnet run -- -w xxx -h xxx -f zzz -p ww -s _style_ -o "anim/out{0:0000}.png" --symmetry x --detail y.y`

`ffmpeg -framerate ww -i "anim/out%04d.png" -c:v libx264 -pix_fmt yuv420p _style_.mp4`

## Examples

```
dotnet run -- -w 900 -h 900 -f 240 -p 30 -s Buddha -o "anim/out{0:0000}.png" --symmetry 10 --detail 0.8
ffmpeg -framerate 30 -i "anim/out%04d.png" -c:v libx264 -pix_fmt yuv420p buddha.mp4

dotnet run -- -w 900 -h 900 -f 240 -p 30 -s Sand -o "anim/out{0:0000}.png" --symmetry 10 --detail 0.85
ffmpeg -framerate 30 -i "anim/out%04d.png" -c:v libx264 -pix_fmt yuv420p sand.mp4

dotnet run -- -w 900 -h 900 -f 240 -p 30 -s Celtic -o "anim/out{0:0000}.png" --symmetry 10 --detail 0.8
ffmpeg -framerate 30 -i "anim/out%04d.png" -c:v libx264 -pix_fmt yuv420p celtic.mp4

dotnet run -- -w 900 -h 900 -f 240 -p 30 -s Lotus -o "anim/out{0:0000}.png" --symmetry 10 --detail 0.8
ffmpeg -framerate 30 -i "anim/out%04d.png" -c:v libx264 -pix_fmt yuv420p lotus.mp4

dotnet run -- -w 900 -h 900 -f 240 -p 30 -s Geometric -o "anim/out{0:0000}.png" --symmetry 10 --detail 0.75
ffmpeg -framerate 30 -i "anim/out%04d.png" -c:v libx264 -pix_fmt yuv420p geometric.mp4

dotnet run -- -w 900 -h 900 -f 240 -p 30 -s Tantric -o "anim/out{0:0000}.png" --symmetry 9 --detail 0.6
ffmpeg -framerate 30 -i "anim/out%04d.png" -c:v libx264 -pix_fmt yuv420p tantric.mp4

dotnet run -- -w 900 -h 900 -f 240 -p 30 -s Chakra -o "anim/out{0:0000}.png" --symmetry 12 --detail 0.7
ffmpeg -framerate 30 -i "anim/out%04d.png" -c:v libx264 -pix_fmt yuv420p chakra.mp4

dotnet run -- -w 900 -h 900 -f 240 -p 30 -s Hindu -o "anim/out{0:0000}.png" --symmetry 10 --detail 0.75
ffmpeg -framerate 30 -i "anim/out%04d.png" -c:v libx264 -pix_fmt yuv420p hindu.mp4
```

Personal favourites: Celtic, Chakra, Tantric

## Links for public uploads

TODO

## Use of AI

As discussed in task 04-Mandala
