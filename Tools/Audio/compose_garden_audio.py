"""Render original swing music and differentiated game cues to offline WAV assets."""
from pathlib import Path
import json
import math
import wave
import numpy as np

RATE = 48000
ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "Assets/Res/Audio/Arranged"
REPORT = ROOT / "Plan/Validation/DEV-008/audio_metrics.json"
METRICS = {}


def save(name, samples, stereo=False):
    samples = np.asarray(samples, dtype=np.float32)
    peak = float(np.max(np.abs(samples)))
    if peak > 0:
        samples *= 0.78 / max(0.78, peak)
    edge = min(480, len(samples) // 10)
    fade = np.linspace(0, 1, edge)
    if stereo:
        fade = fade[:, None]
    samples[:edge] *= fade
    samples[-edge:] *= fade[::-1]
    path = OUT / name
    path.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(path), "wb") as writer:
        writer.setparams((2 if stereo else 1, 2, RATE, 0, "NONE", "not compressed"))
        writer.writeframes((samples * 32767).astype("<i2").tobytes())
    METRICS[name] = {
        "seconds": round(len(samples) / RATE, 3),
        "channels": 2 if stereo else 1,
        "peak_dbfs": round(20 * math.log10(max(float(np.max(np.abs(samples))), 1e-9)), 2),
        "rms_dbfs": round(20 * math.log10(max(float(np.sqrt(np.mean(samples ** 2))), 1e-9)), 2),
        "clipped_samples": int(np.count_nonzero(np.abs(samples) >= 1)),
    }


def tone(note, seconds, instrument="reed"):
    t = np.arange(int(seconds * RATE), dtype=np.float32) / RATE
    hz = 440 * 2 ** ((note - 69) / 12)
    phase = 2 * np.pi * hz * t + 0.025 * np.sin(2 * np.pi * 5.2 * t)
    if instrument == "bass":
        sound = np.sin(phase) + 0.18 * np.sin(phase * 2) + 0.05 * np.sin(phase * 3)
        env = np.exp(-t * 4)
    elif instrument == "keys":
        sound = np.sin(phase + 1.4 * np.sin(phase * 2) * np.exp(-t * 8))
        env = np.exp(-t * 5)
    elif instrument == "brass":
        sound = sum(np.sin(phase * n) / n ** 1.3 for n in range(1, 7)) * 0.6
        env = 0.6 + 0.4 * np.exp(-t * 10)
    else:
        sound = (np.sin(phase) + 0.3 * np.sin(phase * 3) + 0.1 * np.sin(phase * 5)) * 0.7
        env = 0.75 + 0.25 * np.exp(-t * 12)
    attack = np.minimum(t / 0.012, 1)
    release = np.minimum((seconds - t) / min(0.06, seconds / 2), 1)
    return sound * env * attack * np.clip(release, 0, 1)


def drum(kind, rng):
    duration = {"kick": 0.22, "snare": 0.18, "hat": 0.07, "ride": 0.24}[kind]
    t = np.arange(int(duration * RATE)) / RATE
    noise = rng.uniform(-1, 1, len(t))
    if kind == "kick":
        sound = np.sin(2 * np.pi * (45 * t + 2.5 * (1 - np.exp(-t * 35)))) * np.exp(-t * 23)
    elif kind == "snare":
        sound = (noise * 0.65 + np.sin(2 * np.pi * 185 * t) * 0.25) * np.exp(-t * 35)
    elif kind == "hat":
        sound = np.diff(noise, prepend=noise[0]) * np.exp(-t * 80) * 0.3
    else:
        sound = sum(np.sin(2 * np.pi * hz * t) for hz in (3071, 4567, 6181)) * np.exp(-t * 20) * 0.1
    return sound * np.minimum(t / 0.002, 1)


def music(stage):
    bpm = (132, 120, 148)[stage]
    beat = 60 / bpm
    bars = 16
    mix = np.zeros((int(bars * 4 * beat * RATE), 2), dtype=np.float32)
    rng = np.random.default_rng(930 + stage)
    chords = [(48, 51, 55, 58), (53, 56, 60, 63), (46, 50, 53, 56), (51, 55, 58, 62),
              (44, 48, 51, 55), (50, 53, 56, 60), (43, 47, 50, 53), (48, 51, 55, 58)]
    melodies = [
        [72, 75, 79, 77, 75, 72, 70, 0], [72, 75, 80, 79, 77, 75, 72, 0],
        [74, 77, 80, 77, 74, 72, 70, 0], [75, 79, 82, 79, 77, 75, 74, 0],
        [72, 75, 79, 80, 79, 75, 72, 0], [74, 77, 80, 81, 80, 77, 74, 0],
        [71, 74, 77, 80, 79, 77, 74, 71], [72, 0, 79, 77, 75, 0, 72, 0],
    ]

    def add(sound, start, gain, pan=0):
        offset = int(start * RATE)
        length = min(len(sound), len(mix) - offset)
        if length <= 0:
            return
        angle = (pan + 1) * np.pi / 4
        mix[offset:offset + length, 0] += sound[:length] * gain * np.cos(angle)
        mix[offset:offset + length, 1] += sound[:length] * gain * np.sin(angle)

    for bar in range(bars):
        chord = chords[bar % 8]
        for step in range(4):
            start = (bar * 4 + step) * beat
            note = chord[(0, 2, 1, 2)[step]] - 12
            add(tone(note, beat * 0.82, "bass"), start, 0.31)
            add(drum("kick" if step % 2 == 0 else "snare", rng), start, 0.17)
            add(drum("ride", rng), start, 0.16, 0.4)
            add(drum("hat", rng), start + beat * 0.66, 0.13, -0.35)
            if step in (1, 3):
                for n in chord[1:]:
                    add(tone(n + 12, beat * 0.31, "keys"), start, 0.07, -0.3)
        melody = melodies[bar % 8]
        for step, note in enumerate(melody):
            if not note:
                continue
            swing = (step // 2 + (0.66 if step % 2 else 0)) * beat
            instrument = ("reed", "keys", "brass")[stage]
            octave = -12 if stage == 1 and bar % 4 < 2 else 0
            add(tone(note + octave, beat * (0.25 if step % 2 else 0.5), instrument), bar * 4 * beat + swing, 0.21, 0.16)
            if bar >= 8 and step % 2 == 0:
                add(tone(note - 12, beat * 0.24, "brass"), bar * 4 * beat + swing + beat / 8, 0.07, -0.18)
    # Soft saturation keeps short transients below full scale without flattening the mix.
    save(f"Music/garden_{stage + 1}.wav", np.tanh(mix * 1.25) * 0.8, stereo=True)


def cue(name, variant=0):
    seed = sum((i + 1) * ord(c) for i, c in enumerate(name)) + variant
    rng = np.random.default_rng(seed)
    duration = 0.16
    if name in ("charge", "psychic_charge", "emerge", "boss_defeat", "death"):
        duration = 0.55
    if name.startswith("ui_"):
        duration = 0.11 if name in ("ui_focus", "ui_confirm", "ui_cancel") else 0.65
    if name == "super_loop":
        duration = 0.6
    t = np.arange(int(duration * RATE), dtype=np.float32) / RATE
    noise = rng.uniform(-1, 1, len(t))
    f = 1 + variant * 0.045
    envelope = np.minimum(t / 0.003, 1) * np.exp(-t * 15)
    if name == "shot":
        signal = (np.sin(2 * np.pi * (420 * f * t - 900 * t ** 2)) * 0.6 + noise * 0.2) * np.exp(-t * 28)
    elif name in ("parry_success", "pickup", "energy_full", "lock"):
        signal = sum(np.sin(2 * np.pi * hz * f * t) * np.exp(-t * decay) for hz, decay in ((1046, 12), (1568, 22), (2093, 30))) * 0.24
    elif name in ("jump", "double_jump", "dash_start", "dash_end"):
        signal = (np.sin(2 * np.pi * (180 * t + 1000 * t ** 2)) * 0.55 + noise * (0.24 if "dash" in name else 0.08)) * envelope
    elif name in ("hurt", "death", "boss_hit", "boss_defeat", "emerge"):
        signal = (np.sin(2 * np.pi * (75 * t + 2 * (1 - np.exp(-t * 40)))) * 0.7 + noise * 0.2) * envelope
    elif name in ("tear_fall", "tear_splash", "spit", "sob_tell", "wipe", "inhale"):
        signal = (np.sin(2 * np.pi * (280 * f * t + 3 * np.sin(2 * np.pi * 4 * t))) * 0.55 + noise * 0.14) * envelope
    elif name in ("charge", "psychic_charge"):
        signal = np.sin(2 * np.pi * (100 * t + 700 * t ** 2)) * np.sin(np.pi * t / duration) ** 2 * 0.5
    elif name == "super_loop":
        signal = (np.sin(2 * np.pi * 100 * t) + 0.3 * np.sin(2 * np.pi * 300 * t)) * 0.25
    elif name in ("beam", "release", "super_end", "seeker", "stun"):
        signal = (np.sin(2 * np.pi * 160 * t + 2 * np.sin(2 * np.pi * 570 * t)) * 0.5 + noise * 0.15) * envelope
    else:
        notes = (72, 76, 79) if name != "ui_defeat" else (67, 63, 60)
        signal = np.zeros(len(t))
        for i, note in enumerate(notes):
            start = int(i * duration / 4 * RATE)
            sound = tone(note, duration * 0.45, "keys")
            n = min(len(sound), len(signal) - start)
            signal[start:start + n] += sound[:n] * 0.32
    save(f"Combat/{name}_{variant + 1}.wav", signal)


if __name__ == "__main__":
    for stage in range(3):
        music(stage)
    config = json.loads((ROOT / "Assets/Resources/Config/GameConfig.json").read_text(encoding="utf-8-sig"))
    for event in config["audio"]["events"]:
        name = event["eventId"]
        variants = 3 if name == "shot" else 2 if name in ("spit", "parry_success") else 1
        for variant in range(variants):
            cue(name, variant)
    REPORT.parent.mkdir(parents=True, exist_ok=True)
    REPORT.write_text(json.dumps(METRICS, indent=2, ensure_ascii=False), encoding="utf-8")
    print(f"已生成 {len(METRICS)} 个原创编曲与音效资源，削波检查见 {REPORT}")
