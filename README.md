# Local Game Modes
 
A [ROUNDS](https://landfall.se/rounds) mod to extend the game's multiplayer capabilities, built for [BepInEx](https://github.com/BepInEx/BepInEx).

---

# Current Game Modes

The mod supports up to four players in local and private online lobbies.

**Team Deathmatch / 2v2**

The game mode you are used to, except now it supports two additional players. 

Playing in a 2v1 setting is also possible, although there are currently no special rules or handicaps for uneven team sizes.

**Deathmatch / Free for all**

Deathmatch follows the heart of the original team deathmatch game mode, but now it's just you against everyone else.


## Game Enhancements

**Continue or rematch**

In game support, after a team is awarded victory the usual menu that takes you back to the main menu provides new options: 

Continue
 : Which allow the game proceed and 2 additional rounds added to win. 

Reset 
 : Which keeps the same players but resets the card counts.

**Larger Maps**

New maps have been added to support up to 8 players. 

**Competitive Configurations**

- Pick Phase Timer
- Maximum Number of Cards
- Discard Phase
- Pre-Game Card Picks
- Pre-Game Ban Picks
- Win by XXX Points
- Win by XXX Rounds

---


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


---




## Dependencies

- [BepInEx](https://docs.bepinex.dev/master/articles/index.html) : By BipInEx - plugin framework
- [UnboundLib](https://github.com/Rounds-Modding/UnboundLib) : By willis81808 - version **>=2.9.0**
- [MMHook](https://rounds.thunderstore.io/package/willis81808/MMHook/1.0.0/) : By willis81808 - version **>=1.0.0**
- [ModdingUtils](https://rounds.thunderstore.io/package/Pykess/ModdingUtils/0.2.5/) : By Pykess - version **>=0.2.5**
- [ToggleEffectsMod](https://rounds.thunderstore.io/package/CrazyMan/ToggleEffectsMod/1.1.3/) : By CrazyMan - version **>=1.1.3**
- [CardChoiceSpawnUniqueCardPatch](https://rounds.thunderstore.io/package/Pykess/CardChoiceSpawnUniqueCardPatch/0.1.7/) : by Pykess - version **>=0.1.7**




## Manual Installation

#### 0. Download [ThunderStore Mod Manager](https://www.overwolf.com/app/Thunderstore-Thunderstore_Mod_Manager)

This took allows for easy management of downloaded mods between applications. 

#### 1. Create Modded Profile

We suggest creating a new mod profile for this game mode. 

1. Open *ThunderStore Mod Manager*
2. Select the Game **ROUNDS**
3. On the select profile screen, click **Create new**
4. Give it a name, ie: **LocalGameModes**
5. Make sure the new profile is highlighted in blue and press **Select profile**

#### 2. Add Local Mod

1. In the menu 

	**Other** > *Settings* > *Locations* > *Browse profile folder*

2. In the **File Explorer** navigate to 

	.../BepInEx/plugins

3. Copy the zip project into this location.

4. Back to *ThunderStore Mod Manager*: 

	**Other** > *Settings* > *Profile* > *Import local mod* > *Select File*

5. A new file explorer will open, select the `.zip` folder that was copied. 

6. *Import local mode*, the pop-up dialog, Verify the information was imported correctly.

#### 3. Install the Dependencies

1. Navigate to the Installed Mods

	**Mods** > *Installed*

2. Select & Expand the newly added **Zeeke-LocalGameModes**. 
3. The last button within the expanded mod should be: **Download dependency**
4. Click it until it no longer appears.

#### 4. Start Modded

You'll have to run the game with the mods active.

**ROUNDS** > *Start modded*