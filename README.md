# EDSM Scanner

A project containing utility tools for identifying not fully discovered star systems in [Elite Dangerous Star Map](https://www.edsm.net/), in order to help completing the database for the community.

To learn the story behind creaiton of these tools, please feel free to visit the [Wiki page](https://github.com/Suremaker/edsm_scanner/wiki).

# Usage

To obtain the tools, please navigate to [Releases](https://github.com/Suremaker/edsm_scanner/releases/) and download the **binaries.zip** file (for Windows machines)

## EdsmScanner

The EdsmScanner.exe allows to scan for the nearby systems around specified system name using [GET https://www.edsm.net/api-v1/sphere-systems](https://www.edsm.net/en/api-v1) and [GET https://www.edsm.net/api-system-v1/bodies](https://www.edsm.net/en/api-system-v1) EDSM API calls.

Example usage: `> EdsmScanner.exe "Synuefe JM-G b57-1"`

The tool will search for systems around `Synuefe JM-G b57-1` in radius of 50ly (the radius can be specified as second parameter, up to 100).

The outcome of the command will be 2 files:
* `partial_Synuefe JM-G b57-1.txt` - file containing a list of partially discovered systems (reported in EDSM).
* `discovered_Synuefe JM-G b57-1.txt` - file containg a list of id64 identifiers of the fully discovered systems - see VisitedStarCacheMerger description for more details.

The system is qualified as partially discovered, if:
* total bodies count is unknown (`?`),
* total bodies count is greater than discovered bodies count.

The system is qualified as fully discovered if total bodies count is greater than 0 and equal total discovered bodies count.

### Working with partially discovered systems
The `partial_[system name].txt` file contains lines like:
```
Synuefe KM-G b57-3 [8.26ly] (30 bodies) (21 discovered) => https://www.edsm.net/en/system/bodies/id/235051/name/Synuefe+KM-G+b57-3
Synuefe MH-G b57-1 [8.45ly] (? bodies) (0 discovered) => https://www.edsm.net/en/system/bodies/id/20950446/name/Synuefe+MH-G+b57-1
```
... where each line contains the:
* system name, 
* distance to specified origin system,
* list of total bodies in the system, or ? if in case the number is not known
* list of discovered bodies so far
* url to system description in edsm.net


## VisitedStarCacheMerger

The VisitedStarCacheMerger.exe allows to update the local player cache of visited systems with list of fully discovered systems from EdsmScanner.exe

Example usage: `> VisitedStarCacheMerger.exe VisitedStarsCache.dat "discovered_Synuefe JM-G b57-1.txt"`

The `VisitedStarsCache.dat` can be found in `c:\Users\[user_name]\AppData\Local\Frontier Developments\Elite Dangerous\[elite_user_id]\` directory.

After the merge and restart of the game, the fully discovered systems will be displayed as visited star systems on the route planner.

Please note that `VisitedStarsCache.dat` is limited to hold up to 3876 system entries. The `VisitedStarCacheMerger` works the way that most recently added systems will override older ones.

*The merger application has been written based on [this blog post](https://forums.frontier.co.uk/threads/visited-stars-galaxy-map-visitedstarscache-dat-playing-on-multiple-pc.509263/#post-7750676)*
