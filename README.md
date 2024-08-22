# TRX Support Plugin for JetBrains Rider

This plugin provides support for TRX test result files in JetBrains Rider. It allows to import TRX files as unit test sessions, making it easier to work with test results directly within the IDE.

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

To use the TRX Support Plugin, there are 2 options:

1. **Context Menu**:
   - Right-click on a TRX file in the file editor or project view.
   - Select `Import TRX as Unit Test Session` from the context menu.
   - The test results will be displayed in a newly created unit test session.

2. **Editor Banner**:
   - When you open a TRX file in the editor, a banner will appear at the top of the editor.
   - Click the `Import` button in the banner.
   - This will also create a new unit test session and display the test results.

Documentation
-------------
- [Contributor Guide][docs.contributing]

[docs.contributing]: CONTRIBUTING.md
