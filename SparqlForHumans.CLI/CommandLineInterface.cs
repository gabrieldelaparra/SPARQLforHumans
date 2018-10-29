using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleKit;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Models.RDFQuery;

namespace SparqlForHumans.CLI
{
    public class CommandLineInterface
    {
        private static Menu _menu;
        private static List<RDFEntity> entities = new List<RDFEntity>();

        private static RDFGraph rdfGraph = new RDFGraph();

        public CommandLineInterface()
        {
            Console.Title = "Console Kit Demonstration";

            _menu = new Menu()
            {
                HighlightColor = ConsoleColor.Green,

                Options = new string[]
                {
                    "Add new Entity",
                    "Add new Type",
                    "Add Relations",
                    "Remove Element",
                    "View Graph",
                    "Exit"
                }
            };

            PresentMenu();
        }

        private static void PresentMenu()
        {
            var choice = _menu.AwaitInput();

            switch (choice)
            {
                case 0:
                    AddEntity();
                    break;
                case 1:
                    AddType();
                    break;
                case 2:
                    AddRelations();
                    break;
                case 3:
                    RemoveElement();
                    break;
                case 4:
                    PrintGraph();
                    break;
                default:
                    break;
            }
        }

        private static void PrintGraph()
        {
            Console.WriteLine("\nEntities:\n");
            var table1 = new Table(100, 2);
            table1.BuildTable(entities);

            Console.WriteLine("\nGraph:\n");
            var table2 = new Table(100, 2);
            table2.BuildTable(rdfGraph.QueryTriples.ToList());

            DisplayInterstitial("Add Type");
        }

        private static void DisplayInterstitial(string completedPrompt)
        {
            Console.WriteLine("\n{0}, press any key to return to the menu...", completedPrompt);
            Console.ReadKey();

            Console.Clear();
            PresentMenu();
        }

        private static void RemoveElement()
        {
            DisplayInterstitial("Element removed");
        }

        private static void AddRelations()
        {
            //Select Subject
            var subjectMenu = new Menu()
            {
                HighlightColor = ConsoleColor.Green,
                Options = entities?.Select(x => x.ToString()).ToArray()
            };
            Console.WriteLine("Selected Subject:");
            var subjectChoice = subjectMenu.AwaitInput();
            var selectedSubject = entities.ElementAt(subjectChoice);

            Console.WriteLine($"Subject: {selectedSubject}");

            //Select Object
            var objectMenu = new Menu()
            {
                HighlightColor = ConsoleColor.Green,
                Options = entities?.Select(x => x.ToString()).ToArray()
            };
            Console.WriteLine("Selected Object:");
            var objectChoice = objectMenu.AwaitInput();
            var selectedObject = entities.ElementAt(objectChoice);
            Console.WriteLine($"Object: {selectedObject}");

            //Select Predicate
            var propertiesMenu = new Menu()
            {
                HighlightColor = ConsoleColor.Green,
                Options = selectedSubject.Properties.Select(x=>x.ToString()).ToArray(),
            };
            Console.WriteLine("Selected Predicate:");
            var propertyChoice = propertiesMenu.AwaitInput();
            var selectedProperty = selectedSubject.Properties.ElementAt(propertyChoice);
            Console.WriteLine($"Property: {selectedProperty}");

            rdfGraph.QueryTriples.Add(new RDFTriple()
            {
                Subject = selectedSubject,
                Predicate = new RDFProperty(selectedProperty),
                Object =  selectedObject,
            });

            DisplayInterstitial("Relation Added");

        }

        private static void AddType()
        {
            Console.WriteLine("Add new Type.\nPlease type the name of the Entity:");

            var query = string.Empty;

            while (string.IsNullOrWhiteSpace(query))
                query = Console.ReadLine();

            var results = MultiDocumentQueries.QueryEntitiesByLabel(query, true);

            if (results == null || results.Count().Equals(0))
            {
                DisplayInterstitial($"No results found!");
            }

            var entityMenu = new Menu()
            {
                HighlightColor = ConsoleColor.Green,
                Options = results?.Select(x => x.ToRankedString()).ToArray()
            };

            var choice = entityMenu.AwaitInput();

            var selectedEntity = results?.ElementAt(choice);
            entities.Add(new RDFEntity(selectedEntity));

            DisplayInterstitial($"Added {selectedEntity}");

        }

        private static void AddEntity()
        {
            Console.WriteLine("Add new Entity.\nPlease type the name of the Entity:");

            var query = string.Empty;

            while (string.IsNullOrWhiteSpace(query))
                query = Console.ReadLine();

            var results = MultiDocumentQueries.QueryEntitiesByLabel(query);
            if (results == null || results.Count().Equals(0))
            {
                DisplayInterstitial($"No results found!");
            }

            var entityMenu = new Menu()
            {
                HighlightColor = ConsoleColor.Green,
                Options = results?.Select(x => x.ToRankedString()).ToArray()
            };

            var choice = entityMenu.AwaitInput();

            var selectedEntity = results?.ElementAt(choice);
            entities.Add(new RDFEntity(selectedEntity));

            DisplayInterstitial($"Added {selectedEntity}");
        }
    }
}
