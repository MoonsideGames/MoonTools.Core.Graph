# MoonTools.Core.Graph

A GC-friendly graph theory library for C# intended for use with games.

## Usage

`Graph` implements the following graph structures:

* Directed
* Directed Weighted
* Directed Weighted Multigraph
* Undirected

## Notes

`Graph` algorithms return lazy enumerators to avoid creating GC pressure. If you wish to hang on to the results of an evaluation, make sure to call `ToArray()` or `ToList()` on the IEnumerable.