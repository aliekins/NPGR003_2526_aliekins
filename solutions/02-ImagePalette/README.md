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

#### Basic tests

In ./tests/TestImageGenerator
`$ dotnet run`

> out
> `Test images generated.`

In ./02-ImagePalette
`$ dotnet run -- -i ../tests/TestImageGenerator/red.png -c 1`

> out
> `255 0 0`

`$ dotnet run -- -i ../tests/TestImageGenerator/red.png -c 1 -o red_palette.svg`

> out
>
> ```
> 255 0 0
> SVG palette saved to 'red_palette.svg'.
> ```

`$ dotnet run -- -i ../tests/TestImageGenerator/red_green.png -c 2 -o red_green_palette.png`
`$ dotnet run -- -i ../tests/TestImageGenerator/rgb_stripes.png -c 3 -o rgb_stripes_palette.png`
`$ dotnet run -- -i ../tests/TestImageGenerator/gradient.png -c 10 -o gradient_palette.png`

#### Tests on more complex images

`$ dotnet run -- -i ../tests/images/arthur-mazi-c4Eh-VZcWoc-unsplash.jpg -c 10 -o pool_palette.png -r pool_quantized.png`

`$ dotnet run -- -i ../tests/images/gautam-krishnan-esPP01NpBfY-unsplash.jpg -c 10 -o city_palette.png -r city_quantized.png`

`$ dotnet run -- -i ../tests/images/gautam-krishnan-esPP01NpBfY-unsplash.jpg -c 50 -o city_palette.png -r city_quantized.png`

#### Expected output

```
0 255 0
255 0 0
PNG palette saved to 'red_green_palette.png'.
```

```
0 0 255
0 255 0
255 0 0
PNG palette saved to 'rgb_stripes_palette.png'.
```

```
7 7 7
23 23 23
39 39 39
55 55 55
79 79 79
111 111 111
143 143 143
175 175 175
207 207 207
239 239 239
PNG palette saved to 'gradient_palette.png'.

```

```
26 68 94
35 110 140
147 115 46
56 133 157
119 131 118
172 142 68
135 125 130
78 149 170
117 176 193
170 166 154
PNG palette saved to 'pool_palette.png'.
Recreated image saved to 'pool_quantized.png'.
```

```
67 66 33
33 86 136
149 104 63
116 134 75
120 139 135
169 150 77
151 119 135
192 187 176
PNG palette saved to 'city_palette.png'.
Recreated image saved to 'city_quantized.png'.
```

```
38 37 17
55 70 21
72 54 33
9 54 101
156 17 16
35 76 93
82 56 70
89 87 32
61 139 39
227 14 13
33 86 136
101 97 82
155 48 78
37 140 106
143 113 33
116 135 39
151 143 39
34 160 141
218 108 11
218 51 72
151 57 134
139 114 93
117 133 98
222 153 19
120 139 135
153 145 98
148 119 134
203 57 145
216 101 90
183 196 39
63 194 163
61 165 202
206 115 143
111 195 163
248 207 19
109 165 202
180 196 104
173 117 192
216 170 98
168 165 155
221 118 197
114 196 229
239 201 107
200 182 165
181 195 181
182 186 194
187 200 195
205 185 197
225 206 170
217 217 212
PNG palette saved to 'city_palette.png'.
Recreated image saved to 'city_quantized.png'.
```

## Algorithm

Any details of the algorithms worth mentioning should go here.

## Extra work / Bonuses

Be sure to mention any extra work you have done (for bonus points).

## Use of AI

Document how an AI assistant helped you or declare that you haven't used AI.
ChatGPT shared conversations are useful to document your
observations (don't forget to add your personal observations and opinions).
