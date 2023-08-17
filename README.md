# EDSM Scanner

A project containing utility tools for identifying not fully discovered star systems in [Elite Dangerous Star Map](https://www.edsm.net/), in order to help completing the database for the community.

To learn the story behind creaiton of these tools, please feel free to visit the [Wiki page](https://github.com/Suremaker/edsm_scanner/wiki).

# Usage

To obtain the tools, please navigate to [Releases](https://github.com/Suremaker/edsm_scanner/releases/) and download the **binaries.zip** file (for Windows machines)

## EdsmScanner

The EdsmScanner.exe allows to scan for the nearby systems around specified system name using [GET https://www.edsm.net/api-v1/sphere-systems](https://www.edsm.net/en/api-v1) and [GET https://www.edsm.net/api-system-v1/bodies](https://www.edsm.net/en/api-system-v1) EDSM API calls.

Example usage: `> EdsmScanner.exe "Synuefe JM-G b57-1" -fs "IsFullyDiscovered==false"`

The tool will search for systems around `Synuefe JM-G b57-1` in radius of 50ly (the radius can be specified with `--scan-radius` option, up to 100).

The outcome of the command will be 2 files:
* `systems_Synuefe JM-G b57-1.txt` - file containing a list of all systems reported in EDSM which are not fully discovered.
* `visited_Synuefe JM-G b57-1.txt` - file containg a list of id64 identifiers of the fully discovered systems - see VisitedStarCacheMerger description for more details.

The system is qualified as partially discovered, if:
* total bodies count is unknown (`?`),
* total bodies count is greater than discovered bodies count.

The system is qualified as fully discovered if total bodies count is greater than 0 and equal total discovered bodies count.

### Working with partially discovered systems
The `systems_[system name].txt` file contains lines like:
```
# distances calculated to origin system: Synuefe JM-G b57-1
Synuefe KM-G b57-3 [8.26ly] (30 bodies / 21 discovered) => https://www.edsm.net/en/system/bodies/id/235051/name/Synuefe+KM-G+b57-3
Synuefe MH-G b57-1 [8.45ly] (? bodies / 0 discovered) => https://www.edsm.net/en/system/bodies/id/20950446/name/Synuefe+MH-G+b57-1
Synuefe IA-C d14-75 [12.02ly] (? bodies / 1 discovered) => https://www.edsm.net/en/system/bodies/id/14713801/name/Synuefe+IA-C+d14-75
```
... where each line contains the:
* system name, 
* distance to specified origin system,
* list of total bodies in the system, or ? if in case the number is not known
* list of discovered bodies so far
* url to system description in edsm.net

## Journey plotting

The EdsmScanner.exe allows to plot the journey through the partially discovered systems to make it easier to navigate through them.

To use the journey plotting, the tool has to be run with `--plot-journey` option: `> EdsmScanner.exe "Synuefe JM-G b57-1" --plot-journey`.

When executed, the `systems_[system name].txt` file will contain the systems in order allowing easier traversal between them and distance parameter referring to the previous system. The header of the file will contain that information:

```
# distances calculated to previous system, starting from: Synuefe JM-G b57-1
Synuefe KM-G b57-3 [8.26ly] (30 bodies / 21 discovered) => https://www.edsm.net/en/system/bodies/id/235051/name/Synuefe+KM-G+b57-3
Synuefe MH-G b57-1 [4.53ly] (? bodies / 0 discovered) => https://www.edsm.net/en/system/bodies/id/20950446/name/Synuefe+MH-G+b57-1
Synuefe MH-G b57-0 [5.16ly] (8 bodies / 1 discovered) => https://www.edsm.net/en/system/bodies/id/235049/name/Synuefe+MH-G+b57-0
```

## Including bodies

The EdsmScanner.exe allows to include the bodies in the search results too with option `--include-bodies`: `> EdsmScanner.exe "Synuefe JM-G b57-1" --include-bodies`

When executed, the `systems_[system name].txt` file will contain the systems list with all matching bodies and their attributes.

## Filtering

It is possible now to filter the results by applying the `--filter-system` and `--filter-body` options.

For example usages, please run: `> EdsmScanner.exe help usages`.

For details on attributes that can be used during filtering, please run: `> EdsmScanner.exe help filters`.

## Caveats

Please note that EdsmScanner uses https://www.edsm.net/api-system-v1/bodies endpoint to query system details of each found system. This endpoint is throttled, allowing to retrieve roughly 700 system information per minute.  
When the limit is reached, the EdsmScanner will pause and wait until it would be able to resume queries. It may be perceived as EdsmScanner is stalling, but unfortunately, it is a limitation that cannot be overcome by EdsmScanner.

## VisitedStarCacheMerger

The VisitedStarCacheMerger.exe allows to update the local player cache of visited systems with:
* list of fully discovered systems from EdsmScanner.exe (id64 identifiers) file with `.txt` extension,
* list of system names (like from `importstars.txt`) file with `.txt` extension,
* another cache file with `.dat` extension.

Example usage: `> VisitedStarCacheMerger.exe VisitedStarsCache.dat "visited_Synuefe JM-G b57-1.txt"`  
Example usage: `> VisitedStarCacheMerger.exe VisitedStarsCache.dat OtherVisitedStarsCache.dat`

The `VisitedStarsCache.dat` can be found in `c:\Users\[user_name]\AppData\Local\Frontier Developments\Elite Dangerous\[elite_user_id]\` directory.

After the merge and restart of the game, the fully discovered systems will be displayed as visited star systems on the route planner.

Please note that `VisitedStarsCache.dat` is limited to hold up to 3876 system entries. The `VisitedStarCacheMerger` works the way that most recently added systems will override older ones.

*The merger application has been written based on [this blog post](https://forums.frontier.co.uk/threads/visited-stars-galaxy-map-visitedstarscache-dat-playing-on-multiple-pc.509263/#post-7750676)*
