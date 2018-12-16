﻿using System.Linq;
using FluentAssertions;

using Moq;

using NUnit.Framework;

using Zilon.Core.Common;
using Zilon.Core.Components;
using Zilon.Core.Persons;
using Zilon.Core.Props;
using Zilon.Core.Schemes;
using Zilon.Core.Tests.Common.Schemes;

namespace Zilon.Core.Tests.Persons
{
    [TestFixture]
    public class HumanPersonTests
    {
        /// <summary>
        /// Тест проверяет, что персонаж корректно обрабатывает назначение экипировки.
        /// </summary>
        [Test]
        public void SetEquipment_SetSingleEquipment_HasActs()
        {
            // ARRANGE
            var slotSchemes = new[] {
                new PersonSlotSubScheme{
                    Types = EquipmentSlotTypes.Hand
                }
            };

            var personScheme = new PersonScheme
            {
                Slots = slotSchemes
            };

            var defaultActScheme = new TestTacticalActScheme
            {
                Stats = new TestTacticalActStatsSubScheme()
            };

            var evolutionDataMock = new Mock<IEvolutionData>();
            var evolutionData = evolutionDataMock.Object;

            var survivalRandomSourceMock = new Mock<ISurvivalRandomSource>();
            var survivalRandomSource = survivalRandomSourceMock.Object;

            var person = new HumanPerson(personScheme, defaultActScheme, evolutionData, survivalRandomSource);

            var propScheme = new TestPropScheme
            {
                Equip = new TestPropEquipSubScheme
                {
                    SlotTypes = new[] { EquipmentSlotTypes.Hand }
                }
            };

            var tacticalActScheme = new TestTacticalActScheme
            {
                Stats = new TestTacticalActStatsSubScheme()
            };

            var equipment = new Equipment(propScheme, new[] { tacticalActScheme });

            const int expectedSlotIndex = 0;



            // ACT

            person.EquipmentCarrier.SetEquipment(equipment, expectedSlotIndex);



            // ARRANGE
            person.TacticalActCarrier.Acts[0].Stats.Should().Be(tacticalActScheme.Stats);
        }

        /// <summary>
        /// Тест проверяет, что при получении перка характеристики персонажа пересчитываются.
        /// </summary>
        [Test]
        public void HumanPerson_PerkLeveledUp_StatsRecalculated()
        {
            // ARRANGE

            var slotSchemes = new[] {
                new PersonSlotSubScheme{
                    Types = EquipmentSlotTypes.Hand
                }
            };

            var personScheme = new PersonScheme
            {
                Slots = slotSchemes
            };

            var defaultActScheme = new TestTacticalActScheme
            {
                Stats = new TestTacticalActStatsSubScheme()
            };

            var perkMock = new Mock<IPerk>();
            perkMock.SetupGet(x => x.CurrentLevel).Returns(new PerkLevel(0, 0));
            perkMock.SetupGet(x => x.Scheme).Returns(new PerkScheme
            {
                Levels = new[] {
                    new PerkLevelSubScheme{
                        Rules = new []{
                            new PerkRuleSubScheme{
                                Type = PersonRuleType.Ballistic,
                                Level = PersonRuleLevel.Normal
                            }
                        }
                    }
                }
            });
            var perk = perkMock.Object;

            var stats = new[] {
                new SkillStatItem{Stat = SkillStatType.Ballistic, Value = 10 }
            };

            var evolutionDataMock = new Mock<IEvolutionData>();
            evolutionDataMock.SetupGet(x => x.Perks).Returns(new[] { perk });
            evolutionDataMock.SetupGet(x => x.Stats).Returns(stats);
            var evolutionData = evolutionDataMock.Object;

            var survivalRandomSourceMock = new Mock<ISurvivalRandomSource>();
            var survivalRandomSource = survivalRandomSourceMock.Object;



            // ACT
            var person = new HumanPerson(personScheme, defaultActScheme, evolutionData, survivalRandomSource);



            // ASSERT
            var testedStat = person.EvolutionData.Stats.Single(x => x.Stat == SkillStatType.Ballistic);
            testedStat.Value.Should().Be(11);
        }

        /// <summary>
        /// Тест проверяет, что если экипировать предмет с бронёй,
        /// то броня будет записана в боевые характеристики персонажа.
        /// </summary>
        [Test]
        public void HumanPerson_EquipArmor_ArmorInCombatStats()
        {
            // ARRANGE

            var personScheme = new TestPersonScheme
            {
                Slots = new[]{
                    new PersonSlotSubScheme
                    {
                        Types = EquipmentSlotTypes.Body
                    }
                }
            };

            var defaultAct = new TestTacticalActScheme
            {
                Stats = new TestTacticalActStatsSubScheme
                {
                    Efficient = new Roll(6, 1)
                }
            };

            var evolutionDataMock = new Mock<IEvolutionData>();
            var evolutionData = evolutionDataMock.Object;

            var survivalRandomSourceMock = new Mock<ISurvivalRandomSource>();
            var survivalRandomSource = survivalRandomSourceMock.Object;

            var person = new HumanPerson(personScheme, defaultAct, evolutionData, survivalRandomSource);

            var armorPropScheme = new TestPropScheme
            {
                Equip = new TestPropEquipSubScheme
                {
                    Armors = new IPropArmorItemSubScheme[]
                    {
                        new TestPropArmorItemSubScheme{
                            Impact = ImpactType.Kinetic,
                            ArmorRank = 10,
                            AbsorbtionLevel = PersonRuleLevel.Lesser
                        }
                    },
                    SlotTypes = new [] {
                        EquipmentSlotTypes.Body
                    }
                }
            };

            var armorProp = new Equipment(armorPropScheme, new ITacticalActScheme[0]);



            // ACT
            person.EquipmentCarrier.SetEquipment(armorProp, 0);



            // ASSERT
            person.CombatStats.DefenceStats.Armors[0].Impact.Should().Be(ImpactType.Kinetic);
            person.CombatStats.DefenceStats.Armors[0].ArmorRank.Should().Be(10);
            person.CombatStats.DefenceStats.Armors[0].AbsorbtionLevel.Should().Be(PersonRuleLevel.Lesser);
        }

        /// <summary>
        /// Тест проверяет, что если экипировать несколько предметов с бронёй,
        /// то вся броня будет учитываться.
        /// </summary>
        [Test]
        public void HumanPerson_MultipleArmor_ArmorRankIncreased()
        {
            // ARRANGE

            var personScheme = new TestPersonScheme
            {
                Slots = new[]{
                    new PersonSlotSubScheme
                    {
                        Types = EquipmentSlotTypes.Head
                    },
                    new PersonSlotSubScheme
                    {
                        Types = EquipmentSlotTypes.Body
                    }
                }
            };

            var defaultAct = new TestTacticalActScheme
            {
                Stats = new TestTacticalActStatsSubScheme
                {
                    Efficient = new Roll(6, 1)
                }
            };

            var evolutionDataMock = new Mock<IEvolutionData>();
            var evolutionData = evolutionDataMock.Object;

            var survivalRandomSourceMock = new Mock<ISurvivalRandomSource>();
            var survivalRandomSource = survivalRandomSourceMock.Object;

            var person = new HumanPerson(personScheme, defaultAct, evolutionData, survivalRandomSource);

            var armorHeadPropScheme = new TestPropScheme
            {
                Equip = new TestPropEquipSubScheme
                {
                    Armors = new IPropArmorItemSubScheme[]
                    {
                        new TestPropArmorItemSubScheme{
                            Impact = ImpactType.Kinetic,
                            ArmorRank = 10,
                            AbsorbtionLevel = PersonRuleLevel.Lesser
                        }
                    },
                    SlotTypes = new [] {
                        EquipmentSlotTypes.Head
                    }
                }
            };

            var armorBodyPropScheme = new TestPropScheme
            {
                Equip = new TestPropEquipSubScheme
                {
                    Armors = new IPropArmorItemSubScheme[]
                    {
                        new TestPropArmorItemSubScheme{
                            Impact = ImpactType.Kinetic,
                            ArmorRank = 10,
                            AbsorbtionLevel = PersonRuleLevel.Lesser
                        }
                    },
                    SlotTypes = new [] {
                        EquipmentSlotTypes.Body
                    }
                }
            };

            var armorHeadProp = new Equipment(armorHeadPropScheme, new ITacticalActScheme[0]);
            var armorBodyProp = new Equipment(armorBodyPropScheme, new ITacticalActScheme[0]);



            // ACT
            person.EquipmentCarrier.SetEquipment(armorHeadProp, 0);
            person.EquipmentCarrier.SetEquipment(armorBodyProp, 1);



            // ASSERT
            person.CombatStats.DefenceStats.Armors[0].Impact.Should().Be(ImpactType.Kinetic);
            person.CombatStats.DefenceStats.Armors[0].ArmorRank.Should().Be(15);
            person.CombatStats.DefenceStats.Armors[0].AbsorbtionLevel.Should().Be(PersonRuleLevel.Lesser);
        }

        /// <summary>
        /// Тест проверяет, что если экипировать несколько предметов с бронёй,
        /// то вся броня будет учитываться.
        /// </summary>
        [Test]
        public void HumanPerson_MultipleArmor_ArmorRankIncreased2()
        {
            // ARRANGE

            var personScheme = new TestPersonScheme
            {
                Slots = new[]{
                    new PersonSlotSubScheme
                    {
                        Types = EquipmentSlotTypes.Head
                    },
                    new PersonSlotSubScheme
                    {
                        Types = EquipmentSlotTypes.Body
                    }
                }
            };

            var defaultAct = new TestTacticalActScheme
            {
                Stats = new TestTacticalActStatsSubScheme
                {
                    Efficient = new Roll(6, 1)
                }
            };

            var evolutionDataMock = new Mock<IEvolutionData>();
            var evolutionData = evolutionDataMock.Object;

            var survivalRandomSourceMock = new Mock<ISurvivalRandomSource>();
            var survivalRandomSource = survivalRandomSourceMock.Object;

            var person = new HumanPerson(personScheme, defaultAct, evolutionData, survivalRandomSource);

            var armorHeadPropScheme = new TestPropScheme
            {
                Equip = new TestPropEquipSubScheme
                {
                    Armors = new IPropArmorItemSubScheme[]
                    {
                        new TestPropArmorItemSubScheme{
                            Impact = ImpactType.Kinetic,
                            ArmorRank = 18,
                            AbsorbtionLevel = PersonRuleLevel.Normal
                        }
                    },
                    SlotTypes = new [] {
                        EquipmentSlotTypes.Head
                    }
                }
            };

            var armorBodyPropScheme = new TestPropScheme
            {
                Equip = new TestPropEquipSubScheme
                {
                    Armors = new IPropArmorItemSubScheme[]
                    {
                        new TestPropArmorItemSubScheme{
                            Impact = ImpactType.Kinetic,
                            ArmorRank = 10,
                            AbsorbtionLevel = PersonRuleLevel.Lesser
                        }
                    },
                    SlotTypes = new [] {
                        EquipmentSlotTypes.Body
                    }
                }
            };

            var armorHeadProp = new Equipment(armorHeadPropScheme, new ITacticalActScheme[0]);
            var armorBodyProp = new Equipment(armorBodyPropScheme, new ITacticalActScheme[0]);



            // ACT
            person.EquipmentCarrier.SetEquipment(armorHeadProp, 0);
            person.EquipmentCarrier.SetEquipment(armorBodyProp, 1);



            // ASSERT
            person.CombatStats.DefenceStats.Armors[0].Impact.Should().Be(ImpactType.Kinetic);
            person.CombatStats.DefenceStats.Armors[0].ArmorRank.Should().Be(10 + 9 + 6);
            person.CombatStats.DefenceStats.Armors[0].AbsorbtionLevel.Should().Be(PersonRuleLevel.Lesser);
        }

        /// <summary>
        /// Тест проверяет, что если экипировать несколько предметов с бронёй,
        /// то вся броня будет учитываться.
        /// </summary>
        [Test]
        public void HumanPerson_MultipleArmor_ArmorRankIncreased3()
        {
            // ARRANGE

            var personScheme = new TestPersonScheme
            {
                Slots = new[]{
                    new PersonSlotSubScheme
                    {
                        Types = EquipmentSlotTypes.Head
                    },
                    new PersonSlotSubScheme
                    {
                        Types = EquipmentSlotTypes.Body
                    }
                }
            };

            var defaultAct = new TestTacticalActScheme
            {
                Stats = new TestTacticalActStatsSubScheme
                {
                    Efficient = new Roll(6, 1)
                }
            };

            var evolutionDataMock = new Mock<IEvolutionData>();
            var evolutionData = evolutionDataMock.Object;

            var survivalRandomSourceMock = new Mock<ISurvivalRandomSource>();
            var survivalRandomSource = survivalRandomSourceMock.Object;

            var person = new HumanPerson(personScheme, defaultAct, evolutionData, survivalRandomSource);

            var armorHeadPropScheme = new TestPropScheme
            {
                Equip = new TestPropEquipSubScheme
                {
                    Armors = new IPropArmorItemSubScheme[]
                    {
                        new TestPropArmorItemSubScheme{
                            Impact = ImpactType.Kinetic,
                            ArmorRank = 1,
                            AbsorbtionLevel = PersonRuleLevel.Lesser
                        }
                    },
                    SlotTypes = new [] {
                        EquipmentSlotTypes.Head
                    }
                }
            };

            var armorBodyPropScheme = new TestPropScheme
            {
                Equip = new TestPropEquipSubScheme
                {
                    Armors = new IPropArmorItemSubScheme[]
                    {
                        new TestPropArmorItemSubScheme{
                            Impact = ImpactType.Kinetic,
                            ArmorRank = 18,
                            AbsorbtionLevel = PersonRuleLevel.Grand
                        }
                    },
                    SlotTypes = new [] {
                        EquipmentSlotTypes.Body
                    }
                }
            };

            var armorHeadProp = new Equipment(armorHeadPropScheme, new ITacticalActScheme[0]);
            var armorBodyProp = new Equipment(armorBodyPropScheme, new ITacticalActScheme[0]);



            // ACT
            person.EquipmentCarrier.SetEquipment(armorHeadProp, 0);
            person.EquipmentCarrier.SetEquipment(armorBodyProp, 1);



            // ASSERT
            person.CombatStats.DefenceStats.Armors[0].Impact.Should().Be(ImpactType.Kinetic);
            person.CombatStats.DefenceStats.Armors[0].ArmorRank.Should().Be(1 + 9 + 6 * 2);
            person.CombatStats.DefenceStats.Armors[0].AbsorbtionLevel.Should().Be(PersonRuleLevel.Lesser);
        }

        /// <summary>
        /// Тест проверяет, что если экипировать несколько предметов с бронёй,
        /// то вся броня будет учитываться.
        /// </summary>
        [Test]
        public void HumanPerson_MultipleArmor_ArmorRankIncreased4()
        {
            // ARRANGE

            var personScheme = new TestPersonScheme
            {
                Slots = new[]{
                    new PersonSlotSubScheme
                    {
                        Types = EquipmentSlotTypes.Head
                    },
                    new PersonSlotSubScheme
                    {
                        Types = EquipmentSlotTypes.Body
                    }
                }
            };

            var defaultAct = new TestTacticalActScheme
            {
                Stats = new TestTacticalActStatsSubScheme
                {
                    Efficient = new Roll(6, 1)
                }
            };

            var evolutionDataMock = new Mock<IEvolutionData>();
            var evolutionData = evolutionDataMock.Object;

            var survivalRandomSourceMock = new Mock<ISurvivalRandomSource>();
            var survivalRandomSource = survivalRandomSourceMock.Object;

            var person = new HumanPerson(personScheme, defaultAct, evolutionData, survivalRandomSource);

            var armorHeadPropScheme = new TestPropScheme
            {
                Equip = new TestPropEquipSubScheme
                {
                    Armors = new IPropArmorItemSubScheme[]
                    {
                        new TestPropArmorItemSubScheme{
                            Impact = ImpactType.Kinetic,
                            ArmorRank = 1,
                            AbsorbtionLevel = PersonRuleLevel.Normal
                        }
                    },
                    SlotTypes = new [] {
                        EquipmentSlotTypes.Head
                    }
                }
            };

            var armorBodyPropScheme = new TestPropScheme
            {
                Equip = new TestPropEquipSubScheme
                {
                    Armors = new IPropArmorItemSubScheme[]
                    {
                        new TestPropArmorItemSubScheme{
                            Impact = ImpactType.Kinetic,
                            ArmorRank = 18,
                            AbsorbtionLevel = PersonRuleLevel.Normal
                        }
                    },
                    SlotTypes = new [] {
                        EquipmentSlotTypes.Body
                    }
                }
            };

            var armorHeadProp = new Equipment(armorHeadPropScheme, new ITacticalActScheme[0]);
            var armorBodyProp = new Equipment(armorBodyPropScheme, new ITacticalActScheme[0]);



            // ACT
            person.EquipmentCarrier.SetEquipment(armorHeadProp, 0);
            person.EquipmentCarrier.SetEquipment(armorBodyProp, 1);



            // ASSERT
            person.CombatStats.DefenceStats.Armors[0].Impact.Should().Be(ImpactType.Kinetic);
            person.CombatStats.DefenceStats.Armors[0].ArmorRank.Should().Be(1 + 9);
            person.CombatStats.DefenceStats.Armors[0].AbsorbtionLevel.Should().Be(PersonRuleLevel.Normal);
        }

        /// <summary>
        /// Тест проверяет, что если экипировать несколько предметов с бронёй,
        /// то вся броня будет учитываться.
        /// </summary>
        [Test]
        public void HumanPerson_MultipleDifferentArmor_AllArmorsInStats()
        {
            // ARRANGE

            var personScheme = new TestPersonScheme
            {
                Slots = new[]{
                    new PersonSlotSubScheme
                    {
                        Types = EquipmentSlotTypes.Head
                    },
                    new PersonSlotSubScheme
                    {
                        Types = EquipmentSlotTypes.Body
                    }
                }
            };

            var defaultAct = new TestTacticalActScheme
            {
                Stats = new TestTacticalActStatsSubScheme
                {
                    Efficient = new Roll(6, 1)
                }
            };

            var evolutionDataMock = new Mock<IEvolutionData>();
            var evolutionData = evolutionDataMock.Object;

            var survivalRandomSourceMock = new Mock<ISurvivalRandomSource>();
            var survivalRandomSource = survivalRandomSourceMock.Object;

            var person = new HumanPerson(personScheme, defaultAct, evolutionData, survivalRandomSource);

            var armorHeadPropScheme = new TestPropScheme
            {
                Equip = new TestPropEquipSubScheme
                {
                    Armors = new IPropArmorItemSubScheme[]
                    {
                        new TestPropArmorItemSubScheme{
                            Impact = ImpactType.Kinetic,
                            ArmorRank = 1,
                            AbsorbtionLevel = PersonRuleLevel.Lesser
                        },
                        new TestPropArmorItemSubScheme{
                            Impact = ImpactType.Psy,
                            ArmorRank = 10,
                            AbsorbtionLevel = PersonRuleLevel.Normal
                        }
                    },
                    SlotTypes = new [] {
                        EquipmentSlotTypes.Head
                    }
                }
            };

            var armorBodyPropScheme = new TestPropScheme
            {
                Equip = new TestPropEquipSubScheme
                {
                    Armors = new IPropArmorItemSubScheme[]
                    {
                        new TestPropArmorItemSubScheme{
                            Impact = ImpactType.Kinetic,
                            ArmorRank = 18,
                            AbsorbtionLevel = PersonRuleLevel.Normal
                        }
                    },
                    SlotTypes = new [] {
                        EquipmentSlotTypes.Body
                    }
                }
            };

            var armorHeadProp = new Equipment(armorHeadPropScheme, new ITacticalActScheme[0]);
            var armorBodyProp = new Equipment(armorBodyPropScheme, new ITacticalActScheme[0]);



            // ACT
            person.EquipmentCarrier.SetEquipment(armorHeadProp, 0);
            person.EquipmentCarrier.SetEquipment(armorBodyProp, 1);



            // ASSERT
            person.CombatStats.DefenceStats.Armors[0].Impact.Should().Be(ImpactType.Kinetic);
            person.CombatStats.DefenceStats.Armors[0].ArmorRank.Should().Be(1 + 9 + 6);
            person.CombatStats.DefenceStats.Armors[0].AbsorbtionLevel.Should().Be(PersonRuleLevel.Lesser);

            person.CombatStats.DefenceStats.Armors[1].Impact.Should().Be(ImpactType.Psy);
            person.CombatStats.DefenceStats.Armors[1].ArmorRank.Should().Be(10);
            person.CombatStats.DefenceStats.Armors[1].AbsorbtionLevel.Should().Be(PersonRuleLevel.Normal);
        }
    }
}