# CS2-HidePlayers
Allows you to hide player models.

## Requirments
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp/)

## Installation
- Download the newest release from [Releases](https://github.com/qstage/CS2-HidePlayers/releases)
- Move the /gamedata folder to a folder /counterstrikesharp
- Make a folder in /plugins named /HidePlayers.
- Put the plugin files in to the new folder.
- Restart your server.

## Configuration
`css_hide_reload` - Reload configuration
```json
{
    "cmds": "css_hidemodels;css_hide",
    "who_hidden": "@all", // @all / @team / @enemy
    "ConfigVersion": 3
}
```
