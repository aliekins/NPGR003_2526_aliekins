# Documentation of the task "01-AllTheColors"

## Author

Sara Alić-Ekinović

## Command line arguments

-w, --width<br> - not required, default = 4096<br> - image width in pixels<br>

-h, --height<br> - not required, default = 4096<br> - image height in pixels<br>

-o, --output<br> - not required, default = "xxx.png"<br> - output file name<br>

-m, --mode<br> - not required, default = "trivial"<br> - mode: trivial | random | pattern<br>

--seed<br> - not required<br> - random seed (only for random mode)<br>

## Input data

Define the format of the input data if it is relevant to the task.

## Examples

#### Inputs

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
