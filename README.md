# Dota 2 Hero Layout Generator

Generates a usable in-game hero layout for Dota 2. Currently at a very early stage in development.
### Features
- config.json - Editing variables
    - Lane Names (Translation Purpose)
    - Thresholds (percentige which under the hero isn't going to be included in the list)
    - Url for data (if Dotabuff changes the url you can manually keep up with it)
    - Order of lanes are interchangable

### How does it work
0. Reads the config.json file
1. Pings the specified URLs every 5 seconds and stores them as strings.
2. Resizes the strings so only the hero data tables are stored.
3. Using Regex, removes everything that isn't a hero's name, pickrate or winrate.
4. Makes a list from the remaining stuff
5. Generates the layout and saves it in the current directory as _hero_grid_config.json_
    1. Goes through the list.
    2. If the (hero's pickrate >= lane's threshold) is true, includes him/her in the lane's collection.
    3. Writes everything into _hero_grid_config.json_
