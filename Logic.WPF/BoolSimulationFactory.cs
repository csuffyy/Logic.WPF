﻿using Logic.Core;
using Logic.Graph;
using Logic.Simulation;
using Logic.Simulation.Blocks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.WPF
{
    public static class BoolSimulationFactory
    {
        public static IDictionary<string, Func<BoolSimulation>> SimulationsDict
            = new Dictionary<string, Func<BoolSimulation>>()
        {
            // Gates
            { "AND", () => { return new AndSimulation(); } },
            { "INVERTER", () => { return new InverterSimulation(); } },
            { "OR", () => { return new OrSimulation(); } },
            // Memory
            { "SR-RESET", () => { return new MemorySetResetSimulation(MemoryPriority.Reset); } },
            { "SR-RESET-V", () => { return new MemorySetResetSimulation(MemoryPriority.Reset); } },
            { "SR-SET", () => { return new MemorySetResetSimulation(MemoryPriority.Set); } },
            { "SR-SET-V", () => { return new MemorySetResetSimulation(MemoryPriority.Set); } },
             // Shortcut
            { "SHORTCUT", () => { return new ShortcutSimulation(); } },
            // Signal
            { "SIGNAL", () => { return new SignalSimulation(false); } },
            // Timers
            { "TIMER-OFF", () => { return new TimerOffSimulation(1.0); } },
            { "TIMER-ON", () => { return new TimerOnSimulation(1.0); } },
            { "TIMER-PULSE", () => { return new TimerPulseSimulation(1.0); } }
        };

        public static IDictionary<XBlock, BoolSimulation> Create(PageGraphContext context)
        {
            var simulations = new Dictionary<XBlock, BoolSimulation>();
            foreach (var block in context.OrderedBlocks)
            {
                if (SimulationsDict.ContainsKey(block.Name))
                {
                    simulations.Add(block, SimulationsDict[block.Name]());
                }
                else
                {
                    throw new Exception("Not supported block simulation.");
                }
            }

            // find ordered block Inputs
            foreach (var block in context.OrderedBlocks)
            {
                var inputs = block.Pins
                    .Where(pin => context.PinTypes[pin] == PinType.Input)
                    .SelectMany(pin =>
                    {
                        return context.Dependencies[pin]
                            .Where(dep => context.PinTypes[dep.Item1] == PinType.Output);
                    })
                    .Select(pin => pin);

                // convert inputs to BoolInput
                var simulation = simulations[block];
                simulation.Inputs = inputs.Select(input =>
                {
                    return new BoolInput()
                    {
                        Simulation = simulations[input.Item1.Owner],
                        IsInverted = input.Item2
                    };
                }).ToArray();
            }
            return simulations;
        }

        public static void Run(IDictionary<XBlock, BoolSimulation> simulations, IClock clock)
        {
            foreach (var simulation in simulations)
            {
                simulation.Value.Run(clock);
            }
        }
    }
}
