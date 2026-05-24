"""
Restore visual quality on high-visibility sprites for Android:

- Molz (rat character) sprites: animated, very visible -> max 4096,
  no crunch, high-quality compressed (textureCompression=2).
- Mode cards body/face: same treatment, 4096 / 2048.
- Flask animation frames (Resources/Sprites/Flasks): max 2048,
  no crunch, high-quality compressed.

Crunched compression was the main cause of the pixelated look.
We trade some APK size for visible quality on the characters that
the player sees the most.
"""
import os
import re

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
os.chdir(ROOT)

BLOCK = re.compile(
    r"(  - serializedVersion: 4\n"
    r"    buildTarget: (?P<target>[\w]+)\n"
    r"    maxTextureSize: )(?P<max>\d+)(\n"
    r"    resizeAlgorithm: 0\n"
    r"    textureFormat: -1\n"
    r"    textureCompression: )(?P<comp>\d+)(\n"
    r"    compressionQuality: )(?P<q>\d+)(\n"
    r"    crunchedCompression: )(?P<crunch>\d+)(\n"
    r"    allowsAlphaSplitting: \d+\n"
    r"    overridden: )(?P<ovr>\d+)"
)


def patch_meta(path: str, android_max: int, android_comp: int = 2,
               android_quality: int = 100, android_crunch: int = 0,
               android_overridden: int = 1) -> str:
    with open(path, "r", encoding="utf-8") as f:
        src = f.read()

    def repl(m):
        target = m.group("target")
        if target != "Android":
            return m.group(0)
        prefix1 = m.group(1)
        mid1 = m.group(4)
        mid2 = m.group(6)
        mid3 = m.group(8)
        mid4 = m.group(10)
        return (
            f"{prefix1}{android_max}{mid1}{android_comp}{mid2}"
            f"{android_quality}{mid3}{android_crunch}{mid4}{android_overridden}"
        )

    new_src = BLOCK.sub(repl, src)
    if new_src == src:
        return "NOCHANGE"
    with open(path, "w", encoding="utf-8") as f:
        f.write(new_src)
    return "OK"


# (relative path, android_max)
HIGH_QUALITY_TARGETS = [
    # Molz character — high visibility, animated
    ("Assets/Images/Molz/Idle/molz_idle.png.meta", 4096),
    ("Assets/Images/Molz/Run/molz_run.png.meta", 4096),
    ("Assets/Images/Molz/Run (Paper Model)/paper_molz_run.png.meta", 4096),
    # Mode card bodies (the big rat illustration on menu cards)
    ("Assets/Images/Components/EasyModeCard/easy_mode_molz_body_recut.png.meta", 4096),
    ("Assets/Images/Components/MediumModeCard/medium_mode_molz_body.png.meta", 4096),
    ("Assets/Images/Components/HardModeCard/hard_mode_molz_body.png.meta", 4096),
    # Mode card faces (smaller portraits)
    ("Assets/Images/Components/EasyModeCard/easy_mode_molz_face.png.meta", 2048),
    ("Assets/Images/Components/MediumModeCard/medium_mode_molz_face.png.meta", 2048),
    ("Assets/Images/Components/HardModeCard/hard_mode_molz_face.png.meta", 2048),
    # Solution mix animations (visible on every successful mix)
    ("Assets/Images/Solutions Animations/liq_sol_torn_blue.png.meta", 2048),
    ("Assets/Images/Solutions Animations/liq_sol_torn_red.png.meta", 2048),
    ("Assets/Images/Solutions Animations/liq_ins_less_dense.png.meta", 2048),
    ("Assets/Images/Solutions Animations/liq_ins_more_dense.png.meta", 2048),
    ("Assets/Images/Solutions Animations/solid_sol_torn_blue.png.meta", 2048),
    ("Assets/Images/Solutions Animations/solid_sol_NAOH.png.meta", 2048),
    ("Assets/Images/Solutions Animations/solid_ins_more_dense.png.meta", 2048),
]

print("== High-visibility sprites ==")
for rel, max_size in HIGH_QUALITY_TARGETS:
    if not os.path.exists(rel):
        print(f"MISS {rel}")
        continue
    result = patch_meta(rel, android_max=max_size)
    print(f"{result} {rel} (android max={max_size}, crunch=0, comp=2 high)")

# Flask animation frames — bump quality but keep size moderate (lots of frames)
FLASK_FOLDER = "Assets/Resources/Sprites/Flasks"
changed = 0
skipped = 0
print("\n== Flask animation frames ==")
for root, _dirs, files in os.walk(FLASK_FOLDER):
    for fname in files:
        if not fname.endswith(".png.meta"):
            continue
        path = os.path.join(root, fname)
        result = patch_meta(path, android_max=2048,
                            android_comp=2, android_quality=100,
                            android_crunch=0, android_overridden=1)
        if result == "OK":
            changed += 1
        else:
            skipped += 1
print(f"Flasks: changed={changed}, unchanged={skipped}")
