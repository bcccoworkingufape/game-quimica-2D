"""
One-shot helper: enables Android-specific texture import overrides
on heavy PNGs that were falling back to DefaultTexturePlatform's
maxTextureSize=16384 (huge APK + Android OOM/Vulkan crashes).

Each targeted .meta gets:
- buildTarget: Android  -> overridden: 1, crunchedCompression: 1
- buildTarget: DefaultTexturePlatform -> maxTextureSize lowered to 4096

Editor visuals (Standalone) are untouched.
"""
import os, re, sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
os.chdir(ROOT)

# (meta path, android max size)
TARGETS = [
    ("Assets/Images/Components/MediumModeCard/medium_mode_molz_body.png.meta", 2048),
    ("Assets/Images/Components/HardModeCard/hard_mode_molz_body.png.meta",   2048),
    ("Assets/Images/Molz/Idle/molz_idle.png.meta",                           2048),
    ("Assets/Images/Components/EasyModeCard/easy_mode_molz_body_recut.png.meta", 2048),
    ("Assets/Images/Molz/Run/molz_run.png.meta",                             2048),
    ("Assets/Images/Molz/Run (Paper Model)/paper_molz_run.png.meta",         2048),
    ("Assets/Images/Components/HardModeCard/hard_mode_molz_face.png.meta",   1024),
    ("Assets/Images/Components/EasyModeCard/easy_mode_molz_face.png.meta",   1024),
    ("Assets/Images/Components/MediumModeCard/medium_mode_molz_face.png.meta", 1024),
    ("Assets/Images/big background bright.png.meta",                         2048),
    ("Assets/Images/background_bricks.png.meta",                             2048),
    ("Assets/Images/Layouts/Lab Page/background_stones.png.meta",            2048),
    ("Assets/Images/Layouts/Lab Page/big background.png.meta",               2048),
    ("Assets/Images/Layouts/Menu - Shop Page/shopBackground.png.meta",       2048),
    ("Assets/Images/Layouts/Menu - Settings Page/SettingsPage.png.meta",     2048),
    ("Assets/Images/Layouts/Lab Page/labBackground.png.meta",                2048),
    ("Assets/Images/Solutions Animations/liq_sol_torn_blue.png.meta",        1024),
    ("Assets/Images/Solutions Animations/solid_ins_more_dense.png.meta",     1024),
    ("Assets/Images/Solutions Animations/solid_sol_torn_blue.png.meta",      1024),
    ("Assets/Images/Solutions Animations/liq_ins_less_dense.png.meta",       1024),
    ("Assets/Images/Solutions Animations/solid_sol_NAOH.png.meta",           1024),
    ("Assets/Images/Solutions Animations/liq_ins_more_dense.png.meta",       1024),
    ("Assets/Images/Solutions Animations/liq_sol_torn_red.png.meta",         1024),
]

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

def patch(meta_path: str, android_max: int) -> str:
    if not os.path.exists(meta_path):
        return f"MISS {meta_path}"

    with open(meta_path, "r", encoding="utf-8") as f:
        src = f.read()

    changes = []

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
            new_max = str(android_max)
            new_crunch = "1"
            new_ovr = "1"
            changes.append(f"Android max {max_val}->{new_max} crunch {crunch}->{new_crunch} ovr {ovr}->{new_ovr}")
            return f"{prefix1}{new_max}{mid1}{comp}{mid2}{q}{mid3}{new_crunch}{mid4}{new_ovr}"

        if target == "DefaultTexturePlatform" and int(max_val) > 4096:
            new_max = "4096"
            changes.append(f"Default max {max_val}->{new_max}")
            return f"{prefix1}{new_max}{mid1}{comp}{mid2}{q}{mid3}{crunch}{mid4}{ovr}"

        return m.group(0)

    new_src = BLOCK.sub(repl, src)

    if new_src == src:
        return f"NOCHANGE {meta_path}"

    with open(meta_path, "w", encoding="utf-8") as f:
        f.write(new_src)

    return f"OK {meta_path}: {'; '.join(changes)}"


def main():
    for meta, amax in TARGETS:
        print(patch(meta, amax))


if __name__ == "__main__":
    main()
