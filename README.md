# MoonTools.Core.Graph

[![NuGet Badge](https://buildstats.info/nuget/MoonTools.Core.Graph)](https://www.nuget.org/packages/MoonTools.Core.Graph/)
[![CircleCI](https://circleci.com/gh/MoonsideGames/MoonTools.Core.Graph.svg?style=svg)](https://circleci.com/gh/MoonsideGames/MoonTools.Core.Graph)

A GC-friendly graph theory library for C# intended for use with games.

## Usage

`Graph` implements various algorithms on the following graph structures:

* Directed
* Directed Weighted
* Directed Weighted Multigraph
* Undirected
* Undirected Weighted

## Notes

`Graph` algorithms return lazy enumerators to avoid creating GC pressure. If you wish to hang on to the results of an evaluation, make sure to call `ToArray()` or `ToList()` on the IEnumerable.

### TODO

* change Neighbors tests to use Equal instead of Contains
* change Edge id from a Guid to an integer index on the edge
* Prim Minimum Spanning Tree
* Kruskal Minimum Spanning Tree
* Undirected Weighted Multigraph