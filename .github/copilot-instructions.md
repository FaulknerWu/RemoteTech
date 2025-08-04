# GitHub Copilot Instructions for Remote Tech Mod (Continued)

## Overview

**Mod Name**: Remote Tech (Continued)  
**Purpose**:  
Remote Tech is a content-rich mod designed to enhance the strategic capabilities of players in RimWorld by introducing remotely detonated explosives. This mod facilitates more effective base defense strategies, efficient mining operations, and many other advanced in-game activities. It offers a wide variety of explosives ranging from low-tech, makeshift devices beneficial for early colonies to advanced technologies suited for late-game scenarios.

## Key Features and Systems

- **Explosive Crafting and Deployment**: Players begin with low-tech explosive options crafted using basic materials. As the game progresses, more sophisticated technologies become accessible, including chemical charges and automated defenses.
  
- **Detonation Mechanics**: Initial explosives are manually detonated using a detonator wire and a manual detonator. With further research, remote detonation via radio signal becomes possible, allowing for upgraded functionality such as detonation channels for organized explosive deployment.
  
- **Research Tree**: The mod expands the tech tree, offering a progression path for unlocking and refining explosive technologies.
  
- **Compatibility**: The mod is designed to be safely added to existing game saves. It also requires the HugsLib library mod to function correctly.

## Coding Patterns and Conventions

- **C# Standards**: The mod code adheres to standard C# conventions including PascalCase for class names and camelCase for method names and local variables.

- **Classes and Methods**: The codebase is extensively class-based, with specific functionality encapsulated in well-named classes to facilitate maintenance and expansion.

- **Interface Usage**: Interfaces like `IPawnDetonateable`, `IRedButtonFeverTarget`, and `ISwitchable` ensure consistent implementation across various game features.

## XML Integration

- **Integration with Game XML**: The mod integrates C# code with RimWorld's XML definitions for game assets, allowing for extensible and modular feature additions.

- **DefModExtensions**: Utilizes `DefModExtension` to inject additional behavior into existing game definitions via XML.

## Harmony Patching

- **Patching for Game Integration**: The mod employs Harmony patches to modify base game behavior without altering the original code. This is crucial for compatibility and maintaining mod stability across game updates.

- **Specific Patches**: Files like `CultivatedPlants_DeterminePlantDef_Patch` and `ThingDef_ConnectToPower_Patch` illustrate targeted use of patches to modify specific game mechanics related to plant growth and power connectivity respectively.

## Suggestions for Copilot

1. **Contextual Code Snippets**: Incorporate code snippets for common tasks such as defining new `JobDriver` classes, integrating new research nodes in XML, or creating new Harmony patches.

2. **Example Implementations**: Offer examples for implementing new detonator types using existing class hierarchies, or adding new explosive effects through `ThingComp`.

3. **Refactoring Help**: Assist in refactoring existing methods for enhanced performance or readability, and suggest more efficient algorithms where applicable.

4. **XML Templates**: Provide templates for XML definitions and `DefModExtension` usage, facilitating easy addition of new functionalities.

5. **Harmony Patch Suggestions**: Recommend patch methods to address potential compatibility concerns with future RimWorld updates, ensuring continued mod functionality.

By providing these instructions and utilizing the capabilities of GitHub Copilot, developers working on the Remote Tech mod can effectively streamline their workflow, maintain consistency, and incorporate best practices in their codebase.
