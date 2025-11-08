from PIL import Image
import sys

filename = "trivial.png"
if len(sys.argv) > 1:
	filename = sys.argv[1]

image = Image.open(filename)

colors = image.getcolors(maxcolors=2**24)
colors_count = len(colors)

if colors_count == 2**24:
    print(f"Congratulations, your image contains exactly 2^24 = {2**24} unique colors")
else:
    print(f"Failed. Your image contains {colors_count} unique colors instead of 2^24 = {2**24}")
