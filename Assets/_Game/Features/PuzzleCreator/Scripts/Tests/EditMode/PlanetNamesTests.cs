using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PlanetNamesTests
    {
        [Test]
        public void Assign_GivesEachPlanetItsOwnName()
        {
            PuzzleFieldPlanet[] planets = Planets(3);
            PlanetNames names = Names("alpha", "bravo", "charlie");

            names.Assign(planets);

            Assert.AreEqual("alpha", planets[0].name);
            Assert.AreEqual("bravo", planets[1].name);
            Assert.AreEqual("charlie", planets[2].name);
            Object.DestroyImmediate(names);
        }

        [Test]
        public void Assign_IsStableForTheSamePlanets()
        {
            PuzzleFieldPlanet[] first = Planets(3);
            PuzzleFieldPlanet[] second = Planets(3);
            PlanetNames names = Names("alpha", "bravo", "charlie");

            names.Assign(first);
            names.Assign(second);

            for (int i = 0; i < first.Length; i++)
                Assert.AreEqual(first[i].name, second[i].name);

            Object.DestroyImmediate(names);
        }

        [Test]
        public void Bridge_CarriesThePlanetName()
        {
            PuzzleFieldPlanet[] planets = Planets(1);
            planets[0].name = "solmuigawa";

            PuzzleSource[] sources = PuzzleSourceBridge.ToSources(planets);

            Assert.AreEqual("solmuigawa", sources[0].name);
        }

        private static PuzzleFieldPlanet[] Planets(int count)
        {
            PuzzleFieldPlanet[] planets = new PuzzleFieldPlanet[count];
            for (int i = 0; i < count; i++)
                planets[i] = new PuzzleFieldPlanet(Vector3.zero, 0.1f);

            return planets;
        }

        private static PlanetNames Names(params string[] names)
        {
            PlanetNames list = ScriptableObject.CreateInstance<PlanetNames>();
            list.names = names;
            return list;
        }
    }
}
