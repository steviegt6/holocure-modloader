using System;
using System.Collections.Generic;
using System.Linq;
using HoloCure.ModLoader.API.Exceptions;
using NUnit.Framework;

namespace HoloCure.ModLoader.API.Tests
{
    // https://github.com/tModLoader/tModLoader/blob/1.4/test/SortingTests.cs
    // MIT License. TODO: License header.
    public static class SortingTests
    {
	    private static ModMetadata Make(
		    string name,
		    string? version = null,
		    IEnumerable<string>? refs = null,
		    IEnumerable<string>? sortAfter = null,
		    IEnumerable<string>? sortBefore = null
	    ) {
		    return new ModMetadata
		    {
			    UniqueName = name,
			    Version = version ?? "1.0.0.0",
			    Dependencies = refs?.ToList() ?? new List<string>(),
			    SortAfter = sortAfter?.ToList() ?? new List<string>(),
			    SortBefore = sortBefore?.ToList() ?? new List<string>()
		    };
	    }

	    private static void AssertSetsEqual<T>(ICollection<T> set1, ICollection<T> set2) {
			HashSet<T> set = new(set1);
			Assert.That(set1, Has.Count.EqualTo(set2.Count));
			foreach (T e in set2)
				Assert.That(set, Does.Contain(e), "Missing Element: " + set2);
		}

		private static void AssertModException(Action func, string[] mods, string msg) {
			try {
				func();
				Assert.Fail("Test method did not throw expected exception ModOrganizationException.");
			}
			catch (ModOrganizationException e) {
				AssertSetsEqual(e.Errored.Select(m => m.UniqueName).ToList(), mods);
				Assert.That(msg, Is.EqualTo(e.Message.Trim()));
			}
		}

		private static void Swap<T>(ref T a, ref T b) {
			(a, b) = (b, a);
		}

		private static void Reverse<T>(T[] arr, int pos, int len) {
			int end = pos + len - 1;
			for (int i = 0; i < len / 2; i++)
				Swap(ref arr[pos + i], ref arr[end - i]);
		}

		private static IEnumerable<int[]> Permutations(int n) {
			int[] arr = Enumerable.Range(0, n).ToArray();
			int[] b = Enumerable.Range(0, n).ToArray();
			int[] c = Enumerable.Repeat(0, n).ToArray();
			while (true) {
				yield return arr;

				int k = 1;
				while (c[k] == k) {
					c[k++] = 0;
					if (k == n) yield break;
				}
				c[k]++;
				Reverse(b, 1, k-1);
				Swap(ref arr[0], ref arr[b[k]]);
			}
		}

		private static List<ModMetadata> AssertSortSatisfied(List<ModMetadata> list) {
			List<ModMetadata> sorted = ModOrganizer.Sort(list);
			Dictionary<string, int> indexMap = sorted.ToDictionary(m => m.UniqueName, sorted.IndexOf);
			foreach (ModMetadata mod in list) {
				int index = indexMap[mod.UniqueName];
				foreach (string dep in mod.SortAfter) {
					if (indexMap.TryGetValue(dep, out int i) && i > index)
						Assert.Fail(mod.UniqueName + " sorted after " + dep);
				}
				foreach (string dep in mod.SortBefore) {
					if (indexMap.TryGetValue(dep, out int i) && i < index)
						Assert.Fail(mod.UniqueName + " sorted before " + dep);
				}
			}

			return sorted;
		}

		/*private static void AssertSortNameIndependent(List<LoadingMod> list) {
			var base_perm = list.Select(m => m.Name).ToList();
			var nameToIndex = base_perm.ToDictionary(m => m, base_perm.IndexOf);
			foreach (var perm in Permutations(list.Count)) {
				foreach (var m in list) {
					m.modFile.name = base_perm[perm[nameToIndex]]
				}
			}
		}*/

		//test missing dependencies
		[Test]
		public static void TestDependenciesExist() {
			//test A -> B
			List<ModMetadata> list1 = new()
			{
				Make("A", refs: new[] {"B"}),
				Make("B"),
			};
			ModOrganizer.EnsureDependenciesExist(list1);

			//test A -> B (missing)
			List<ModMetadata> list2 = new()
			{
				Make("A", refs: new[] {"B"})
			};
			AssertModException(
				() => ModOrganizer.EnsureDependenciesExist(list2),
				new[] {"A"},
				"Missing mod: B required by A");

			//test multi reference
			List<ModMetadata> list3 = new()
			{
				Make("A", refs: new[] {"B"}),
				Make("B"),
				Make("C", refs: new[] {"A"})
			};
			ModOrganizer.EnsureDependenciesExist(list3);

			//test one missing reference
			List<ModMetadata> list4 = new()
			{
				Make("A", refs: new[] {"B"}),
				Make("B", refs: new[] {"C"})
			};
			AssertModException(
				() => ModOrganizer.EnsureDependenciesExist(list4),
				new[] {"B"},
				"Missing mod: C required by B");

			/*//test weak reference (missing)
			var list5 = new List<ModMetadata> {
				Make("A", weakRefs: new[] {"B"})
			};
			ModOrganizer.EnsureDependenciesExist(list5, false);
			AssertModException(
				() => ModOrganizer.EnsureDependenciesExist(list5, true),
				new[] {"A"},
				"Missing mod: B required by A");

			//test weak reference (found)
			var list6 = new List<LocalMod> {
				Make("A", weakRefs: new[] {"B"}),
				Make("B")
			};
			ModOrganizer.EnsureDependenciesExist(list6, true);*/

			/*//test strong (found) and weak (missing)
			var list7 = new List<LocalMod> {
				Make("A", refs: new[] {"B"}),
				Make("B", weakRefs: new[] {"C"})
			};
			ModOrganizer.EnsureDependenciesExist(list7, false);
			AssertModException(
				() => ModOrganizer.EnsureDependenciesExist(list7, true),
				new[] {"B"},
				"Missing mod: C required by B");

			//multi test case (missing)
			var list8 = new List<LocalMod> {
				Make("A", refs: new[] {"X"}),
				Make("B", refs: new[] {"Y"}),
				Make("C", refs: new[] {"D"}),
				Make("D", weakRefs: new[] {"E"}),
				Make("E", weakRefs: new[] {"Z"}),
				Make("F", weakRefs: new[] {"Z"})
			};
			AssertModException(
				() => ModOrganizer.EnsureDependenciesExist(list8, false),
				new[] {"A", "B"},
				"Missing mod: X required by A\r\n" +
				"Missing mod: Y required by B");
			AssertModException(
				() => ModOrganizer.EnsureDependenciesExist(list8, true),
				new[] {"A", "B", "E", "F"},
				"Missing mod: X required by A\r\n" +
				"Missing mod: Y required by B\r\n" +
				"Missing mod: Z required by E\r\n" +
				"Missing mod: Z required by F");

			//multi test case (found)
			var list9 = new List<LocalMod> {
				Make("A", refs: new[] {"C"}),
				Make("B", refs: new[] {"C"}),
				Make("C", refs: new[] {"D"}),
				Make("D", weakRefs: new[] {"E"}),
				Make("E", weakRefs: new[] {"F"}),
				Make("F")
			};
			ModOrganizer.EnsureDependenciesExist(list9, false);
			ModOrganizer.EnsureDependenciesExist(list9, true);*/
		}

		//test missing dependencies
		[Test]
		public static void TestVersionRequirements() {
			//test version on missing mod
			List<ModMetadata> list1 = new()
			{
				Make("A", refs: new[] {"B@1.2"})
			};
			ModOrganizer.EnsureTargetVersionsMet(list1);

			//test passed version check
			List<ModMetadata> list2 = new()
			{
				Make("A", refs: new[] {"B@1.2"}),
				Make("B", version: "1.2")
			};
			ModOrganizer.EnsureTargetVersionsMet(list2);

			//test failed version check
			List<ModMetadata> list3 = new()
			{
				Make("A", refs: new[] {"B@1.2"}),
				Make("B")
			};
			AssertModException(
				() => ModOrganizer.EnsureTargetVersionsMet(list3),
				new[] { "A" },
				"A requires version 1.2+ of B but version 1.0.0.0 is installed");

			//test one pass, two fail version check
			List<ModMetadata> list4 = new()
			{
				Make("A"),
				Make("B", refs: new[] {"A@0.9"}),
				Make("C", refs: new[] {"A@1.1"}),
				Make("D", refs: new[] {"A@1.0.0.1"})
			};
			AssertModException(
				() => ModOrganizer.EnsureTargetVersionsMet(list4),
				new[] { "C", "D" },
				"C requires version 1.1+ of A but version 1.0.0.0 is installed\r\n" +
				"D requires version 1.0.0.1+ of A but version 1.0.0.0 is installed");
			
			/*//test weak version check (missing)
			var list5 = new List<LocalMod> {
				Make("A", weakRefs: new[] {"B@1.1"})
			};
			ModOrganizer.EnsureDependenciesExist(list5, false);
			ModOrganizer.EnsureTargetVersionsMet(list5);

			//test weak version check (too low)
			var list6 = new List<LocalMod> {
				Make("A", weakRefs: new[] {"B@1.1"}),
				Make("B")
			};
			AssertModException(
				() => ModOrganizer.EnsureTargetVersionsMet(list6),
				new[] { "A" },
				"A requires version 1.1+ of B but version 1.0.0.0 is installed");*/
		}

		[Test]
		public static void TestSortOrder() {
			//general complex one way edge sort
			List<ModMetadata> list = new()
			{
				Make("A"),
				Make("B", sortAfter: new [] {"A"}),
				Make("C", sortAfter: new [] {"A"}, sortBefore: new[] {"B"}),
				Make("D", sortAfter: new [] {"H"}),
				Make("E", sortAfter: new [] {"C"}),
				Make("F", sortBefore: new [] {"G"}),
				Make("G", sortAfter: new [] {"B", "C"}),
				Make("H", sortAfter: new [] {"G"}, sortBefore: new [] {"D"}),
			};
			AssertSortSatisfied(list);

			//mutually satisfiable cycle
			List<ModMetadata> list2 = new()
			{
				Make("A", sortBefore: new [] {"B"}),
				Make("B", sortAfter: new [] {"A"})
			};
			AssertSortSatisfied(list2);

			//direct cycle
			List<ModMetadata> list3 = new()
			{
				Make("A", sortAfter: new [] {"B"}),
				Make("B", sortAfter: new [] {"A"})
			};
			AssertModException(
				() => AssertSortSatisfied(list3),
				new[] { "A", "B" },
				"Dependency Cycle: A -> B -> A");

			//complex unsatisfiable sort
			List<ModMetadata> list4 = new()
			{
				Make("A"),
				Make("B", sortAfter: new [] {"A"}),
				Make("C", sortBefore: new [] {"A", "D"}),
				Make("D", sortAfter: new [] {"I"}),
				Make("E", sortAfter: new [] {"C"}),
				Make("F", sortBefore: new [] {"A, I"}),
				Make("G", sortAfter: new [] {"B", "C"}),
				Make("H", sortBefore: new [] {"G", "F"}),
				Make("I", sortAfter: new [] {"G", "H"}, sortBefore: new[] {"C"})
			};
			AssertModException(
				() => AssertSortSatisfied(list4),
				new[] { "A", "B", "C", "G", "I" },
				"Dependency Cycle: A -> C -> I -> G -> B -> A\n" +
				"Dependency Cycle: C -> I -> G -> C");
		}

		/*[Test]
		public static void TestSidedSorts() {
			//basic B is a client mod
			var list = new List<LocalMod> {
				Make("A"),
				Make("B", sortAfter: new[] {"A"}, side: ModSide.Client),
				Make("C", sortAfter: new[] {"B"})
			};
			AssertModException(
				() => AssertSortSatisfied(list),
				new[] { "C" },
				"C indirectly depends on A via C -> B -> A\r\n"+
				"Some of these mods may not exist on both client and server. Add a direct sort entries or weak references.");
			
			//apply above advice
			var list2 = new List<LocalMod> {
				Make("A"),
				Make("B", sortAfter: new[] {"A"}, side: ModSide.Client),
				Make("C", sortAfter: new[] {"B", "A"})
			};
			AssertSortSatisfied(list2);

			//diamond pattern
			var list3 = new List<LocalMod> {
				Make("A"),
				Make("B", sortAfter: new[] {"A"}, side: ModSide.Client),
				Make("C", sortAfter: new[] {"A"}, side: ModSide.Client),
				Make("D", sortAfter: new[] {"C", "B"}, side: ModSide.Client),
				Make("E", sortAfter: new[] {"D"})
			};
			AssertModException(
				() => AssertSortSatisfied(list3),
				new[] { "E" },
				"E indirectly depends on A via E -> D -> C -> A\r\n" +
				"E indirectly depends on A via E -> D -> B -> A\r\n" +
				"Some of these mods may not exist on both client and server. Add a direct sort entries or weak references.");

			//diamond pattern (fixed)
			var list4 = new List<LocalMod> {
				Make("A"),
				Make("B", sortAfter: new[] {"A"}, side: ModSide.Client),
				Make("C", sortAfter: new[] {"A"}, side: ModSide.Client),
				Make("D", sortAfter: new[] {"C", "B"}, side: ModSide.Client),
				Make("E", sortAfter: new[] {"D", "A"})
			};
			AssertSortSatisfied(list4);
		}*/

		/*[Test]
		public static void TestSidedSortsMatch() {
			//diamond pattern
			var list1 = new List<LocalMod>
			{
				Make("A"),
				Make("B", sortAfter: new[] {"A"}, side: ModSide.Client),
				Make("C", sortAfter: new[] {"A"}, side: ModSide.Client),
				Make("D", sortAfter: new[] {"C", "B"}, side: ModSide.Client),
				Make("E", sortAfter: new[] {"D", "A"})
			};
			List<ModMetadata> s1 = AssertSortSatisfied(list1).Where(m => m.properties.side == ModSide.Both).ToList();
			List<ModMetadata> s2 = AssertSortSatisfied(list1.Where(m => m.properties.side == ModSide.Both).ToList());
			Assert.IsTrue(Enumerable.SequenceEqual(s1, s2));

			//reverse the order
			var list2 = new List<LocalMod>
			{
				Make("E"),
				Make("D", sortAfter: new[] {"E"}, side: ModSide.Client),
				Make("C", sortAfter: new[] {"E"}, side: ModSide.Client),
				Make("B", sortAfter: new[] {"C", "D"}, side: ModSide.Client),
				Make("A", sortAfter: new[] {"B", "E"})
			};
			s1 = AssertSortSatisfied(list2).Where(m => m.properties.side == ModSide.Both).ToList();
			s2 = AssertSortSatisfied(list2.Where(m => m.properties.side == ModSide.Both).ToList());
			Assert.IsTrue(Enumerable.SequenceEqual(s1, s2));

			//mostly independent sort with random client only before/afters
			var list3 = new List<LocalMod>
			{
				Make("A"),
				Make("B", ModSide.Client, sortBefore: new[] {"A"}),
				Make("C"),
				Make("D", ModSide.Client, sortAfter: new[] {"F"}),
				Make("E", sortAfter: new[] {"G"}),
				Make("F", ModSide.Client, sortAfter: new[] {"E, G"}),
				Make("G"),
				Make("H"),
			};
			s1 = AssertSortSatisfied(list3).Where(m => m.properties.side == ModSide.Both).ToList();
			s2 = AssertSortSatisfied(list3.Where(m => m.properties.side == ModSide.Both).ToList());
			Assert.IsTrue(Enumerable.SequenceEqual(s1, s2));
		}*/
    }
}