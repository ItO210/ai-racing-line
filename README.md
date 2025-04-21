# AIRacingLine: AI-Generated Racing Line with Dynamic Feedback

AIRacingLine is an AI-driven system designed to generate dynamic racing lines in Unity, providing real-time feedback based on player speed. An agent is trained using ML-Agents to drive around a track optimally. Its recorded path is then used to display a racing line that changes color (green, yellow, or red) depending on how the player's current speed compares to the agent's at the same point, helping guide players through acceleration and braking zones.

This project was developed on **September 2024.** for Rewind Games, an  indie game studio based in Vancouver, BC.

## Features

- **AI Agent Training**: Fine-tuned agent training using reward shaping and sensor experimentation.
- **Racing Line Generation**: The racing line is generated from data collected from the trained agent.
- **Dynamic Color Feedback**: Real-time line color updates based on player speed.

## Contents

- **AIRacingLine/**: Unity project containing the racing line visualization and training scenes.
- **ml-agents-release-21/**: ML-Agents package used for training and running the AI agents.

## Preview
![12](https://github.com/user-attachments/assets/ae002af8-31eb-43d3-be09-b2cf3899466a)

## Training & Research

- Sensor Types Tested:
  - Raycast Sensor
  - Grid Sensor (yielded the best results with more consistent and stable racing lines)
  - Combination of Raycast and Grid Sensors
    
![5](https://github.com/user-attachments/assets/8fef2c5e-ae2e-4621-bc97-510d5d03600c)

- Reward Schemes Evaluated:
  - Velocity-based only
  - Velocity + Time Between Checkpoints
  - Weighted Checkpoints (with higher multipliers for reaching checkpoints quickly)
    
![6](https://github.com/user-attachments/assets/befdaceb-a4fd-4fbc-b56a-012d60b8b5ff)

- Training Tracks:
  - Tested both synthetic training tracks (zig-zag patterns with increasing difficulty) and real race tracks.
  - Agents trained on real tracks consistently outperformed those trained on synthetic ones, leading to better generalization and racing line quality.
    
![8](https://github.com/user-attachments/assets/158ceb35-a781-4a85-acad-9c6610dcc00b)
![9](https://github.com/user-attachments/assets/fe0c304d-b69d-44b8-96df-027f11b55194)

- Divided Checkpoints:
  - Introduced asymmetric rewards:
    - Positive rewards for passing through the inner side of curves.
    - Negative rewards for passing through the outer side.
  - Resulted in more aggressive and optimized racing lines, especially in sharp turns.
    
![10](https://github.com/user-attachments/assets/69ab651c-e033-4434-a58d-765641d98514)

- Key Observations:
  - Excessive training led to performance degradation.
  - Incremental difficulty in training environments sped up learning.

- Results:
  
![7](https://github.com/user-attachments/assets/aae19f25-3964-4f31-83c6-d404d1b72f3f)
![11](https://github.com/user-attachments/assets/bf9a8bdc-aeb9-413a-b163-d4e1ecce6294)

## Contributors  
This project was developed collaboratively by:  

- [Carlos Ito]()  
- [Juan Pablo Sebastián]()
