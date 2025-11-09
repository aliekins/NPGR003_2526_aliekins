# Documentation of the task "01-AllTheColors"

## Author

Sara Alić-Ekinović

## Command line arguments
```
-w, --width
  not required, default = 4096
  image width in pixels

-h, --height
  not required, default = 4096
  image height in pixels

-o, --output
  not required, default = "xxx.png"
  output file name

-m, --mode
  not required, default = "trivial"
  mode: trivial | random | pattern

--seed
  not required
  random seed (only for random mode)

--pattern-style
  not required, default = "snake"
  pattern style: snake | diagonal | spiral | blocks

--pattern-block-size
  not required, default = 64
  block size for pattern=blocks

--mandala-arms
  not required, default = 8
  number of symmetry arms for mandala mode

--mandala-center-x
  not required, default = -1
  center X for mandala (default = middle)

--mandala-center-y
  not required, default = -1
  center Y for mandala (default = middle)

--ornament-depth
  not required, default = 3
  recursion depth for ornament mode

--ornament-min
  not required, default = 32
  minimal region size for ornament mode
```
## Input data

## Examples

`dotnet run -- -w 4096 -h4096 -o trivial.png -m trivial`

`dotnet run -- -w 4096 -h4096 -o random.png -m random --seed `
`dotnet run -- -w 4096 -h4096 -o pattern.png -m pattern`

`dotnet run -- -m pattern --pattern-style diagonal -o diag.png`

`dotnet run -- -m pattern --pattern-style spiral -o spiral.png`

`dotnet run -- -m pattern --pattern-style blocks --pattern-block-size 16 -o blocks.png`

`dotnet run -- -m ornament --ornament-depth 128 --ornament-min 256 -o ornament.png`

`dotnet run -- -m mandala --mandala-arms 64 -o mandala.png`

#### Possible Outputs

`Image 'xxx.png' created successfully.`

`ERROR image too small: needs at least 16777216 pixels (got 163840).`

`ERROR unknown mode: xxx`

`ERROR ...`

#### Color count check

`python3 check.py xxx.png`

Expected output: `Congratulations, your image contains exactly 2^24 = 16777216 unique colors`

## Extra work / Bonuses

Done:

- more patterns (mandala, ornament)
- more command-line arguments
