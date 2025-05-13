<details>
  <summary>Table of Contents</summary>
  <ol>
    <li><a href="#about-the-project">About The Project</a></li>
    <li><a href="#features">Features</a></li>
  </ol>
</details>

## About the project

UAudio Subtitle Editor is a custom editor plugin for Unity that lets you place precise time markers on audio clips and define subtitles that display in sync with playback. It’s designed to provide accurate, localized subtitles even when the game is paused, minimized, or experiencing lag.

# Background

The original system I started out with relied on Unity’s Coroutines to display subtitles and introduce timed pauses. However, this approach presented a significant issue: when the game was paused or minimized, Coroutines would also pause, while the audio continued playing. This caused a mismatch between the subtitles and the audio, leading to desynchronization.

To resolve this, I considered potential solutions, drawing inspiration from a previous project in C++ where I used FMOD. In that project, I could easily access the sample position of the currently playing audio, which allowed me to synchronize subtitles and perform beat matching within a custom engine. I then wondered, *Could I replicate this functionality in Unity*?

## Features

This repository has an assets folder with the plugin code and an example clip with subtitles.

🎯 Custom Editor Timeline
Add subtitle markers directly to audio clips via a user-friendly timeline interface.

⏱ Time-Based Playback Sync
Subtitles are triggered using the actual playback time of the audio clip, not dependent on Coroutines or game time.

🛑 Handles Pausing & Minimizing
Because it's based on real audio time, the system displays missed subtitles instantly when resuming from pause or minimizing.

🌐 Localization Support
Define subtitles per language to support localized text for your game.

🧩 Built on Unity UI
Seamlessly integrated with Unity’s native audio clip inspector to maintain a familiar workflow.

https://github.com/user-attachments/assets/e2ee44dd-5e7e-49f1-a0d7-0fc710f8f599
