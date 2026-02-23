from PIL import Image
import math

size = 64
img = Image.new("RGBA", (size, size))
cx = cy = (size - 1) / 2

for y in range(size):
    for x in range(size):
        dx = (x - cx) / cx
        dy = (y - cy) / cy
        r = math.sqrt(dx*dx + dy*dy)

        # Flat core until 70% radius, then fade
        if r < 0.7:
            a = 1.0
        else:
            t = (r - 0.7) / (1.0 - 0.7)
            a = max(0.0, 1.0 - t)

        # Gentle falloff
        a = a ** 1.5

        img.putpixel((x, y), (255, 255, 255, int(255 * a)))

img.save("particle.png")