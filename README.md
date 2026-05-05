# BLinq
Perform any query in a *blink* with this blazingly fast, fully Burst compatible Unity LINQ library. 
Blinq utilizes an ILPostProcessor to allow LINQ syntax in Bursted methods, making it as accessible as possible.

## TO-DO:
* Convert source generators to use T4 text.
* Add documentation to all methods. (only generated Sum/Average remain).
* Materialization should be streamlined.
* Optimize ILPP.
* Create a SKILL.md to describe how to create a "lift" a feature.

## Missing Core Operators:
- [x] Select
- [x] SelectMany
- [x] Where
- [x] AggregateBy
- [x] Contains
- [x] First
- [x] FirstOrDefault
- [x] GroupBy
- [x] Min
- [x] Max
- [x] OrderBy
- [x] ThenBy
- [x] Sum
- [x] Average
- [x] SequenceEqual
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
- [ ] Cast                // Impossible in Unmanaged C#
- [ ] OfType              // Impossible in Unmanaged C#
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