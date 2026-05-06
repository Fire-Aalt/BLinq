# BLinq
Perform any query in a *blink* with this blazingly fast, fully Burst compatible Unity LINQ library. 
Blinq utilizes an ILPostProcessor to allow LINQ syntax in Bursted methods, making it as accessible as possible.

## TO-DO:
* Add fast paths with known Length

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
- [x] Any
- [x] All
- [x] Count
- [x] LongCount
- [x] Aggregate
- [x] ElementAt
- [x] ElementAtOrDefault
- [x] Last
- [x] LastOrDefault
- [x] Single
- [x] SingleOrDefault
- [x] DefaultIfEmpty
- [ ] Cast                // Impossible in Unmanaged C#
- [ ] OfType              // Impossible in Unmanaged C#
- [x] Distinct
- [x] DistinctBy
- [x] Union
- [x] UnionBy
- [x] Intersect
- [x] IntersectBy 
- [x] Except
- [x] ExceptBy
- [x] Concat 
- [x] Append
- [x] Prepend
- [x] Skip
- [x] SkipWhile
- [x] Take
- [x] TakeWhile
- [x] TakeLast
- [x] Reverse
- [x] Join
- [x] GroupJoin
- [ ] MinBy
- [ ] MaxBy

## Missing Special operators/sequences:
- [ ] InfiniteSequence
- [ ] Shuffle

## TO-DO when C# in Unity will get more language features:
* CORE CLR: Revise Mono.Cecil dependency.
* CORE CLR: Static Abstract Interface for `IAggregatable`. Remove the source generator for the current `IAggregatable` and implement the new interface.
* FUTURE C#: Extend types with addition/devision operators so that extensions can also participate in contractual sites without owning the original type.
