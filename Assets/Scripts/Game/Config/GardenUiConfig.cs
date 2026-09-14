using System;
using UnityEngine;
namespace MMORPG.Game.Config
{
    [Serializable] public sealed class GardenUiConfig
    {
        public string ink = "#202525", paper = "#F5F4EA", green = "#34785B", red = "#BA443E", focus = "#F3D36A", energy = "#E96BAD";
        public int menuFont = 28, bodyFont = 24, auxiliaryFont = 20;
        public Rect bossName = new Rect(420, 24, 440, 32), bossBar = new Rect(380, 64, 520, 20);
        public Rect healthCards = new Rect(32, 616, 148, 64), energyCards = new Rect(208, 632, 124, 48);
        public Rect title = new Rect(96, 144, 480, 140), cast = new Rect(720, 160, 460, 400);
        public Rect programme = new Rect(400, 112, 480, 496), defeatTicket = new Rect(280, 154, 720, 390);
        public float healthFlip = 0.18f, energyPulse = 0.24f, energyPulseScale = 1.08f;
        public float healthTrailDelay = 0.12f, healthTrailDuration = 0.25f, bossFill = 0.3f;
        public float titleEnter = 0.35f, titleExit = 0.25f, menuEnter = 0.16f, menuExit = 0.12f;
        public float defeatEnter = 0.22f, victoryEnter = 0.28f, hudExit = 0.15f, stampInterval = 0.08f;
        public float focusDuration = 0.1f, pressDuration = 0.06f, releaseDuration = 0.1f, shadeAlpha = 0.42f;
    }
}
