# Documentation of the task "06-ImageRecoloring"

## Author

Sara Alić-Ekinović

## Command line arguments

```
-i', --input
    required,
    input image file

-o, --output
    required,
    output image file

-h, --hue
    not required, default = 0.0,
    hue delta in degrees (float); positive/negative allowed; 0 = no recoloring; outputs skin mask for debugging
```

## Input data

`dotnet run -- -i ./images/NN-XXX.jpg -o ./results/recolored/recolorNN_YY.png --hue YY`

> out
> `Saved: ./results/recolored/recolorNN_YY.png`

## Examples (best detection from the given pool of images)

`dotnet run -- -i ./images/01-beard.jpg -o ./results/recolored/recolor01_40.png --hue 40`

> out
> `Saved: ./results/recolored/recolor01_40.png`

`dotnet run -- -i ./images/03-bicycle.jpg -o ./results/recolored/recolor03_80.png --hue 80`

> out
> `Saved: ./results/recolored/recolor03_80.png`

`dotnet run -- -i ./images/07-laugh.jpg -o ./results/recolored/recolor07_-50.png --hue -50`

> out
> `Saved: ./results/recolored/recolor07_-50.png`

`dotnet run -- -i ./images/10-hat.jpg -o ./results/recolored/recolor10_-80.png --hue -80`

> out
> `Saved: ./results/recolored/recolor10_-80.png`

## Note

The quality of this solution is not very high. I have started working on it on time, however, I got sick (tonsilitis/angina) in the process. Thus, the quality and my patience plummeted. (I am mentioning all of this so You would know that such a big difference has nothing to do with the topic nor the subject itself, but me alone :D)
