# GitHub Copilot Instruction Guide for RimWorld Mod: Colonist Bar Enhancement

## Mod Overview and Purpose
The Colonist Bar Enhancement mod is designed for RimWorld, a popular colony management simulation game. This mod aims to enhance and expand the functionality of the default colonist bar, providing players with more customization and improved usability. It includes various features that allow players to interact with and manage their colonists more effectively.

## Key Features and Systems
- **FloatMenu Enhancements**: Custom float menus such as `FloatMenuLabels`, `FloatMenuNested`, and `FloatMenuOptionNoClose` to improve user interaction.
  
- **Tools and Utilities**: A collection of static utilities in the `Tools` class to aid various functionalities within the mod.

- **Colonist Bar Customization**: Classes like `ColBarHelper_KF`, `ColonistBar_KF`, and `ColonistBarColonistDrawer_KF` provide enhanced drawing and handling of the colonist bar, including custom drawing, click handling, and group management.

- **Graphical Enhancements**: Custom texture handling through the `Textures` class for enhanced graphical assets.

- **Game Component Extensions**: `FollowMe` and `ZoomToMouse` game components introduce camera and view enhancements for better gameplay experience.

- **Icon Handlers**: Structs like `IconEntryBar` and `IconEntryPSI` for managing icon entries, particularly in mod-specific scenarios.

- **Settings Management**: Classes `Settings`, `SettingsColonistBar`, and `SettingsPSI` manage user settings, providing customization options for the mod features.

## Coding Patterns and Conventions
- **Class Names**: Classes are named using PascalCase with clear, descriptive names indicating their responsibility, such as `ColonistBarDrawLocsFinder_KF`.

- **Method Naming**: Methods follow PascalCase, with names clearly describing their purpose, e.g., `DrawColonist`, `TryGetEntryAt`.

- **Access Modifiers**: All classes and methods are explicitly marked with appropriate access modifiers like `public`, `internal`, or `private`.

- **Interfaces**: Implements interfaces where applicable, such as `IExposable` in `ColBarHelper_KF` to handle data exposure.

## XML Integration
While there is no detailed XML summary provided, you may need to interact with XML files for RimWorld-specific definitions, such as `Defs` for items, traits, and other game elements. Ensure to define and structure XML data that complements your C# logic for the mod's functionalities.

## Harmony Patching
- The mod uses Harmony for patching game methods, as evident in the `HarmonyPatches` class. Harmony is indispensable for modifying base game behavior without directly altering the game files.
  
- **Patch Structure**: Organize patches logically, targeting specific methods that need alterations or enhancements. Use prefixes `[HarmonyPatch]`, `[HarmonyPrefix]`, `[HarmonyPostfix]`, etc., as appropriate.

## Suggestions for Copilot
- **Code Generation**: Use GitHub Copilot to accelerate the development process by generating repetitive code structures like getter/setter methods and boilerplate Harmony patch code.

- **Error Handling**: Prompt Copilot to suggest potential exception handling for methods that interact with game data to improve mod stability.

- **Optimization Suggestions**: Leverage Copilot's suggestions for optimizing existing code, especially methods involved in frequent calculations or UI rendering.

- **Pattern Enforcement**: Encourage Copilot to adhere to existing coding patterns and conventions to maintain a consistent coding style throughout the mod.

By following these structured Copilot instructions, you can effectively develop, maintain, and enhance mods for RimWorld, providing a rich, user-friendly experience for players.
