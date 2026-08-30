# Artificial Life — Unity Evolution Simulation

An artificial-life sandbox where simple organisms live in a flat world, perceive it through
directional sensors, decide with small feed-forward neural networks, and **evolve over generations**
by reproduction and mutation. Behaviour is never scripted — the environment supplies the selection
pressure and useful behaviour (foraging, avoidance, movement patterns) is expected to *emerge*.

> Design rule: **don't program intelligence when evolution can discover it.**
> `Sensor → Neural Network → Action`, and a genetic algorithm tunes the network.

## What it does

Each simulation tick, every organism:

1. **Senses** — 8 normalised numbers: five forward food-proximity rays, own energy, own age,
   nearest other organism ahead.
2. **Thinks** — a fixed `8 → 6 → 3` `tanh` network (75 weights + biases) runs one forward pass.
3. **Acts** — output 0 steers, output 1 is throttle, output 2 gates eating.
4. **Pays energy** — metabolism plus a movement cost; eating food restores energy.
5. **Lives or dies** — energy `≤ 0` or age past its lifespan ends the organism.

Between generations the population is scored (time alive + food eaten + offspring), parents are
chosen by tournament selection with elitism, and the next generation is their mutated offspring.
Given a seed, a whole run is bit-for-bit reproducible.

There is no neural-network *training* (no backpropagation, no labelled data). The network's
parameters **are** the genome; mutation perturbs them and selection keeps what works.

## Status

Prototype in progress — the doc's Milestones 1–5.

| Milestone | State |
|---|---|
| 1 — organism moves, burns energy, dies | ✅ |
| 2 — food, eating, energy economy | ✅ |
| 3 — feed-forward neural-network brains + sensors | ✅ |
| 4 — real genome, reproduction, mutation | ⬜ next |
| 5 — fixed generations, selection, statistics, HUD | ⬜ |

Out of scope for now: predators, multiple species, physical-trait evolution, NEAT topology
evolution, save/load, in-editor graphs.

## Running it

- **Unity 6000.5.9f1** (see `ProjectSettings/ProjectVersion.txt`).
- Open the project, open `Assets/Scenes/Simulation.unity`, press **Play**.
- The `SimulationManager` object holds a `SimulationSettings` asset
  (`Assets/ScriptableObjects/DefaultSimSettings.asset`) — every tunable (seed, population, energy
  economy, mutation rates, fitness weights) lives there and can be edited while playing.
- Select an organism in the Hierarchy during Play to see its vision-ray gizmos and live
  energy/age/food-consumed in the Inspector.
- EditMode tests: **Window → General → Test Runner → EditMode → Run All**.

Speed: `DeterministicLockstep` runs a fixed number of sim ticks per frame (reproducible). Turn it
off for a real-time-ish accumulator with a per-frame cap.

## Project layout

```
Assets/Scripts/
  Core/         Rng (seeded), SimulationSettings (ScriptableObject), SimulationManager (fixed-tick clock)
  Environment/  Food, EnvironmentManager (food pool + regeneration + registries)
  Organisms/    Organism (sense→think→act, energy, death), Sensors (world → input vector)
  Brain/        ActivationFunctions, Layer, NeuralNetwork  — pure C#, no MonoBehaviour
  Evolution/    Genome, Mutation  (Selection / EvolutionManager / stats arrive in M4–M5)
  UI/           (SimulationHud arrives in M5)
Assets/Tests/   ArtificialLife.Tests EditMode assembly — NeuralNetworkTests
```

Design choices: transform-based motion on the XZ plane (no Rigidbody/PhysX), one central
fixed `1/60 s` tick (nothing else uses `Update()` for sim logic), one seeded RNG threaded
everywhere (never `UnityEngine.Random`), brain as plain serialisable data.

## Documentation

Two documents live in `../docs/` (one directory up from this repo):

- **`Artificial Life — Unity Evolution Simulation.md`** — the full design doc: goals, the long-term
  vision, every planned system through predators / species / NEAT.
- **`guide.md`** — the build manual for the prototype: step-by-step instructions, complete code for
  every file, and the neural-network theory worked from scratch (neuron model, activation
  functions, forward pass by hand, why a genetic algorithm replaces backprop, weight
  initialisation). Milestone checkpoints match the status table above.
