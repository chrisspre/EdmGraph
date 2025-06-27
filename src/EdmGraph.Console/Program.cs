using LabeledPropertyGraph.Net;

namespace EdmGraph.Console;

static class Program
{
    static void Main(string[] args)
    {
        System.Console.WriteLine("=== EdmGraph Labeled Property Graph Demo ===");
        System.Console.WriteLine();

        DemoBasicGraphOperations();
        System.Console.WriteLine();
        DemoGraphTraversal();
        System.Console.WriteLine();
        DemoGraphQueries();



        System.Console.WriteLine();
        // System.Console.WriteLine("Press any key to exit...");
        // System.Console.ReadKey();
    }

    static void DemoBasicGraphOperations()
    {
        System.Console.WriteLine("=== Basic Graph Operations Demo ===");

        var graph = new Graph();

        // Create nodes
        if (graph.TryCreateNode("Person", out var alice) && alice != null)
        {
            alice.AddProperty("name", "Alice")
                 .AddProperty("age", 30)
                 .AddProperty("city", "Seattle");
        }

        if (graph.TryCreateNode("Person", out var bob) && bob != null)
        {
            bob.AddProperty("name", "Bob")
               .AddProperty("age", 25)
               .AddProperty("city", "Portland");
        }

        if (graph.TryCreateNode("Company", out var company) && company != null)
        {
            company.AddProperty("name", "TechCorp")
                   .AddProperty("industry", "Technology");
        }

        // Create edges
        Edge? worksAt1 = null;
        if (alice != null && company != null && graph.TryCreateEdge("WORKS_AT", alice, company, out worksAt1) && worksAt1 != null)
        {
            worksAt1.AddProperty("role", "Software Engineer")
                    .AddProperty("startDate", "2020-01-15");
        }

        if (bob != null && company != null && graph.TryCreateEdge("WORKS_AT", bob, company, out var worksAt2) && worksAt2 != null)
        {
            worksAt2.AddProperty("role", "Product Manager")
                    .AddProperty("startDate", "2021-03-01");
        }

        if (alice != null && bob != null && graph.TryCreateEdge("KNOWS", alice, bob, out var knows) && knows != null)
        {
            knows.AddProperty("since", "2019-05-10")
                 .AddProperty("relationship", "colleague");
        }

        var stats = graph.GetStatistics();
        System.Console.WriteLine($"Graph created with {stats.NodeCount} nodes and {stats.EdgeCount} edges");
        System.Console.WriteLine($"Node labels: {stats.NodeLabels}, Edge labels: {stats.EdgeLabels}");

        // Test property access
        if (alice?.TryGetProperty<string>("name", out var aliceName) == true)
        {
            System.Console.WriteLine($"Alice's name: {aliceName}");
        }

        if (worksAt1?.TryGetProperty<string>("role", out var role) == true)
        {
            System.Console.WriteLine($"Alice's role: {role}");
        }



        graph.WriteGraphToMermaidHtmlFile("graph.html");
    }

    static void DemoGraphTraversal()
    {
        System.Console.WriteLine("=== Graph Traversal Demo ===");

        var graph = new Graph();

        // Create a simple social network
        graph.TryCreateNode("Person", out var alice);
        alice?.AddProperty("name", "Alice");

        graph.TryCreateNode("Person", out var bob);
        bob?.AddProperty("name", "Bob");

        graph.TryCreateNode("Person", out var charlie);
        charlie?.AddProperty("name", "Charlie");

        graph.TryCreateNode("Person", out var diana);
        diana?.AddProperty("name", "Diana");

        // Create relationships
        if (alice != null && bob != null)
        {
            graph.TryCreateEdge("FOLLOWS", alice, bob, out _);
        }
        if (alice != null && charlie != null)
        {
            graph.TryCreateEdge("FOLLOWS", alice, charlie, out _);
        }
        if (bob != null && diana != null)
        {
            graph.TryCreateEdge("FOLLOWS", bob, diana, out _);
        }
        if (alice != null && charlie != null)
        {
            graph.TryCreateEdge("FRIENDS", alice, charlie, out _);
        }

        // Demonstrate single-step traversal
        System.Console.WriteLine("Who does Alice follow?");
        if (alice != null)
        {
            var aliceFollows = alice.TraverseOut("FOLLOWS");
            foreach (var person in aliceFollows)
            {
                if (person.TryGetProperty<string>("name", out var name))
                {
                    System.Console.WriteLine($"  - {name}");
                }
            }
        }

        System.Console.WriteLine("Who follows Bob?");
        if (bob != null)
        {
            var bobFollowers = bob.TraverseIn("FOLLOWS");
            foreach (var person in bobFollowers)
            {
                if (person.TryGetProperty<string>("name", out var name))
                {
                    System.Console.WriteLine($"  - {name}");
                }
            }
        }

        System.Console.WriteLine("All of Alice's outgoing connections:");
        if (alice != null)
        {
            var aliceConnections = alice.TraverseOut();
            foreach (var person in aliceConnections)
            {
                if (person.TryGetProperty<string>("name", out var name))
                {
                    System.Console.WriteLine($"  - {name}");
                }
            }
        }
    }

    static void DemoGraphQueries()
    {
        System.Console.WriteLine("=== Graph Queries Demo ===");

        var graph = new Graph();

        // Create nodes with different labels
        graph.TryCreateNode("Person", out var p1);
        graph.TryCreateNode("Person", out var p2);
        graph.TryCreateNode("Company", out var c1);
        graph.TryCreateNode("Company", out var c2);
        graph.TryCreateNode("Department", out var d1);

        // Query by label
        var people = graph.GetNodesByLabel("Person");
        System.Console.WriteLine($"Found {people.Count()} people in the graph");

        var companies = graph.GetNodesByLabel("Company");
        System.Console.WriteLine($"Found {companies.Count()} companies in the graph");

        // Create some edges
        if (p1 != null && c1 != null)
        {
            graph.TryCreateEdge("WORKS_AT", p1, c1, out _);
        }
        if (p2 != null && c2 != null)
        {
            graph.TryCreateEdge("WORKS_AT", p2, c2, out _);
        }
        if (p1 != null && d1 != null)
        {
            graph.TryCreateEdge("MANAGES", p1, d1, out _);
        }

        var workRelations = graph.GetEdgesByLabel("WORKS_AT");
        System.Console.WriteLine($"Found {workRelations.Count()} work relationships");

        // Test node containment
        if (p1 != null && graph.ContainsNode(p1))
        {
            System.Console.WriteLine($"Found node: {p1}");
        }

        // Test removal
        System.Console.WriteLine("Removing person p2...");
        if (p2 != null && graph.TryRemoveNode(p2))
        {
            var newStats = graph.GetStatistics();
            System.Console.WriteLine($"After removal: {newStats.NodeCount} nodes, {newStats.EdgeCount} edges");
        }
    }
}
