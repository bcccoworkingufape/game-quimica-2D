"""
Enables Android override + crunched compression on every PNG meta
under Assets/Resources/Sprites/Flasks. These 285 frames are forced
into the build because the Animator Controller lives in Resources.
Crunching them cuts a large chunk of APK + runtime memory.
"""
import os, re

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
os.chdir(ROOT)

FOLDER = "Assets/Resources/Sprites/Flasks"

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


def patch(meta_path: str) -> str:
    with open(meta_path, "r", encoding="utf-8") as f:
        src = f.read()

    def repl(m):
        target = m.group("target")
        prefix1 = m.group(1)
        max_val = m.group("max")
        mid1 = m.group(4)
        comp = m.group("comp")
        mid2 = m.group(6)
        q = m.group("q")
        mid3 = m.group(8)
        crunch = m.group("crunch")
        mid4 = m.group(10)
        ovr = m.group("ovr")

        if target == "Android":
            new_max = min(int(max_val), 1024)
            return f"{prefix1}{new_max}{mid1}{comp}{mid2}{q}{mid3}1{mid4}1"
        if target == "DefaultTexturePlatform" and int(max_val) > 2048:
            return f"{prefix1}2048{mid1}{comp}{mid2}{q}{mid3}{crunch}{mid4}{ovr}"
        return m.group(0)

    new_src = BLOCK.sub(repl, src)

    if new_src == src:
        return "NOCHANGE"

    with open(meta_path, "w", encoding="utf-8") as f:
        f.write(new_src)
    return "OK"


changed = 0
skipped = 0
for root, _dirs, files in os.walk(FOLDER):
    for fname in files:
        if not fname.endswith(".png.meta"):
            continue
        result = patch(os.path.join(root, fname))
        if result == "OK":
            changed += 1
        else:
            skipped += 1

print(f"changed={changed}, unchanged={skipped}")
