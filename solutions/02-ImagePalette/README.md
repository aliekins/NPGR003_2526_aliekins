# Documentation of the task "NN-XXX"

## Author

Alić-Ekinović Sara

## Command line arguments

```
-i, --input
    required
    input image file name

-o, --output
    not required, default = ""
    output file-name (PNG or SVG)

-c, --colors
    not required, default = 5
    required number of colors (3–10 is recommended)

-r, --recreate
    not required, default = ""
    recreated image output file-name
```

## Examples

Exceptions/Errors are handled similarly as in previous homework (in console there is something along the lines of `ERROR: ...`).

#### Basic tests

In ./tests/TestImageGenerator<br>
`dotnet run`

> out <br> `Test images generated.`

In ./02-ImagePalette<br>
`dotnet run -- -i ../tests/TestImageGenerator/red.png -c 1`

> out <br> `255 0 0`

`dotnet run -- -i ../tests/TestImageGenerator/red.png -c 1 -o red_palette.svg`

> out
>
> ```
> 255 0 0
> SVG palette saved to 'red_palette.svg'.
> ```

`dotnet run -- -i ../tests/TestImageGenerator/red_green.png -c 2 -o red_green_palette.png`

> out
>
> ```
> 0 255 0
> 255 0 0
> PNG palette saved to 'red_green_palette.png'.
> ```

`dotnet run -- -i ../tests/TestImageGenerator/rgb_stripes.png -c 3 -o rgb_stripes_palette.png`

> out
>
> ```
> 0 0 255
> 0 255 0
> 255 0 0
> PNG palette saved to 'rgb_stripes_palette.png'.
> ```

`dotnet run -- -i ../tests/TestImageGenerator/gradient.png -c 10 -o gradient_palette.png`

> out
>
> ```
> 1 1 1
> 33 33 33
> 67 67 67
> 107 107 107
> 131 131 131
> 155 155 155
> 179 179 179
> 203 203 203
> 227 227 227
> 251 251 251
> PNG palette saved to 'gradient_palette.png'.
> ```

#### Tests on more complex images

_Note: Arbitrary number of colors in a palette is of course available, here are just the results I found interesting/pleasent to look at_<br>
_Note: For brievety, I will not be adding long outputs. Their format, however is the same as above._<br>

`dotnet run -- -i ../tests/images/arthur-mazi-c4Eh-VZcWoc-unsplash.jpg -c 8 -o pool_palette.png -r pool_quantized.png`

`dotnet run -- -i ../tests/images/gautam-krishnan-esPP01NpBfY-unsplash.jpg -c 2 -o city_palette.png -r city_quantized.png`

`dotnet run -- -i ../tests/images/gautam-krishnan-esPP01NpBfY-unsplash.jpg -c 28 -o city_palette.png -r city_quantized.png`

`dotnet run -- -i ../tests/images/ethan-ihP15orhXT4-unsplash.jpg -c 4 -o fruit_market_palette.png -r fruit_market_quantized.png`

`dotnet run -- -i ../tests/images/ethan-ihP15orhXT4-unsplash.jpg -c 64 -o fruit_market_palette.png -r fruit_market_quantized.png`

`dotnet run -- -i ../tests/images/david-kohler-7xsBS4vFR-g-unsplash.jpg -c 3 -o cat_palette.png -r cat_quantized.png`

`dotnet run -- -i ../tests/images/jan-jakub-nanista-z9hvkSDWMIM-unsplash.jpg -c 5 -o house_palette.png -r house_quantized.png`

`dotnet run -- -i ../tests/images/jan-jakub-nanista-z9hvkSDWMIM-unsplash.jpg -c 128 -o house_palette.png -r house_quantized.png`

`dotnet run -- -i ../tests/images/ruben-gutierrez-nTTh5UXkHp8-unsplash.jpg -c 9 -o street_palette.png -r street_quantized.png`

`dotnet run -- -i ../tests/images/seiya-maeda-Ow3ycF_ZYI4-unsplash.jpg -c 2 -o snow_palette.png -r snow_quantized.png`

`dotnet run -- -i ../tests/images/seiya-maeda-Ow3ycF_ZYI4-unsplash.jpg -c 12 -o snow_palette.png -r snow_quantized.png`

## Extra work / Bonuses

First two points are, of course, up to You to decide. For the last one, (output colors are pleasantly sorted) I have tried my best :D
