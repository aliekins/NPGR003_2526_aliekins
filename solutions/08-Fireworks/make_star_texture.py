from PIL import Image
import math

def clamp01(x: float) -> float:
    if x < 0.0:
        return 0.0
    if x > 1.0:
        return 1.0
    return x

def make_star_texture(size: int = 64, filename: str = "particle_star.png") -> None:
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    px = img.load()

    cx = (size - 1) * 0.5
    cy = (size - 1) * 0.5
    rmax = min(cx, cy)

    # Tunables
    core_sharpness = 3.0   # higher => tighter bright core
    glow_power = 2.0       # higher => tighter overall glow
    spike_power = 6.0      # higher => thinner spikes
    spike_strength = 0.9   # 0..1 how bright spikes are relative to core
    glow_strength = 0.7    # 0..1 base radial glow contribution

    for y in range(size):
        for x in range(size):
            dx = (x - cx) / rmax
            dy = (y - cy) / rmax

            r = math.sqrt(dx * dx + dy * dy)
            if r >= 1.0:
                continue

            # Base radial glow (soft circle)
            glow = (1.0 - r)
            glow = clamp01(glow) ** glow_power

            # Bright core (tighter than glow)
            core = (1.0 - r)
            core = clamp01(core) ** core_sharpness

            # Cross spikes (horizontal + vertical)
            # Values near the axes should be brighter.
            ax = 1.0 - abs(dx)  # brightest at dx=0
            ay = 1.0 - abs(dy)  # brightest at dy=0
            ax = clamp01(ax) ** spike_power
            ay = clamp01(ay) ** spike_power

            spikes = max(ax, ay)  # cross shape

            # Combine components into alpha
            a = glow_strength * glow + core + spike_strength * spikes

            # Keep in range
            a = clamp01(a)

            # White RGB with computed alpha; tinting happens in your shader via particle color
            alpha = int(a * 255.0)
            px[x, y] = (255, 255, 255, alpha)

    img.save(filename)
    print(f"Saved {filename} ({size}x{size})")

if __name__ == "__main__":
    make_star_texture(size=64, filename="particle_star.png")