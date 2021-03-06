﻿using System;
using System.Linq;
using SettlementSimulation.Host.Common.Models.Dtos;

namespace SettlementSimulation.Host.Common.Models
{
    public class RunSimulationResponse
    {
        public int CurrentEpoch { get; set; }
        public int CurrentGeneration { get; set; }
        public RoadDto MainRoad { get; set; }
        public RoadDto[] Roads { get; set; }
        public BuildingDto[] Buildings { get; set; }
        public RoadDto[] LastGeneratedRoads { get; set; }
        public BuildingDto[] LastGeneratedBuildings { get; set; }
        public Tuple<RoadDto, RoadDto>[] LastUpdatedRoads { get; set; }
        public Tuple<BuildingDto, BuildingDto>[] LastUpdatedBuildings { get; set; }
        public BuildingDto[] LastRemovedBuildings { get; set; }

        public override string ToString()
        {
            return $"\n\n {nameof(CurrentGeneration)}: {CurrentGeneration}\n" +
                   $"{nameof(MainRoad)}: [{MainRoad.Locations.First()}, {MainRoad.Locations.Last()}] \n" +
                   $"{nameof(CurrentEpoch)}: {CurrentEpoch}  \n" +
                   $"{nameof(Buildings)}: {Buildings.Count()} \n" +
                   $"{nameof(Roads)}: {Roads.Count()} \n" +
                   $"\n{nameof(LastGeneratedRoads)}: \n" +
                   $"{LastGeneratedRoads.Aggregate("", (s1, s2) => s1 + "\n" + s2)}" +
                   $"\n{nameof(LastGeneratedBuildings)}: \n" +
                   $"{LastGeneratedBuildings.Aggregate("", (s1, s2) => s1 + "\n" + s2)}" +
                   $"\n{nameof(LastUpdatedBuildings)}: {LastUpdatedBuildings.Count()} \n" +
                   $"{nameof(LastUpdatedRoads)}: {LastUpdatedRoads.Count()} \n" +
                   $"{nameof(LastRemovedBuildings)}: {LastRemovedBuildings.Count()}";
        }
    }
}