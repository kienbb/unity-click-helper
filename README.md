# Click Helper

[![GitHub Repository](https://img.shields.io/badge/GitHub-Repository-blue?logo=github)](https://github.com/kienbb/unity-click-helper)

A powerful Unity Editor tool designed to simplify selecting overlapping objects in the Scene View. No more struggling to click the right object when multiple GameObjects or UI elements are stacked on top of each other.

![Click Helper](https://via.placeholder.com/800x400?text=Click+Helper+Preview)

## Features

- **Smart Selection Popup**: Displays a list of all objects under your mouse cursor.
- **Precision Highlighting**: Hover over items in the list to visually highlight them in the Scene View.
- **Component Drill-down**: Inspect and select specific components attached to a GameObject directly from the popup.
- **Configurable Activation**: Choose your preferred shortcut (Shift+Right Click, Ctrl+Right Click, etc.).
- **Scene View Integration**: Includes a convenient Overlay toolbar for quick toggling and settings access.
- **Layer Filtering**: Exclude specific layers from being picked to focus only on what matters.
- **UI Support**: Works seamlessly with both 3D objects and 2D UI elements (Canvas, RectTransform).

## Installation

### Via Unity Package Manager (UPM)

1. Open your Unity project.
2. Go to **Window** > **Package Manager**.
3. Click the **+** (plus) button in the top-left corner.
4. Select **Add package from git URL...**.
5. Enter the following URL:
   ```
   https://github.com/kienbb/unity-click-helper.git
   ```
6. Click **Add**.

### Via manifest.json

Alternatively, you can add it directly to your project's `Packages/manifest.json` file:

```json
{
  "dependencies": {
    "com.kienbb.click-helper": "https://github.com/kienbb/unity-click-helper.git",
    ...
  }
}
```

## Usage

1. **Activate**: In the Scene View, hold **Shift** and **Right Click** (default) on any object.
2. **Navigate**: A popup window will appear listing all objects under your cursor.
   - **Hover** over an item to highlight it in the scene.
   - **Click** an item to select it.
   - **Arrow Keys** to navigate the list.
   - **Enter** to confirm selection.
3. **Components**: If enabled, expanding an item will show its attached components, allowing you to select specific components.

## Settings & Overlay

The tool adds a **Click Helper** overlay to your Scene View toolbar.

- **Toggle**: Click the "Click Helper" button to quickly enable/disable the tool.
- **Settings (\u25BC)**: Click the small arrow next to the button to access:
  - **Activation**: Change the shortcut (e.g., Right Click, Middle Click).
  - **Selection Behavior**: Choose between "Select Only" or "Select and Focus".
  - **Show Components**: Toggle component display in the popup.
  - **Excluded Layers**: Filter out layers you don't want to select (e.g., "Ignore Raycast").

## Requirements

- Unity 2021.3 or later.

## License

This project is licensed under the MIT License.
