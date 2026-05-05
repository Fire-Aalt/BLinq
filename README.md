# BLinq
Blazingly fast, fully Burst compatible Unity LINQ library. Utilizes ILPostProcessors to allow LINQ syntax in Bursted methods.

## TO-DO:
* Materialization should be streamlined.
* Add documentation to all methods.
* Optimize ILPP.
* Convert source generators to use T4 text.
* Create a SKILL.md to how to create a "lift" a feature

## Missing Core Operators:
- [ ] Any
- [ ] All
- [ ] Count
- [ ] LongCount
- [ ] Aggregate
- [ ] ElementAt
- [ ] ElementAtOrDefault
- [ ] Last
- [ ] LastOrDefault
- [ ] Single
- [ ] SingleOrDefault
- [ ] DefaultIfEmpty
- [ ] Cast
- [ ] OfType
- [ ] Distinct
- [ ] DistinctBy
- [ ] Union
- [ ] UnionBy
- [ ] Intersect
- [ ] IntersectBy 
- [ ] Except
- [ ] ExceptBy
- [ ] Concat 
- [ ] Append
- [ ] Prepend
- [ ] Skip
- [ ] SkipWhile
- [ ] Take
- [ ] TakeWhile
- [ ] TakeLast
- [ ] Reverse
- [ ] Join
- [ ] GroupJoin
- [ ] MinBy
- [ ] MaxBy

## Missing Overloads:
- [ ] Select/Where/SelectMany indexed and result-selector overloads
- [ ] OrderBy/ThenBy key-selector overloads
- [ ] GroupBy/ToLookup overloads with element selectors, result selectors, and comparers
- [ ] Contains comparer overloads

## Missing Materializers:
- [ ] ToManagedDictionary
- [ ] ToManagedHashSet
- [ ] ToNativeHashMap
- [ ] ToNativeHashSet

## Missing Special operators/sequences:
- [ ] InfiniteSequence
- [ ] Shuffle

## TO-DO when C# in Unity will get more language features:
* CORE CLR: Revise Mono.Cecil dependency.
* CORE CLR: Static Abstract Interface for `IAggregatable`. Remove the source generator for the current `IAggregatable` and implement the new interface.
* FUTURE C#: Extend types with addition/devision operators so that extensions can also participate in contractual sites without owning the original type.