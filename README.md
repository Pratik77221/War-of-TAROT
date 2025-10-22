# War of TAROT

![Unity](https://img.shields.io/badge/Unity-2022.3+-black?logo=unity)
![Platform](https://img.shields.io/badge/Platform-Android%20%7C%20iOS-blue)
![AR](https://img.shields.io/badge/AR-ARCore%20%7C%20ARKit-green)
![Multiplayer](https://img.shields.io/badge/Multiplayer-Photon-red)

**War of TAROT** is an innovative multiplayer augmented reality card game that brings physical tarot cards to life in immersive AR battles. Players scan their physical tarot cards to summon powerful characters into the AR battlefield, then engage in real-time combat against each other using magical spells and melee attacks.

## 🎮 Overview

War of TAROT transforms traditional card gaming into an exciting AR experience. Using your mobile device's camera, scan physical tarot cards from your deck to spawn corresponding 3D characters in the augmented reality space. Once your warriors are summoned, battle against your opponent in a shared AR battlefield where strategy, timing, and skill determine the victor.

## ✨ Key Features

### 🃏 Card Recognition & Spawning
- **Physical Card Scanning**: Use your device camera to scan physical tarot cards
- **Vuforia Integration**: Advanced image recognition technology identifies and tracks cards
- **AR Character Spawning**: Scanned cards instantly spawn corresponding 3D character models in AR space
- **Card-to-Character Mapping**: Extensive library mapping tarot cards to unique warrior characters

### ⚔️ Combat System
- **Multiple Attack Types**:
  - **Hook Punch**: Quick melee attack (10% damage)
  - **Heavy Punch**: Powerful melee strike (30% damage)
  - **Magic Fireball**: Ranged magical projectile (50% damage)
- **Real-time Health System**: Visual health bars track character vitality
- **Character Deaths**: Defeated characters play death animations and are removed from battle
- **Hit Detection**: Precise collision detection for attacks and projectiles

### 🌐 Multiplayer Experience
- **Photon Networking**: Real-time multiplayer powered by Photon PUN
- **2-Player Battles**: Head-to-head combat between two players
- **Synchronized Gameplay**: All actions synchronized across both devices
- **Lobby System**: Player matchmaking and room management
- **Cloud Anchors**: Shared AR space using Google ARCore Extensions

### 🎯 AR Technology
- **ARCore & ARKit Support**: Cross-platform AR on Android and iOS
- **Plane Detection**: Automatic surface detection for character placement
- **Light Estimation**: Dynamic lighting adjustment for realistic character appearance
- **Environment Placement**: Position shared battlefields in physical space
- **AR Raycasting**: Precise interaction with AR objects

### 🎨 Character Features
- **Multiple Character Models**: Diverse roster of warriors with unique designs
- **Animated Characters**: Full animation sets including idle, walk, attack, and death
- **Character Movement**: AI-controlled or player-directed movement on battlefield
- **Character Selection**: Choose your deck before battle begins

## 🛠️ Technology Stack

### Game Engine
- **Unity 2022.3+**: Cross-platform game development
- **C#**: Primary programming language

### AR Frameworks
- **Unity XR Foundation**: Core AR functionality
- **ARCore Extensions**: Google's AR platform for Android
- **ARKit**: Apple's AR framework for iOS
- **Vuforia Engine**: Image tracking and recognition

### Networking
- **Photon PUN 2**: Real-time multiplayer networking
- **Photon Realtime**: Connection and room management

### Additional Technologies
- **TextMesh Pro**: Advanced text rendering
- **Unity Input System**: Modern input handling
- **LeanTouch**: Touch input processing
- **Git LFS**: Large file storage for assets

## 📋 Prerequisites

- **Unity**: Version 2022.3 or higher
- **Mobile Device**: 
  - Android 7.0+ with ARCore support
  - iOS 11.0+ with ARKit support
- **Photon Account**: Free account at [Photon Engine](https://www.photonengine.com/)
- **Git LFS**: For downloading large asset files

## 🚀 Installation

### 1. Clone the Repository

```bash
git clone https://github.com/Pratik77221/War-of-TAROT.git
cd War-of-TAROT
```

### 2. Download LFS Files

This project contains large `.tif` texture files stored using **Git LFS (Large File Storage)**. After cloning, download the actual files:

```bash
git lfs install  # Ensure Git LFS is installed
git lfs pull     # Download the actual .tif files
```

**Note**: The BattleField scene requires these LFS files to load properly.

### 3. Open in Unity

1. Open **Unity Hub**
2. Click **Open** or **Add**
3. Navigate to the cloned `War-of-TAROT` folder
4. Select the folder and open the project
5. Unity will import all assets (this may take several minutes)

### 4. Configure Photon

1. Get your Photon App ID from [Photon Dashboard](https://dashboard.photonengine.com/)
2. In Unity, go to `Window > Photon Unity Networking > PUN Wizard`
3. Enter your Photon App ID
4. Click **Setup Project**

### 5. Configure Vuforia

1. Get a Vuforia license key from [Vuforia Developer Portal](https://developer.vuforia.com/)
2. In Unity, go to `Window > Vuforia Configuration`
3. Add your license key
4. Upload your tarot card images as targets in the Vuforia portal
5. Download the target database and import it into Unity

## 🎮 How to Play

### Game Flow

1. **Launch the App**: Start the game on your mobile device
2. **Login**: Enter your player name
3. **Join Lobby**: Connect to Photon servers and join/create a room
4. **Wait for Opponent**: The game requires 2 players to start
5. **Card Selection**: Each player scans their physical tarot cards
   - Player 1 scans their 5 cards
   - Player 2 scans their 5 cards
6. **Place Battlefield**: The host places the AR battlefield in the physical environment
7. **Character Spawning**: Your scanned cards spawn as 3D characters in AR
8. **Battle**: Engage in real-time combat!
   - Tap to select your characters
   - Use attack buttons to punch or cast spells
   - Fireballs fly through the air toward enemies
   - Watch health bars deplete as damage is dealt
9. **Victory**: Defeat all enemy characters to win!

### Controls

- **Tap**: Select character or place environment
- **Attack Buttons**: Trigger different attack types
- **Character UI**: Monitor health and status
- **Camera Movement**: Move your device to view the AR battlefield from different angles

### Supported Cards

The game supports various tarot card characters. Check the card mapping in the Character Spawner to see which physical cards correspond to which game characters.

## 📁 Project Structure

```
War-of-TAROT/
├── Assets/
│   ├── Scripts/               # All C# game scripts
│   │   ├── ARGameManager.cs      # Main AR game loop
│   │   ├── CharacterSpawner.cs   # Card-to-character spawning
│   │   ├── CharacterHealth.cs    # Health system
│   │   ├── Fireball.cs           # Magic projectile
│   │   ├── LobbyManager.cs       # Multiplayer lobby
│   │   ├── GameManager.cs        # Game state management
│   │   └── ...
│   ├── Scenes/                # Unity scene files
│   │   ├── Lobby.unity           # Multiplayer lobby
│   │   ├── PlayerSelection.unity # Card scanning
│   │   ├── ARGame.unity          # Main AR game
│   │   ├── BattleField.unity     # Battle environment
│   │   └── ...
│   ├── Prefabs/               # Reusable game objects
│   ├── Models/                # 3D character models
│   ├── Animation/             # Character animations
│   ├── Materials/             # Textures and materials
│   ├── Audio/                 # Sound effects and music
│   ├── Photon/                # Photon networking assets
│   └── StreamingAssets/       # Vuforia databases
├── Packages/                  # Unity package dependencies
├── ProjectSettings/           # Unity project configuration
└── README.md                  # This file
```

## 🔧 Development

### Building for Mobile

#### Android
1. Go to `File > Build Settings`
2. Select **Android** platform
3. Click **Switch Platform**
4. Ensure ARCore is enabled in XR settings
5. Configure player settings:
   - Minimum API Level: Android 7.0 (API 24)
   - Target API Level: Latest
6. Click **Build** or **Build and Run**

#### iOS
1. Go to `File > Build Settings`
2. Select **iOS** platform
3. Click **Switch Platform**
4. Ensure ARKit is enabled in XR settings
5. Configure player settings:
   - Minimum iOS Version: 11.0
   - Target SDK: Latest
6. Click **Build**
7. Open the generated Xcode project
8. Configure signing and build in Xcode

### Key Scripts Overview

- **ARGameManager.cs**: Manages AR environment placement, cloud anchors, and character interaction
- **CharacterSpawner.cs**: Maps scanned cards to character prefabs and spawns them at designated points
- **CharacterHealth.cs**: Handles character health, damage, and death
- **CharacterMovementController.cs**: Controls character movement and attack animations
- **AttackHitDetector.cs**: Detects collision between attacks and characters
- **Fireball.cs**: Magic projectile behavior and collision
- **LobbyManager.cs**: Multiplayer lobby, room creation, and player matchmaking
- **PlayerSelectionManager.cs**: Card scanning and selection management
- **GameManager.cs**: Singleton that persists player data across scenes

## 🎯 Features in Development

- [ ] More character types and cards
- [ ] Additional spell types
- [ ] Power-ups and special abilities
- [ ] Ranked matchmaking
- [ ] Tournament mode
- [ ] Card collection system
- [ ] Character customization

## 🐛 Known Issues

- Large `.tif` files require Git LFS - ensure you download them before using the BattleField scene
- AR performance may vary based on device capabilities
- Cloud anchors require good lighting and textured surfaces

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Coding Standards

- Follow C# naming conventions
- Add comments for complex logic
- Test multiplayer features thoroughly
- Ensure AR functionality works on both Android and iOS

## 📄 License

This project is available for educational and non-commercial purposes. Please contact the repository owner for commercial licensing inquiries.

## 👥 Authors

- **Pratik77221** - [GitHub Profile](https://github.com/Pratik77221)

## 🙏 Acknowledgments

- **Unity Technologies** for the game engine and XR frameworks
- **Photon Engine** for real-time multiplayer networking
- **Vuforia** for image recognition technology
- **Google ARCore** and **Apple ARKit** for AR capabilities
- All asset creators and contributors

## 📞 Support

For issues, questions, or suggestions:
- Open an issue on [GitHub Issues](https://github.com/Pratik77221/War-of-TAROT/issues)
- Contact the repository owner


---

**Happy Battling! May the cards be in your favor!** 🃏⚔️✨

