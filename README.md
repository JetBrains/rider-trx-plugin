<p align="center">
<img src="https://github.com/artem3605/TrxPlugin/blob/main/src/rider/main/resources/META-INF/pluginIcon.svg" alt="drawing" width="200"/>
</p>

# TRX Support Plugin for JetBrains Rider

This plugin provides support for TRX test result files in JetBrains Rider. It allows to import TRX files as unit test sessions, making it easier to work with test results directly within the IDE.

![image_2024-08-26_18-19-03](https://github.com/user-attachments/assets/bb7d4ec6-31e3-48ac-9024-fd2a29ad3c61)

## Features

- **Import TRX Files**: Import TRX files directly from the file editor or project view context menu.
- **Unit Test Session Creation**: Automatically creates a unit test session based on the TRX file.
- **Tree View for Test Results**: View test results in a tree structure for better organization and clarity.
- **Default Grouping by Namespaces**: The plugin sets default grouping by namespaces, which can be adjusted in the unit test sessions tool window.
- **Support for Output and Errors**: Displays standard output, error information, and error stack traces.
- **Test Duration**: Includes support for displaying test duration.

## Installation

You can install the TRX Support Plugin directly from the JetBrains Plugin Repository or via the plugins section inside your JetBrains Rider IDE.

1. Open your IDE.
2. Navigate to `File | Settings | Plugins` (or `JetBrains Rider | Preferences | Plugins` on macOS).
3. Search for "TRX Support" and install the plugin.

After installation, the plugin will be available for use without requiring any additional configuration.

## Usage

To use the TRX Support Plugin, there are 4 options:

1. **Context Menu**:
   - Right-click on a TRX file in the file editor or project view.
   - Select `Import TRX as Unit Test Session` from the context menu.
   - The test results will be displayed in a newly created unit test session.

2. **Editor Banner**:
   - When you open a TRX file in the editor, a banner will appear at the top of the editor.
   - Click the `Import` button in the banner.
   - This will also create a new unit test session and display the test results.

3. **Main Menu Tests Tab**:
   - Navigate to the Tests tab in the main menu.
   - Select `Import TRX...`
   - Choose the TRX file you wish to import.
   - A new unit test session will be created, and the test results will be displayed.

4. **Drag and Drop**:
   - Drag a TRX file from your file explorer and drop it directly into the editor.
   - This will create a new unit test session without opening the file.

Documentation
-------------
- [Changelog][docs.changelog]
- [License][docs.license] (Apache-2.0)

Acknowledgements
----------------
We are very grateful to the initial developers of the plugin:
- [Artem Abaturov](https://github.com/artem3605)
- [Nikita Magomedeminov](https://github.com/Kreativshikkk)

[docs.changelog]: CHANGELOG.md
[docs.license]: LICENSE.txt
