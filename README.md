# Local Game Modes
 
A [ROUNDS](https://landfall.se/rounds) mod to extend the game's multiplayer capabilities, built for [BepInEx](https://github.com/BepInEx/BepInEx).

# Current Features

The mod supports up to four players in local and private online lobbies.

## Team Deathmatch / 2v2

The game mode you are used to, except now it supports two additional players. Playing in a 2v1 setting is also possible, although there are currently no special rules or handicaps for uneven team sizes.

## Deathmatch / Free for all

Deathmatch follows the heart of the original team deathmatch game mode, but now it's just you against everyone else.

## Continue or rematch

After the game ends, you can choose to play a little longer, or to start a new game.






## In Development

### Double Up

Free for all game mode type, but each card selection round, allows the user to select 2 cards from their hands. 

*Resolve*
- [ ] Toggle Game Types no longer loops
- [ ] Selecting a card triggers re-render and selecting the second card is from a new list entirely
- [ ] On second select, user is no longer visible

### Juggernaut

Free for all or Teams, no limit to those that can join. 

*Resolve*
- [ ] Store each team, with members and score. 
- [ ] Score calculated: =Team1Score +/- 40 * (1 - 1/(1 + 10^((Team1Score - Team2Score)/400)))
	- +/- depending on Win (+) or Lose (-)
	- Score is in favor of the weaker team. 
		*If the weaker team Wins, they gain more points and the stronger team loses more.* 
		*If the Stronger team wins, they won't gain as many and the loser won't lose as many.*
- [ ] Losers choose a card. 
- [ ] Winers play next round.







## Dependencies

- [BepInEx](https://docs.bepinex.dev/master/articles/index.html) plugin framework
- [UnboundLib](https://github.com/Rounds-Modding/UnboundLib) version **>=2.1.3**


## Manual Installation

#### 0. Locate your ROUNDS install directory.

This directory is usually located at `C:\Program Files (x86)\Steam\steamapps\common\ROUNDS` or similar.

#### 1. Install **BepInEx**.

See [BepInEx installation](https://docs.bepinex.dev/master/articles/user_guide/installation/index.html) for details.

You can download the latest release directly from [BepInEx/releases](https://github.com/BepInEx/BepInEx/releases). Choose the correct version for your platform and extract the downloaded zip file to your ROUNDS installation directory in its entirety.

**Run the game once to generate BepInEx folder structure and configuration files.**

#### 2. Download **UnboundLib**.

Download the [latest release](https://github.com/Rounds-Modding/UnboundLib/releases/latest) of **[UnboundLib](https://github.com/Rounds-Modding/UnboundLib)** and extract the downloaded dll files to `ROUNDS/BepInEx/plugins`.
