using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotaLeagueForm.Entities
{
    public static class HeroesDB
    {
        static Dictionary<uint, string> heroes = new Dictionary<uint, string>();

        public static string GetHeroById(uint id)
        {
            if (heroes.Count == 0)
            {
                heroes.Add(1, "Anti-Mage");
                heroes.Add(2, "Axe");
                heroes.Add(3, "Bane");
                heroes.Add(4, "Bloodseeker");
                heroes.Add(5, "Crystal Maiden");
                heroes.Add(6, "Drow Ranger");
                heroes.Add(7, "Earthshaker");
                heroes.Add(8, "Juggernaut");
                heroes.Add(9, "Mirana");
                heroes.Add(11, "Shadow Fiend");
                heroes.Add(10, "Morphling");
                heroes.Add(12, "Phantom Lancer");
                heroes.Add(13, "Puck");
                heroes.Add(14, "Pudge");
                heroes.Add(15, "Razor");
                heroes.Add(16, "Sand King");
                heroes.Add(17, "Storm Spirit");
                heroes.Add(18, "Sven");
                heroes.Add(19, "Tiny");
                heroes.Add(20, "Vengeful Spirit");
                heroes.Add(21, "Windranger");
                heroes.Add(22, "Zeus");
                heroes.Add(23, "Kunkka");
                heroes.Add(25, "Lina");
                heroes.Add(31, "Lich");
                heroes.Add(26, "Lion");
                heroes.Add(27, "Shadow Shaman");
                heroes.Add(28, "Slardar");
                heroes.Add(29, "Tidehunter");
                heroes.Add(30, "Witch Doctor");
                heroes.Add(32, "Riki");
                heroes.Add(33, "Enigma");
                heroes.Add(34, "Tinker");
                heroes.Add(35, "Sniper");
                heroes.Add(36, "Necrophos");
                heroes.Add(37, "Warlock");
                heroes.Add(38, "Beastmaster");
                heroes.Add(39, "Queen of Pain");
                heroes.Add(40, "Venomancer");
                heroes.Add(41, "Faceless Void");
                heroes.Add(42, "Skeleton King");
                heroes.Add(43, "Death Prophet");
                heroes.Add(44, "Phantom Assassin");
                heroes.Add(45, "Pugna");
                heroes.Add(46, "Templar Assassin");
                heroes.Add(47, "Viper");
                heroes.Add(48, "Luna");
                heroes.Add(49, "Dragon Knight");
                heroes.Add(50, "Dazzle");
                heroes.Add(51, "Clockwerk");
                heroes.Add(52, "Leshrac");
                heroes.Add(53, "Nature's Prophet");
                heroes.Add(54, "Lifestealer");
                heroes.Add(55, "Dark Seer");
                heroes.Add(56, "Clinkz");
                heroes.Add(57, "Omniknight");
                heroes.Add(58, "Enchantress");
                heroes.Add(59, "Huskar");
                heroes.Add(60, "Night Stalker");
                heroes.Add(61, "Broodmother");
                heroes.Add(62, "Bounty Hunter");
                heroes.Add(63, "Weaver");
                heroes.Add(64, "Jakiro");
                heroes.Add(65, "Batrider");
                heroes.Add(66, "Chen");
                heroes.Add(67, "Spectre");
                heroes.Add(69, "Doom");
                heroes.Add(68, "Ancient Apparition");
                heroes.Add(70, "Ursa");
                heroes.Add(71, "Spirit Breaker");
                heroes.Add(72, "Gyrocopter");
                heroes.Add(73, "Alchemist");
                heroes.Add(74, "Invoker");
                heroes.Add(75, "Silencer");
                heroes.Add(76, "Outworld Devourer");
                heroes.Add(77, "Lycanthrope");
                heroes.Add(78, "Brewmaster");
                heroes.Add(79, "Shadow Demon");
                heroes.Add(80, "Lone Druid");
                heroes.Add(81, "Chaos Knight");
                heroes.Add(82, "Meepo");
                heroes.Add(83, "Treant Protector");
                heroes.Add(84, "Ogre Magi");
                heroes.Add(85, "Undying");
                heroes.Add(86, "Rubick");
                heroes.Add(87, "Disruptor");
                heroes.Add(88, "Nyx Assassin");
                heroes.Add(89, "Naga Siren");
                heroes.Add(90, "Keeper of the Light");
                heroes.Add(91, "Wisp");
                heroes.Add(92, "Visage");
                heroes.Add(93, "Slark");
                heroes.Add(94, "Medusa");
                heroes.Add(95, "Troll Warlord");
                heroes.Add(96, "Centaur Warrunner");
                heroes.Add(97, "Magnus");
                heroes.Add(98, "Timbersaw");
                heroes.Add(99, "Bristleback");
                heroes.Add(100, "Tusk");
                heroes.Add(101, "Skywrath Mage");
                heroes.Add(102, "Abaddon");
                heroes.Add(103, "Elder Titan");
                heroes.Add(104, "Legion Commander");
                heroes.Add(106, "Ember Spirit");
                heroes.Add(107, "Earth Spirit");
                heroes.Add(108, "Abyssal Underlord");
                heroes.Add(109, "Terrorblade");
                heroes.Add(110, "Phoenix");
                heroes.Add(105, "Techies");
            }
            return heroes[id];
        }
    }
}

